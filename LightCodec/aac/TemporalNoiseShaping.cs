using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
    using static LightCodec.aac.OutputConfiguration;
    
	/// <summary>
    /// Temporal Noise Shaping
    /// </summary>
    public class TemporalNoiseShaping
	{
		public bool present;
		public int[] nFilt = new int[8];

		public int[][] Length = RectangularArrays.ReturnRectangularIntArray(8, 4);

		public bool[][] direction = RectangularArrays.ReturnRectangularBoolArray(8, 4);

		public int[][] order = RectangularArrays.ReturnRectangularIntArray(8, 4);

		public float[][][] coef = RectangularArrays.ReturnRectangularFloatArray(8, 4, TNS_MAX_ORDER);

		public virtual void copy(TemporalNoiseShaping that)
		{
			present = that.present;
			Utils.copy(nFilt, that.nFilt);
			Utils.copy(Length, that.Length);
			Utils.copy(direction, that.direction);
			Utils.copy(order, that.order);
			Utils.copy(coef, that.coef);
		}
	}

}