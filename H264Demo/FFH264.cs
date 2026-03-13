using FFmpeg.AutoGen;
using LightCodec;
using System;
using System.IO;

namespace FF
{
    public sealed unsafe class H264Decoder
    {
        AVFrame* picture;
        AVCodecContext* context;
        AVPacket pkt;
        int frame;
        private bool Initialized = false;

        Stream stream;

        int dataPointer;
        bool hasMoreNAL;
        public const int PADDING_SIZE = 80;
        const int INBUF_SIZE = 65535;
        byte[] inbuf = new byte[INBUF_SIZE + PADDING_SIZE];
        bool First;

        public H264Decoder(Stream Stream)
        {
            this.stream = Stream;
        }

        ~H264Decoder()
        {
            if (Initialized)
            {
                fixed (AVPacket* p = &pkt)
                {
                    ffmpeg.av_packet_unref(p);
                }
                fixed (AVCodecContext** p = &context)
                {
                    ffmpeg.avcodec_free_context(p);
                }
            }
        }

        public void init(byte[] extraData)
        {
            if (Initialized) return;

            ffmpeg.RootPath = @"D:\Tools\ffmpeg-8.0.1-full_build-shared\bin";
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_FATAL);

            var codec = ffmpeg.avcodec_find_decoder_by_name("h264");
            if (codec == null) throw new Exception("Codec not found");

            context = ffmpeg.avcodec_alloc_context3(codec);
            if (context == null) throw new Exception("Could not allocate codec context");

            if (extraData != null)
            {
                context->extradata_size = extraData.Length;
                context->extradata = (byte*)ffmpeg.av_malloc((ulong)(extraData.Length + 4));
                fixed (byte* p = extraData)
                {
                    for (int i = 0; i < extraData.Length; i++)
                    {
                        context->extradata[i] = p[i];
                    }
                }
            }

            var ret = ffmpeg.avcodec_open2(context, codec, null);
            if (ret < 0) throw new Exception("Could not open codec");

            picture = ffmpeg.av_frame_alloc();
            if (picture == null) throw new Exception("Could not allocate frame");

            pkt = new AVPacket();

            pkt.data = null;
            pkt.size = 0;

            Initialized = true;
            hasMoreNAL = true;
        }

        public void freeframe()
        {
            fixed (AVFrame** p = &picture)
            {
                ffmpeg.av_frame_free(p);
            }
        }

        private int ReadByte()
        {
            var Value = stream.ReadByte();
            if (Value == -1) hasMoreNAL = false;
            return Value;
        }

        private void FirstPacket()
        {
            var cacheRead = stackalloc int[3];

            cacheRead[0] = ReadByte();
            cacheRead[1] = ReadByte();
            cacheRead[2] = ReadByte();

            while (!(cacheRead[0] == 0x00 && cacheRead[1] == 0x00 && cacheRead[2] == 0x01))
            {
                cacheRead[0] = cacheRead[1];
                cacheRead[1] = cacheRead[2];
                cacheRead[2] = ReadByte();
                if (cacheRead[2] == -1) throw new EndOfStreamException();
            }

            inbuf[0] = inbuf[1] = inbuf[2] = 0x00;
            inbuf[3] = 0x01;

            First = true;
        }

        private AVPacket _ReadPacket()
        {
            if (hasMoreNAL)
            {
                if (!First) FirstPacket();

                dataPointer = 4;

                var cacheRead = stackalloc int[3];

                cacheRead[0] = ReadByte();
                cacheRead[1] = ReadByte();
                cacheRead[2] = ReadByte();
                while (!(cacheRead[0] == 0x00 && cacheRead[1] == 0x00 && cacheRead[2] == 0x01) && hasMoreNAL)
                {
                    inbuf[dataPointer++] = (byte)cacheRead[0];
                    cacheRead[0] = cacheRead[1];
                    cacheRead[1] = cacheRead[2];
                    cacheRead[2] = ReadByte();
                }

                fixed (byte* p = inbuf)
                {
                    pkt.data = p;// + dataPointer - (pkt.size);
                    pkt.size = dataPointer;// - (pkt.size);
                }

                return pkt;
            }
            else
            {
                throw new EndOfStreamException();
            }
        }

        public AVFrame* DecodeFrame()
        {
            if (!Initialized) return null;

            while (hasMoreNAL)
            {
                _ReadPacket();

                while (pkt.size > 0)
                {
                    int ret;

                    fixed (AVPacket* p = &pkt)
                    {
                        ret = ffmpeg.avcodec_send_packet(context, p);
                    }
                    if (ret < 0)
                    {
                        //Console.WriteLine("FFmpeg: Error sending packet to decoder");
                        break;
                    }

                    ret = ffmpeg.avcodec_receive_frame(context, picture);
                    if (ret == 0)
                    {
                        frame++;
                        return picture;
                    }
                    else if (ret == ffmpeg.AVERROR(ffmpeg.AVERROR_EOF))
                    {
                        pkt.size = 0;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("FFmpeg: Error receiving frame from decoder");
                        break;
                    }
                }
            }
            throw new EndOfStreamException();
        }
    }
}
