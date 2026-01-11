using static LightCodec.mp3.Mp3Decoder;

namespace LightCodec.mp3
{
    public class Granule
    {
        public int scfsi;
        public int part23Length;
        public int bigValues;
        public int globalGain;
        public int scalefacCompress;
        public int blockType;
        public int switchPoint;
        public int[] tableSelect = new int[3];
        public int[] subblockGain = new int[3];
        public int scalefacScale;
        public int count1tableSelect;
        public int[] regionSize = new int[3]; // number of huffman codes in each region
        internal int preflag;
        public int shortStart, longEnd; // long/short band indexes
        public int[] scaleFactors = new int[40];
        public float[] sbHybrid = new float[SBLIMIT * 18]; // 576 samples
        public int granuleStartPosition;
    }

}