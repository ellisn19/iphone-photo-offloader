using System;
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
	private readonly HashSet<string> transferredFiles = new HashSet<string>();
	private readonly List<string> failedFiles = new List<string>();
	private const int MaxRetries = 3;

	public int TransferredCount => transferredFiles.Count;
	public int FailedCount => failedFiles.Count;

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

	private async Task<bool> CopyFileWithRetriesAsync(string source, string dest)
	{
		for (int attempt = 1; attempt <= MaxRetries; attempt++)
		{
			try
			{
				// Create directory asynchronously
				await Task.Run(() => Directory.CreateDirectory(Path.GetDirectoryName(dest)));

				// Wrap the MediaDevice download in a Task to make it async
				await Task.Run(() => device.DownloadFile(source, dest));

				Console.WriteLine($"Copied {source} → {dest}");
				transferredFiles.Add(dest);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Attempt {attempt} failed for {source}: {ex.Message}");
				if (attempt == MaxRetries)
				{
					failedFiles.Add($"{source} → {dest}: {ex.Message}");
					return false;
				}
				else
				{
					// Use async delay instead of Thread.Sleep
					await Task.Delay(500 * attempt);
				}
			}
		}
		return false;
	}

	private async Task CopyFolderRecursivelyAsync(string sourceFolder, string targetFolder)
	{
		// Ensure folder exists
		await Task.Run(() => Directory.CreateDirectory(targetFolder));
		Console.WriteLine($"Created directory (or already exists): {targetFolder}");

		var entries = device.GetFileSystemEntries(sourceFolder);

		foreach (var entry in entries)
		{
			if (device.DirectoryExists(entry))
			{
				// Recurse into subfolder
				string subFolder = Path.Combine(targetFolder, Path.GetFileName(entry));
				await CopyFolderRecursivelyAsync(entry, subFolder);
			}
			else
			{
				string destPath = Path.Combine(targetFolder, Path.GetFileName(entry));

				// ✅ Get source file info from device
				var sourceInfo = device.GetFileInfo(entry);
				long sourceSize = (long)sourceInfo.Length;

				// ✅ Skip if file already exists and sizes match
				if (File.Exists(destPath))
				{
					long existingSize = new FileInfo(destPath).Length;

					if (existingSize == sourceSize)
					{
						Console.WriteLine($"Skipping existing file: {destPath}");
						continue;
					}
					else
					{
						Console.WriteLine($"Re-copying (size mismatch): {destPath}");
					}
				}

				// Copy with retries
				await CopyFileWithRetriesAsync(entry, destPath);
				transferredFiles.Add(destPath);
			}
		}
	}


	public async Task CopyAllMediaAsync(string targetFolder)
	{
		if (device == null)
		{
			Console.WriteLine("No iPhone detected.");
			return;
		}

		string root = @"\Internal Storage";

		if (!await Task.Run(() => device.DirectoryExists(root)))
		{
			Console.WriteLine($"Directory {root} not found.");
			return;
		}

		var rootEntries = await Task.Run(() => device.GetFileSystemEntries(root));

		foreach (var folder in rootEntries)
		{
			if (await Task.Run(() => device.DirectoryExists(folder)))
			{
				string destFolder = Path.Combine(targetFolder, Path.GetFileName(folder));
				await CopyFolderRecursivelyAsync(folder, destFolder);
			}
		}

		Console.WriteLine($"Total copied files: {transferredFiles.Count}");

		if (failedFiles.Count > 0)
		{
			string projectRoot = AppContext.BaseDirectory;
			projectRoot = Path.GetFullPath(Path.Combine(projectRoot, @"..\..\.."));
			string failLog = Path.Combine(projectRoot, "failed_transfers.txt");
			await File.WriteAllLinesAsync(failLog, failedFiles);
			Console.WriteLine($"Failed transfers logged to: {failLog}");
		}
	}
}