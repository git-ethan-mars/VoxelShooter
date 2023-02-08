using System.Collections;
using GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PaletteCreator : MonoBehaviour
    {
        [SerializeField] private Texture2D colorAtlas;
        [SerializeField] private Image[] colorBlocks;
        [SerializeField] private Image pointer;
        [SerializeField] private Sprite blackBoarder;
        [SerializeField] private Sprite blueBoarder;
        private Image[,] _colorMatrix;
        private int indexX;
        private int indexY;

        private void Start()
        {
            
            _colorMatrix = new Image[4,4];
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    colorBlocks[y+4*x].color = colorAtlas.GetPixel(32 * x, 32 * y);
                    _colorMatrix[x, y] = colorBlocks[y + 4 * x];
                }
            }

            GlobalEvents.SendColorBlockChange(_colorMatrix[0, 0].color);
            StartCoroutine(ChangePointerColor());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (indexX > 0)
                {
                    indexX--;
                    pointer.transform.position = _colorMatrix[indexY, indexX].transform.position;
                    GlobalEvents.SendColorBlockChange(_colorMatrix[indexY, indexX].color);
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (indexX < 3)
                {
                    indexX++;
                    pointer.transform.position = _colorMatrix[indexY, indexX].transform.position;
                    GlobalEvents.SendColorBlockChange(_colorMatrix[indexY, indexX].color);
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (indexY > 0)
                {
                    indexY--;
                    pointer.transform.position = _colorMatrix[indexY, indexX].transform.position;
                    GlobalEvents.SendColorBlockChange(_colorMatrix[indexY, indexX].color);
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (indexY < 3)
                {
                    indexY++;
                    pointer.transform.position = _colorMatrix[indexY, indexX].transform.position;
                    GlobalEvents.SendColorBlockChange(_colorMatrix[indexY, indexX].color);
                }
            }
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
    }
}