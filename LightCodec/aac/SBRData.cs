using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	public class SBRData
	{
		public const int SBR_SYNTHESIS_BUF_SIZE = (1280 - 128) * 2;

		// Main bitstream data variables
		internal int bsFrameClass;
		internal bool bsAddHarmonicFlag;
		internal int bsNumEnv;
		internal int[] bsFreqRes = new int[7];
		internal int bsNumNoise;
		internal int[] bsDfEnv = new int[5];
		internal int[] bsDfNoise = new int[2];

		internal int[][] bsInvfMode = RectangularArrays.ReturnRectangularIntArray(2, 5);
		internal int[] bsAddHarmonic = new int[48];
		internal bool bsAmpRes;

		// State variables
		internal float[] synthesisFilterbankSamples = new float[SBR_SYNTHESIS_BUF_SIZE];
		internal float[] analysisFilterbankSamples = new float[1312];
		internal int synthesisFilterbankSamplesOffset;
		///l_APrev and l_A
		internal int[] eA = new int[2];
		///Chirp factors
		internal float[] bwArray = new float[5];
		///QMF values of the original signal

		internal float[][][][] W = RectangularArrays.ReturnRectangularFloatArray(2, 32, 32, 2);
		///QMF output of the HF adjustor
		internal int Ypos;

		internal float[][][][] Y = RectangularArrays.ReturnRectangularFloatArray(2, 38, 64, 2);

		internal float[][] gTemp = RectangularArrays.ReturnRectangularFloatArray(42, 48);

		internal float[][] qTemp = RectangularArrays.ReturnRectangularFloatArray(42, 48);

		internal int[][] sIndexmapped = RectangularArrays.ReturnRectangularIntArray(8, 48);

		internal float[][] envFacs = RectangularArrays.ReturnRectangularFloatArray(6, 48);

		internal float[][] noiseFacs = RectangularArrays.ReturnRectangularFloatArray(3, 5);
		///Envelope time borders
		internal int[] tEnv = new int[8];
		///Envelope time border of the last envelope of the previous frame
		internal int tEnvNumEnvOld;
		///Noise time borders
		internal int[] tQ = new int[3];
		internal int fIndexnoise;
		internal int fIndexsine;

		public virtual void copy(SBRData that)
		{
			bsFrameClass = that.bsFrameClass;
			bsAddHarmonicFlag = that.bsAddHarmonicFlag;
			bsNumEnv = that.bsNumEnv;
			Utils.copy(bsFreqRes, that.bsFreqRes);
			bsNumNoise = that.bsNumNoise;
			Utils.copy(bsDfEnv, that.bsDfEnv);
			Utils.copy(bsDfNoise, that.bsDfNoise);
			Utils.copy(bsInvfMode, that.bsInvfMode);
			Utils.copy(bsAddHarmonic, that.bsAddHarmonic);
			bsAmpRes = that.bsAmpRes;

			// State variables
			Utils.copy(synthesisFilterbankSamples, that.synthesisFilterbankSamples);
			Utils.copy(analysisFilterbankSamples, that.analysisFilterbankSamples);
			synthesisFilterbankSamplesOffset = that.synthesisFilterbankSamplesOffset;
			Utils.copy(eA, that.eA);
			Utils.copy(bwArray, that.bwArray);
			Utils.copy(W, that.W);
			Utils.copy(Y, that.Y);
			Utils.copy(gTemp, that.gTemp);
			Utils.copy(qTemp, that.qTemp);
			Utils.copy(sIndexmapped, that.sIndexmapped);
			Utils.copy(envFacs, that.envFacs);
			Utils.copy(noiseFacs, that.noiseFacs);
			Utils.copy(tEnv, that.tEnv);
			tEnvNumEnvOld = that.tEnvNumEnvOld;
			Utils.copy(tQ, that.tQ);
			fIndexnoise = that.fIndexnoise;
			fIndexsine = that.fIndexsine;
		}
	}

}