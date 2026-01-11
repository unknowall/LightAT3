using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	public class PSContext
	{
		public const int PS_MAX_NUM_ENV = 5;
		public const int PS_MAX_NR_IIDICC = 34;
		public const int PS_MAX_NR_IPDOPD = 17;
		public const int PS_MAX_SSB = 91;
		public const int PS_MAX_AP_BANDS = 50;
		public const int PS_QMF_TIME_SLOTS = 32;
		public const int PS_MAX_DELAY = 14;
		public const int PS_AP_LINKS = 3;
		public const int PS_MAX_AP_DELAY = 5;

		public bool start;
		internal bool enableIid;
		internal int iidQuant;
		internal int nrIidPar;
		internal int nrIpdopdPar;
		internal bool enableIcc;
		internal int iccMode;
		internal int nrIccPar;
		internal bool enableExt;
		internal int frameClass;
		internal int numEnvOld;
		internal int numEnv;
		internal bool enableIpdopd;
		internal int[] borderPosition = new int[PS_MAX_NUM_ENV + 1];

		internal int[][] iidPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-channel Intensity Difference Parameters

		internal int[][] iccPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-Channel Coherence Parameters
		// ipd/opd is iid/icc sized so that the same functions can handle both
		internal int[][] ipdPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Inter-channel Phase Difference Parameters
		internal int[][] opdPar = RectangularArrays.ReturnRectangularIntArray(PS_MAX_NUM_ENV, PS_MAX_NR_IIDICC); ///< Overall Phase Difference Parameters
		internal bool is34bands;
		internal bool is34bandsOld;

		internal float[][][] inBuf = RectangularArrays.ReturnRectangularFloatArray(5, 44, 2);
		internal float[][][] delay = RectangularArrays.ReturnRectangularFloatArray(PS_MAX_SSB, PS_QMF_TIME_SLOTS + PS_MAX_DELAY, 2);
		internal float[][][][] apDelay = RectangularArrays.ReturnRectangularFloatArray(PS_MAX_AP_BANDS, PS_AP_LINKS, PS_QMF_TIME_SLOTS + PS_MAX_AP_DELAY, 2);
		internal float[] peakDecayNrg = new float[34];
		internal float[] powerSmooth = new float[34];
		internal float[] peakDecayDiffSmooth = new float[34];

		internal float[][][] H11 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);

		internal float[][][] H12 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);

		internal float[][][] H21 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);

		internal float[][][] H22 = RectangularArrays.ReturnRectangularFloatArray(2, PS_MAX_NUM_ENV + 1, PS_MAX_NR_IIDICC);
		internal int[] opdHist = new int[PS_MAX_NR_IIDICC];
		internal int[] ipdHist = new int[PS_MAX_NR_IIDICC];

        public virtual void copy(PSContext that)
		{
			start = that.start;
			enableIid = that.enableIid;
			iidQuant = that.iidQuant;
			nrIidPar = that.nrIidPar;
			nrIpdopdPar = that.nrIpdopdPar;
			enableIcc = that.enableIcc;
			iccMode = that.iccMode;
			nrIccPar = that.nrIccPar;
			enableExt = that.enableExt;
			frameClass = that.frameClass;
			numEnvOld = that.numEnvOld;
			numEnv = that.numEnv;
			enableIpdopd = that.enableIpdopd;
            Utils.copy(borderPosition, that.borderPosition);
			Utils.copy(iidPar, that.iidPar);
			Utils.copy(iccPar, that.iccPar);
			Utils.copy(ipdPar, that.ipdPar);
			Utils.copy(opdPar, that.opdPar);
			is34bands = that.is34bands;
			is34bandsOld = that.is34bandsOld;

			Utils.copy(inBuf, that.inBuf);
			Utils.copy(delay, that.delay);
            Utils.copy(apDelay, that.apDelay);
			Utils.copy(peakDecayNrg, that.peakDecayNrg);
			Utils.copy(powerSmooth, that.powerSmooth);
			Utils.copy(peakDecayDiffSmooth, that.peakDecayDiffSmooth);
			Utils.copy(H11, that.H11);
			Utils.copy(H12, that.H12);
			Utils.copy(H21, that.H21);
			Utils.copy(H22, that.H22);
			Utils.copy(opdHist, that.opdHist);
			Utils.copy(ipdHist, that.ipdHist);
		}
	}

}