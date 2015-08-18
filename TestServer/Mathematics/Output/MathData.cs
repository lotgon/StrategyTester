using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Output
{
	public class MathData
	{
		#region constants
		private const string cCommentExtension = ".txt";
		private const string cDataExtension = ".dat";
		#endregion
		public MathData()
		{
		}
		public MathData(string directory)
		{
			Construct(directory);
		}
		protected void Construct(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentException("Invalid output directory");
			}
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			m_directory = directory;
		}
		#region helper methods
		public MathComment CommentFile(string name)
		{
			return File<MathComment>(name, cCommentExtension);
		}
		public MathList2D List2DFile(string name)
		{
			return File<MathList2D>(name, cDataExtension);
		}
		public MathValue ValueFile(string name)
		{
			return File<MathValue>(name, cDataExtension);
		}
		public MathArray1D Array1DFile(string name)
		{
			return File<MathArray1D>(name, cDataExtension);
		}
		public MathArray2D Array2DFile(string name)
		{
			return File<MathArray2D>(name, cDataExtension);
		}
		public MathList3D List3DFile(string name)
		{
			return File<MathList3D>(name, cDataExtension);
		}
		#endregion

		private T File<T>(string name, string extenstion) where T : MathFile, new()
		{
			if (string.IsNullOrEmpty(m_directory))
			{
				throw new InvalidOperationException("Output directory is not initialized");
			}
			string fullName = name + extenstion;
			MathFile file = null;
			if (!m_files.TryGetValue(fullName, out file))
			{
				file = new T();
				m_files[fullName] = file;
			}
			T result = (T)file;
			return result;
		}
		public void Save()
		{			
			foreach (var element in m_files)
			{
				string path = Path.Combine(m_directory, element.Key);
				MathFile file = element.Value;
				file.Save(path);
			}
		}
		#region members
		readonly Dictionary<string, MathFile> m_files = new Dictionary<string, MathFile>();
		string m_directory;
		#endregion
	}
}
