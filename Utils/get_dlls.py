import os


def get_dlls():
    current_dir = os.path.dirname(os.path.abspath(__file__))
    to_copy = os.path.join(current_dir, 'to_copy.txt')

    with open(to_copy, 'r') as f:
        to_copy_dlls_raw = f.readlines()

    to_copy_dlls = []
    for dll in to_copy_dlls_raw:
        d = dll.rstrip('\n')
        to_copy_dlls.append(d)

    return to_copy_dlls
