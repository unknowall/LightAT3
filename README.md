## Lightweight ATRAC3/AT3plus/MP3 decoder for .NET

LightCodec is a lightweight ATRAC3/AT3plus/MP3 audio decoder for .NET, designed to work without external dependencies like FFmpeg. 

#### for PSP emulator
 - ScePSP - https://github.com/unknowall/ScePSP

#### Example:
```csharp
using Lightcodec;
......
ILightCodec Codec;
byte[] Data = new byte[Format.BytesPerFrame];
short[] AudioBuf = new short[8192];

Codec = CodecFactory.Get(AudioCodec.AT3plus);
Codec.init(Format.BytesPerFrame, Format.Channels, Format.Channels, 0);

while (!DataStream.Eof())
{
	DataStream.Read(Data, 0, Format.BytesPerFrame);
	len = 0;
	fixed (byte* Ptr = Data)
	{
		fixed (short* OutPtr = AudioBuf)
		{
			rs = Codec.decode(Ptr, Format.BytesPerFrame, OutPtr, out len);
		}
		......
	}
}
```
