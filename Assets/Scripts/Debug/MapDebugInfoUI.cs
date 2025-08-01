using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapDebugInfoUI : MonoBehaviour
{
    public TextMeshProUGUI chapterDebugUI;
    public TextMeshProUGUI positionDebugUI;
    public Transform player;

    private void Start()
    {
        MapManager.Instance.OnNextRoomLoaded += () =>
        {
            chapterDebugUI.text = $"{MapManager.Instance.CurrentRoom.name}";
        };
        if(player == null)
        {
            player = PlayerRef.Instance.transform;
        }
    }

    private void Update()
    {
        positionDebugUI.text = $"{player.position.x.ToString("F1")} , {player.position.y.ToString("F1")}";
    }
}
