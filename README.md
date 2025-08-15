# iPhonePhotoOffloader

C# tool to bulk copy photos and videos from an iPhone to a Windows PC.  
Optionally preserves folder structure or organizes photos by date.

## Features

- Enumerates connected iPhone devices
- Copies all photos from DCIM folders
- Optional organization by date
- Can be extended to delete photos after copy

## Requirements

- Windows 10 or 11
- .NET 6+ or .NET Framework 4.8
- COM reference: Portable Devices 1.0 Type Library

## Usage

1. Clone the repo
2. Open `iPhonePhotoOffloader.csproj` in Visual Studio
3. Add COM reference to "Portable Devices 1.0 Type Library"
4. Build and run
5. Photos are copied to `C:\iPhonePhotos` by default

## Future Enhancements

- Delete photos after successful copy
- Skip already transferred files
- Extract date taken from metadata for better organization
