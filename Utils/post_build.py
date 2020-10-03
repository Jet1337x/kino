import subprocess
import shutil
import sys
import os

import get_dlls

dest = sys.argv[1]
config = sys.argv[2]

current_dir = os.path.dirname(os.path.abspath(__file__))
in_path = os.path.join(current_dir, '..', 'TestLauncher', 'bin', config)


for root, dirs, files in os.walk(in_path):
    to_copy = os.path.join(current_dir, 'to_copy.txt')

    modules = get_dlls.get_dlls(to_copy)
    for file in files:
        if any(file in s for s in modules):
            print(os.path.join(root, file))
            shutil.copyfile(os.path.join(root, file), os.path.join(dest, file))

# run carx
subprocess.run('cmd /c start steam://run/635260')
