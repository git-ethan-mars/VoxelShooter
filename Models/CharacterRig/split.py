"""Detach the head from the body at the neck and cap both openings.

Works on the original (git HEAD) meshes: the head (everything above the head bone
origin that the head already influenced) becomes a rigid, closed piece skinned 100% to
`head`; the body loses all head influence and gets a lid at the neck. Rotating the head
then moves a closed block instead of stretching the seam triangles."""
import sys, os, re, struct, math, collections, subprocess
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from meshio import Mesh, NAMES, BASE

REPO = BASE.rsplit('Assets/', 1)[0]
DRY = '--dry' in sys.argv


def load_original(name):
    m = Mesh(name)
    m.txt = subprocess.run(['git', '-C', REPO, 'show', f'HEAD:Assets/Meshes/{name}/{name}.mesh'],
                           capture_output=True, check=True).stdout.decode()
    m.m = re.search(r"_typelessdata: ([0-9a-f]+)", m.txt)
    m.data = bytearray(bytes.fromhex(m.m.group(1)))
    m.vc = int(re.search(r"m_VertexData:\s+serializedVersion: 3\s+m_VertexCount: (\d+)", m.txt).group(1))
    m.st1 = 12 if re.search(r"stream: 1\s+offset: 4", m.txt) else 8
    m.s1 = (m.vc * 40 + 15) // 16 * 16
    m.s2 = (m.s1 + m.vc * m.st1 + 15) // 16 * 16
    assert m.s2 + m.vc * 32 == len(m.data)
    return m


def _unused_ear_clip(poly2d):
    """Triangulate a simple polygon given as list of (x, y); returns index triples (CCW input expected)."""
    idx = list(range(len(poly2d)))
    def area(a, b, c):
        return (b[0] - a[0]) * (c[1] - a[1]) - (b[1] - a[1]) * (c[0] - a[0])
    def inside(p, a, b, c):
        return area(a, b, p) > 1e-12 and area(b, c, p) > 1e-12 and area(c, a, p) > 1e-12
    tris, guard = [], 0
    while len(idx) > 3 and guard < 100000:
        guard += 1
        n = len(idx)
        for k in range(n):
            i0, i1, i2 = idx[k - 1], idx[k], idx[(k + 1) % n]
            a, b, c = poly2d[i0], poly2d[i1], poly2d[i2]
            ar = area(a, b, c)
            if ar < -1e-12:
                continue
            if ar <= 1e-12:  # collinear: drop the middle vertex
                idx.pop(k)
                break
            if any(inside(poly2d[j], a, b, c) for j in idx if j not in (i0, i1, i2)):
                continue
            tris.append((i0, i1, i2))
            idx.pop(k)
            break
        else:
            return None
    if len(idx) == 3:
        tris.append(tuple(idx))
    return tris


def process(name):
    m = load_original(name)
    H = m.bones.index('head')
    SP = m.bones.index('spine')
    ib = re.search(r"m_IndexBuffer: ([0-9a-f]+)", m.txt).group(1)
    idx = list(struct.unpack(f'<{len(ib) // 4}H', bytes.fromhex(ib)))
    key = lambda i: tuple(round(c, 5) for c in m.pos(i))
    hp = lambda i: m.local('head', m.pos(i))

    # --- classify vertices (same rule as before: head-influenced and above the neck pivot) ---
    groups = collections.defaultdict(list)
    for i in range(m.vc):
        groups[key(i)].append(i)
    vhead = [False] * m.vc
    for k, ids in groups.items():
        hw = sum(sum(w for w, b in zip(*m.skin(i)) if b == H) for i in ids) / len(ids)
        if hw >= 0.3 and m.local('head', k)[1] > 0.005:
            for i in ids:
                vhead[i] = True

    # --- classify triangles by majority ---
    ntri = len(idx) // 3
    thead = [sum(vhead[v] for v in idx[3 * t:3 * t + 3]) >= 2 for t in range(ntri)]

    # --- vertex records ---
    rec0 = [bytes(m.data[i * 40:(i + 1) * 40]) for i in range(m.vc)]
    rec1 = [bytes(m.data[m.s1 + i * m.st1:m.s1 + (i + 1) * m.st1]) for i in range(m.vc)]
    skins = [m.skin(i) for i in range(m.vc)]

    def head_skin():
        return ([1.0, 0, 0, 0], [H, 0, 0, 0])

    def body_skin(i):
        w, ix = skins[i]
        pairs = [(w[k], ix[k]) for k in range(4) if w[k] > 0 and ix[k] != H]
        if not pairs:
            pairs = [(1.0, SP)]
        s = sum(p[0] for p in pairs)
        pairs = sorted(((a / s, b) for a, b in pairs), reverse=True) + [(0.0, 0)] * (4 - len(pairs))
        return ([p[0] for p in pairs], [p[1] for p in pairs])

    out0, out1, outs = list(rec0), list(rec1), [None] * m.vc
    for i in range(m.vc):
        outs[i] = head_skin() if vhead[i] else body_skin(i)

    # duplicate vertices that end up on the other side of their triangle's piece
    dup = {}
    def side_vertex(i, head):
        if vhead[i] == head:
            return i
        if (i, head) not in dup:
            dup[(i, head)] = len(out0)
            out0.append(rec0[i]); out1.append(rec1[i])
            outs.append(head_skin() if head else body_skin(i))
        return dup[(i, head)]
    new_idx = []
    for t in range(ntri):
        new_idx += [side_vertex(v, thead[t]) for v in idx[3 * t:3 * t + 3]]

    # --- winding convention: sign of cross(b-a, c-a) . vertex normal for existing faces ---
    def P(v): return struct.unpack_from('<3f', out0[v], 0)
    def N(v): return struct.unpack_from('<3f', out0[v], 12)
    def cross(u, v): return (u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0])
    def sub(a, b): return tuple(x - y for x, y in zip(a, b))
    def dot(a, b): return sum(x * y for x, y in zip(a, b))
    conv = 0
    for t in range(0, len(new_idx), 3):
        a, b, c = new_idx[t:t + 3]
        conv += 1 if dot(cross(sub(P(b), P(a)), sub(P(c), P(a))), N(a)) > 0 else -1
    conv = 1 if conv > 0 else -1

    # head "up" axis in mesh space = row 1 of the head bind pose rotation
    bp = m.bp[H]
    up = (bp[4], bp[5], bp[6])

    # --- boundary loops of each piece at the seam ---
    seam_keys = set()
    pieces = {True: collections.defaultdict(int), False: collections.defaultdict(int)}
    owner = {True: {}, False: {}}
    for t in range(ntri):
        vs = new_idx[3 * t:3 * t + 3]
        for a, b in ((vs[0], vs[1]), (vs[1], vs[2]), (vs[2], vs[0])):
            ka, kb = key_of(out0, a), key_of(out0, b)
            e = (ka, kb) if ka < kb else (kb, ka)
            pieces[thead[t]][e] += 1
            owner[thead[t]].setdefault(ka, a); owner[thead[t]].setdefault(kb, b)
    keys_head = {k for e in pieces[True] for k in e}
    keys_body = {k for e in pieces[False] for k in e}
    seam_keys = keys_head & keys_body

    caps = 0
    report = []
    for head in (True, False):
        bedges = [e for e, c in pieces[head].items() if c == 1 and e[0] in seam_keys and e[1] in seam_keys]
        ring = sorted({k for e in bedges for k in e})
        if len(ring) < 3:
            continue
        local = {k: m.local('head', k) for k in ring}
        hull = convex_hull([(local[k][0], local[k][2]) for k in ring])
        ys = [local[k][1] for k in ring]
        # head lid at the lowest seam point, body lid at the highest: both stay inside the other piece at rest
        y = min(ys) if head else max(ys)
        normal = tuple(-c for c in up) if head else up
        inv = invert_bindpose(m.bp[H])
        verts = []
        for hx, hz in hull:
            p = apply(inv, (hx, y, hz))
            near = min(ring, key=lambda k: (local[k][0] - hx) ** 2 + (local[k][2] - hz) ** 2 + (local[k][1] - y) ** 2)
            src_v = owner[head][near]
            r0 = bytearray(out0[src_v])
            struct.pack_into('<3f', r0, 0, *p)
            struct.pack_into('<3f', r0, 12, *normal)
            verts.append(len(out0))
            out0.append(bytes(r0)); out1.append(out1[src_v]); outs.append(outs[src_v])
        for j in range(1, len(verts) - 1):
            va, vb, vc_ = verts[0], verts[j], verts[j + 1]
            if dot(cross(sub(P(vb), P(va)), sub(P(vc_), P(va))), normal) * conv < 0:
                vb, vc_ = vc_, vb
            new_idx += [va, vb, vc_]
        caps += 1
        xs = [h[0] for h in hull]; zs = [h[1] for h in hull]
        report.append(f"{'head' if head else 'body'} lid: {len(ring)} seam pts, hull {len(hull)}, y={y:.3f} (seam y {min(ys):.3f}..{max(ys):.3f}), "
                      f"x {min(xs):.3f}..{max(xs):.3f}, z {min(zs):.3f}..{max(zs):.3f}")

    nv = len(out0)
    assert nv < 65536
    print(f"{name}: {m.vc} -> {nv} verts, {ntri} -> {len(new_idx) // 3} tris, seam dups {len(dup)}, caps {caps}")
    for r in report:
        print("   ", r)
    if DRY:
        return

    # --- write back ---
    s1 = (nv * 40 + 15) // 16 * 16
    s2 = (s1 + nv * m.st1 + 15) // 16 * 16
    data = bytearray(s2 + nv * 32)
    for i in range(nv):
        data[i * 40:(i + 1) * 40] = out0[i]
        data[s1 + i * m.st1:s1 + (i + 1) * m.st1] = out1[i]
        struct.pack_into('<4f', data, s2 + i * 32, *outs[i][0])
        struct.pack_into('<4I', data, s2 + i * 32 + 16, *outs[i][1])
    t = m.txt
    t = re.sub(r"(m_SubMeshes:\s+- serializedVersion: 2\s+firstByte: 0\s+indexCount: )\d+", lambda g: g.group(1) + str(len(new_idx)), t)
    t = re.sub(r"(baseVertex: 0\s+firstVertex: 0\s+vertexCount: )\d+", lambda g: g.group(1) + str(nv), t)
    t = re.sub(r"(m_IndexBuffer: )[0-9a-f]+", lambda g: g.group(1) + struct.pack(f'<{len(new_idx)}H', *new_idx).hex(), t)
    t = re.sub(r"(m_VertexData:\s+serializedVersion: 3\s+m_VertexCount: )\d+", lambda g: g.group(1) + str(nv), t)
    t = re.sub(r"(m_DataSize: )\d+", lambda g: g.group(1) + str(len(data)), t, count=1)
    t = re.sub(r"(_typelessdata: )[0-9a-f]+", lambda g: g.group(1) + data.hex(), t)
    path = BASE + f"Meshes/{name}/{name}.mesh"
    open(path, 'w', newline='').write(t)


def convex_hull(pts):
    pts = sorted(set((round(x, 6), round(y, 6)) for x, y in pts))
    def cr(o, a, b): return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])
    lo, hi = [], []
    for p in pts:
        while len(lo) >= 2 and cr(lo[-2], lo[-1], p) <= 0: lo.pop()
        lo.append(p)
    for p in reversed(pts):
        while len(hi) >= 2 and cr(hi[-2], hi[-1], p) <= 0: hi.pop()
        hi.append(p)
    return lo[:-1] + hi[:-1]


def invert_bindpose(bp):
    r = [[bp[0], bp[1], bp[2]], [bp[4], bp[5], bp[6]], [bp[8], bp[9], bp[10]]]
    t = [bp[3], bp[7], bp[11]]
    rt = [[r[j][i] for j in range(3)] for i in range(3)]  # orthonormal rotation
    return rt, [-sum(rt[i][k] * t[k] for k in range(3)) for i in range(3)]


def apply(inv, p):
    rt, t = inv
    return tuple(sum(rt[i][k] * p[k] for k in range(3)) + t[i] for i in range(3))


def key_of(out0, v):
    return tuple(round(c, 5) for c in struct.unpack_from('<3f', out0[v], 0))


if __name__ == '__main__':
    for n in (sys.argv[1].split(',') if len(sys.argv) > 1 and not sys.argv[1].startswith('--') else NAMES):
        process(n)
