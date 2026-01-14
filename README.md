## LightCodec - Lightweight ATRAC3/AT3plus/MP3/AAC decoder for .NET

LightCodec is a lightweight ATRAC3/AT3plus/MP3/AAC audio decoder for .NET, designed to work without external dependencies like FFmpeg.

The decoding output has only minimal LSB (Least Significant Bit) differences compared to FFmpeg (tested with 6Mb audio samples), with no perceptible audio distinction.

## Features
- Decode ATRAC3/AT3plus/MP3/AAC audio formats
- Decode H264 video format
- Pure C# implementation with no external dependencies
- No FFmpeg or native libraries required

## Installation
### NuGet
```bash
dotnet add package LightCodec
```

## Related Projects
- [ScePSP](https://github.com/unknowall/ScePSP): A PSP emulator project that uses this decoder

## Demo
- [Demo Project](https://github.com/unknowall/LightAT3/tree/master/Demo)

## Usage Example
```csharp
using Lightcodec;
ILightCodec codec;
byte[] data = new byte[Format.BytesPerFrame];
short[] audioBuf = new short[8192];
codec = CodecFactory.Get(AudioCodec.AT3plus);
codec.init(Format.BytesPerFrame, Format.Channels, Format.Channels, 0);
while (!DataStream.Eof())
{
    DataStream.Read(data, 0, Format.BytesPerFrame);
    int len = 0;
    fixed (byte* ptr = data)
    {
        fixed (short* outPtr = audioBuf)
        {
            int result = codec.decode(ptr, Format.BytesPerFrame, outPtr, out len);
        }
    }
}
```
## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## License
This project is licensed under the GPL-2.0 license.
