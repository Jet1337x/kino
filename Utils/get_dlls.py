def get_dlls(to_copy_path):
    with open(to_copy_path, 'r') as f:
        to_copy_dlls_raw = f.readlines()

    to_copy_dlls = []
    for dll in to_copy_dlls_raw:
        d = dll.rstrip('\n')
        to_copy_dlls.append(d)

    return to_copy_dlls
