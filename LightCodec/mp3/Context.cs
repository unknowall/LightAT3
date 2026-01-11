using LightCodec.Utils;
using static LightCodec.mp3.Mp3Decoder;

namespace LightCodec.mp3
{
    using BitReader = LightCodec.Utils.BitReader;

    public class Context
    {
        public BitReader br;
        public Mp3Header header = new Mp3Header();
        public float[][] samples;
        public int[] lastBuf = new int[LAST_BUF_SIZE];
        public int lastBufSize;
        public float[][] synthBuf = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, 512 * 2);
        public int[] synthBufOffset = new int[MP3_MAX_CHANNELS];
        public float[][] sbSamples = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, 36 * SBLIMIT);
        public float[][] mdctBuf = RectangularArrays.ReturnRectangularFloatArray(MP3_MAX_CHANNELS, SBLIMIT * 18); // previous samples, for layer 3 MDCT
        public Granule[][] granules = ReturnRectangularGranuleArray(2, 2); // Used in Layer 3
        public int aduMode; ///<0 for standard mp3, 1 for adu formatted mp3
		public int[] ditherState = new int[1];
        public int errRecognition;
        public int outputChannels;

        public static Granule[][] ReturnRectangularGranuleArray(int size1, int size2)
        {
            Granule[][] newArray = new Granule[size1][];
            for (int array1 = 0; array1 < size1; array1++)
            {
                newArray[array1] = new Granule[size2];
            }

            return newArray;
        }

        public Context()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    granules[i][j] = new Granule();
                }
            }
        }
    }

}