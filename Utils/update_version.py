import os
import re
from tempfile import mkstemp
from shutil import move, copymode
from os import fdopen, remove

version = ['1', '2', '0']

versionInt = ''.join(version)
versionString = '{0}.{1}.{2}'.format(version[0], version[1], version[2])

current_dir = os.path.dirname(os.path.abspath(__file__))

config_path = os.path.join(current_dir, '..', 'KN_Core', 'src', 'KnConfig.cs')

def module_path(module):
    return os.path.join(current_dir, '..', module, 'Loader.cs')

def replace_version(file_path):
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found = False
            for line in old_file:
                if found:
                    new_file.write(line)
                else:
                    if 'Version = ' in line:
                        found = True
                        new_file.write(re.sub('\d{3}', versionInt, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

def replace_config_version(file_path):
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            foundInt = False
            foundString = False
            for line in old_file:
                if foundInt and foundString:
                    new_file.write(line)
                else:
                    if not foundInt and 'Version = ' in line:
                        foundInt = True
                        new_file.write(re.sub('\d{3}', versionInt, line))
                    elif not foundString and 'StringVersion = ' in line:
                        foundString = True
                        new_file.write(re.sub('\s*([\d.]+)', versionString, line))
                    else:
                        new_file.write(line) 

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

modules = [
    module_path('KN_Cinematic'),
    module_path('KN_Lights'),
    module_path('KN_Maps'),
    module_path('KN_Visuals')
]

for m in modules:    
    replace_version(m)
    
replace_config_version(config_path)