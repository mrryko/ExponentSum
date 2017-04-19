using System;
using System.Numerics;
using System.Text;

namespace Exponent
{
	public struct BigDecimal : IComparable<BigDecimal>
	{
		public static BigDecimal Zero = new BigDecimal();

		public BigInteger Digit { get; set; }
		public int Scale { get; set; }

		/// <summary>
		/// Specifies whether the significant digits should be truncated to the given precision after each operation.
		/// </summary>
		public static bool IsAlwaysTruncate = true;

		/// <summary>
		/// Sets the maximum precision of division operations.
		/// If AlwaysTruncate is set to true all operations are affected.
		/// </summary>
		public const int PRECISION = 39;

		private BigDecimal(BigInteger digit, int scale)
		{
			Digit = digit;
			Scale = scale;
			Normalize();
		}


		public static BigDecimal Parse(string rawDigit)
		{
			//if (BigInteger.Parse(rawDigit) == 0)
			//{
			//    return new BigDecimal(0,0); //doesn't parse	for some odd reason 
			//}
			BigInteger digit = 0;
			int scale = 0;
			string number = rawDigit.ToLower();
			if (number.Contains("e"))
			{
				if (number[0] == '0')
				{
					throw new FormatException();
				}
				string[] splittedArray = number.Split('e');
				scale -= Int32.Parse(splittedArray[1]);
				number = splittedArray[0];


			}
			if (number.Contains("."))
			{
				string[] sc = number.Split('.');
				if (sc[1].Length > PRECISION)
				{
					string trimmed = sc[1].Remove(PRECISION);
					sc[1] = trimmed;
				}
				scale += sc[1].Length;
				string finalNumber = string.Join("", sc);
				digit = BigInteger.Parse(finalNumber.Replace(".", string.Empty));
			}
			else
			{
				digit = BigInteger.Parse(number);
				scale += 0;
			}
			return new BigDecimal(digit, scale);
		}


		/// <summary>
		/// Removes trailing zeros on the mantissa
		/// </summary>
		/// 
		public void Normalize()
		{
			if (Digit.IsZero)
			{
				Scale = 0;
			}
			else
			{
				BigInteger excess = 0;
				while (excess == 0)
				{
					var shortened = BigInteger.DivRem(Digit, 10, out excess);
					if (excess == 0)
					{
						Digit = shortened;
						Scale--;
					}
				}
			}
		}



		public static BigDecimal operator +(BigDecimal left, BigDecimal right)
		{
			return Add(left, right);
		}

		public static BigDecimal Add(BigDecimal left, BigDecimal right)
		{

			BigInteger digit;
			int scale;
			if (left.Scale == right.Scale)  //if scales equal each other
			{
				digit = left.Digit + right.Digit;
				scale = digit % 10 == 0 ? --left.Scale : left.Scale; //if sum equals 10 (0.7+0.3, etc)
			}
			else
			{
				if (left.Scale > right.Scale)
				{
					// Add(right, left);                       //enters here again
					Swap(ref left, ref right);
				}

				if (right.Digit == 0)
				{
					digit = left.Digit;
					scale = left.Scale;
				}

				else if (left.Scale >= 0 && right.Scale >= 0) //if scales are bigger than 0; digits are less than 0, eg. 0.0005. 0.2
				{
					digit = left.Digit * BigInteger.Pow(10, Math.Abs(right.Scale - left.Scale)) + right.Digit;
					scale = right.Scale;
					if (Math.Abs(left.Scale - right.Scale) > PRECISION - 1 && left.Scale != 0) //if difference is bigger than PRECISION-1 
					{
						scale = left.Scale;
					}
				}

				else if (left.Scale < 0 && right.Scale < 0) //if scales are less than 0; digits are bigger than 0, eg. 500, 23
				{
					digit = left.Digit * BigInteger.Pow(10, Math.Abs(right.Scale - left.Scale)) + right.Digit;
					scale = digit % 10 == 0 ? --right.Scale : right.Scale;
					if (Math.Abs(left.Scale - right.Scale) > PRECISION - 1) //if difference is bigger than PRECISION-1 
					{
						scale = left.Scale;
					}
				}

				else if (right.Scale == 0)
				{
					digit = left.Digit * BigInteger.Pow(10, Math.Abs(left.Scale)) + right.Digit;
					scale = 0;
				}

				else
				{
					digit = left.Digit * BigInteger.Pow(10, Math.Abs(right.Scale)) * BigInteger.Pow(10, Math.Abs(left.Scale)) + right.Digit;
					scale = right.Scale;
					if (Math.Abs(left.Scale - right.Scale) > PRECISION - 1) //if difference is bigger than PRECISION-1 
					{
						scale = left.Scale;
					}
				}
			}

			string tempNumber = digit.ToString();

			if (digit.ToString().Length > PRECISION) //trim useless numbers
			{
				tempNumber = tempNumber.Substring(0, PRECISION);
			}

			digit = digit == 0 ? digit : BigInteger.Parse(tempNumber.TrimEnd('0'));
			return new BigDecimal(digit, scale);
		}

		public override string ToString()
		{
			string digit = Digit.ToString();
			int digitLenght = digit.Length;
			char exponentSign = '\0';
			int exponent = 0;

			if (Scale == 0)
			{
				return Digit.ToString();
			}
			else if (Scale > 0)
			{
				if (Scale >= digitLenght)
				{
					exponent = Scale - digitLenght + 1;
					exponentSign = '-';
				}
				else
				{
					exponent = digitLenght - Scale - 1;
					exponentSign = '+';
				}
			}
			else
			{
				exponent = Math.Abs(Scale);
				exponentSign = '+';
				if (digitLenght > 1)
				{
					exponent += digitLenght - 1;
				}
			}

			if (digitLenght < 3)
			{
				digit = AppendZeros(digit, 3 - digitLenght);
				digitLenght = digit.Length;
			}
			digit = digit.Insert(digitLenght + 1 - digitLenght, ".");

			digit = $"{digit}e{exponentSign}{exponent}";
			return digit;
		}

		public static string AppendZeros(string changed, int count)
		{
			StringBuilder sb = new StringBuilder(changed);
			while (count != 0)
			{
				sb.Append("0");
				count--;
			}
			return sb.ToString();
		}

		public int CompareTo(BigDecimal other)
		{
			if (Scale > other.Scale)
				return 1;
			if (Scale < other.Scale)
				return -1;
			else
				return 0;
		}

		private static void Swap(ref BigDecimal left, ref BigDecimal right)
		{
			BigInteger tempDigit;
			int tempScale;
			tempDigit = left.Digit;
			tempScale = left.Scale;
			left.Scale = right.Scale;
			left.Digit = right.Digit;
			right.Digit = tempDigit;
			right.Scale = tempScale;
		}

	}
}
