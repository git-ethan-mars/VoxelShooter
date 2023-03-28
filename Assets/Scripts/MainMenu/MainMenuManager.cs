
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GamePlay;
using TMPro;
using Debug = UnityEngine.Debug;

namespace MainMenu
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private RectTransform savePanel;
        [SerializeField] private Button newMapButton;
        [SerializeField] private Button loadMapButton;
        [SerializeField] private Button RCHButton;

        // Start is called before the first frame update
        void Start()
        {
            newMapButton.onClick.AddListener(CreateNewMap);
            loadMapButton.onClick.AddListener(LoadMap);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateNewMap()
        {
            SceneManager.LoadScene("SampleScene");
        }

        private void LoadMap()
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = Path.GetDirectoryName(strExeFilePath) + @"\..\..\Assets\Maps\";
            string[] files = (Directory.GetFiles(filePath));
            List<string> saves = new List<string>();
            foreach (var file in files)
                if (Path.GetExtension(file) == ".rch")
                {
                    saves.Add(file);
                }

            foreach (var save in saves)
            {
                Debug.Log(save);
            }

        }

        public void CreateButton()
        {
            GameObject saveButton = new GameObject("saveButton", typeof(Button), typeof(LayoutElement));
            saveButton.transform.SetParent(savePanel.transform);
        }
    }
}