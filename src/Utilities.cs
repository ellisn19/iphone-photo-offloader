using System;
using System.IO;

namespace iPhonePhotoOffloader
{
	public static class Utilities
	{
		public static void EnsureFolderExists(string path)
		{
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		public static string GetSafeFileName(string filename)
		{
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				filename = filename.Replace(c, '_');
			}
			return filename;
		}
	}
}
