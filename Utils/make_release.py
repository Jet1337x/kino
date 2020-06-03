import os
import get_dlls
from zipfile import ZipFile
from datetime import datetime

current_dir = os.path.dirname(os.path.abspath(__file__))
in_path = os.path.join(current_dir, '..', 'TestLauncher', 'bin', 'Release')
out_path = os.path.join(current_dir, '..', 'Release')

if not os.path.exists(out_path):
    os.mkdir(out_path)

zip_name = 'release_' + datetime.now().strftime("%m.%d.%y_%H.%M.%S") + '.zip'
zip_path = os.path.join(out_path, zip_name)
zip_archive = ZipFile(zip_path, 'w')

for root, dirs, files in os.walk(in_path):
    modules = get_dlls.get_dlls()
    for file in files:
        if any(file in s for s in modules):
            dll_path = os.path.join(root, file)
            zip_archive.write(dll_path, os.path.basename(dll_path))

zip_archive.close()
