using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class RoomSelectLevel : MonoBehaviour
{
    private MyNetworkManager Manager => NetworkManager.singleton as MyNetworkManager;

    [Scene] [SerializeField] string[] gameScenes;
    [SerializeField] string[] levelNames;
    [SerializeField] Sprite[] screenGrabs;
    [SerializeField] Image levelImage;
    [SerializeField] TMP_Text levelName;

    private int selectedScene;
    void Start()
    {
        levelImage.sprite = screenGrabs[0];
        levelName.text = levelNames[0];
        Manager.GameplayScene = gameScenes[selectedScene];
    }

    public void DecrementLevel()
    {
        if (selectedScene - 1 == -1)
            selectedScene = gameScenes.Length - 1;
        else
            selectedScene--;

        Manager.GameplayScene = gameScenes[selectedScene];
        levelImage.sprite = screenGrabs[selectedScene];
        levelName.text = levelNames[selectedScene];
    }

    public void IncrementLevel()
    {
        if (selectedScene + 1 == gameScenes.Length)
            selectedScene = 0;
        else
            selectedScene++;

        Manager.GameplayScene = gameScenes[selectedScene];
        levelImage.sprite = screenGrabs[selectedScene];
        levelName.text = levelNames[selectedScene];
    }
}
