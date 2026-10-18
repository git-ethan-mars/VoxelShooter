"""Find how a MagicaVoxel model lines up with the original (git HEAD) character mesh.

Mapping per axis k: mesh[k] = sign[k] * vox[k] * S + offset[k]; every sign combination is tried and
the one that puts the most mesh vertices on corners of occupied voxels wins."""
import sys, os, itertools, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from vox import read_vox
from split import load_original

S = 0.01


def fit(name, vox_path):
    m = load_original(name)
    P = [m.pos(i) for i in range(m.vc)]
    size, vl, pal = read_vox(vox_path)
    occ = set((v[0], v[1], v[2]) for v in vl)
    vlo = [min(v[k] for v in vl) for k in range(3)]; vhi = [max(v[k] for v in vl) + 1 for k in range(3)]
    lo = [min(p[k] for p in P) for k in range(3)]; hi = [max(p[k] for p in P) for k in range(3)]
    best = None
    for sign in itertools.product((1, -1), repeat=3):
        # mesh lo corresponds to vox lo when sign > 0, to vox hi when sign < 0
        off = [lo[k] - (vlo[k] if sign[k] > 0 else vhi[k]) * S * sign[k] for k in range(3)]
        good = 0
        for p in P[::7]:
            v = [(p[k] - off[k]) / (S * sign[k]) for k in range(3)]
            r = [round(c) for c in v]
            if max(abs(v[k] - r[k]) for k in range(3)) < 0.05 and any(
                    (r[0] - a, r[1] - b, r[2] - c) in occ for a in (0, 1) for b in (0, 1) for c in (0, 1)):
                good += 1
        score = good / len(P[::7])
        if best is None or score > best[0]:
            best = (score, sign, off)
    score, sign, off = best
    print(f'{name}: sign {sign} offset {[round(o, 5) for o in off]} vertices on voxel corners {score:.3f}')
    return {'sign': sign, 'offset': off, 'score': score}


if __name__ == '__main__':
    res = {}
    for arg in sys.argv[1:]:
        name, path = arg.split('=')
        res[name] = fit(name, path)
    json.dump(res, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'fit.json'), 'w'), indent=1)
