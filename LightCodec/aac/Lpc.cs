using System;
using LightCodec.Utils;
using OutputConfiguration = LightCodec.aac.OutputConfiguration;

namespace LightCodec.aac
{
	public class Lpc
	{
		/// <summary>
		/// Levinson-Durbin recursion.
		/// Produce LPC coefficients from autocorrelation data.
		/// </summary>
		public static int computeLpcCoefs(float[] autoc, int maxOrder, float[] lpc, int lpcStride, bool fail, bool normalize)
		{
			float err = 0f;
			int autocOffset = 0;
			int lpcOffset = 0;
			int lpcLast = lpcOffset;

			if (!(normalize || !fail))
			{
				System.Console.WriteLine(string.Format("computeLpcCoefs invalid parameters"));
			}

			if (normalize)
			{
				err = autoc[autocOffset++];
			}

			if (fail && (autoc[autocOffset + maxOrder - 1] == 0 || err <= 0f))
			{
				return -1;
			}

			for (int i = 0; i < maxOrder; i++)
			{
				float r = -autoc[autocOffset + i];

				if (normalize)
				{
					for (int j = 0; j < i; j++)
					{
						r -= lpc[lpcLast + j] * autoc[autocOffset + i - j - 1];
					}

					r /= err;
					err *= 1f - (r * r);
				}

				lpc[lpcOffset + i] = r;

				for (int j = 0; j < ((i + 1) >> 1); j++)
				{
					float f = lpc[lpcLast + j];
					float b = lpc[lpcLast + i - 1 - j];
					lpc[lpcOffset + j] = f + r * b;
					lpc[lpcOffset + i - 1 - j] = b + r * f;
				}

				if (fail && err < 0)
				{
					return -1;
				}

				lpcLast = lpcOffset;
				lpcOffset += lpcStride;
			}

			return 0;
		}
	}

}