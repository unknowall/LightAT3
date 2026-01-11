using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	/// <summary>
	/// Individual Channel Stream
	/// </summary>
	public class IndividualChannelStream
	{
		public int maxSfb; ///< number of scalefactor bands per group
		public int[] windowSequence = new int[2];
		public bool[] useKbWindow = new bool[2]; ///< If set, use Kaiser-Bessel window, otherwise use a sine window.
		public int numWindowGroups;
		public int[] groupLen = new int[8];
		public LongTermPrediction ltp = new LongTermPrediction();
		public int[] swbOffset; ///< table of offsets to the lowest spectral coefficient of a scalefactor band, sfb, for a particular window
		public int[] swbSizes; ///< table of scalefactor band sizes for a particular window
		public int numSwb; ///< number of scalefactor window bands
		public int numWindows;
		public int tnsMaxBands;
		public bool predictorPresent;
		public bool predictorInitialized;
		public int predictorResetGroup;
		public bool[] predictionUsed = new bool[41];

		public virtual void copy(IndividualChannelStream that)
		{
			maxSfb = that.maxSfb;
			Utils.copy(windowSequence, that.windowSequence);
			Utils.copy(useKbWindow, that.useKbWindow);
			numWindowGroups = that.numWindowGroups;
			Utils.copy(groupLen, that.groupLen);
			ltp.copy(that.ltp);
			swbOffset = that.swbOffset;
			swbSizes = that.swbSizes;
			numSwb = that.numSwb;
			numWindows = that.numWindows;
			tnsMaxBands = that.tnsMaxBands;
			predictorPresent = that.predictorPresent;
			predictorInitialized = that.predictorInitialized;
			predictorResetGroup = that.predictorResetGroup;
			Utils.copy(predictionUsed, that.predictionUsed);
		}
	}

}