using System;
using System.Threading.Tasks;

class Program
{
	static IPhoneMediaTransfer transfer;

	static async Task Main()
	{
		string targetFolder = @"G:\iPhonePictures2";
		transfer = new IPhoneMediaTransfer();

		Console.CancelKeyPress += OnExitInterrupt;
		AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

		await transfer.CopyAllMediaAsync(targetFolder);

		DisplayStats();
		Console.WriteLine("Transfer complete!");
	}

	static void OnExitInterrupt(object sender, ConsoleCancelEventArgs e)
	{
		Console.WriteLine("\nTransfer interrupted by user!");
		DisplayStats();
		e.Cancel = false; // allow program to exit after this handler
	}

	static void OnProcessExit(object sender, EventArgs e)
	{
		DisplayStats();
		Console.WriteLine("Transfer complete!");
	}

	static void DisplayStats()
	{
		Console.WriteLine($"\nTotal files copied: {transfer.TransferredCount}");
		Console.WriteLine($"Failed transfers: {transfer.FailedCount}");
	}
}