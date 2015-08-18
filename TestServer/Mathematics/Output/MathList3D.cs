using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics;

namespace Mathematics.Output
{
	public class MathList3D : MathFile
	{
		public MathList3D()
		{
		}
		protected override void DoSave(string path)
		{
			m_helper.Save(path);
		}
		public Point3D[] Data
		{
			get
			{
				return m_data;
			}
			set
			{
				double[,] data = new double[value.Length, 3];
				for (int index = 0; index < value.Length; ++index)
				{
					data[index, 0] = value[index].X;
					data[index, 1] = value[index].Y;
					data[index, 2] = value[index].Z;
				}
				m_helper.Data = data;
				m_data = value;
			}
		}
		public void SetData(double[] x, double[] y, double[] z)
		{
			int count = Math.Min(x.Length, y.Length);
			count = Math.Min(count, z.Length);
			Point3D[] data = new Point3D[count];
			for (int index = 0; index < count; ++index)
			{
				data[index].X = x[index];
				data[index].Y = y[index];
				data[index].Z = z[index];
			}
			this.Data = data;
		}
		#region members
		private readonly MathArray2D m_helper = new MathArray2D();
		private Point3D[] m_data;
		#endregion
	}
}
