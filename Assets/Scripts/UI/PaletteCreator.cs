using System.Collections;
using System.Collections.Generic;
using GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PaletteCreator : MonoBehaviour
    {
        [SerializeField] private Image pointer;
        [SerializeField] private Sprite blackBoarder;
        [SerializeField] private Sprite blueBoarder;
        private int _indexX;
        private int _indexY;
        private const int PaletteSize = 8;
        private Dictionary<byte, Color> _blockColorById;

        private void Start()
        {
            var i = 0;
            _blockColorById = BlockColor.GetBlockColorDictionary();
            foreach (var color in _blockColorById.Values)
            {
                transform.GetChild(i).GetComponent<Image>().color = color;
                i++;
            }

            GlobalEvents.SendPaletteUpdate((byte)(_indexX + _indexY * PaletteSize + 1));
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow) &&
                !Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKeyDown(KeyCode.DownArrow)) return;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                if (_indexX > 0)
                    _indexX--;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                if (_indexX < PaletteSize - 1)
                    _indexX++;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_indexY > 0) 
                    _indexY--;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (_indexY < PaletteSize - 1) 
                    _indexY++;
            }

            pointer.transform.position = transform.GetChild(_indexX + _indexY * PaletteSize).position;
            GlobalEvents.SendPaletteUpdate((byte)(_indexX + _indexY * PaletteSize + 1));
        }

        private IEnumerator ChangePointerColor()
        {
            while (true)
            {
                pointer.sprite = blackBoarder;
                yield return new WaitForSeconds(0.5f);
                pointer.sprite = blueBoarder;
                yield return new WaitForSeconds(0.5f);
            }
        }


        private void OnEnable()
        {
            StartCoroutine(ChangePointerColor());
        }
    }
}