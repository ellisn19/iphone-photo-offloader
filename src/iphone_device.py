import os
from pathlib import Path

class IPhoneDevice:
    """
    Represents an iPhone connected via MTP.
    For simplicity, this version assumes the iPhone is mounted as a media device
    and accessible via Windows Explorer.
    """

    def __init__(self):
        self.name = "iPhone"

    def connect(self):
        """
        Checks if the iPhone DCIM folder exists.
        """
        self.dcim_path = self._get_dcim_path()
        return self.dcim_path is not None

    def _get_dcim_path(self):
        # Windows automatically mounts the iPhone under a media device path
        possible_drives = [Path(f"{d}:") for d in "DEFGHIJKLMNOPQRSTUVWXYZ"]
        for drive in possible_drives:
            dcim = drive / "DCIM"
            if dcim.exists():
                return dcim
        return None

    def list_photos(self):
        """
        Returns a list of all photo file paths under DCIM recursively.
        """
        photos = []
        for root, dirs, files in os.walk(self.dcim_path):
            for file in files:
                if file.lower().endswith((".jpg", ".jpeg", ".png", ".heic")):
                    photos.append(Path(root) / file)
        return photos
