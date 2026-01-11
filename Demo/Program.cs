using SDL2;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static SDL2.SDL;

//using FFmpeg.AutoGen;

using LightCodec;
using LightCodec.atrac3plus;

#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS0649

class Program
{
    private static uint audiodeviceid;
    private static SDL.SDL_AudioCallback audioCallbackDelegate;
    private static short[] CurrentPcm;
    private static int CurrentPlayPos = 0, CurrentPcmLength = 0;
    private static readonly object BufferLock = new object();

    [STAThreadAttribute]
    private static void Main(string[] args)
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO) != 0)
        {
            Console.Error.WriteLine("Couldn't initialize SDL");
            return;
        }

        audioCallbackDelegate = AudioCallback;

        SDL_AudioSpec desired = new SDL_AudioSpec
        {
            channels = 2,
            format = AUDIO_S16,
            freq = 44100,
            samples = 2048,
            callback = audioCallbackDelegate,
            userdata = IntPtr.Zero

        };
        SDL_AudioSpec obtained = new SDL_AudioSpec();

        audiodeviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);

        if (audiodeviceid != 0)
            SDL_PauseAudioDevice(audiodeviceid, 0);

        //ffdecode();

        string Fn;

        if (args.Length < 1)
            Fn = "./Demo.at3";
        else
            Fn = args[0];

        Player player = new Player();
        FileStream stream = new FileStream(Fn, FileMode.Open, FileAccess.Read);
        int ret = -1;

        Task.Factory.StartNew(() =>
        {
            ret = player.Play(stream);
        });

        while (ret == -1)
        {
            Thread.Sleep(100);
        }
    }

    private unsafe static void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        int requiredSamples = len / sizeof(short);
        var streamSpan = new Span<short>((void*)stream, requiredSamples);
        streamSpan.Fill(0);

        lock (BufferLock)
        {
            if (CurrentPcm == null) return;

            int copyCount = Math.Min(CurrentPcmLength - CurrentPlayPos, requiredSamples);

            if (copyCount > 0)
            {
                new Span<short>(CurrentPcm, CurrentPlayPos, copyCount).CopyTo(streamSpan);
                CurrentPlayPos += copyCount;
            }
        }
    }

    public unsafe class Player
    {
        public At3FormatStruct Format;
        public FactStruct Fact;
        public SmplStruct Smpl;
        public LoopInfoStruct[] LoopInfoList;
        public SliceStream DataStream;

        //public FrameDecoder Decoder = new FrameDecoder();

        static ICodec Codec;
        static MemoryStream WaveStream;

        public static short[] pBuf;

        public string ReadString(Stream stream, int length, int offset = 0)
        {
            var buffer = new byte[length];
            stream.Seek(offset, SeekOrigin.Current);
            stream.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public int Play(Stream stream)
        {
            var strt = ReadString(stream, 4);

            if (strt != "RIFF")
            {
                Console.WriteLine("Not a RIFF File");

                return 0;
            }

            var RiffSize = new BinaryReader(stream).ReadUInt32();
            var RiffStream = stream.ReadStream(RiffSize);

            strt = ReadString(RiffStream, 4);

            if (strt != "WAVE")
            {
                Console.WriteLine("Not a RIFF.WAVE File");
                return 0;
            }

            while (!RiffStream.Eof())
            {
                var ChunkType = ReadString(RiffStream, 4);
                var ChunkSize = new BinaryReader(RiffStream).ReadUInt32();
                var ChunkStream = RiffStream.ReadStream(ChunkSize);

                switch (ChunkType)
                {
                    case "fmt ":
                        Format = ChunkStream.ReadStructPartially<At3FormatStruct>();
                        Console.WriteLine($"Format: {Format.CompressionCode} Bitrate {Format.Bitrate} BlockSize {Format.BlockSize} BytesPerSecond {Format.AverageBytesPerSecond}");
                        continue;

                    case "fact":
                        Fact = ChunkStream.ReadStructPartially<FactStruct>();
                        continue;

                    case "smpl":
                        Smpl = ChunkStream.ReadStructPartially<SmplStruct>();
                        LoopInfoList = ChunkStream.ReadStructVector<LoopInfoStruct>(Smpl.LoopCount);

                        Console.WriteLine($"AT3 smpl: {Smpl.LoopCount}");
                        foreach (var LoopInfo in LoopInfoList)
                            Console.WriteLine($"Loop: StartSample {LoopInfo.StartSample} EndSample {LoopInfo.EndSample} " +
                                $"PlayCount {LoopInfo.PlayCount} Type {LoopInfo.Type} Fraction {LoopInfo.Fraction}");

                        continue;

                    case "data":
                        this.DataStream = ChunkStream;
                        break;

                    default:
                        Console.WriteLine($"Can't handle chunk '{ChunkType}'");
                        return 0;
                }
            }

            //SaveWaveStreamToFile(DataStream, "./Data.bin", false);

            DataStream.Position = 0;

            switch (Format.CompressionCode)
            {
                case CompressionCode.Atrac3:
                    Codec = CodecFactory.Get(AudioCodec.AT3);
                    break;
                case CompressionCode.Atrac3Plus:
                    Codec = CodecFactory.Get(AudioCodec.AT3plus);
                    break;
                case CompressionCode.Mpeg:
                    Codec = CodecFactory.Get(AudioCodec.MP3);
                    break;
                default:
                    Console.WriteLine($"no sopport codec: {Format.CompressionCode}");
                    return 0;
            }

            var BlockSize = this.Format.BlockSize;

            if (BlockSize <= 0)
            {
                Console.WriteLine("BlockSize <= 0");
                return 0;
            }

            if (this.DataStream.Available() < BlockSize)
            {
                Console.WriteLine("EndOfData {0} < {1}", this.DataStream.Available(), BlockSize);
                return 0;
            }

            var Data = new byte[BlockSize];
            short[] pBuf = new short[8192];
            byte[] _byteBuffer = new byte[pBuf.Length * 2];
            int rs, len = 0;

            WaveStream = new MemoryStream();

            Codec.init(BlockSize, Format.AtracChannels, Format.AtracChannels, 0);

            while (!DataStream.Eof())
            {
                this.DataStream.Read(Data, 0, Data.Length);

                fixed (byte* Ptr = Data)
                {
                    fixed (short* OutPtr = pBuf)
                    {
                        len = 0;
                        rs = Codec.decode(Ptr, BlockSize, OutPtr, out len);
                        //rs = Decoder.Decode(Ptr, BlockSize, out int chs, out short[] pbuf);
                        //len = (int)pbuf.Length;
                        if (rs > 0)
                        {
                            Console.WriteLine($"decode Read {rs} Out {len}");
                        }
                        if (rs == 0)
                        {
                            Console.WriteLine($"decode DONE.");
                        }
                        if (rs < 0)
                        {
                            Console.WriteLine($"decode ERROR.");
                        }
                    }
                }

                //int byteLen = len * sizeof(short);
                //if (byteLen > _byteBuffer.Length) _byteBuffer = new byte[byteLen];
                //Buffer.BlockCopy(pBuf, 0, _byteBuffer, 0, byteLen);
                //WaveStream.Write(_byteBuffer, 0, byteLen);

                lock (BufferLock)
                {
                    CurrentPcm = pBuf;
                    CurrentPcmLength = len;
                    CurrentPlayPos = 0;
                }

                while (true)
                {
                    bool finished;

                    lock (BufferLock)
                    {
                        finished = CurrentPlayPos >= CurrentPcmLength;
                    }

                    if (finished) break;

                    Thread.Sleep(1);
                }
            }

            //SaveWaveStreamToFile(WaveStream, "./out.wav");

            return 1;
        }
    }

    static byte[] wavHeader = new byte[44];

    static void BuildWavHeader(int totalSampleCount)
    {
        int sampleRate = 44100;
        int channels = 2;
        short bitsPerSample = 16;

        int totalBytes = totalSampleCount * channels * bitsPerSample / 8;
        int dataChunkSize = totalBytes;
        int riffChunkSize = 36 + dataChunkSize;

        // RIFF Chunk
        wavHeader[0] = 0x52; wavHeader[1] = 0x49; wavHeader[2] = 0x46; wavHeader[3] = 0x46; // "RIFF"
        BitConverter.GetBytes(riffChunkSize).CopyTo(wavHeader, 4);
        wavHeader[8] = 0x57; wavHeader[9] = 0x41; wavHeader[10] = 0x56; wavHeader[11] = 0x45; // "WAVE"

        // fmt Chunk
        wavHeader[12] = 0x66; wavHeader[13] = 0x6D; wavHeader[14] = 0x74; wavHeader[15] = 0x20; // "fmt "
        BitConverter.GetBytes(16).CopyTo(wavHeader, 16); // fmt
        BitConverter.GetBytes((short)1).CopyTo(wavHeader, 20); // 1=PCM
        BitConverter.GetBytes((short)channels).CopyTo(wavHeader, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(wavHeader, 24);
        BitConverter.GetBytes(sampleRate * channels * bitsPerSample / 8).CopyTo(wavHeader, 28);
        BitConverter.GetBytes((short)(channels * bitsPerSample / 8)).CopyTo(wavHeader, 32);
        BitConverter.GetBytes(bitsPerSample).CopyTo(wavHeader, 34);

        // data Chunk
        wavHeader[36] = 0x64; wavHeader[37] = 0x61; wavHeader[38] = 0x74; wavHeader[39] = 0x61; // "data"
        BitConverter.GetBytes(dataChunkSize).CopyTo(wavHeader, 40);
    }

    static void SaveWaveStreamToFile(Stream waveStream, string filePath, bool Head = true)
    {
        BuildWavHeader((int)waveStream.Length / 2);

        waveStream.Position = 0;

        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        {
            if (Head) fs.Write(wavHeader, 0, wavHeader.Length);

            byte[] buffer = new byte[4096];
            int readLen;

            while ((readLen = waveStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, readLen);
            }

            fs.Flush(true);
        }
    }

    //static unsafe void ffdecode()
    //{
    //    ffmpeg.RootPath = @"D:\Tools\ffmpeg-8.0.1-full_build-shared\bin";
    //    ffmpeg.av_log_set_level(ffmpeg.AV_LOG_ERROR);

    //    string inputFile = "./demo.at3";
    //    AVFormatContext* formatCtx = null;
    //    if (ffmpeg.avformat_open_input(&formatCtx, inputFile, null, null) < 0)
    //    {
    //        Console.WriteLine("无法打开文件");
    //        return;
    //    }

    //    AVCodec* codec;
    //    int streamIndex = ffmpeg.av_find_best_stream(formatCtx, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &codec, 0);
    //    if (streamIndex < 0) { Console.WriteLine("无法找到流"); return; }

    //    AVStream* stream = formatCtx->streams[streamIndex];
    //    AVCodecContext* codecCtx = ffmpeg.avcodec_alloc_context3(codec);

    //    if (ffmpeg.avcodec_parameters_to_context(codecCtx, stream->codecpar) < 0) { Console.WriteLine("无法复制参数"); return; }
    //    if (ffmpeg.avcodec_open2(codecCtx, codec, null) < 0) { Console.WriteLine("无法打开解码器"); return; }

    //    SwrContext* swrCtx = ffmpeg.swr_alloc();
    //    AVChannelLayout src_ch_layout = codecCtx->ch_layout;
    //    AVChannelLayout dst_ch_layout = new AVChannelLayout();
    //    ffmpeg.av_channel_layout_default(&dst_ch_layout, codecCtx->ch_layout.nb_channels);

    //    ffmpeg.swr_alloc_set_opts2(&swrCtx,
    //        &dst_ch_layout, AVSampleFormat.AV_SAMPLE_FMT_S16, codecCtx->sample_rate,
    //        &src_ch_layout, codecCtx->sample_fmt, codecCtx->sample_rate,
    //        0, null);
    //    ffmpeg.swr_init(swrCtx);

    //    AVPacket pkt = new AVPacket();
    //    AVFrame* frame = ffmpeg.av_frame_alloc();

    //    byte* dstBuffer = null;
    //    int dstLinesize = 0;
    //    int currentDstBufferSize = 0;

    //    Console.WriteLine("开始解码...");

    //    while (ffmpeg.av_read_frame(formatCtx, &pkt) >= 0)
    //    {
    //        if (pkt.stream_index != streamIndex) continue;

    //        ffmpeg.avcodec_send_packet(codecCtx, &pkt);

    //        while (ffmpeg.avcodec_receive_frame(codecCtx, frame) >= 0)
    //        {
    //            int max_dst_nb_samples = (int)ffmpeg.av_rescale_rnd(frame->nb_samples, dst_ch_layout.nb_channels, src_ch_layout.nb_channels, AVRounding.AV_ROUND_UP);

    //            int requiredSize = ffmpeg.av_samples_get_buffer_size(&dstLinesize, dst_ch_layout.nb_channels, max_dst_nb_samples, AVSampleFormat.AV_SAMPLE_FMT_S16, 1);

    //            if (requiredSize > currentDstBufferSize)
    //            {
    //                if (dstBuffer != null) ffmpeg.av_free((void*)dstBuffer);

    //                dstBuffer = (byte*)ffmpeg.av_malloc((ulong)requiredSize);
    //                currentDstBufferSize = requiredSize;
    //                Console.WriteLine($"重新分配缓冲区: {requiredSize} 字节");
    //            }

    //            for (uint ch = 0; ch < 2; ch++)
    //            {
    //                Console.Write($"Ch {ch}: ");

    //                float* channelSamples = (float*)frame->data[ch];

    //                for (int i = 0; i < 10; i++)
    //                {
    //                    if (i < frame->nb_samples)
    //                    {
    //                        float val = channelSamples[i];
    //                        Console.Write($"{val:F8}  ");
    //                    }
    //                }
    //                Console.WriteLine();
    //            }

    //            int out_samples = ffmpeg.swr_convert(swrCtx, &dstBuffer, max_dst_nb_samples, (byte**)&frame->data, frame->nb_samples);

    //            if (out_samples <= 0) continue;

    //            int outDataSize = out_samples * dst_ch_layout.nb_channels * sizeof(short);

    //            short[] correctPcm = new short[out_samples * dst_ch_layout.nb_channels];
    //            fixed (short* p = correctPcm)
    //            {
    //                Buffer.MemoryCopy(dstBuffer, p, (uint)outDataSize, (uint)outDataSize);
    //            }

    //            lock (BufferLock)
    //            {
    //                CurrentPcm = correctPcm;
    //                CurrentPcmLength = correctPcm.Length;
    //                CurrentPlayPos = 0;
    //            }

    //            while (true)
    //            {
    //                bool finished;
    //                lock (BufferLock) { finished = CurrentPlayPos >= CurrentPcmLength; }
    //                if (finished) break;
    //                Thread.Sleep(1);
    //            }
    //        }
    //        ffmpeg.av_packet_unref(&pkt);
    //    }

    //    if (dstBuffer != null) ffmpeg.av_free((void*)dstBuffer);
    //    ffmpeg.swr_free(&swrCtx);
    //    ffmpeg.av_frame_free(&frame);
    //    ffmpeg.avcodec_free_context(&codecCtx);
    //    ffmpeg.avformat_close_input(&formatCtx);
    //}

    public struct UshortLe
    {
        public ushort NativeValue { set; get; }

        public static implicit operator uint(UshortLe that)
        {
            return that.NativeValue;
        }

        public static implicit operator UshortLe(ushort that)
        {
            return new UshortLe()
            {
                NativeValue = that,
            };
        }

        public static implicit operator UshortLe(UshortBe that)
        {
            return (ushort)that;
        }

        public override string ToString()
        {
            return NativeValue.ToString();
        }
    }

    public struct UshortBe
    {
        public static ushort ByteSwap(ushort value)
        {
            return (ushort)((value >> 8) | (value << 8));
        }

        private ushort _internalValue;

        public ushort NativeValue
        {
            set => _internalValue = ByteSwap(value);
            get => ByteSwap(_internalValue);
        }

        public static implicit operator ushort(UshortBe that) => that.NativeValue;

        public static implicit operator UshortBe(ushort that) => new UshortBe
        {
            NativeValue = that,
        };

        public static implicit operator UshortBe(UshortLe that) => (ushort)that;

        public override string ToString() => NativeValue.ToString();

        public byte Low => (byte)(NativeValue >> 0);

        public byte High => (byte)(NativeValue >> 8);
    }

    public enum CompressionCode : ushort
    {
        Unknown = 0x0000,
        PcmUncompressed = 0x0001,
        MicrosoftAdpcm = 0x0002,
        ItuG711ALaw = 0x0006,
        ItuG711AmLaw = 0x0007,
        ImaAdpcm = 0x0011,
        ItuG723AdpcmYamaha = 0x0016,
        Gsm610 = 0x0031,
        ItuG721Adpcm = 0x0040,
        Mpeg = 0x0050,
        Atrac3 = 0x0270,
        Atrac3Plus = 0xFFFE,
        Experimental = 0xFFFF,
    }

    public struct WavFormatStruct
    {
        public CompressionCode CompressionCode;

        public ushort AtracChannels;

        public int Bitrate;

        public uint BytesPerSecond;

        public ushort BlockAlignment;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct At3FormatStruct
    {
        /// <summary>
        /// 01 00 - For Uncompressed PCM (linear quntization)
        /// FE FF - For AT3+
        /// </summary>
        [FieldOffset(0x0000)] public CompressionCode CompressionCode;

        /// <summary>
        /// 02 00       - Stereo
        /// </summary>
        [FieldOffset(0x0002)] public ushort AtracChannels;

        /// <summary>
        /// 44 AC 00 00 - 44100
        /// </summary>
        [FieldOffset(0x0004)] public uint Bitrate;

        /// <summary>
        /// Should be on uncompressed PCM : sampleRate * short.sizeof * numberOfChannels
        /// </summary>
        [FieldOffset(0x0008)] public uint AverageBytesPerSecond;

        /// <summary>
        /// short.sizeof * numberOfChannels
        /// </summary>
        [FieldOffset(0x000A)] public ushort BlockAlignment;

        [FieldOffset(0x000C)] public ushort BytesPerFrame;

        [FieldOffset(0x0010)] private fixed uint Unknown[6];

        [FieldOffset(0x0028)] public uint OmaInfo;

        [FieldOffset(0x0028)] private UshortBe _Unk2;

        [FieldOffset(0x002A)] private UshortBe _BlockSize;

        public int BlockSize => (_BlockSize & 0x3FF) * 8 + 8;
    }

    public struct FactStruct
    {
        public int EndSample;

        public int SampleOffset;
    }

    /// <summary>
    /// Loop Info
    /// </summary>
    public unsafe struct SmplStruct
    {
        /// <summary>
        /// 0000 -
        /// </summary>
        private fixed uint Unknown[7];

        /// <summary>
        /// 001C -
        /// </summary>
        public uint LoopCount;

        /// <summary>
        /// 0020 - 
        /// </summary>
        private fixed uint Unknown2[1];
    }

    public struct LoopInfoStruct
    {
        /// <summary>
        /// 0000 -
        /// </summary>
        public uint CuePointID;

        /// <summary>
        /// 0004 -
        /// </summary>
        public uint Type;

        /// <summary>
        /// 0008 -
        /// </summary>
        public int StartSample;

        /// <summary>
        /// 000C -
        /// </summary>
        public int EndSample;

        /// <summary>
        /// 0010 -
        /// </summary>
        public uint Fraction;

        /// <summary>
        /// 0014 -
        /// </summary>
        public int PlayCount;
    }

}