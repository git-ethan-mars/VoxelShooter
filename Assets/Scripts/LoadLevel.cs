using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    [SerializeField] private string fileName;
    [SerializeField] private GameObject cube;

    private void Awake()
    {
        var blocks = new Block[Level.Width * Level.Height * Level.Depth];
        blocks[0] = new Block {color = 1, isInvincible = false};
        blocks[1] = new Block {color = 1, isInvincible = false};
        LevelWriter.SaveLevel(fileName, blocks);
        blocks = LevelReader.ReadLevel(fileName);
        for (var i = 0; i < Level.Width; i++)
        {
            for (var j = 0; j < Level.Height; j++)
            {
                for (var k = 0; k < Level.Depth; k++)
                {
                    var block = blocks[i * Level.Height * Level.Depth + j * Level.Depth + k];
                    if (block.color == 0)
                        continue;
                    Instantiate(cube, new Vector3(i, j, k), Quaternion.identity);
                    Debug.Log(new Vector3(i,j,k));

                }
            }
        }
    }
}