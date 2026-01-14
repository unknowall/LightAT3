using LightCodec.av;
using LightCodec.h264.decoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightCodec.h264
{
    public sealed unsafe class H264Decoder
    {
        AVFrame picture;
        decoder.H264Decoder Codec;
        MpegEncContext Context = null;
        AVPacket pkt = new AVPacket();
        int[] got_picture = new int[1];
        int frame, len;
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
            if (Initialized) Context.avclose();
            Context = null;
            picture = null;
        }

        public void init(byte[] extraData)
        {
            if (Initialized) return;

            pkt.init();

            Codec = new decoder.H264Decoder();

            Context = MpegEncContext.init();

            picture = AVFrame.init();

            if ((Codec.capabilities & decoder.H264Decoder.CODEC_CAP_TRUNCATED) != 0)
            {
                Context.flags |= MpegEncContext.CODEC_FLAG_TRUNCATED;
            }

            if (extraData != null)
            {
                Context.extradata_size = extraData.Length;
                // Add 4 additional values to avoid exceptions while parsing
                byte[] extraDataPlus4 = new byte[Context.extradata_size + 4];
                Array.Copy(extraData, 0, extraDataPlus4, 0, Context.extradata_size);
                Context.extradata = extraDataPlus4;
            }

            Context.open(Codec);

            Initialized = true;

            hasMoreNAL = true;
        }

        public void Reset()
        {
            frame = 0;
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
                
                // Find next NAL
                cacheRead[0] = ReadByte();
                cacheRead[1] = ReadByte();
                cacheRead[2] = ReadByte();
                while (!(cacheRead[0] == 0x00 && cacheRead[1] == 0x00 && cacheRead[2] == 0x01) && hasMoreNAL)
                {
                    inbuf[dataPointer++] = (byte)cacheRead[0];
                    cacheRead[0] = cacheRead[1];
                    cacheRead[1] = cacheRead[2];
                    cacheRead[2] = ReadByte();
                } // while

                pkt.size = dataPointer;
                pkt.data_base = inbuf;
                pkt.data_offset = 0;
                return pkt;
            }
            else
            {
                throw new EndOfStreamException();
            }
        }

        public AVFrame DecodeFrame()
        {
            if (!Initialized) return null;

            while (hasMoreNAL)
            {
                _ReadPacket();

                while (pkt.size > 0)
                {
                    len = Context.avdecode(picture, got_picture, pkt);
                    if (len < 0 && len != -9)
                    {
                        Console.WriteLine("LightCodec: Error decoding NALU, skipping.");
                        break;
                    }
                    if (len > 0)
                    {
                        pkt.size -= len;
                        pkt.data_offset += len;
                        frame++;
                        if (got_picture[0] != 0)
                        {
                            return Context.priv_data.displayPicture;
                        }
                    }

                    break;
                }
            }
            throw new EndOfStreamException();
        }
    }
}
