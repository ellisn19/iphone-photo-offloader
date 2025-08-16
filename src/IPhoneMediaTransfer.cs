using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using MediaDevices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class IPhoneMediaTransfer
{
	private readonly MediaDevice device;
	private readonly ConcurrentDictionary<string, byte> transferredFiles = new ConcurrentDictionary<string, byte>();
	private readonly ConcurrentBag<string> failedFiles = new ConcurrentBag<string>();
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
				transferredFiles.TryAdd(dest, 0);
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
					Thread.Sleep(500 * attempt);
				}
			}
		}
	}

	private void CopyFolderRecursively(string sourceFolder, string targetFolder)
	{
		try
		{
			Directory.CreateDirectory(targetFolder);

			var entries = device.GetFileSystemEntries(sourceFolder);

			// Run in parallel with a cap (avoid overloading USB / device)
			Parallel.ForEach(entries,
				new ParallelOptions { MaxDegreeOfParallelism = 4 }, // adjust based on performance
				entry =>
				{
					string name = Path.GetFileName(entry);
					string destPath = Path.Combine(targetFolder, name);

					if (device.FileExists(entry))
					{
						if (!transferredFiles.ContainsKey(destPath))
						{
							CopyFileWithRetries(entry, destPath);
						}
					}
					else if (device.DirectoryExists(entry))
					{
						CopyFolderRecursively(entry, destPath);
					}
				});
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
