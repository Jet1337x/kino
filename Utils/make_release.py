import os
import get_dlls
import update_version
from zipfile import ZipFile
from datetime import datetime

current_dir = os.path.dirname(os.path.abspath(__file__))
in_path = os.path.join(current_dir, '..', 'TestLauncher', 'bin', 'Release')
out_path = os.path.join(current_dir, '..', 'Release')

print('Making release ...')
print('Input path: ' + in_path)

if not os.path.exists(out_path):
    os.mkdir(out_path)

zip_name = 'release_' + update_version.version_string + '.zip'
zip_path = os.path.join(out_path, zip_name)

print('Zip path: ' + zip_path)

zip_archive = ZipFile(zip_path, 'w')

for root, dirs, files in os.walk(in_path):
    modules = get_dlls.get_dlls()
    for file in files:
        if any(file in s for s in modules):
            dll_path = os.path.join(root, file)
            zip_archive.write(dll_path, os.path.basename(dll_path))
            print('Added file: ' + os.path.basename(dll_path))

zip_archive.close()
