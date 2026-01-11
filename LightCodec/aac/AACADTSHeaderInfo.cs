using System;
using LightCodec.Utils;
using static LightCodec.aac.OutputConfiguration;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	using IBitReader = LightCodec.Utils.IBitReader;

	public class AACADTSHeaderInfo
	{
		internal int sampleRate;
		internal int samples;
		internal int bitRate;
		internal bool crcAbsent;
		internal int objectType;
		internal int samplingIndex;
		internal int chanConfig;
		internal int numAacFrames;

		private const int AAC_ADTS_HEADER_SIZE = 7;

		private static readonly int[] mpeg4audioSampleRates = new int[] {96000, 88200, 64000, 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11025, 8000, 7350, 0, 0, 0};

		public virtual int parse(IBitReader br)
		{
			if (br.read(12) != 0xFFF)
			{
				return AAC_ERROR;
			}

			br.skip(1); // id
			br.skip(2); // layer
			bool crcAbs = br.readBool(); // protection_absent
			int aot = br.read(2); // profile_objecttype
			int sr = br.read(4); // sample_frequency_index
			if (mpeg4audioSampleRates[sr] == 0)
			{
				return AAC_ERROR;
			}
			br.skip(1); // private_bit
			int ch = br.read(3); // channel_configuration

			br.skip(1); // original/copy
			br.skip(1); // home

			// adts_variable_header
			br.skip(1); // copyright_identification_bit
			br.skip(1); // copyright_identification_start
			int size = br.read(13); // aac_frame_length
			if (size < AAC_ADTS_HEADER_SIZE)
			{
				return AAC_ERROR;
			}

			br.skip(11); // adts_buffer_fullness
			int rdb = br.read(2); // number_of_raw_data_blocks_in_frame

			objectType = aot + 1;
			chanConfig = ch;
			crcAbsent = crcAbs;
			numAacFrames = rdb + 1;
			samplingIndex = sr;
			sampleRate = mpeg4audioSampleRates[sr];
			samples = (rdb + 1) * 1024;
			bitRate = size * 8 * sampleRate / samples;

			return size;
		}
	}

}