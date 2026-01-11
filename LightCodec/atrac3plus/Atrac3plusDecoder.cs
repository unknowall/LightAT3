using static LightCodec.util.CodecUtils;
using BitReader = LightCodec.util.BitReader;
using FFT = LightCodec.util.FFT;

namespace LightCodec.atrac3plus
{
    public class Atrac3plusDecoder : ICodec
    {
        public const int AT3P_ERROR = -1;
        public const int CH_UNIT_MONO = 0; //< unit containing one coded channel
        public const int CH_UNIT_STEREO = 1; //< unit containing two jointly-coded channels
        public const int CH_UNIT_EXTENSION = 2; //< unit containing extension information
        public const int CH_UNIT_TERMINATOR = 3; //< unit sequence terminator
        public const int ATRAC3P_POWER_COMP_OFF = 15; //< disable power compensation
        public const int ATRAC3P_SUBBANDS = 16; //< number of PQF subbands
        public const int ATRAC3P_SUBBAND_SAMPLES = 128; //< number of samples per subband
        public static readonly int ATRAC3P_FRAME_SAMPLES = ATRAC3P_SUBBANDS * ATRAC3P_SUBBAND_SAMPLES;
        public const int ATRAC3P_PQF_FIR_LEN = 12; //< Length of the prototype FIR of the PQF

        private Context ctx;

        public virtual int init(int bytesPerFrame, int channels, int outputChannels, int codingMode)
        {
            ChannelUnit.init();

            ctx = new Context();
            ctx.outputChannels = outputChannels;
            ctx.dsp = new Atrac3plusDsp();
            for (int i = 0; i < ctx.numChannelBlocks; i++)
            {
                ctx.channelUnits[i] = new ChannelUnit();
                ctx.channelUnits[i].Dsp = ctx.dsp;
            }

            // initialize IPQF
            ctx.ipqfDctCtx = new FFT();
            ctx.ipqfDctCtx.mdctInit(5, true, 31.0 / 32768.9);

            ctx.mdctCtx = new FFT();
            ctx.dsp.initImdct(ctx.mdctCtx);

            Atrac3plusDsp.initWaveSynth();

            ctx.gaincCtx = new Atrac();
            ctx.gaincCtx.initGainCompensation(6, 2);

            return 0;
        }

        public virtual unsafe int decode(void* inputAddr, int inputLength, void* output, out int outputLength)
        {
            outputLength = 0;
            int ret;

            if (ctx == null)
            {
                return AT3P_ERROR;
            }

            if (inputLength < 0)
            {
                return AT3P_ERROR;
            }
            if (inputLength == 0)
            {
                return 0;
            }

            ctx.br = new BitReader(inputAddr, inputLength);
            if (ctx.br.readBool())
            {
                System.Console.WriteLine(string.Format("Invalid start bit"));
                return AT3P_ERROR;
            }

            int chBlock = 0;
            int channelsToProcess = 0;
            while (ctx.br.BitsLeft >= 2)
            {
                int chUnitId = ctx.br.read(2);

                if (chUnitId == CH_UNIT_TERMINATOR)
                {
                    break;
                }
                if (chUnitId == CH_UNIT_EXTENSION)
                {
                    System.Console.WriteLine(string.Format("Non implemented channel unit extension"));
                    return AT3P_ERROR;
                }

                if (chBlock >= ctx.channelUnits.Length)
                {
                    System.Console.WriteLine(string.Format("Too many channel blocks"));
                    return AT3P_ERROR;
                }

                ctx.channelUnits[chBlock].BitReader = ctx.br;

                ctx.channelUnits[chBlock].ctx.unitType = chUnitId;

                channelsToProcess = chUnitId + 1;

                ctx.channelUnits[chBlock].NumChannels = channelsToProcess;

                ret = ctx.channelUnits[chBlock].decode();
                if (ret < 0)
                {
                    return ret;
                }

                ctx.channelUnits[chBlock].decodeResidualSpectrum(ctx.samples);

                ctx.channelUnits[chBlock].reconstructFrame(ctx);

                int sampleBytes = ATRAC3P_FRAME_SAMPLES * ctx.outputChannels * sizeof(short);

                writeOutput(ctx.outpBuf, (short*)output + outputLength / sizeof(short), ATRAC3P_FRAME_SAMPLES, channelsToProcess, ctx.outputChannels);

                outputLength += sampleBytes;

                chBlock++;
            }

            //System.Console.WriteLine(string.Format("Bytes read 0x{0:X}", ctx.br.BytesRead));

            return ctx.br.BytesRead;
        }

        public virtual int NumberOfSamples
        {
            get
            {
                return ATRAC3P_FRAME_SAMPLES;
            }
        }
    }

}