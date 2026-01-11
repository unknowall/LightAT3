using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
    using static LightCodec.aac.OutputConfiguration;
    
	/// <summary>
    /// Dynamic Range Control - decoded from the bitstream but not processed further.
    /// </summary>
    public class DynamicRangeControl
	{
		internal int pceInstanceTag; ///< Indicates with which program the DRC info is associated.
		internal int[] dynRngSgn = new int[17]; ///< DRC sign information; 0 - positive, 1 - negative
		internal int[] dynRngCtl = new int[17]; ///< DRC magnitude information
		internal int[] excludeMask = new int[MAX_CHANNELS]; ///< Channels to be excluded from DRC processing.
		internal int bandIncr; ///< Number of DRC bands greater than 1 having DRC info.
		internal int interpolationScheme; ///< Indicates the interpolation scheme used in the SBR QMF domain.
		internal int[] bandTop = new int[17]; ///< Indicates the top of the i-th DRC band in units of 4 spectral lines.
		internal int progRefLevel; //< A reference level for the long-term program audio level for all channels combined.
	}

}