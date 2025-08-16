using System;
using System.Collections.Generic;
using System.IO;
using MediaDevices;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;

public class IPhoneMediaTransfer
{
	private readonly MediaDevice device;
	private readonly HashSet<string> transferredFiles = new HashSet<string>();
	private readonly List<string> failedFiles = new List<string>();
	private const int MaxRetries = 3;

	public IPhoneMediaTransfer()
	{
		device = GetIPhone();
	}

	private MediaDevice GetIPhone()
	{
		var phone = MediaDevice.GetDevices().FirstOrDefault();
		if (phone != null)
		{
			phone.Connect();
			Console.WriteLine($"Connected to iPhone: {phone.FriendlyName}");
		}
		return phone;
	}

	private void CopyFileWithRetries(string source, string dest)
	{
		for (int attempt = 1; attempt <= MaxRetries; attempt++)
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(dest));
				device.DownloadFile(source, dest);
				Console.WriteLine($"Copied {source} → {dest}");
				transferredFiles.Add(dest);
				return;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Attempt {attempt} failed for {source}: {ex.Message}");
				if (attempt == MaxRetries)
				{
					failedFiles.Add($"{source} → {dest}: {ex.Message}");
				}
				else
				{
					Thread.Sleep(500);
				}
			}
		}
	}

	private void CopyFolderRecursively(string sourceFolder, string targetFolder)
	{
		try
		{
			Directory.CreateDirectory(targetFolder);

			foreach (var entry in device.GetFileSystemEntries(sourceFolder))
			{
				string name = Path.GetFileName(entry);
				string destPath = Path.Combine(targetFolder, name);

				if (device.FileExists(entry))
				{
					if (!transferredFiles.Contains(destPath))
					{
						CopyFileWithRetries(entry, destPath);
					}
				}
				else if (device.DirectoryExists(entry))
				{
					CopyFolderRecursively(entry, destPath);
				}
			}
		}
		catch (COMException ex)
		{
			Console.WriteLine($"Failed to enumerate folder {sourceFolder}: {ex.Message}");
			failedFiles.Add($"Folder {sourceFolder}: {ex.Message}");
		}
	}


	public void CopyAllMedia(string targetFolder)
	{
		if (device == null)
		{
			Console.WriteLine("No iPhone detected.");
			return;
		}

		string root = @"\Internal Storage";

		if (!device.DirectoryExists(root))
		{
			Console.WriteLine($"Directory {root} not found.");
			return;
		}

		foreach (var folder in device.GetFileSystemEntries(root))
		{
			if (device.DirectoryExists(folder))
			{
				string destFolder = Path.Combine(targetFolder, Path.GetFileName(folder));
				CopyFolderRecursively(folder, destFolder);
			}
		}

		Console.WriteLine($"Total copied files: {transferredFiles.Count}");

		if (failedFiles.Count > 0)
		{
			string projectRoot = AppContext.BaseDirectory;
			projectRoot = Path.GetFullPath(Path.Combine(projectRoot, @"..\..\.."));
			string failLog = Path.Combine(projectRoot, "failed_transfers.txt");
			File.WriteAllLines(failLog, failedFiles);
			Console.WriteLine($"Failed transfers logged to: {failLog}");
		}
	}
}
