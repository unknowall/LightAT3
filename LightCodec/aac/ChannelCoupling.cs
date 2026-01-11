using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	/// <summary>
	/// coupling parameters
	/// </summary>
	public class ChannelCoupling
	{
		public int couplingPoint; ///< The point during decoding at which coupling is applied.
		public int numCoupled; ///< number of target elements
		public int[] type = new int[8]; ///< Type of channel element to be coupled - SCE or CPE.
		public int[] idSelect = new int[8]; ///< element id
		public int[] chSelect = new int[8]; /**< [0] shared list of gains; [1] list of gains for right channel;
	                                         *   [2] list of gains for left channel; [3] lists of gains for both channels
	                                         */

		public float[][] gain = RectangularArrays.ReturnRectangularFloatArray(16, 120);

		public virtual void copy(ChannelCoupling that)
		{
			couplingPoint = that.couplingPoint;
			numCoupled = that.numCoupled;
			Utils.copy(type, that.type);
			Utils.copy(idSelect, that.idSelect);
			Utils.copy(chSelect, that.chSelect);
			Utils.copy(gain, that.gain);
		}
	}

}