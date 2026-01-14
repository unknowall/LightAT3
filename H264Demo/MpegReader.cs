using LightCodec;
using ScePSPUtils;
using System;
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable CS0675

/// <summary>
/// MPEG Program Stream
/// 
/// Glosary:
/// - AU : Access Unit
/// - TS : Transport Stream
/// - PS : Program Stream
/// - PSI: Program Specific Information
/// - PAT: Program Association Table
/// - PMT: Program Map Tables
/// - PES: Packetized Elementary Stream
/// - GOP: Group Of Pictures
/// - PTS: Presentation TimeStamp
/// - DTS: Decode TimeStamp
/// - PID: Packet IDentifier
/// - PCR: Program Clock Reference
/// </summary>
public unsafe class MpegReader
{
    public enum ChunkType : uint
    {
        ST_PSP_PMF = 0x0000015F,
        Start = 0x000001BA,
        SystemHeader = 0x000001BB,
        ST_PSMapTable = 0x000001BC,
        ST_Private1 = 0x000001BD,
        ST_Padding = 0x000001BE,
        ST_Private2 = 0x000001BF,
        ST_Audio1 = 0x000001C0,
        ST_Audio2 = 0x000001DF,
        ST_Video1 = 0x000001E0,
        ST_Video2 = 0x000001EF,
        ST_ECM = 0x000001F0,
        ST_EMM = 0x000001F1,
        ST_DSMCC = 0x000001F2,
        ST_ISO_13522 = 0x000001F3,
        ST_ITUT_A = 0x000001F4,
        ST_ITUT_B = 0x000001F5,
        ST_ITUT_C = 0x000001F6,
        ST_ITUT_D = 0x000001F7,
        ST_ITUT_E = 0x000001F8,
        ST_PSDirectory = 0x000001FF,
        Invalid = 0xFFFFFFFF,
    }

    protected Stream Stream;

    public MpegReader(Stream Stream)
    {
        this.Stream = Stream;
    }

    public class Packet
    {
        public ChunkType Type;
        public MemoryStream? Stream;
    }

    public bool HasMorePackets => !Stream.Eof();

    public Packet ReadStreamHeader()
    {
        while (!Stream.Eof())
        {
            var StartCode = (uint)GetNextPacketAndSync();
            var ChunkCodeType = (ChunkType)StartCode;

            //Console.Error.WriteLine("StreamHeader: {0}: {1:X2}", (ChunkType)StartCode, StartCode);

            switch (ChunkCodeType)
            {
                // PACK_START_CODE
                case ChunkType.Start:
                    Stream.Skip(10);
                    continue;
                // SYSTEM_HEADER_START_CODE
                case ChunkType.SystemHeader:
                    Stream.Skip(14);
                    continue;
                // PADDING_STREAM
                // PRIVATE_STREAM_2
                case ChunkType.ST_Private2:
                case ChunkType.ST_Padding:
                    Stream.Skip(Read16());
                    continue;
            }

            if (
                // Audio Stream
                StartCode >= 0x1C0 && StartCode <= 0x1CF ||
                // Video Stream
                StartCode >= 0x1E0 && StartCode <= 0x1EF ||
                // Private Stream (Atrac3+)
                ChunkCodeType == ChunkType.ST_Private1 ||
                // ???
                StartCode == 0x1fd
            )
            {
                //Console.WriteLine("Position: 0x{0:X}", Stream.Position);

                ushort PacketSize = Read16();
                var PacketStream = Stream.ReadStreamCopy(PacketSize);
                if (PacketStream.Length != PacketSize) throw new Exception("Didn't read the entire packet");
                return new Packet()
                {
                    Type = (ChunkType)StartCode,
                    Stream = PacketStream,
                };
            }
        }
        throw new EndOfStreamException();
    }

    public struct PacketStream
    {
        //public TimeStamp dts, pts;
        public MemoryStream Stream;
    }

    public PacketStream ParseStream(Stream PacketStream)
    {
        var c = (byte)PacketStream.ReadByte();

        var PacketizedStream = default(PacketStream);

        // mpeg 2 PES
        if ((c & 0xC0) == 0x80)
        {
            var flags = (byte)PacketStream.ReadByte();
            var header_len = (byte)PacketStream.ReadByte();
            var HeaderStream = PacketStream.ReadStreamCopy(header_len);
            if (HeaderStream.Length != header_len) throw new Exception("Didn't read the entire packet");

            //// Has PTS/DTS
            //if ((flags & 0x80) != 0)
            //{
            //    PacketizedStream.dts = PacketizedStream.pts = HeaderStream.ReadStruct<TimeStamp>();

            //    // Has DTS
            //    // On PSP, video DTS is always 1 frame behind PTS
            //    if ((flags & 0x40) != 0)
            //    {
            //        PacketizedStream.dts = HeaderStream.ReadStruct<TimeStamp>();
            //    }
            //}
        }

        PacketizedStream.Stream = PacketStream.ReadStreamCopy();

        return PacketizedStream;
    }

    public ushort Read16()
    {
        var Out = new byte[2];
        Stream.Read(Out, 0, 2);
        byte Hi = Out[0];
        byte Lo = Out[1];
        return (ushort)(((ushort)Hi << 8) | (ushort)Lo);
    }

    public ChunkType GetNextPacketAndSync()
    {
        uint Value = 0xFFFFFFFF;
        int Byte;
        while ((Byte = Stream.ReadByte()) != -1)
        {
            Value <<= 8;
            Value |= (byte)Byte;
            if ((Value & 0xFFFFFF00) == 0x00000100)
            {
                return (ChunkType)Value;
            }
        }
        return (ChunkType)0xFFFFFFFF;
    }
}

#pragma warning restore CS0675