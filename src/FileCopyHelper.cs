using System.IO;

public static class FileCopyHelper
{
	public static void CopyStreamToFile(Stream source, string destinationPath)
	{
		using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
		source.CopyTo(fileStream);
	}
}
