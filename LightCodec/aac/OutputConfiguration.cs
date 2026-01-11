using System;
using LightCodec.Utils;

namespace LightCodec.aac
{
    internal static class Float
    {
        public static int floatToRawIntBits(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            int result = BitConverter.ToInt32(bytes, 0);

            return result;
        }

        public static float intBitsToFloat(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            float f = BitConverter.ToSingle(bytes, 0);
            return f;
        }

        public static int floatToIntBits(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            int i = BitConverter.ToInt32(bytes, 0);
            return i;
        }
    }

    public static class Utils
    {
        public static void fill(int[][] a, int value)
        {
            for (int i = 0; i < a.Length; i++)
            {
                Arrays.Fill(a[i], value);
            }
        }

        public static void fill(float[] a, float value)
        {
            Arrays.Fill(a, value);
        }

        public static void fill(float[][] a, float value)
        {
            for (int i = 0; i < a.Length; i++)
            {
                Arrays.Fill(a[i], value);
            }
        }

        public static void fill(float[][][] a, float value)
        {
            for (int i = 0; i < a.Length; i++)
            {
                fill(a[i], value);
            }
        }

        public static void fill(float[][][][] a, float value)
        {
            for (int i = 0; i < a.Length; i++)
            {
                fill(a[i], value);
            }
        }

        public static float clipf(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static int clip(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }

            return value;
        }

        public static void copy(bool[] to, bool[] from)
        {
            Array.Copy(from, 0, to, 0, to.Length);
        }

        public static void copy(bool[][] to, bool[][] from)
        {
            for (int i = 0; i < to.Length; i++)
            {
                copy(to[i], from[i]);
            }
        }

        public static void copy(float[] to, float[] from)
        {
            Array.Copy(from, 0, to, 0, to.Length);
        }

        public static void copy(float[][] to, float[][] from)
        {
            for (int i = 0; i < to.Length; i++)
            {
                copy(to[i], from[i]);
            }
        }

        public static void copy(int[] to, int[] from)
        {
            Array.Copy(from, 0, to, 0, to.Length);
        }

        public static void copy(int[][] to, int[][] from)
        {
            for (int i = 0; i < to.Length; i++)
            {
                copy(to[i], from[i]);
            }
        }

        public static void copy(float[][][] to, float[][][] from)
        {
            for (int i = 0; i < to.Length; i++)
            {
                copy(to[i], from[i]);
            }
        }

        public static void copy(float[][][][] to, float[][][][] from)
        {
            for (int i = 0; i < to.Length; i++)
            {
                copy(to[i], from[i]);
            }
        }
    }

    public class OutputConfiguration
	{
        public const int AAC_ERROR = -4;
        public const int MAX_CHANNELS = 64;
        public const int MAX_ELEM_ID = 16;
        public const int TNS_MAX_ORDER = 20;
        public const int MAX_LTP_LONG_SFB = 40;
        public const int MAX_PREDICTORS = 672;
        // Raw Data Block Types:
        public const int TYPE_SCE = 0;
        public const int TYPE_CPE = 1;
        public const int TYPE_CCE = 2;
        public const int TYPE_LFE = 3;
        public const int TYPE_DSE = 4;
        public const int TYPE_PCE = 5;
        public const int TYPE_FIL = 6;
        public const int TYPE_END = 7;
        // Channel layouts
        public const int CH_FRONT_LEFT = 0x001;
        public const int CH_FRONT_RIGHT = 0x002;
        public const int CH_FRONT_CENTER = 0x004;
        public const int CH_LOW_FREQUENCY = 0x008;
        public const int CH_BACK_LEFT = 0x010;
        public const int CH_BACK_RIGHT = 0x020;
        public const int CH_FRONT_LEFT_OF_CENTER = 0x040;
        public const int CH_FRONT_RIGHT_OF_CENTER = 0x080;
        public const int CH_BACK_CENTER = 0x100;
        public const int CH_SIDE_LEFT = 0x200;
        public const int CH_SIDE_RIGHT = 0x400;
        public const int CH_LAYOUT_MONO = CH_FRONT_CENTER;
        public const int CH_LAYOUT_STEREO = CH_FRONT_LEFT | CH_FRONT_RIGHT;
        public const int CH_LAYOUT_SURRound = CH_LAYOUT_STEREO | CH_FRONT_CENTER;
        public const int CH_LAYOUT_4POINT0 = CH_LAYOUT_SURRound | CH_BACK_CENTER;
        public const int CH_LAYOUT_5POINT0_BACK = CH_LAYOUT_SURRound | CH_BACK_LEFT | CH_BACK_RIGHT;
        public const int CH_LAYOUT_5POINT1_BACK = CH_LAYOUT_5POINT0_BACK | CH_LOW_FREQUENCY;
        public const int CH_LAYOUT_7POINT1_WIDE_BACK = CH_LAYOUT_5POINT1_BACK | CH_FRONT_LEFT_OF_CENTER | CH_FRONT_RIGHT_OF_CENTER;
        // Extension payload IDs
        public const int EXT_FILL = 0x0;
        public const int EXT_FILL_DATA = 0x1;
        public const int EXT_DATA_ELEMENT = 0x2;
        public const int EXT_DYNAMIC_RANGE = 0xB;
        public const int EXT_SBR_DATA = 0xD;
        public const int EXT_SBR_DATA_CRC = 0xE;
        // Channel positions
        public const int AAC_CHANNEL_OFF = 0;
        public const int AAC_CHANNEL_FRONT = 1;
        public const int AAC_CHANNEL_SIDE = 2;
        public const int AAC_CHANNEL_BACK = 3;
        public const int AAC_CHANNEL_LFE = 4;
        public const int AAC_CHANNEL_CC = 5;
        // Audio object types
        public const int AOT_AAC_MAIN = 1;
        public const int AOT_AAC_LC = 2;
        public const int AOT_AAC_LTP = 4;
        public const int AOT_ER_AAC_LC = 17;
        public const int AOT_ER_AAC_LTP = 19;
        public const int AOT_ER_AAC_LD = 23;
        public const int AOT_ER_AAC_ELD = 39;
        // Window Sequences
        public const int ONLY_LONG_SEQUENCE = 0;
        public const int LONG_START_SEQUENCE = 1;
        public const int EIGHT_SHORT_SEQUENCE = 2;
        public const int LONG_STOP_SEQUENCE = 3;
        // Band Types
        public const int ZERO_BT = 0; ///< Scalefactors and spectral data are all zero.
		public const int FIRST_PAIR_BT = 5; ///< This and later band types encode two values (rather than four) with one code word.
		public const int ESC_BT = 11; ///< Spectral data are coded with an escape sequence.
		public const int NOISE_BT = 13; ///< Spectral data are scaled white noise not coded in the bitstream.
		public const int INTENSITY_BT2 = 14; ///< Scalefactor data are intensity stereo positions.
		public const int INTENSITY_BT = 15; ///< Scalefactor data are intensity stereo positions.
        // Coupling Points
        public const int BEFORE_TNS = 0;
        public const int BETWEEN_TNS_AND_IMDCT = 1;
        public const int AFTER_IMDCT = 3;
        
        public const int OC_NONE = 0; ///< Output unconfigured
		public const int OC_TRIAL_PCE = 1; ///< Output configuration under trial specified by an inband PCE
		public const int OC_TRIAL_FRAME = 2; ///< Output configuration under trial specified by a frame header
		public const int OC_GLOBAL_HDR = 3; ///< Output configuration set in a global header but not yet locked
		public const int OC_LOCKED = 4; ///< Output configuration locked in place

		public MPEG4AudioConfig m4ac = new MPEG4AudioConfig();
		public int[][] layoutMap = RectangularArrays.ReturnRectangularIntArray(MAX_ELEM_ID * 4, 3);
		public int layoutMapTags;
		public int channels;
		public int channelLayout;
		public int status;

        public virtual void copy(OutputConfiguration that)
		{
			m4ac.copy(that.m4ac);
            Utils.copy(layoutMap, that.layoutMap);
			layoutMapTags = that.layoutMapTags;
			channels = that.channels;
			channelLayout = that.channelLayout;
			status = that.status;
		}
	}

}