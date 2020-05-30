import subprocess
import shutil
import sys
import os

dest = sys.argv[1]
config = sys.argv[2]

current_dir = os.path.dirname(os.path.abspath(__file__))
in_path = os.path.join(current_dir, '..', 'TestLauncher', 'bin', config)


def get_dlls():
    to_copy = os.path.join(current_dir, 'to_copy.txt')

    with open(to_copy, 'r') as f:
        to_copy_dlls_raw = f.readlines()

    to_copy_dlls = []
    for dll in to_copy_dlls_raw:
        d = dll.rstrip('\n')
        to_copy_dlls.append(d)

    return to_copy_dlls


for root, dirs, files in os.walk(in_path):
    modules = get_dlls()
    for file in files:
        if any(file in s for s in modules):
            print(os.path.join(root, file))
            shutil.copyfile(os.path.join(root, file), os.path.join(dest, file))

# run carx
subprocess.run('cmd /c start steam://run/635260')
