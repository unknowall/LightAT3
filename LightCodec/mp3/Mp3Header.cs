namespace LightCodec.mp3
{
    public class Mp3Header
    {
        public int frameSize;
        public int errorProtection;
        public int layer;
        public int sampleRate;
        public int sampleRateIndex; // between 0 and 8
        public int rawSampleRateIndex; // between 0 and 2
        public int bitRate;
        public int bitrateIndex; // between 0 and 14
        public int nbChannels;
        public int mode;
        public int modeExt;
        public int lsf;
        public int mpeg25;
        public int version;
        public int maxSamples;

        public override string ToString()
        {
            return string.Format("Mp3Header[version {0:D}, layer{1:D}, {2:D} Hz, {3:D} kbits/s, {4}]", version, layer, sampleRate, bitRate, nbChannels == 2 ? "stereo" : "mono");
        }
    }

}