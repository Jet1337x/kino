def replace_mod_version(file_path):
    import re
    from tempfile import mkstemp
    from shutil import move, copymode
    from os import fdopen, remove
    import version

    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh, 'w') as new_file:
        with open(file_path) as old_file:
            found = False
            found_patch = False
            found_client = False
            found_string = False
            for line in old_file:
                if found and found_patch and found_client and found_string:
                    new_file.write(line)
                else:
                    if not found and (' Version = ' in line or ' ModVersion = ' in line):
                        found = True
                        new_file.write(re.sub('\d{3}', version.version_int, line))
                    elif not found_client and ' ClientVersion = ' in line:
                        found_client = True
                        new_file.write(re.sub('\d{3}', version.client_version_int, line))
                    elif not found_patch and ' Patch = ' in line:
                        found_patch = True
                        new_file.write(re.sub('\d{1}', version.patch, line))
                    elif not found_string and ' StringVersion = ' in line:
                        found_string = True
                        new_file.write(re.sub('\s*([\d.]+)', version.version_string, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)


def replace_version(file_path):
    import re
    from tempfile import mkstemp
    from shutil import move, copymode
    from os import fdopen, remove
    import version

    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh, 'w') as new_file:
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
                        new_file.write(re.sub('\d{3}', version.version_int, line))
                    elif not found_patch and 'Patch=' in line:
                        found_patch = True
                        new_file.write(re.sub('\d{1}', version.patch, line))
                    elif not found_updater and 'Updater=' in line:
                        found_updater = True
                        new_file.write(re.sub('\d{2}', version.updater, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)


def replace_updater_version(file_path):
    import re
    from tempfile import mkstemp
    from shutil import move, copymode
    from os import fdopen, remove
    import version

    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh, 'w') as new_file:
        with open(file_path) as old_file:
            found = False
            for line in old_file:
                if found:
                    new_file.write(line)
                else:
                    if not found and ' Version = ' in line:
                        found = True
                        new_file.write(re.sub('\d{2}', version.updater, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)


def replace_assembly_version(file_path):
    import re
    from tempfile import mkstemp
    from shutil import move, copymode
    from os import fdopen, remove
    import version

    print('Updating version for: ' + file_path)
    fh, abs_path = mkstemp()
    with fdopen(fh, 'w') as new_file:
        with open(file_path) as old_file:
            found = False
            found_file = False
            for line in old_file:
                if found and found_file:
                    new_file.write(line)
                else:
                    if not found and 'assembly: AssemblyVersion' in line and '//' not in line:
                        found = True
                        new_file.write(re.sub('\s*([\d.]+)', version.version_string_long, line))
                    elif not found_file and 'assembly: AssemblyFileVersion' in line and '//' not in line:
                        found_file = True
                        new_file.write(re.sub('\s*([\d.]+)', version.version_string_long, line))
                    else:
                        new_file.write(line)

    copymode(file_path, abs_path)
    remove(file_path)
    move(abs_path, file_path)


def main():
    import os

    current_dir = os.path.dirname(os.path.abspath(__file__))

    modules = [
        os.path.join(current_dir, '..', 'KN_Core', 'src', 'Core.cs'),
        os.path.join(current_dir, '..', 'KN_Loader', 'ModLoader.cs'),
        os.path.join(current_dir, '..', 'KN_Lights', 'Loader.cs'),
        os.path.join(current_dir, '..', 'KN_Maps', 'Loader.cs'),
        os.path.join(current_dir, '..', 'KN_Visuals', 'Loader.cs')
        os.path.join(current_dir, '..', 'KN_Cinematic', 'Loader.cs')
    ]

    assemblies = [
        os.path.join(current_dir, '..', 'KN_Loader', 'Properties', 'AssemblyInfo.cs'),
        os.path.join(current_dir, '..', 'KN_Core', 'Properties', 'AssemblyInfo.cs'),
        os.path.join(current_dir, '..', 'KN_Lights', 'Properties', 'AssemblyInfo.cs'),
        os.path.join(current_dir, '..', 'KN_Maps', 'Properties', 'AssemblyInfo.cs'),
        os.path.join(current_dir, '..', 'KN_Visuals', 'Properties', 'AssemblyInfo.cs')
        os.path.join(current_dir, '..', 'KN_Cinematic', 'Properties', 'AssemblyInfo.cs')
    ]

    for m in modules:
        replace_mod_version(m)

    for a in assemblies:
        replace_assembly_version(a)

    replace_version(os.path.join(current_dir, '..', 'version'))
    replace_updater_version(os.path.join(current_dir, '..', 'KN_Updater', 'Program.cs'))


if __name__ == "__main__":
    main()
