using System;
using static System.Math;

namespace LightCodec.mp3
{
    using LightCodec.Utils;
    using static LightCodec.mp3.Mp3Decoder;
    using static LightCodec.mp3.Mp3Data;
    using Dct32 = LightCodec.Utils.Dct32;
    using Mp3Data = LightCodec.mp3.Mp3Data;

    public class Mp3Dsp
    {
        public static void initMpadspTabs()
        {
            // compute mdct windows
            for (int i = 0; i < 36; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j == 2 && (i % 3) != 1)
                    {
                        continue;
                    }

                    double d = Sin(PI * (i + 0.5) / 36.0);
                    if (j == 1)
                    {
                        if (i >= 30)
                        {
                            d = 0.0;
                        }
                        else if (i >= 24)
                        {
                            d = Sin(PI * (i - 18 + 0.5) / 12.0);
                        }
                        else if (i >= 18)
                        {
                            d = 1.0;
                        }
                    }
                    else if (j == 3)
                    {
                        if (i < 6)
                        {
                            d = 0.0;
                        }
                        else if (i < 12)
                        {
                            d = Sin(PI * (i - 6 + 0.5) / 12.0);
                        }
                        else if (i < 18)
                        {
                            d = 1.0;
                        }
                    }
                    // merge last stage of imdct into the window coefficients
                    d *= 0.5 * IMDCT_SCALAR / Cos(PI * (2 * i + 19) / 72.0);

                    if (j == 2)
                    {
                        mdct_win[j][i / 3] = (float)(d / (1 << 5));
                    }
                    else
                    {
                        int idx = i < 18 ? i : i + (MDCT_BUF_SIZE / 2 - 18);
                        mdct_win[j][idx] = (float)(d / (1 << 5));
                    }
                }
            }

            // NOTE: we do frequency inversion after the MDCT by changing
            // the sign of the right window coeffs
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < MDCT_BUF_SIZE; i += 2)
                {
                    mdct_win[j + 4][i] = mdct_win[j][i];
                    mdct_win[j + 4][i + 1] = -mdct_win[j][i + 1];
                }
            }
        }

        public static void synthInit(float[] window)
        {
            // max = 18760, max sum over all 16 coeffs: 44736
            for (int i = 0; i < 257; i++)
            {
                float v = mpa_enwindow[i];
                v *= 1f / (1L << (16 + FRAC_BITS));
                window[i] = v;
                if ((i & 63) != 0)
                {
                    v = -v;
                }
                if (i != 0)
                {
                    window[512 - i] = v;
                }
            }

            // Needed for avoiding shuffles in ASM implementations
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    window[512 + 16 * i + j] = window[64 * i + 32 - j];
                }
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    window[512 + 128 + 16 * i + j] = window[64 * i + 48 - j];
                }
            }
        }

        private static void applyWindow(float[] synthBuf, int synthBufOffset, float[] window, int[] ditherState, float[] samples, int samplesOffset, int incr)
        {
            Array.Copy(synthBuf, synthBufOffset, synthBuf, synthBufOffset + 512, 32);

            int samples2 = samplesOffset + 31 * incr;
            int w = 0;
            int w2 = 31;

            float sum = ditherState[0];
            int p = synthBufOffset + 16;
            for (int i = 0; i < 8; i++)
            {
                sum += window[w + i * 64] * synthBuf[p + i * 64];
            }
            p = synthBufOffset + 48;
            for (int i = 0; i < 8; i++)
            {
                sum -= window[w + 32 + i * 64] * synthBuf[p + i * 64];
            }
            samples[samplesOffset] = sum;
            sum = 0f;
            samplesOffset += incr;
            w++;

            // we calculate two samples at the same time to avoid one memory
            // access per two sample
            for (int j = 1; j < 16; j++)
            {
                float sum2 = 0f;
                sum = 0f;
                p = synthBufOffset + 16 + j;
                for (int i = 0; i < 8; i++)
                {
                    float tmp = synthBuf[p + i * 64];
                    sum += window[w + i * 64] * tmp;
                    sum2 -= window[w2 + i * 64] * tmp;
                }
                p = synthBufOffset + 48 - j;
                for (int i = 0; i < 8; i++)
                {
                    float tmp = synthBuf[p + i * 64];
                    sum -= window[w + 32 + i * 64] * tmp;
                    sum2 -= window[w2 + 32 + i * 64] * tmp;
                }

                samples[samplesOffset] = sum;
                samplesOffset += incr;
                samples[samples2] = sum2;
                samples2 -= incr;
                w++;
                w2--;
            }

            p = synthBufOffset + 32;
            sum = 0f;
            for (int i = 0; i < 8; i++)
            {
                sum -= window[w + 32 + i * 64] * synthBuf[p + i * 64];
            }
            samples[samplesOffset] = sum;
            sum = 0f;
            ditherState[0] = (int)sum;
        }

        // 32 sub band synthesis filter. Input: 32 sub band samples, Output: 32 samples.
        public static void synthFilter(Context ctx, int ch, float[] samples, int samplesOffset, int incr, float[] sbSamples, int sbSamplesOffset)
        {
            int offset = ctx.synthBufOffset[ch];

            Dct32.dct32(ctx.synthBuf[ch], offset, sbSamples, sbSamplesOffset);
            applyWindow(ctx.synthBuf[ch], offset, mpa_synth_window, ctx.ditherState, samples, samplesOffset, incr);

            offset = (offset - 32) & 511;
            ctx.synthBufOffset[ch] = offset;
        }

        // using Lee like decomposition followed by hand coded 9 points DCT
        private static void imdct36(float[] @out, int outOffset, float[] buf, int bufOffset, float[] @in, int inOffset, float[] win)
        {
            float[] tmp = new float[18];

            for (int n = 17; n >= 1; n--)
            {
                @in[inOffset + n] += @in[inOffset + n - 1];
            }
            for (int nn = 17; nn >= 3; nn -= 2)
            {
                @in[inOffset + nn] += @in[inOffset + nn - 2];
            }

            for (int j = 0; j < 2; j++)
            {
                float tt0;
                int tmp1 = j;
                int in1 = inOffset + j;

                float t2 = @in[in1 + 2 * 4] + @in[in1 + 2 * 8] - @in[in1 + 2 * 2];

                float t3 = @in[in1 + 2 * 0] + @in[in1 + 2 * 6] * 0.5f;
                float tt1 = @in[in1 + 2 * 0] - @in[in1 + 2 * 6];
                tmp[tmp1 + 6] = tt1 - t2 * 0.5f;
                tmp[tmp1 + 16] = tt1 + t2;

                tt0 = (@in[in1 + 2 * 2] + @in[in1 + 2 * 4]) * C2 * 2f;
                tt1 = (@in[in1 + 2 * 4] - @in[in1 + 2 * 8]) * C8 * -2f;
                t2 = (@in[in1 + 2 * 2] + @in[in1 + 2 * 8]) * C4 * -2f;

                tmp[tmp1 + 10] = t3 - tt0 - t2;
                tmp[tmp1 + 2] = t3 + tt0 + tt1;
                tmp[tmp1 + 14] = t3 + t2 - tt1;

                tmp[tmp1 + 4] = (@in[in1 + 2 * 5] + @in[in1 + 2 * 7] - @in[in1 + 2 * 1]) * C3 * -2f;
                t2 = (@in[in1 + 2 * 1] + @in[in1 + 2 * 5]) * C1 * 2f;
                t3 = (@in[in1 + 2 * 5] - @in[in1 + 2 * 7]) * C7 * -2f;
                tt0 = @in[in1 + 2 * 3] * C3 * 2f;

                tt1 = (@in[in1 + 2 * 1] + @in[in1 + 2 * 7]) * C5 * -2f;

                tmp[tmp1 + 0] = t2 + t3 + tt0;
                tmp[tmp1 + 12] = t2 + tt1 - tt0;
                tmp[tmp1 + 8] = t3 - tt1 - tt0;
            }

            int i = 0;
            for (int j = 0; j < 4; j++)
            {
                float tt0 = tmp[i];
                float tt1 = tmp[i + 2];
                float ss0 = tt1 + tt0;
                float s2 = tt1 - tt0;

                float t2 = tmp[i + 1];
                float t3 = tmp[i + 3];
                float ss1 = (t3 + t2) * icos36h[j] * 2f;
                float s3 = (t3 - t2) * icos36[8 - j];

                tt0 = ss0 + ss1;
                tt1 = ss0 - ss1;
                @out[outOffset + (9 + j) * SBLIMIT] = tt1 * win[9 + j] + buf[bufOffset + 4 * (9 + j)];
                @out[outOffset + (8 - j) * SBLIMIT] = tt1 * win[8 - j] + buf[bufOffset + 4 * (8 - j)];
                buf[bufOffset + 4 * (9 + j)] = tt0 * win[MDCT_BUF_SIZE / 2 + 9 + j];
                buf[bufOffset + 4 * (8 - j)] = tt0 * win[MDCT_BUF_SIZE / 2 + 8 - j];

                tt0 = s2 + s3;
                tt1 = s2 - s3;
                @out[outOffset + (9 + 8 - j) * SBLIMIT] = tt1 * win[9 + 8 - j] + buf[bufOffset + 4 * (9 + 8 - j)];
                @out[outOffset + j * SBLIMIT] = tt1 * win[j] + buf[bufOffset + 4 * j];
                buf[bufOffset + 4 * (9 + 8 - j)] = tt0 * win[MDCT_BUF_SIZE / 2 + 9 + 8 - j];
                buf[bufOffset + 4 * j] = tt0 * win[MDCT_BUF_SIZE / 2 + j];
                i += 4;
            }

            float s0 = tmp[16];
            float s1 = tmp[17] * icos36h[4] * 2f;
            float t0 = s0 + s1;
            float t1 = s0 - s1;
            @out[outOffset + (9 + 4) * SBLIMIT] = t1 * win[9 + 4] + buf[bufOffset + 4 * (9 + 4)];
            @out[outOffset + (8 - 4) * SBLIMIT] = t1 * win[8 - 4] + buf[bufOffset + 4 * (8 - 4)];
            buf[bufOffset + 4 * (9 + 4)] = t0 * win[MDCT_BUF_SIZE / 2 + 9 + 4];
            buf[bufOffset + 4 * (8 - 4)] = t0 * win[MDCT_BUF_SIZE / 2 + 8 - 4];
        }

        public static void imdct36Blocks(float[] @out, int outOffset, float[] buf, int bufOffset, float[] @in, int inOffset, int count, int switchPoint, int blockType)
        {
            for (int j = 0; j < count; j++)
            {
                // apply window & overlap with previous buffer

                // select window
                int winIdx = (switchPoint != 0 && j < 2) ? 0 : blockType;
                float[] win = mdct_win[winIdx + (4 & -(j & 1))];

                imdct36(@out, outOffset, buf, bufOffset, @in, inOffset, win);

                inOffset += 18;
                bufOffset += ((j & 3) != 3 ? 1 : (72 - 3));
                outOffset++;
            }
        }
    }

}