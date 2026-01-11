using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
    using static LightCodec.aac.OutputConfiguration;
    
	/// <summary>
    /// Single Channel Element - used for both SCE and LFE elements.
    /// </summary>
    public class SingleChannelElement
	{
		public IndividualChannelStream ics = new IndividualChannelStream();
		public TemporalNoiseShaping tns = new TemporalNoiseShaping();
		public Pulse pulse = new Pulse();
		public int[] bandType = new int[128]; ///< band types
		public int[] bandTypeRunEnd = new int[120]; ///< band type run end points
		public float[] sf = new float[120]; ///< scalefactors
		public float[] coeffs = new float[1024]; ///< coefficients for IMDCT
		public float[] saved = new float[1536]; ///< overlap
		public float[] retBuf = new float[2048]; ///< PCM output buffer
		public float[] ltpState = new float[3072]; ///< time signal for LTP
		public PredictorState[] predictorState = new PredictorState[MAX_PREDICTORS];
		public float[] ret;

		public SingleChannelElement()
		{
			for (int i = 0; i < predictorState.Length; i++)
			{
				predictorState[i] = new PredictorState();
			}
		}

		public virtual void copy(SingleChannelElement that)
		{
			ics.copy(that.ics);
			tns.copy(that.tns);
			pulse.copy(that.pulse);
			Utils.copy(bandType, that.bandType);
			Utils.copy(bandTypeRunEnd, that.bandTypeRunEnd);
			Utils.copy(sf, that.sf);
			Utils.copy(coeffs, that.coeffs);
			Utils.copy(saved, that.saved);
			Utils.copy(retBuf, that.retBuf);
			Utils.copy(ltpState, that.ltpState);
			for (int i = 0; i < predictorState.Length; i++)
			{
				predictorState[i].copy(that.predictorState[i]);
			}
			ret = that.ret;
		}
	}

}