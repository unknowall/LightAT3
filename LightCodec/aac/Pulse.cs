using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	public class Pulse
	{
		public int numPulse;
		public int start;
		public int[] pos = new int[4];
		public int[] amp = new int[4];

		public virtual void copy(Pulse that)
		{
			numPulse = that.numPulse;
			start = that.start;
			Utils.copy(pos, that.pos);
			Utils.copy(amp, that.amp);
		}
	}

}