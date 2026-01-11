namespace LightCodec.aac
{
	public class PredictorState
	{
		public float cor0;
		public float cor1;
		public float var0;
		public float var1;
		public float r0;
		public float r1;

		public virtual void copy(PredictorState that)
		{
			cor0 = that.cor0;
			cor1 = that.cor1;
			var0 = that.var0;
			var1 = that.var1;
			r0 = that.r0;
			r1 = that.r1;
		}
	}

}