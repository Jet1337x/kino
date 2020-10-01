import os
import re
from tempfile import mkstemp
from shutil import move, copymode
from os import fdopen, remove

version = ['1', '2', '4']
patch = '1'
updater = '01'
client_version = ['2', '7', '1']

version_int = ''.join(version)
version_string = '{0}.{1}.{2}'.format(version[0], version[1], version[2])

client_version_int = ''.join(client_version)


current_dir = os.path.dirname(os.path.abspath(__file__))

config_path = os.path.join(current_dir, '..', 'KN_Core', 'src', 'KnConfig.cs')
version_path = os.path.join(current_dir, '..', 'version')
updater_prog_path = os.path.join(current_dir, '..', 'KN_Updater', 'Program.cs')

def module_path(module):
    return os.path.join(current_dir, '..', module, 'Loader.cs')

def replace_version(file_path):
    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found = False
            found_patch = False
            found_client = False
            for line in old_file:
                if found and found_patch and found_client:
                    new_file.write(line)
                else:
                    if not found and ' Version = ' in line:
                        found = True
                        new_file.write(re.sub('\d{3}', version_int, line))
                    elif not found_client and ' ClientVersion = ' in line:
                        found_client = True
                        new_file.write(re.sub('\d{3}', client_version_int, line))
                    elif not found_patch and ' Patch = ' in line:
                        found_patch = True
                        new_file.write(re.sub('\d{1}', patch, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

def replace_config_version(file_path):
    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found_int = False
            found_patch = False
            found_string = False
            found_client = False
            for line in old_file:
                if found_int and found_patch and found_string and found_client:
                    new_file.write(line)
                else:
                    if not found_int and ' Version = ' in line:
                        found_int = True
                        new_file.write(re.sub('\d{3}', version_int, line))
                    elif not found_patch and ' Patch = ' in line:
                        found_patch = True
                        new_file.write(re.sub('\d{1}', patch, line))
                    elif not found_string and ' StringVersion = ' in line:
                        found_string = True
                        new_file.write(re.sub('\s*([\d.]+)', version_string, line))
                    elif not found_client and ' ClientVersion = ' in line:
                        found_client = True
                        new_file.write(re.sub('\d{3}', client_version_int, line))
                    else:
                        new_file.write(line) 

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

def replace_core_version(file_path):
    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found = False
            found_patch = False
            found_updater = False
            for line in old_file:
                if found and found_patch and found_updater:
                    new_file.write(line)
                else:
                    if not found and 'Version=' in line:
                        found = True
                        new_file.write(re.sub('\d{3}', version_int, line))
                    elif not found_patch and 'Patch=' in line:
                        found_patch = True
                        new_file.write(re.sub('\d{1}', patch, line))
                    elif not found_updater and 'Updater=' in line:
                        found_updater = True
                        new_file.write(re.sub('\d{2}', updater, line))
                    else:
                        new_file.write(line) 

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)

def replace_updater_version(file_path):
    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh,'w') as new_file:
        with open(file_path) as old_file:
            found = False
            for line in old_file:
                if found:
                    new_file.write(line)
                else:
                    if not found and ' Version = ' in line:
                        found = True
                        new_file.write(re.sub('\d{2}', updater, line))
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
replace_core_version(version_path)
replace_updater_version(updater_prog_path)