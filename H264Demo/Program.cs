using ScePSPUtils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS0649

using SDL2;

using LightCodec;
using LightCodec.av;
using LightCodec.h264;
using System.Runtime.InteropServices;

class Program
{
    [STAThreadAttribute]
    private static void Main(string[] args)
    {
        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) != 0)
        {
            Console.Error.WriteLine("Couldn't initialize SDL");
            return;
        }

        string Fn;

        if (args.Length < 1)
            Fn = "./demo.pmf";
        else
            Fn = args[0];

        int ret = -1;

        Player player = new Player();

        Console.WriteLine();
        Console.WriteLine("\nLightCodec H264 DEMO.\n");
        Console.WriteLine();

        Task.Factory.StartNew(() =>
        {
            ret = player.Play(Fn);
        });

        while (ret == -1)
        {
            Thread.Sleep(100);
        }
    }

    public unsafe class Player
    {
        FileStream stream;

        PlayH264 play;

        public int Play(string Fn)
        {
            stream = new FileStream(Fn, FileMode.Open, FileAccess.Read);

            play = new PlayH264(stream);

            play.Play();

            stream.Close();

            Console.WriteLine("\n\nStream End.");

            Console.ReadKey();

            return 1;
        }
    }
}

unsafe class PlayH264
{
    public ConsumerBufferStream MainStream;

    public ConsumerBufferStream AudioStream;

    public ConsumerBufferStream VideoStream;

    public MpegReader Reader;

    public H264Decoder H264Codec;

    int FrameIndex;

    public bool HasData => true;

    private const bool DumpStreams = false;

    static IntPtr window;
    static IntPtr _renderer;
    private static IntPtr _texture = IntPtr.Zero;
    private static int _lastW = 0;
    private static int _lastH = 0;

    public PlayH264(Stream stream)
    {
        window = SDL.SDL_CreateWindow(
            "LightCodec H264",
            SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            480 * 2, 272 * 2,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
        );

        _renderer = SDL.SDL_CreateRenderer(window, -1, 0);

        FrameIndex = 0;

        MainStream = new ConsumerBufferStream();

        AudioStream = new ConsumerBufferStream();

        VideoStream = new ConsumerBufferStream();

        Reader = new MpegReader(MainStream);

        H264Codec = new H264Decoder(VideoStream);

        stream.CopyTo(MainStream);
    }

    public bool FeedData()
    {
        MainStream.TransactionBegin();

        try
        {
            if (Reader.HasMorePackets)
            {
                var Packet = Reader.ReadStreamHeader();
                var Info = Reader.ParseStream(Packet.Stream);

                //Console.WriteLine("\nSplit: {0}[{1:X4}] {2}", Packet.Type, (int)Packet.Type, Info);

                switch (Packet.Type)
                {
                    case MpegReader.ChunkType.ST_Video1:
                        Info.Stream.Slice().CopyToFast(VideoStream);
                        break;
                    case MpegReader.ChunkType.ST_Private1:
                        Info.Stream.Slice().CopyToFast(AudioStream);
                        break;
                    default:
                        Console.WriteLine("Unknown PacketType: {0}", Packet.Type);
                        break;
                }
            }

            MainStream.TransactionCommit();

            return true;
        }
        catch //(EndOfStreamException EndOfStreamException)
        {
            MainStream.TransactionRevert();

            return false;
        }
    }

    public void Play()
    {
        H264Codec.init(null);

        while (Reader.HasMorePackets)
        {
            while (VideoStream.Length <= 1024 * 1024) if (!FeedData()) break;

            if (VideoStream.Length == 0) return;

            try
            {
                AVFrame Frame = H264Codec.DecodeFrame();

                if (Frame != null)
                {
                    Console.Write($"\rDecodedFrame: {FrameIndex}");

                    DrawFrame(Frame);

                    //var Bitmap = FrameUtils.imageFromFrameWithoutEdges(*Frame, FrameWidth, 272);

                    //Bitmap.Save($"./decoded-{FrameIndex}.png");

                    FrameIndex++;
                }

                Thread.Sleep(30);

            }
            catch (EndOfStreamException)
            {
                Console.WriteLine("H264 Codec.DecodeFrame: EndOfStreamException");
            }
        }
    }

    static int[] toRGB;

    public static void DrawFrame(AVFrame frame)
    {
        int width = frame.imageWidthWOEdge;
        int height = frame.imageHeightWOEdge;

        if (width != _lastW || height != _lastH || _texture == IntPtr.Zero)
        {
            if (_texture != IntPtr.Zero) SDL.SDL_DestroyTexture(_texture);

            _texture = SDL.SDL_CreateTexture(
                _renderer,
                SDL.SDL_PIXELFORMAT_IYUV,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                width,
                height);

            _lastW = width;
            _lastH = height;

            toRGB = new int[width * height * 2];
        }

        SDL.SDL_Rect rect = new SDL.SDL_Rect() { x = 0, y = 0, w = _lastW, h = _lastH };

        //YUV2RGB(frame, toRGB);

        //IntPtr ptr1 = Marshal.UnsafeAddrOfPinnedArrayElement(toRGB, 0);

        //SDL.SDL_UpdateTexture(_texture, ref rect, ptr1, 2048);

        IntPtr ptr1 = Marshal.UnsafeAddrOfPinnedArrayElement(frame.data_base[0], 0);
        IntPtr ptr2 = Marshal.UnsafeAddrOfPinnedArrayElement(frame.data_base[1], 0);
        IntPtr ptr3 = Marshal.UnsafeAddrOfPinnedArrayElement(frame.data_base[2], 0);

        SDL.SDL_UpdateYUVTexture(
            _texture,
            ref rect,
        ptr1, frame.linesize[0],
        ptr2, frame.linesize[1],
        ptr3, frame.linesize[2]
        );

        SDL.SDL_RenderClear(_renderer);
        SDL.SDL_RenderCopy(_renderer, _texture, IntPtr.Zero, IntPtr.Zero);
        SDL.SDL_RenderPresent(_renderer);
    }

    public static void YUV2RGB(AVFrame f, int[] rgb)
    {
        var luma = f.data_base[0];
        var cb = f.data_base[1];
        var cr = f.data_base[2];
        int stride = f.linesize[0];
        int strideChroma = f.linesize[1];

        fixed (int* rgbPtr = rgb)
        {
            for (int y = 0; y < f.imageHeight; y++)
            {
                int lineOffLuma = y * stride;
                int lineOffChroma = (y >> 1) * strideChroma;

                for (int x = 0; x < f.imageWidth; x++)
                {
                    int c = luma[lineOffLuma + x] - 16;
                    int d = cb[lineOffChroma + (x >> 1)] - 128;
                    int e = cr[lineOffChroma + (x >> 1)] - 128;

                    var c298 = 298 * c;

                    byte red = (byte)Math.Clamp((c298 + 409 * e + 128) >> 8, 0, 255);
                    byte green = (byte)Math.Clamp((c298 - 100 * d - 208 * e + 128) >> 8, 0, 255);
                    byte blue = (byte)Math.Clamp((c298 + 516 * d + 128) >> 8, 0, 255);
                    byte alpha = 255;

                    rgbPtr[lineOffLuma + x] = (alpha << 24) | (red << 16) | (green << 8) | (blue << 0);
                }
            }
        }
    }

}