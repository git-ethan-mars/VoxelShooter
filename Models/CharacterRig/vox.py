import struct


def read_vox(path):
    d = open(path, 'rb').read()
    assert d[:4] == b'VOX '
    i = 8 + 12  # skip MAIN header
    size, voxels, palette = None, [], None
    while i < len(d):
        cid = d[i:i + 4]
        n, _ = struct.unpack('<ii', d[i + 4:i + 12])
        body = d[i + 12:i + 12 + n]
        if cid == b'SIZE':
            size = struct.unpack('<iii', body[:12])
        elif cid == b'XYZI':
            cnt = struct.unpack('<i', body[:4])[0]
            voxels = [tuple(body[4 + 4 * k:8 + 4 * k]) for k in range(cnt)]
        elif cid == b'RGBA':
            palette = [tuple(body[4 * k:4 * k + 4]) for k in range(256)]
        i += 12 + n
    return size, voxels, palette


def to_unity(vx, vy, vz, yflip):
    """MagicaVoxel voxel corner coords -> Unity Sniper.mesh space (0.01 per voxel)."""
    x = -(vx - 108) * 0.01
    y = ((34 - vy) if yflip else (vy - 34)) * 0.01
    z = -vz * 0.01
    return x, y, z
