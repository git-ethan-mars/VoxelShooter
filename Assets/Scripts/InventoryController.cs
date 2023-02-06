using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<GameObject> blocks;
    [SerializeField] private List<Image> slots;
    [SerializeField] private List<GameObject> selectSlot;
    private int _itemIndex;
    private void Start()
    {
        for (var i = 0; i< blocks.Count; i++)
        {
            slots[i].sprite = blocks[i].GetComponent<Cube>().Avatar;
        }
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
        {
            selectSlot[_itemIndex].SetActive(false);
            _itemIndex = (_itemIndex + 1) % blocks.Count;
            selectSlot[_itemIndex].SetActive(true);
        }
    }
}