## Lightweight ATRAC3/AT3plus/MP3/AAC decoder for .NET

LightCodec is a lightweight ATRAC3/AT3plus/MP3/AAC audio decoder for .NET, designed to work without external dependencies like FFmpeg. 

### Features
 - Decode ATRAC3 audio format
- Decode ATRAC3plus audio format
- Decode MP3 audio format
- Decode AAC audio format
- Pure C# implementation - no external dependencies
- No ffmpeg or other native libraries required

### NuGet
```bash
    dotnet add package LightCodec-1.0.0
```

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
