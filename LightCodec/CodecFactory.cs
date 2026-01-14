using LightCodec.aac;
using LightCodec.atrac3;
using LightCodec.atrac3plus;
using LightCodec.mp3;

namespace LightCodec
{
    public enum AudioCodec
    {
        AT3 = 0,
        AT3plus = 1,
        AAC = 2,
        MP3 = 3,
		NULL = 4
    }

    public class NullCodec : ILightCodec
    {
        public int NumberOfSamples => 0;

        public int init(int bytesPerFrame, int channels, int outputChannels, int codingMode)
        {
            return -1;
        }

        public unsafe int decode(void* inputAddr, int inputLength, void* output, out int outputLength)
        {
            outputLength = 0;

            return 0;
        }
    }

    public class CodecFactory
    {
        public static ILightCodec Get(AudioCodec codecType)
        {
            ILightCodec codec = new NullCodec();

            switch (codecType)
            {
                case AudioCodec.AT3plus:
                    codec = new Atrac3plusDecoder();
                    break;
                case AudioCodec.AT3:
                    codec = new Atrac3Decoder();
                    break;
                case AudioCodec.MP3:
                    codec = new Mp3Decoder();
                    break;
                case AudioCodec.AAC:
                    codec = new AacDecoder();
                    break;
            }

            return codec;
        }

        //public static ILightVideo Get(VideoCodec codecType)
        //{
        //    ILightVideo codec = new NullVideoCodec();

        //    switch (codecType)
        //    {
        //        case VideoCodec.H264:
        //            codec = new H264Decoder();
        //            break;
        //    }

        //    return codec;
        //}
    }

}