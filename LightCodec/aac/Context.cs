using LightCodec.Utils;
using static LightCodec.aac.OutputConfiguration;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	using BitReader = LightCodec.Utils.BitReader;
	using FFT = LightCodec.Utils.FFT;

	public class Context
	{
		public BitReader br;
		public int frameSize;
		public int channels;
		public int skipSamples;
		public int nbSamples;
		public int randomState;
		public int sampleRate;
		public int outputChannels;

		//internal bool isSaved; //< Set if elements have stored overlap from previous frame
		internal DynamicRangeControl cheDrc = new DynamicRangeControl();

		// Channel element related data
		internal ChannelElement[][] che = ReturnRectangularChannelElementArray(4, MAX_ELEM_ID);

		internal ChannelElement[][] tagCheMap = ReturnRectangularChannelElementArray(4, MAX_ELEM_ID);
		public int tagsMapped;

		public float[] bufMdct = new float[1024];

		internal FFT mdct;
		internal FFT mdctSmall;
		internal FFT mdctLd;
		internal FFT mdctLtp;

		// Members user for output
		internal SingleChannelElement[] outputElement = new SingleChannelElement[MAX_CHANNELS]; //< Points to each SingleChannelElement

		public int dmonoMode;

		public float[] temp = new float[128];

		public OutputConfiguration[] oc = new OutputConfiguration[2];

		public float[][] samples = RectangularArrays.ReturnRectangularFloatArray(2, 2048);

        public static ChannelElement[][] ReturnRectangularChannelElementArray(int size1, int size2)
        {
            ChannelElement[][] newArray = new ChannelElement[size1][];
            for (int array1 = 0; array1 < size1; array1++)
            {
                newArray[array1] = new ChannelElement[size2];
            }

            return newArray;
        }


        public Context()
		{
			for (int i = 0; i < oc.Length; i++)
			{
				oc[i] = new OutputConfiguration();
			}
		}
	}

}