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

using LightCodec;

#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS0649

//#define DECODETOFILE

class Program
{
#if !DECODETOFILE
    private static uint audiodeviceid;
    private static SDL.SDL_AudioCallback audioCallbackDelegate;
    private static short[] CurrentPcm;
    private static int CurrentPlayPos = 0, CurrentPcmLength = 0;
    private static readonly object BufferLock = new object();
#endif

    [STAThreadAttribute]
    private static void Main(string[] args)
    {
#if !DECODETOFILE
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
            samples = 1024,
            callback = audioCallbackDelegate,
            userdata = IntPtr.Zero

        };
        SDL_AudioSpec obtained = new SDL_AudioSpec();

        audiodeviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);

        if (audiodeviceid != 0)
            SDL_PauseAudioDevice(audiodeviceid, 0);
#endif
        string Fn;

        if (args.Length < 1)
            Fn = "./Demo.at3+";
        else
            Fn = args[0];

        int ret = -1;

        Player player = new Player();

        Task.Factory.StartNew(() =>
        {
            ret = player.Play(Fn);
        });

        while (ret == -1)
        {
            Thread.Sleep(100);
        }
    }

#if !DECODETOFILE
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
#endif

    public unsafe class Player
    {
        FileStream stream;
        public FmtStruct Format;
        public FactStruct Fact;
        public SmplStruct Smpl;
        public LoopInfoStruct[] LoopInfoList;
        public SliceStream DataStream;
        public static short[] AudioBuf = new short[8192];

        static ILightCodec Codec;

        public int Play(string Fn)
        {
            stream = new FileStream(Fn, FileMode.Open, FileAccess.Read);

            var strt = stream.ReadString(4);

            if (strt != "RIFF")
            {
                Console.WriteLine("Not a RIFF File");

                return 0;
            }

            var RiffSize = new BinaryReader(stream).ReadUInt32();
            var RiffStream = stream.ReadStream(RiffSize);

            strt = RiffStream.ReadString(4);

            if (strt != "WAVE")
            {
                Console.WriteLine("Not a RIFF.WAVE File");
                return 0;
            }

            bool HasData = false;
            uint HeadSize = 0;
            while (!RiffStream.Eof() && !HasData)
            {
                var ChunkType = RiffStream.ReadString(4);
                HeadSize += 4;
                var ChunkSize = new BinaryReader(RiffStream).ReadUInt32();
                HeadSize += ChunkSize;
                var ChunkStream = RiffStream.ReadStream(ChunkSize);

                switch (ChunkType)
                {
                    case "fmt ":
                        Format = ChunkStream.ReadStructPartially<FmtStruct>();
                        Console.WriteLine($"Format: {Format.CompressionCode} Bitrate {Format.Bitrate} BlockSize {Format.BlockSize} BytesPerSecond {Format.AverageBytesPerSecond}");
                        continue;

                    case "fact":
                        Fact = ChunkStream.ReadStructPartially<FactStruct>();
                        continue;

                    case "smpl":
                        Smpl = ChunkStream.ReadStructPartially<SmplStruct>();
                        LoopInfoList = ChunkStream.ReadStructVector<LoopInfoStruct>(Smpl.LoopCount);

                        //Console.WriteLine($"AT3 smpl: {Smpl.LoopCount}");
                        foreach (var LoopInfo in LoopInfoList)
                            Console.WriteLine($"Loop: StartSample {LoopInfo.StartSample} EndSample {LoopInfo.EndSample} " +
                                $"PlayCount {LoopInfo.PlayCount} Type {LoopInfo.Type} Fraction {LoopInfo.Fraction}");

                        continue;

                    case "data":
                        DataStream = ChunkStream;
                        HeadSize -= ChunkSize;
                        HasData = true;
                        break;

                    default:
                        Console.WriteLine($"Can't handle chunk '{ChunkType}'");
                        if (RiffSize - ChunkStream.Available() < 0x100) continue;
                        return 0;
                }
            }

            //DataStream.CopyToFile("./Data.bin");

            DataStream.Position = 0;

            var BlockSize = Format.BlockSize;
            if (BlockSize <= 0)
            {
                Console.WriteLine("BlockSize <= 0");
                return 0;
            }

            if (DataStream.Available() < BlockSize)
            {
                Console.WriteLine("EndOfData {0} < {1}", DataStream.Available(), BlockSize);
                return 0;
            }

            switch (Format.CompressionCode)
            {
                case CompressionCode.Atrac3:
                    Codec = CodecFactory.Get(AudioCodec.AT3);
                    BlockSize = Format.BytesPerFrame;
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

            byte[] Data = new byte[BlockSize];
            byte[] _byteBuffer = new byte[AudioBuf.Length * 2];
            int rs, len = 0 , FrameIdx = 0;
#if DECODETOFILE
            MemoryStream WaveStream = new MemoryStream();
#endif
            Codec.init(BlockSize, Format.Channels, Format.Channels, 0);

            Console.WriteLine();

            while (!DataStream.Eof())
            {
                DataStream.Read(Data, 0, Data.Length);

                len = 0;
                FrameIdx++;
                fixed (byte* Ptr = Data)
                {
                    fixed (short* OutPtr = AudioBuf)
                    {
                        rs = Codec.decode(Ptr, BlockSize, OutPtr, out len);
                    }
                }
                if (rs > 0)
                {
                    Console.Write($"\rFrame {FrameIdx} decode Read {rs} Out {len}");
                }
                if (rs == 0)
                {
                    Console.WriteLine($"decode DONE.");
                }
                if (rs < 0)
                {
                    Console.WriteLine($"decode ERROR.");
                }
#if DECODETOFILE
                int byteLen = len * sizeof(short);
                if (byteLen > _byteBuffer.Length) _byteBuffer = new byte[byteLen];
                Buffer.BlockCopy(AudioBuf, 0, _byteBuffer, 0, byteLen);
                WaveStream.Write(_byteBuffer, 0, byteLen);
#else
                lock (BufferLock)
                {
                    CurrentPcm = AudioBuf;
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
#endif
            }
#if DECODETOFILE
            SaveWaveFile(WaveStream, "./out.wav");
#endif
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

    static void SaveWaveFile(Stream waveStream, string filePath)
    {
        BuildWavHeader((int)waveStream.Length / 2);

        waveStream.Position = 0;

        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        {
            fs.Write(wavHeader, 0, wavHeader.Length);

            byte[] buffer = new byte[4096];
            int readLen;

            while ((readLen = waveStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, readLen);
            }

            fs.Flush(true);
        }
    }

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
    public unsafe struct FmtStruct
    {
        /// <summary>
        /// 01 00 - For Uncompressed PCM (linear quntization)
        /// FE FF - For AT3+
        /// </summary>
        [FieldOffset(0x0000)] public CompressionCode CompressionCode;

        /// <summary>
        /// 02 00       - Stereo
        /// </summary>
        [FieldOffset(0x0002)] public ushort Channels;

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