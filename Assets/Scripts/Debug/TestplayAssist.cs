using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestplayAssist : MonoBehaviour
{
    public TMP_Dropdown roomSelectDropdown;
    public TMP_InputField Xcoord, Ycoord;

    private void Start()
    {
        // 드롭다운 메뉴에 MapManager에서 관리되는 모든 방 추가
        if (roomSelectDropdown != null)
        {
            List<SORoom> rooms = MapManager.Instance.map.Rooms;
            List<TMP_Dropdown.OptionData> dropDownList = new List<TMP_Dropdown.OptionData>();
            foreach (SORoom room in rooms)
            {
                dropDownList.Add(new TMP_Dropdown.OptionData(room.name));
            }
            roomSelectDropdown.ClearOptions();
            roomSelectDropdown.AddOptions(dropDownList);
        }
    }

    [Button]
    public void Teleport()
    {
        // 드롭다운 메뉴에서 방 선택 정보 가져오기
        string roomName = roomSelectDropdown.options[roomSelectDropdown.value].text;
        SORoom roomSO = MapManager.Instance.map.GetSORoomBySceneName(roomName);


        Vector2 pos = Vector2.zero;
        if (Xcoord.text.Length < 1 && Ycoord.text.Length < 1)
        {
            foreach (PortDirection dir in (PortDirection[])Enum.GetValues(typeof(PortDirection)))
            {
                RoomPort port = roomSO.GetRoomPort(dir, 0);
                if (port == null)
                    continue;
                pos = new Vector3(port.ports[0].x, port.ports[0].y, 0) + MapManager.GetMargin(dir);
            }
        }
        else
        {
            // 좌표 정보 가져오기
            float x, y;
            if (!float.TryParse(Xcoord.text, out x)) x = 0f;
            if (!float.TryParse(Ycoord.text, out y)) y = 0f;
            pos = new Vector2(x, y);
        }

        // 로깅
        Debug.Log(roomSO.name + ", " + pos + "로 이동");

        if (MapManager.Instance.CurrentRoom == roomSO)
        {
            PlayerRef.Instance.transform.position = pos;
        }
        else
        {
            // 강제이동 수행
            MapManager.Instance.Enter(roomSO, pos);
        }
        GetComponentInParent<PauseMenuUI>().ClosePauseMenu();
    }

    public void EnablePlayerAttack()
    {
        PlayerRef.Instance.movement.EnableAttack();
    }

    public void EnablePlayerDash()
    {
        PlayerRef.Instance.movement.EnableDash();
    }

    public void EnablePlayerMushJump()
    {
        PlayerRef.Instance.movement.EnableMushJump();
    }

    public void EnablePlayerGliding()
    {
        PlayerRef.Instance.movement.EnableGliding();
    }
}
