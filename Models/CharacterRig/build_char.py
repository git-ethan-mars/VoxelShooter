"""Rebuild a character .mesh from its MagicaVoxel source with rigid, closed body parts (generalised Sniper rebuild).

Each voxel is assigned to one bone by cutting planes through the joints; faces are
emitted wherever a voxel borders empty space or a voxel of another part, so every part
is a closed block. Faces are greedy-merged per part / direction / colour. Bones and bind
poses are taken unchanged from the existing Sniper.mesh, so the prefab, rig and
animations keep working."""
import sys, os, re, struct, math, collections, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from vox import read_vox
from split import load_original, invert_bindpose, apply
from meshio import BASE

HERE = os.path.dirname(os.path.abspath(__file__))
S = 0.01
NAME = sys.argv[1]
VOX = sys.argv[2]
FIT = json.load(open(os.path.join(HERE, 'fit.json')))[NAME]
SIGN, OFF = FIT['sign'], FIT['offset']


def corner(vx, vy, vz):
    v = (vx, vy, vz)
    return tuple(SIGN[k] * v[k] * S + OFF[k] for k in range(3))


def original_bones(m):
    """Dominant bone of every original vertex, keyed by its voxel-grid corner."""
    grid = collections.defaultdict(collections.Counter)
    for i in range(m.vc):
        p = m.pos(i)
        v = tuple(round((p[k] - OFF[k]) / (S * SIGN[k])) for k in range(3))
        w, ix = m.skin(i)
        grid[v][m.bones[ix[max(range(4), key=lambda k: w[k])]]] += 1
    return grid


def segment(m, voxels):
    origin = {}
    for b, bp in zip(m.bones, m.bp):
        origin[b] = apply(invert_bindpose(bp), (0, 0, 0))
    def dirn(a, b):
        d = [origin[b][k] - origin[a][k] for k in range(3)]
        l = math.sqrt(sum(c * c for c in d))
        return [c / l for c in d]
    def past(p, joint, d):
        return sum((p[k] - origin[joint][k]) * d[k] for k in range(3)) >= 0
    arm_x = abs(origin['upper_arm.L'][0])
    hip_z = origin['thigh.L'][2]
    neck_z = origin['head'][2]
    chains = {}
    for s in ('L', 'R'):
        chains['arm' + s] = [(f'forearm.{s}', dirn(f'upper_arm.{s}', f'forearm.{s}')),
                             (f'hand.{s}', dirn(f'forearm.{s}', f'hand.{s}'))]
        chains['leg' + s] = [(f'shin.{s}', dirn(f'thigh.{s}', f'shin.{s}')),
                             (f'foot.{s}', dirn(f'shin.{s}', f'foot.{s}'))]
    sideL = origin['upper_arm.L'][0] < 0  # which x sign is the .L side
    up = 1 if origin['head'][2] > origin['thigh.L'][2] else -1  # direction of mesh z that points up
    arm_level = (hip_z + neck_z) / 2
    part = {}
    for (vx, vy, vz) in voxels:
        c = corner(vx + 0.5, vy + 0.5, vz + 0.5)  # voxel centre
        s = 'L' if (c[0] < 0) == sideL else 'R'
        if abs(c[0]) > arm_x and (c[2] - arm_level) * up > 0:  # arms stick out sideways at shoulder height
            b = f'upper_arm.{s}'
            for joint, d in chains['arm' + s]:
                if past(c, joint, d):
                    b = joint
        elif (c[2] - hip_z) * up < 0:  # below the hips
            b = f'thigh.{s}'
            for joint, d in chains['leg' + s]:
                if past(c, joint, d):
                    b = joint
        elif (c[2] - neck_z) * up > 0:  # above the neck
            b = 'head'
        else:
            b = 'spine'
        part[(vx, vy, vz)] = m.bones.index(b)

    # Things carried behind the head (a rolled mat on a backpack) rise above the neck too. They only touch the head
    # along thin seams, so eroding the head voxels splits them off; every big piece other than the head stays with
    # the body, and the eroded voxels go back to the nearest piece
    H, SP = m.bones.index('head'), m.bones.index('spine')
    head = {p for p, b in part.items() if b == H}
    N6 = ((1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1))
    core = head
    for _ in range(2):
        core = {p for p in core if all((p[0] + d[0], p[1] + d[1], p[2] + d[2]) in core for d in N6)}
    label, sizes = {}, []
    for p in core:
        if p in label:
            continue
        stack, n = [p], 0
        label[p] = len(sizes)
        while stack:
            q = stack.pop(); n += 1
            for d in N6:
                r = (q[0] + d[0], q[1] + d[1], q[2] + d[2])
                if r in core and r not in label:
                    label[r] = len(sizes); stack.append(r)
        sizes.append(n)
    main = max(range(len(sizes)), key=lambda k: sizes[k])
    queue = collections.deque(label)
    while queue:
        q = queue.popleft()
        for d in N6:
            r = (q[0] + d[0], q[1] + d[1], q[2] + d[2])
            if r in head and r not in label:
                label[r] = label[q]; queue.append(r)
    detached = {k for k, n in enumerate(sizes) if k != main and n >= 200}
    moved = 0
    for p in head:
        if label.get(p, main) in detached:
            part[p] = SP; moved += 1
    print('head pieces after erosion', sorted(sizes, reverse=True)[:5], '-> voxels kept with the body:', moved)
    return part


DIRS = [((1, 0, 0), 0), ((-1, 0, 0), 0), ((0, 1, 0), 1), ((0, -1, 0), 1), ((0, 0, 1), 2), ((0, 0, -1), 2)]


def build(m, voxels, part):
    """Greedy meshing. Returns list of quads: (corners[4] in vox coords, outward dir, colour, bone)."""
    quads = []
    for d, axis in DIRS:
        u_ax, v_ax = [a for a in range(3) if a != axis]
        slices = collections.defaultdict(dict)
        for p, col in voxels.items():
            n = (p[0] + d[0], p[1] + d[1], p[2] + d[2])
            if n in voxels and part[n] == part[p]:
                continue
            slices[p[axis]][(p[u_ax], p[v_ax])] = (col, part[p])
        for layer, cells in slices.items():
            done = set()
            for (u, v) in sorted(cells):
                if (u, v) in done:
                    continue
                key = cells[(u, v)]
                w = 1
                while (u + w, v) in cells and (u + w, v) not in done and cells[(u + w, v)] == key:
                    w += 1
                h = 1
                while all((u + k, v + h) in cells and (u + k, v + h) not in done and cells[(u + k, v + h)] == key
                          for k in range(w)):
                    h += 1
                for a in range(w):
                    for b in range(h):
                        done.add((u + a, v + b))
                plane = layer + (1 if d[axis] > 0 else 0)
                cs = []
                for (du, dv) in ((0, 0), (w, 0), (w, h), (0, h)):
                    c = [0, 0, 0]
                    c[axis] = plane; c[u_ax] = u + du; c[v_ax] = v + dv
                    cs.append(tuple(c))
                quads.append((cs, d, key[0], key[1]))
    return quads


def main():
    m = load_original(NAME)
    size, vox_list, palette = read_vox(VOX)
    voxels = {(x, y, z): c for x, y, z, c in vox_list}
    part = segment(m, voxels)
    counts = collections.Counter(m.bones[b] for b in part.values())
    print('voxels per part:', dict(counts))
    quads = build(m, voxels, part)
    print('quads', len(quads))

    # winding convention of the original mesh (sign of cross . normal)
    ib = re.search(r"m_IndexBuffer: ([0-9a-f]+)", m.txt).group(1)
    oidx = struct.unpack(f'<{len(ib) // 4}H', bytes.fromhex(ib))
    conv = 0
    for t in range(0, len(oidx), 3):
        a, b, c = (m.pos(i) for i in oidx[t:t + 3])
        n = struct.unpack_from('<3f', m.data, oidx[t] * 40 + 12)
        u = [b[k] - a[k] for k in range(3)]; v = [c[k] - a[k] for k in range(3)]
        cr = (u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0])
        conv += 1 if sum(cr[k] * n[k] for k in range(3)) > 0 else -1
    conv = 1 if conv > 0 else -1

    pos, nrm, tan, uv, bone, idx = [], [], [], [], [], []
    for cs, d, col, b in quads:
        n = tuple(SIGN[k] * d[k] for k in range(3))  # outward direction mapped into mesh space
        t = (0.0, 0.0, 1.0, 1.0) if abs(n[2]) < 0.5 else (1.0, 0.0, 0.0, 1.0)
        u = ((col - 1) + 0.5) / 256.0
        base = len(pos)
        P = [corner(*c) for c in cs]
        for p in P:
            pos.append(p); nrm.append(n); tan.append(t); uv.append((u, 0.5)); bone.append(b)
        e1 = [P[1][k] - P[0][k] for k in range(3)]; e2 = [P[2][k] - P[0][k] for k in range(3)]
        cr = (e1[1] * e2[2] - e1[2] * e2[1], e1[2] * e2[0] - e1[0] * e2[2], e1[0] * e2[1] - e1[1] * e2[0])
        if sum(cr[k] * n[k] for k in range(3)) * conv > 0:
            idx += [base, base + 1, base + 2, base, base + 2, base + 3]
        else:
            idx += [base, base + 2, base + 1, base, base + 3, base + 2]
    nv = len(pos)
    print('vertices', nv, 'triangles', len(idx) // 3)
    write_mesh(m, pos, nrm, tan, uv, bone, idx)
    write_palette(palette)


def fmt(v):
    s = repr(float(v))
    return '0' if s in ('0.0', '-0.0') else (s.rstrip('0').rstrip('.') if 'e' not in s else s)


def write_mesh(m, pos, nrm, tan, uv, bone, idx):
    nv = len(pos)
    s1 = (nv * 40 + 15) // 16 * 16
    st1 = m.st1  # 12 when the second stream also carries a (white) vertex colour before the UV
    s2 = (s1 + nv * st1 + 15) // 16 * 16
    data = bytearray(s2 + nv * 32)
    for i in range(nv):
        struct.pack_into('<3f3f4f', data, i * 40, *pos[i], *nrm[i], *tan[i])
        if st1 == 12:
            struct.pack_into('<4B2f', data, s1 + i * 12, 255, 255, 255, 255, *uv[i])
        else:
            struct.pack_into('<2f', data, s1 + i * 8, *uv[i])
        struct.pack_into('<4f4I', data, s2 + i * 32, 1.0, 0, 0, 0, bone[i], 0, 0, 0)
    wide = nv > 65535
    ibuf = struct.pack(f'<{len(idx)}{"I" if wide else "H"}', *idx)
    lo = [min(p[k] for p in pos) for k in range(3)]; hi = [max(p[k] for p in pos) for k in range(3)]
    ctr = [(lo[k] + hi[k]) / 2 for k in range(3)]; ext = [(hi[k] - lo[k]) / 2 for k in range(3)]
    aabb = f"m_Center: {{x: {fmt(ctr[0])}, y: {fmt(ctr[1])}, z: {fmt(ctr[2])}}}"
    aabb2 = f"m_Extent: {{x: {fmt(ext[0])}, y: {fmt(ext[1])}, z: {fmt(ext[2])}}}"
    # per-bone AABB in bone space
    nb = len(m.bones)
    blo = [[float('inf')] * 3 for _ in range(nb)]; bhi = [[float('-inf')] * 3 for _ in range(nb)]
    for p, b in zip(pos, bone):
        bp = m.bp[b]
        q = [bp[4 * r] * p[0] + bp[4 * r + 1] * p[1] + bp[4 * r + 2] * p[2] + bp[4 * r + 3] for r in range(3)]
        for k in range(3):
            blo[b][k] = min(blo[b][k], q[k]); bhi[b][k] = max(bhi[b][k], q[k])
    t = m.txt
    nl = '\r\n' if '\r\n' in t else '\n'
    section = t.split("m_BonesAABB:")[1].split("m_VariableBoneCountWeights")[0]
    stored = re.findall(r"  - m_Min: [^\r\n]*\r?\n    m_Max: [^\r\n]*\r?\n", section)
    parts = []
    for b in range(nb):
        if blo[b][0] == float('inf'):
            parts.append(f"  - m_Min: {{x: 0, y: 0, z: 0}}{nl}    m_Max: {{x: -0, y: -0, z: -0}}{nl}")
        else:
            parts.append(f"  - m_Min: {{x: {fmt(blo[b][0])}, y: {fmt(blo[b][1])}, z: {fmt(blo[b][2])}}}{nl}"
                         f"    m_Max: {{x: {fmt(bhi[b][0])}, y: {fmt(bhi[b][1])}, z: {fmt(bhi[b][2])}}}{nl}")
    assert len(stored) == nb
    t, c = re.subn(r"(  m_BonesAABB:\r?\n)(?:  - m_Min: [^\r\n]*\r?\n    m_Max: [^\r\n]*\r?\n)+", lambda g: g.group(1) + ''.join(parts), t); assert c == 1
    t, c = re.subn(r"(firstByte: 0\s+indexCount: )\d+", lambda g: g.group(1) + str(len(idx)), t); assert c == 1
    t, c = re.subn(r"(firstVertex: 0\s+vertexCount: )\d+", lambda g: g.group(1) + str(nv), t); assert c == 1
    t, c = re.subn(r"m_Center: \{[^}]*\}", aabb, t); assert c == 2
    t, c = re.subn(r"m_Extent: \{[^}]*\}", aabb2, t); assert c == 2
    t, c = re.subn(r"(m_IndexFormat: )\d", lambda g: g.group(1) + ('1' if wide else '0'), t); assert c == 1
    t, c = re.subn(r"(m_IndexBuffer: )[0-9a-f]+", lambda g: g.group(1) + ibuf.hex(), t); assert c == 1
    t, c = re.subn(r"(m_VertexData:\s+serializedVersion: 3\s+m_VertexCount: )\d+", lambda g: g.group(1) + str(nv), t); assert c == 1
    t, c = re.subn(r"(m_DataSize: )\d+", lambda g: g.group(1) + str(len(data)), t, count=1); assert c == 1
    t, c = re.subn(r"(_typelessdata: )[0-9a-f]+", lambda g: g.group(1) + data.hex(), t); assert c == 1
    open(BASE + f"Meshes/{NAME}/{NAME}.mesh", 'w', newline='').write(t)
    print(f'wrote {NAME}.mesh, index format', 'UInt32' if wide else 'UInt16')


def write_palette(palette):
    import zlib
    row = b'\0' + bytes(c for rgba in palette for c in rgba)
    def chunk(tg, d): return struct.pack('>I', len(d)) + tg + d + struct.pack('>I', zlib.crc32(tg + d))
    png = (b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', 256, 1, 8, 6, 0, 0, 0))
           + chunk(b'IDAT', zlib.compress(row)) + chunk(b'IEND', b''))
    open(BASE + f"Resources/Materials/{NAME}/{NAME}_Palette.png", 'wb').write(png)
    print(f'wrote {NAME}_Palette.png')


if __name__ == '__main__':
    main()
