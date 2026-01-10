using LightAT3;
using SDL2;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;

#pragma warning disable CS8618
#pragma warning disable CS8625

class Program
{
    private static uint audiodeviceid;
    private static SDL.SDL_AudioCallback audioCallbackDelegate;
    private static short[] CurrentPcm;
    private static int CurrentPlayPos = 0;
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
            samples = 16384,
            callback = audioCallbackDelegate,
            userdata = IntPtr.Zero

        };
        SDL_AudioSpec obtained = new SDL_AudioSpec();

        audiodeviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);

        if (audiodeviceid != 0)
            SDL_PauseAudioDevice(audiodeviceid, 0);

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

            int copyCount = Math.Min(CurrentPcm.Length - CurrentPlayPos, requiredSamples);

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
                        continue;

                    case "fact":
                        Fact = ChunkStream.ReadStructPartially<FactStruct>();
                        continue;

                    case "smpl":
                        Smpl = ChunkStream.ReadStructPartially<SmplStruct>();
                        LoopInfoList = ChunkStream.ReadStructVector<LoopInfoStruct>(Smpl.LoopCount);

                        Console.WriteLine($"AT3 smpl: {Smpl.LoopCount} BlockSize 0x{Format.BlockSize:X}");
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
            var FrameDecoder = new FrameDecoder();
            short[] pBuf;
            int rs, chns;

            while (!DataStream.Eof())
            {
                this.DataStream.Read(Data, 0, Data.Length);

                fixed (byte* Ptr = Data)
                {
                    if ((rs = FrameDecoder.DecodeFrame(Ptr, BlockSize, out chns, out pBuf)) != 0)
                    {
                        Console.WriteLine("decode error {0}", rs);
                    }
                }

                Console.WriteLine($"Channels: {chns}, Decoded Size {pBuf.Length}");

                if (chns > 2) Console.WriteLine("warning: waveout doesn't support {0} chns\n", chns);

                lock (BufferLock)
                {
                    CurrentPcm = pBuf;
                    CurrentPlayPos = 0;
                }

                while (true)
                {
                    bool finished;

                    lock (BufferLock)
                    {
                        finished = CurrentPlayPos >= CurrentPcm.Length;
                    }

                    if (finished) break;

                    Thread.Sleep(1);
                }
            }

            return 1;
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