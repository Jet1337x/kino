import os
import re
from tempfile import mkstemp
from shutil import move, copymode
from os import fdopen, remove

version = ['1', '2', '0']
client_version = ['2', '7', '1']

version_int = ''.join(version)
version_string = '{0}.{1}.{2}'.format(version[0], version[1], version[2])

client_version_int = ''.join(version)


current_dir = os.path.dirname(os.path.abspath(__file__))

config_path = os.path.join(current_dir, '..', 'KN_Core', 'src', 'KnConfig.cs')

def module_path(module):
    return os.path.join(current_dir, '..', module, 'Loader.cs')

def replace_version(file_path):
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found = False
            found_client = False
            for line in old_file:
                if found and found_client:
                    new_file.write(line)
                else:
                    if not found and 'Version = ' in line:
                        found = True
                        new_file.write(re.sub('\d{3}', version_int, line))
                    elif not found_client and 'ClientVersion = ' in line:
                        found_client = True
                        new_file.write(re.sub('\d{3}', client_version_int, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

def replace_config_version(file_path):
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found_int = False
            found_string = False
            for line in old_file:
                if found_int and found_string:
                    new_file.write(line)
                else:
                    if not found_int and 'Version = ' in line:
                        found_int = True
                        new_file.write(re.sub('\d{3}', version_int, line))
                    elif not found_string and 'StringVersion = ' in line:
                        found_string = True
                        new_file.write(re.sub('\s*([\d.]+)', version_string, line))
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