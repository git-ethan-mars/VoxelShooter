import re, struct, sys, collections
sys.path.insert(0, __file__.rsplit('\\', 1)[0])
from bones import prefab_bones

BASE = r"C:/Users/diman/Desktop/Projects/VoxelShooter/Assets/"
NAMES = ['Builder', 'Combatant', 'Grenadier', 'Sniper']


class Mesh:
    def __init__(self, name):
        self.name = name
        self.path = BASE + f"Meshes/{name}/{name}.mesh"
        self.bones, _ = prefab_bones(BASE + "Resources/Prefabs/Entities/Characters/" + name + ".prefab")
        self.txt = open(self.path, newline='').read()
        self.vc = int(re.search(r"m_VertexData:\s+serializedVersion: 3\s+m_VertexCount: (\d+)", self.txt).group(1))
        self.m = re.search(r"_typelessdata: ([0-9a-f]+)", self.txt)
        self.data = bytearray(bytes.fromhex(self.m.group(1)))
        bp = re.findall(r"- e00: (\S+)\s+e01: (\S+)\s+e02: (\S+)\s+e03: (\S+)\s+e10: (\S+)\s+e11: (\S+)\s+e12: (\S+)\s+e13: (\S+)\s+e20: (\S+)\s+e21: (\S+)\s+e22: (\S+)\s+e23: (\S+)", self.txt)
        self.bp = [list(map(float, b)) for b in bp]
        st1 = 12 if re.search(r"stream: 1\s+offset: 4", self.txt) else 8
        s1off = (self.vc * 40 + 15) // 16 * 16
        self.s2 = (s1off + self.vc * st1 + 15) // 16 * 16
        assert self.s2 + self.vc * 32 == len(self.data), self.name

    def pos(self, i):
        return struct.unpack_from('<3f', self.data, i * 40)

    def skin(self, i):
        o = self.s2 + i * 32
        return list(struct.unpack_from('<4f', self.data, o)), list(struct.unpack_from('<4I', self.data, o + 16))

    def set_skin(self, i, w, ix):
        o = self.s2 + i * 32
        struct.pack_into('<4f', self.data, o, *w)
        struct.pack_into('<4I', self.data, o + 16, *ix)

    def local(self, bone, v):
        m = self.bp[self.bones.index(bone)]
        return [m[4 * r] * v[0] + m[4 * r + 1] * v[1] + m[4 * r + 2] * v[2] + m[4 * r + 3] for r in range(3)]

    def save(self):
        t = self.txt[:self.m.start(1)] + self.data.hex() + self.txt[self.m.end(1):]
        open(self.path, 'w', newline='').write(t)
