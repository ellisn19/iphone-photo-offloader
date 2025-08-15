using System;
using PortableDeviceApiLib;
using PortableDeviceTypesLib;

namespace iPhonePhotoOffloader
{
	public class IPhoneDevice
	{
		public string DeviceId { get; private set; }
		public string FriendlyName { get; private set; }

		private PortableDevice _device;

		public IPhoneDevice(string deviceId, string friendlyName)
		{
			DeviceId = deviceId;
			FriendlyName = friendlyName;
			_device = new PortableDevice();
		}

		public void Connect()
		{
			_device.Open(DeviceId);
		}

		public void Disconnect()
		{
			_device.Close();
		}

		public PortableDevice GetDeviceObject() => _device;

		// Static method to enumerate all connected iPhones
		public static IPhoneDevice[] GetConnectedDevices()
		{
			var deviceManager = new PortableDeviceManager();
			deviceManager.RefreshDeviceList();

			uint count = 0;
			deviceManager.GetDevices(null, ref count);
			if (count == 0) return new IPhoneDevice[0];

			string[] deviceIds = new string[count];
			deviceManager.GetDevices(deviceIds, ref count);

			var devices = new IPhoneDevice[count];
			for (int i = 0; i < count; i++)
			{
				deviceManager.GetDeviceFriendlyName(deviceIds[i], out string name, ref count);
				devices[i] = new IPhoneDevice(deviceIds[i], name);
			}

			return devices;
		}
	}
}
