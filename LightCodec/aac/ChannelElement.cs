using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	/// <summary>
	/// channel element - generic struct for SCE/CPE/CCE/LFE
	/// </summary>
	public class ChannelElement
	{
		// CPE specific
		public int commonWindow;
		public int msMode;
		public int[] msMask = new int[128];
		// shared
		public SingleChannelElement[] ch = new SingleChannelElement[2];
		// CCE specific
		public ChannelCoupling coup = new ChannelCoupling();
		public SpectralBandReplication sbr = new SpectralBandReplication();

		public ChannelElement()
		{
			for (int i = 0; i < ch.Length; i++)
			{
				ch[i] = new SingleChannelElement();
			}
		}

		public virtual void copy(ChannelElement that)
		{
			commonWindow = that.commonWindow;
			msMode = that.msMode;
			Utils.copy(msMask, that.msMask);
			for (int i = 0; i < ch.Length; i++)
			{
				ch[i].copy(that.ch[i]);
			}
			coup.copy(that.coup);
			sbr.copy(that.sbr);
		}
	}

}