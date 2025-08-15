from pathlib import Path
import re

def ensure_folder_exists(folder: Path):
    folder.mkdir(parents=True, exist_ok=True)

def get_safe_filename(filename: str) -> str:
    """
    Removes illegal characters for Windows filenames.
    """
    return re.sub(r'[<>:"/\\|?*]', '_', filename)
