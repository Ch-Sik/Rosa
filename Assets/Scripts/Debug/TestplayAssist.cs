using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        // 좌표 정보 가져오기
        float x = float.Parse(Xcoord.text);
        float y = float.Parse(Ycoord.text);
        Vector2 pos = new Vector2(x, y);

        // 로깅
        Debug.Log(roomSO.name + ", " + pos + "로 이동");

        // 강제이동 수행
        MapManager.Instance.Enter(roomSO, pos);
    }
}
