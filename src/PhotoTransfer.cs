using System;
using System.IO;
using System.Security.Cryptography;
using PortableDeviceApiLib;
using PortableDeviceTypesLib;

namespace iPhonePhotoOffloader
{
	public static class PhotoTransfer
	{
		public static void CopyAllPhotos(IPhoneDevice device, string targetFolder, bool organizeByDate = false, bool deleteAfterCopy = false)
		{
			Utilities.EnsureFolderExists(targetFolder);
			var portableDevice = device.GetDeviceObject();
			EnumerateAndCopy(portableDevice, "DEVICE", targetFolder, organizeByDate, deleteAfterCopy);
		}

		private static void EnumerateAndCopy(PortableDevice device, string parentId, string targetFolder, bool organizeByDate, bool deleteAfterCopy)
		{
			device.Content(out IPortableDeviceContent content);
			content.EnumObjects(0, parentId, null, out IEnumPortableDeviceObjectIDs enumObj);

			uint fetched = 0;
			string[] objectIds = new string[1];

			while (enumObj.Next(1, objectIds, ref fetched) == 0 && fetched > 0)
			{
				string objectId = objectIds[0];

				content.Properties(out IPortableDeviceProperties properties);
				properties.GetValues(objectId, null, out IPortableDeviceValues values);

				values.GetStringValue(ref WPD_OBJECT_NAME, out string name);
				values.GetGuidValue(ref WPD_OBJECT_CONTENT_TYPE, out Guid contentType);

				if (contentType == WPD_CONTENT_TYPE_FOLDER)
				{
					string folderPath = Path.Combine(targetFolder, Utilities.GetSafeFileName(name));
					Utilities.EnsureFolderExists(folderPath);
					EnumerateAndCopy(device, objectId, folderPath, organizeByDate, deleteAfterCopy);
				}
				else
				{
					string destPath = Path.Combine(targetFolder, Utilities.GetSafeFileName(name));

					if (organizeByDate)
					{
						string dateFolder = Path.Combine(targetFolder, DateTime.Now.ToString("yyyy-MM-dd"));
						Utilities.EnsureFolderExists(dateFolder);
						destPath = Path.Combine(dateFolder, Utilities.GetSafeFileName(name));
					}

					// Skip if file already exists
					if (File.Exists(destPath))
					{
						Console.WriteLine($"Skipping existing file: {destPath}");
						continue;
					}

					if (CopyFile(content, objectId, destPath))
					{
						Console.WriteLine($"Copied: {destPath}");
						if (deleteAfterCopy)
						{
							DeleteFile(content, objectId);
							Console.WriteLine($"Deleted from iPhone: {name}");
						}
					}
					else
					{
						Console.WriteLine($"Failed to copy: {name}");
					}
				}
			}
		}

		private static bool CopyFile(IPortableDeviceContent content, string objectId, string destPath)
		{
			try
			{
				content.Transfer(out IStream wpdStream, objectId, 0);
				using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
				{
					byte[] buffer = new byte[4096];
					int bytesRead;
					while ((bytesRead = wpdStream.Read(buffer, buffer.Length, IntPtr.Zero)) > 0)
					{
						fileStream.Write(buffer, 0, bytesRead);
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void DeleteFile(IPortableDeviceContent content, string objectId)
		{
			string[] ids = new string[] { objectId };
			content.Delete(0, ids, out IPortableDevicePropVariantCollection results);
		}

		// WPD property constants
		private static readonly _tagpropertykey WPD_OBJECT_NAME = new _tagpropertykey
		{
			fmtid = new Guid("EF6B490D-5CD8-437A-AFFC-DA8B60EE4A3C"),
			pid = 4
		};

		private static readonly _tagpropertykey WPD_OBJECT_CONTENT_TYPE = new _tagpropertykey
		{
			fmtid = new Guid("EF6B490D-5CD8-437A-AFFC-DA8B60EE4A3C"),
			pid = 7
		};

		private static readonly Guid WPD_CONTENT_TYPE_FOLDER = new Guid("27E2E392-A111-48E0-AB0C-E17705A05F85");
	}
}
