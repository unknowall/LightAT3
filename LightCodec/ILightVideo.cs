namespace LightCodec
{
	public interface ILightVideo
	{
		int init(int[] extraData);
		int decode(int[] input, int inputOffset, int inputLength);
		bool hasImage();
		int ImageWidth {get;}
		int ImageHeight {get;}
		int getImage(int[] luma, int[] cb, int[] cr);
		bool KeyFrame {get;}
		void getAspectRatio(int[] numDen);
	}

}