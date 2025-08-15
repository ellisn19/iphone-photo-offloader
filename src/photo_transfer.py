import shutil
from pathlib import Path
from utils import ensure_folder_exists, get_safe_filename

class PhotoTransfer:
    @staticmethod
    def copy_all_photos(device, target_folder: Path, organize_by_date=True, delete_after_copy=False):
        ensure_folder_exists(target_folder)
        photos = device.list_photos()
        print(f"Found {len(photos)} photos")

        for photo in photos:
            # Determine destination path
            dest_folder = target_folder
            if organize_by_date:
                date_folder = photo.stat().st_mtime
                from datetime import datetime
                date_folder_str = datetime.fromtimestamp(date_folder).strftime("%Y-%m-%d")
                dest_folder = target_folder / date_folder_str

            ensure_folder_exists(dest_folder)
            dest_path = dest_folder / get_safe_filename(photo.name)

            # Skip if file exists
            if dest_path.exists():
                print(f"Skipping existing file: {dest_path}")
                continue

            # Copy photo
            try:
                shutil.copy2(photo, dest_path)
                print(f"Copied: {dest_path}")
                if delete_after_copy:
                    photo.unlink()
                    print(f"Deleted from iPhone: {photo.name}")
            except Exception as e:
                print(f"Failed to copy {photo.name}: {e}")
