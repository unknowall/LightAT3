using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
    /// <summary>
    /// Spectral Band Replication header - spectrum parameters that invoke a reset if they differ from the previous header.
    /// </summary>
#pragma warning disable CS0659
    public class SpectrumParameters
#pragma warning restore CS0659
    {
		public int bsStartFreq;
		public int bsStopFreq;
		public int bsXoverBand;

		public int bsFreqScale;
		public int bsAlterScale;
		public int bsNoiseBands;

		public virtual void copy(SpectrumParameters that)
		{
			bsStartFreq = that.bsStartFreq;
			bsStopFreq = that.bsStopFreq;
			bsXoverBand = that.bsXoverBand;
			bsFreqScale = that.bsFreqScale;
			bsAlterScale = that.bsAlterScale;
			bsNoiseBands = that.bsNoiseBands;
		}

		public virtual void reset()
		{
			bsStartFreq = -1;
			bsStopFreq = -1;
			bsXoverBand = -1;
			bsFreqScale = -1;
			bsAlterScale = -1;
			bsNoiseBands = -1;
		}

#pragma warning disable CS8765
        public override bool Equals(object obj)
#pragma warning restore CS8765
        {
			if (obj is SpectrumParameters)
			{
				SpectrumParameters that = (SpectrumParameters) obj;

				if (bsStartFreq != that.bsStartFreq)
				{
					return false;
				}
				if (bsStopFreq != that.bsStopFreq)
				{
					return false;
				}
				if (bsXoverBand != that.bsXoverBand)
				{
					return false;
				}
				if (bsFreqScale != that.bsFreqScale)
				{
					return false;
				}
				if (bsAlterScale != that.bsAlterScale)
				{
					return false;
				}
				if (bsNoiseBands != that.bsNoiseBands)
				{
					return false;
				}

				return true;
			}

			return base.Equals(obj);
		}
	}

}