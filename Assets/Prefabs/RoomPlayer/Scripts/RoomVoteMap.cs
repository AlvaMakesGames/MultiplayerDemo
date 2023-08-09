using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class RoomVoteMap : MonoBehaviour
{
    [Scene] [SerializeField] string[] gameScenes;
    [SerializeField] string[] levelNames;
    [SerializeField] Sprite[] screenGrabs;
    [SerializeField] Image levelImage;
    [SerializeField] TMP_Text levelName;
    [SerializeField] RoomPlayer roomPlayer;

    private int selectedScene;

    void Start()
    {
        levelImage.sprite = screenGrabs[0];
        levelName.text = levelNames[0];
    }

    public void DecrementLevel()
    {
        if (selectedScene - 1 == -1)
            selectedScene = gameScenes.Length - 1;
        else
            selectedScene--;

        levelImage.sprite = screenGrabs[selectedScene];
        levelName.text = levelNames[selectedScene];
    }

    public void IncrementLevel()
    {
        if (selectedScene + 1 == gameScenes.Length)
            selectedScene = 0;
        else
            selectedScene++;

        levelImage.sprite = screenGrabs[selectedScene];
        levelName.text = levelNames[selectedScene];
    }

    public void Confirm()
    {
        string map = gameScenes[selectedScene];
        roomPlayer.CmdSetMap(map);
        roomPlayer.CmdSetReady();
        gameObject.SetActive(false);
    }
}
