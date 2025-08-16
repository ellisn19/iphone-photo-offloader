using System;

class Program
{
	static void Main()
	{
		string targetFolder = @"G:\iPhonePictures2";
		var transfer = new IPhoneMediaTransfer();
		transfer.CopyAllMedia(targetFolder);
		Console.WriteLine("Transfer complete!");
	}
}
