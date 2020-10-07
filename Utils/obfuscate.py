def obfuscate(utils_path, project_path):
    import subprocess

    print('Running obfuscator ...')
    subprocess.run(f'{utils_path}/confuser/Confuser.CLI.exe {project_path} -n')
