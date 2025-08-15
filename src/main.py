from iphone_device import IPhoneDevice
from photo_transfer import PhotoTransfer
from pathlib import Path

TARGET_FOLDER = Path("C:/iPhonePhotos")  # Change as needed
ORGANIZE_BY_DATE = True
DELETE_AFTER_COPY = False

def main():
    device = IPhoneDevice()
    if not device.connect():
        print("No iPhone detected. Make sure it's unlocked and connected via USB.")
        return

    print(f"Connected to {device.name}")
    PhotoTransfer.copy_all_photos(
        device,
        target_folder=TARGET_FOLDER,
        organize_by_date=ORGANIZE_BY_DATE,
        delete_after_copy=DELETE_AFTER_COPY
    )

if __name__ == "__main__":
    main()
