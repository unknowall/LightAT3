using LightCodec.Utils;
using BitReader = LightCodec.Utils.BitReader;
using FFT = LightCodec.Utils.FFT;
using static LightCodec.atrac3plus.Atrac3plusData2;
using Atrac3plusData2 = LightCodec.atrac3plus.Atrac3plusData2;

namespace LightCodec.atrac3plus
{
    public class Context
    {
        public BitReader br;
        public Atrac3plusDsp dsp;

        public ChannelUnit[] channelUnits = new ChannelUnit[16]; //< global channel units
        public int numChannelBlocks = 2; //< number of channel blocks
        public int outputChannels;

        public Atrac gaincCtx; //< gain compensation context
        public FFT mdctCtx;
        public FFT ipqfDctCtx; //< IDCT context used by IPQF

        public float[][] samples = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES); //< quantized MDCT sprectrum

        public float[][] mdctBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES + ATRAC3P_SUBBAND_SAMPLES); //< output of the IMDCT

        public float[][] timeBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES); //< output of the gain compensation

        public float[][] outpBuf = RectangularArrays.ReturnRectangularFloatArray(2, ATRAC3P_FRAME_SAMPLES);
    }

}