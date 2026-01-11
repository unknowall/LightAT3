using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
    using FFT = LightCodec.Utils.FFT;

    /// <summary>
    /// Spectral Band Replication
    /// </summary>
    public class SpectralBandReplication
	{
		internal int sampleRate;
		internal bool start;
		internal bool reset;
		internal SpectrumParameters spectrumParams = new SpectrumParameters();
		internal bool bsAmpResHeader;
		internal int bsLimiterBands;
		internal int bsLimiterGains;
		internal bool bsInterpolFreq;
		internal bool bsSmoothingMode;
		internal bool bsCoupling;
		internal int[] k = new int[5]; ///< k0, k1, k2
		///kx', and kx respectively, kx is the first QMF subband where SBR is used.
		///kx' is its value from the previous frame
		internal int[] kx = new int[2];
		///M' and M respectively, M is the number of QMF subbands that use SBR.
		internal int[] m = new int[2];
		internal bool kxAndMPushed;
		///The number of frequency bands in f_master
		internal int nMaster;
		internal SBRData[] data = new SBRData[2];
		internal PSContext ps = new PSContext();
		///N_Low and N_High respectively, the number of frequency bands for low and high resolution
		internal int[] n = new int[2];
		///Number of noise floor bands
		internal int nQ;
		///Number of limiter bands
		internal int nLim;
		///The master QMF frequency grouping
		internal int[] fMaster = new int[49];
		///Frequency borders for low resolution SBR
		internal int[] fTablelow = new int[25];
		///Frequency borders for high resolution SBR
		internal int[] fTablehigh = new int[49];
		///Frequency borders for noise floors
		internal int[] fTablenoise = new int[6];
		///Frequency borders for the limiter
		internal int[] fTablelim = new int[30];
		internal int numPatches;
		internal int[] patchNumSubbands = new int[6];
		internal int[] patchStartSubband = new int[6];
		///QMF low frequency input to the HF generator
		internal float[][][] Xlow = RectangularArrays.ReturnRectangularFloatArray(32, 40, 2);
		///QMF output of the HF generator
		internal float[][][] Xhigh = RectangularArrays.ReturnRectangularFloatArray(64, 40, 2);
		///QMF values of the reconstructed signal
		internal float[][][][] X = RectangularArrays.ReturnRectangularFloatArray(2, 2, 38, 64);
		///Zeroth coefficient used to filter the subband signals
		internal float[][] alpha0 = RectangularArrays.ReturnRectangularFloatArray(64, 2);
		///First coefficient used to filter the subband signals
		internal float[][] alpha1 = RectangularArrays.ReturnRectangularFloatArray(64, 2);
		///Dequantized envelope scalefactors, remapped
		internal float[][] eOrigmapped = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Dequantized noise scalefactors, remapped
		internal float[][] qMapped = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Sinusoidal presence, remapped
		internal int[][] sMapped = RectangularArrays.ReturnRectangularIntArray(7, 48);
		///Estimated envelope
		internal float[][] eCurr = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Amplitude adjusted noise scalefactors
		internal float[][] qM = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		///Sinusoidal levels
		internal float[][] sM = RectangularArrays.ReturnRectangularFloatArray(7, 48);

		internal float[][] gain = RectangularArrays.ReturnRectangularFloatArray(7, 48);
		internal float[] qmfFilterScratch = new float[5 * 64]; // originally: float[5][64]
		internal FFT mdctAna = new FFT();
		internal FFT mdct = new FFT();

		public SpectralBandReplication()
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = new SBRData();
			}
		}

		public virtual void copy(SpectralBandReplication that)
		{
			sampleRate = that.sampleRate;
			start = that.start;
			reset = that.reset;
			spectrumParams.copy(that.spectrumParams);
			bsAmpResHeader = that.bsAmpResHeader;
			bsLimiterBands = that.bsLimiterBands;
			bsLimiterGains = that.bsLimiterGains;
			bsInterpolFreq = that.bsInterpolFreq;
			bsSmoothingMode = that.bsSmoothingMode;
			bsCoupling = that.bsCoupling;
			Utils.copy(k, that.k);
			Utils.copy(kx, that.kx);
			Utils.copy(m, that.m);
			kxAndMPushed = that.kxAndMPushed;
			nMaster = that.nMaster;
			for (int i = 0; i < data.Length; i++)
			{
				data[i].copy(that.data[i]);
			}
			ps.copy(that.ps);
			Utils.copy(n, that.n);
			nQ = that.nQ;
			nLim = that.nLim;
			Utils.copy(fMaster, that.fMaster);
			Utils.copy(fTablelow, that.fTablelow);
			Utils.copy(fTablehigh, that.fTablehigh);
			Utils.copy(fTablenoise, that.fTablenoise);
			Utils.copy(fTablelim, that.fTablelim);
			numPatches = that.numPatches;
			Utils.copy(patchNumSubbands, that.patchNumSubbands);
			Utils.copy(patchStartSubband, that.patchStartSubband);
			Utils.copy(Xlow, that.Xlow);
			Utils.copy(Xhigh, that.Xhigh);
			Utils.copy(X, that.X);
			Utils.copy(alpha0, that.alpha0);
			Utils.copy(alpha1, that.alpha1);
			Utils.copy(eOrigmapped, that.eOrigmapped);
			Utils.copy(qMapped, that.qMapped);
			Utils.copy(sMapped, that.sMapped);
			Utils.copy(eCurr, that.eCurr);
			Utils.copy(qM, that.qM);
			Utils.copy(sM, that.sM);
			Utils.copy(gain, that.gain);
			Utils.copy(qmfFilterScratch, that.qmfFilterScratch);
			mdctAna.copy(that.mdctAna);
			mdct.copy(that.mdct);
		}
	}

}