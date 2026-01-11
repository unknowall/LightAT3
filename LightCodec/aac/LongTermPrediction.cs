using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	using static  LightCodec.aac.OutputConfiguration;

    /// <summary>
    /// Long Term Prediction
    /// </summary>
    public class LongTermPrediction
	{
		public bool present;
		public int lag;
		public float coef;
		public bool[] used = new bool[MAX_LTP_LONG_SFB];

		public virtual void copy(LongTermPrediction that)
		{
			present = that.present;
			lag = that.lag;
			coef = that.coef;
			Utils.copy(used, that.used);
		}
	}

}