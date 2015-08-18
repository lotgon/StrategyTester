using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public struct Pixel2 : IComparable<Pixel2>
	{
		public int X;
		public int Y;
		public Pixel2(int x, int y): this()
		{
			X = x;
			Y = y;
		}
		public int CompareTo(Pixel2 pixel)
		{ 
			if (X < pixel.X)
			{
				return 1;
			}
			else if (X > pixel.X)
			{
				return -1;
			}
		
			if (Y < pixel.Y)
			{
				return 1;
			}
			else if (Y > pixel.Y)
			{
				return -1;
			}
			return 0;
		}
	}
}
