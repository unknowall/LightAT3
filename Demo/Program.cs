using LightAT3;
using SDL2;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;

#pragma warning disable CS8618
#pragma warning disable CS8625

class Program
{
    private static uint audiodeviceid;

    private static SDL_AudioCallback audioCallbackDelegate;

    private static ConcurrentQueue<short[]> Queue = new ConcurrentQueue<short[]>();

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
            samples = 1024,
            callback = audioCallbackDelegate,
            userdata = IntPtr.Zero

        };
        SDL_AudioSpec obtained = new SDL_AudioSpec();

        audiodeviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);

        if (audiodeviceid != 0)
            SDL_PauseAudioDevice(audiodeviceid, 0);

        if (args[1] == null)
            args[1] = "./Demo.at3";

        Player player = new Player();
        string File = args[1];
        FileStream stream = new FileStream(File, FileMode.Append, FileAccess.Read);

        Task.Factory.StartNew(() =>
        {
            player.Play(stream);
        });

        do
        {
            Thread.Sleep(100);

        } while (!stream.Eof());
    }

    private unsafe static void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        int requiredSamples = len / sizeof(short);
        var streamSpan = new Span<short>((void*)stream, requiredSamples);
        streamSpan.Fill(0);

        if (Queue.Count == 0)
        {
            return;
        }

        int filledSamples = 0;
        while (filledSamples < requiredSamples && Queue.TryDequeue(out var buffer))
        {
            if (buffer == null || buffer.Length == 0)
            {
                continue;
            }
            int copyCount = Math.Min(buffer.Length, requiredSamples - filledSamples);
            new Span<short>(buffer, 0, copyCount).CopyTo(streamSpan.Slice(filledSamples));
            filledSamples += copyCount;
        }
    }

    public unsafe class Player
    {
        public string ReadString(Stream stream, int length, int offset = 0)
        {
            byte[] buffer = new byte[length];
            stream.Read(buffer, offset, length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public int Play(Stream stream)//, Stream outStream)
        {
            var strt = ReadString(stream, 3);

            stream.Position = 0;

            ushort bztmp;

            if (strt == "ea3")
            {
                Console.WriteLine("ea3 header\n");

                stream.Position = 0x6;
                var tmp = stream.ReadBytes(4);
                var skipbytes = 0;
                for (var a0 = 0; a0 < 4; a0++)
                {
                    skipbytes <<= 7;
                    skipbytes += tmp[a0] & 0x7F;
                }
                stream.Skip(skipbytes);
            }

            if (strt == "RIF") //RIFF
            {
                Console.WriteLine("RIFF header\n");
                stream.Position = 0x10;
                var fmtSize = stream.ReadStruct<int>();
                var fmt = stream.ReadStruct<ushort>();
                if (fmt != 0xFFFE)
                {
                    Console.WriteLine("RIFF File fmt error\n");
                    return -1;
                }
                stream.Skip(0x28);
                bztmp = stream.ReadStruct<UshortBe>();
                stream.Skip(fmtSize - 0x2c);

                for (var a0 = 0; a0 < 0x100; a0++)
                {
                    if (ReadString(stream, 4, a0) == "atad") break;
                }

                var tmpr = stream.ReadStruct<int>();
            }
            else
            {
                Console.WriteLine("EA3 header");
                stream.Skip(0x22);

                Console.WriteLine("{0:X}", stream.Position);
                bztmp = stream.ReadStruct<UshortBe>();
                stream.Skip(0x3c);
            }

            var blocksz = bztmp & 0x3FF;
            var buf0 = new byte[0x3000];
            var chns = 0;

            fixed (byte* buf0Ptr = buf0)
            {
                var blockSize = blocksz * 8 + 8;

                Console.WriteLine("frame_block_size 0x{0:X}\n", blockSize);

                var d2 = new FrameDecoder();

                stream.Read(buf0, 0, blockSize);

                short[] pBuf;
                int rs;

                if ((rs = d2.DecodeFrame(buf0Ptr, blockSize, out chns, out pBuf)) != 0)
                {
                    Console.WriteLine("decode error {0}", rs);
                }
                Console.WriteLine("channels: {0}\n", chns);
                if (chns > 2) Console.WriteLine("warning: waveout doesn't support {0} chns\n", chns);

                while (!stream.Eof())
                {
                    stream.Read(buf0, 0, blockSize);

                    if ((rs = d2.DecodeFrame(buf0Ptr, blockSize, out chns, out pBuf)) != 0)
                        Console.WriteLine("decode error {0}", rs);

                    //outStream.WriteStructVector(pBuf, 0x800 * chns);

                    Queue.Enqueue(pBuf);

                    while (Queue.Count > 0) Thread.Sleep(1);

                    //mwo0.enqueue((Mai_I8*)p_buf, 0x800 * chns * 2);
                }

                return 0;
            }
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

}