import re
def prefab_bones(path):
    txt = open(path).read()
    docs = re.split(r"^--- !u!(\d+) &(-?\d+).*$", txt, flags=re.M)
    objs = {}
    for i in range(1, len(docs), 3):
        objs[docs[i+1]] = (docs[i], docs[i+2])
    go_name = {fid: (re.search(r"m_Name: (.*)", b) or re.search("(x)","x")).group(1) for fid,(t,b) in objs.items() if t == '1'}
    tr = {}
    for fid,(t,b) in objs.items():
        if t == '4' and 'm_GameObject' in b and 'm_LocalPosition' in b:
            g = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", b).group(1)
            pos = re.search(r"m_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", b).groups()
            par = re.search(r"m_Father: \{fileID: (-?\d+)\}", b).group(1)
            tr[fid] = (go_name.get(g), tuple(map(float,pos)), par)
    smr = [b for t,b in objs.values() if t == '137'][0]
    ids = re.findall(r"- \{fileID: (-?\d+)\}", smr.split("m_Bones:")[1].split("m_BlendShapeWeights")[0])
    return [tr[i][0] for i in ids], tr
