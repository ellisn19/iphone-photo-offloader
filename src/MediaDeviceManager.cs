using MediaDevices;
using System.Linq;

public class MediaDeviceManager
{
	public MediaDevice GetIPhone()
	{
		var devices = MediaDevice.GetDevices();
		foreach (var dev in devices)
		{
			if (dev.FriendlyName.ToLower().Contains("iphone"))
			{
				dev.Connect();
				return dev;
			}
		}
		return null;
	}
}
