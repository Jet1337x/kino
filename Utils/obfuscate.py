def obfuscate(project_path):
    import subprocess

    print('Running obfuscator ...')
    subprocess.run(f'confuser/Confuser.CLI.exe {project_path} -n')
