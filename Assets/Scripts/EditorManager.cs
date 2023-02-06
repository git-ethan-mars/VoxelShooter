using UnityEngine;

public class EditorManager : MonoBehaviour
{
    [SerializeField] private string fileName;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject blockRendererPrefab;
    private Map Map { get; set; }
    private void Start()
    {
        //Map = MapReader.Read(fileName);
        //var blockRenderer = Instantiate(blockRendererPrefab);
        //blockRenderer.GetComponent<BlockRenderer>().Initialize(Map);
    }

    private void OnDestroy()
    {
        MapWriter.SaveMap(fileName, Map);
    }
}