using System;

namespace iPhonePhotoOffloader
{
	class Program
	{
		static void Main(string[] args)
		{
			string targetFolder = @"C:\iPhonePhotos"; // Change as needed

			Console.WriteLine("Starting iPhone photo offload...");

			try
			{
				var devices = IPhoneDevice.GetConnectedDevices();
				if (devices.Length == 0)
				{
					Console.WriteLine("No iPhone devices found.");
					return;
				}

				foreach (var device in devices)
				{
					Console.WriteLine($"Found device: {device.FriendlyName}");
					device.Connect();
					PhotoTransfer.CopyAllPhotos(device, targetFolder, organizeByDate: true, deleteAfterCopy: true);
					device.Disconnect();
				}

				Console.WriteLine("Transfer complete!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}
}
