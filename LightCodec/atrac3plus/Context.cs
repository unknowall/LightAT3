using LightCodec.util;
using BitReader = LightCodec.util.BitReader;
using FFT = LightCodec.util.FFT;

namespace LightCodec.atrac3plus
{
    public class Context
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