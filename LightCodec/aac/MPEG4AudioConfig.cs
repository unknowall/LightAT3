namespace LightCodec.aac
{
	public class MPEG4AudioConfig
	{
		public int objectType;
		public int samplingIndex;
		public int sampleRate;
		public int chanConfig;
		public int sbr; //< -1 implicit, 1 presence
		public int extObjectType;
		public int extSamplingIndex;
		public int extSampleRate;
		public int extChanConfig;
		public int ps; //< -1 implicit, 1 presence

		public virtual void copy(MPEG4AudioConfig that)
		{
			objectType = that.objectType;
			samplingIndex = that.samplingIndex;
			sampleRate = that.sampleRate;
			chanConfig = that.chanConfig;
			sbr = that.sbr;
			extObjectType = that.extObjectType;
			extSamplingIndex = that.extSamplingIndex;
			extSampleRate = that.extSampleRate;
			extChanConfig = that.extChanConfig;
		}
	}

}