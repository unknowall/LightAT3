//using AacDecoder = LightCodec.aac.AacDecoder;
using Atrac3Decoder = LightCodec.atrac3.Atrac3Decoder;
using Atrac3plusDecoder = LightCodec.atrac3plus.Atrac3plusDecoder;
using Mp3Decoder = LightCodec.mp3.Mp3Decoder;

namespace LightCodec
{
    public enum AudioCodec
    {
        AT3 = 0,
        AT3plus = 1,
        AAC = 2,
        MP3 = 3,
    }

    public class NullCodec : ICodec
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
        public static ICodec Get(AudioCodec codecType)
        {
            ICodec? codec = new NullCodec();

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
                    //codec = new AacDecoder();
                    break;
            }

            return codec;
        }
    }

}