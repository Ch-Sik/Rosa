using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "", fileName = "")]
public class SORoom : ScriptableObject
{
    public SceneField scene;
    public string flagName;
    public int flagIndex;
    public string title;
    public string subTitle;
    public AudioClip defaultBGM;

    public Vector2Int size;
    public Vector2Int offset;

    public HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();
    public List<RoomPort> topPorts = new List<RoomPort>();
    public List<RoomPort> botPorts = new List<RoomPort>();
    public List<RoomPort> rigPorts = new List<RoomPort>();
    public List<RoomPort> lefPorts = new List<RoomPort>();

    #region Port Methods
    public void SetRoomPorts(List<RoomPort> top, List<RoomPort> bot, List<RoomPort> rig, List<RoomPort> lef)
    {
        topPorts = new List<RoomPort>(top);
        botPorts = new List<RoomPort>(bot);
        rigPorts = new List<RoomPort>(rig);
        lefPorts = new List<RoomPort>(lef);
    }

    public RoomPort GetRoomPort(PortDirection direction, int index)
    {
        try
        {
            switch (direction)
            {
                case PortDirection.Top:
                    return topPorts[index];
                case PortDirection.Bot:
                    return botPorts[index];
                case PortDirection.Rig:
                    return rigPorts[index];
                case PortDirection.Lef:
                    return lefPorts[index];
                default:
                    return null;
            }
        }
        catch
        { 
            return null;
        }
    }

    public List<ConnectedPort> GetConnectedPort(PortDirection direction, int index)
    {
        switch (direction)
        {
            case PortDirection.Top:
                return new List<ConnectedPort>(topPorts[index].connectedPorts);
            case PortDirection.Bot:
                return new List<ConnectedPort>(botPorts[index].connectedPorts);
            case PortDirection.Rig:
                return new List<ConnectedPort>(rigPorts[index].connectedPorts);
            case PortDirection.Lef:
                return new List<ConnectedPort>(lefPorts[index].connectedPorts);

            default: return null;
        }
    }
    #endregion

    #region Room Methods
    public SORoom GetConnectedRoom(PortDirection direction, int index, int flag = 0)
    {
        switch (direction)
        {
            case PortDirection.Top:
//                return topPorts[index].connectedPorts[flag].room;
            case PortDirection.Bot:
//                return botPorts[index].connectedPorts[flag].room;
            case PortDirection.Rig:
//                return rigPorts[index].connectedPorts[flag].room;
            case PortDirection.Lef:
//                return lefPorts[index].connectedPorts[flag].room;

            default: return null;
        }
    }

    public List<SORoom> GetConnectedRooms()
    {
        List<SORoom> scenes = new List<SORoom>();

        foreach (RoomPort port in topPorts)
            scenes.AddRange(port.GetConnectedRoom());
        foreach (RoomPort port in botPorts)
            scenes.AddRange(port.GetConnectedRoom());
        foreach (RoomPort port in rigPorts)
            scenes.AddRange(port.GetConnectedRoom());
        foreach (RoomPort port in lefPorts)
            scenes.AddRange(port.GetConnectedRoom());

        return scenes;
    }

    public RoomPort GetPort(PortDirection direction, int index)
    {
        switch (direction)
        {
            case PortDirection.Top:
                return topPorts[index];
            case PortDirection.Bot:
                return botPorts[index];
            case PortDirection.Rig:
                return rigPorts[index];
            case PortDirection.Lef:
                return lefPorts[index];

            default: return null;
        }
    }

    public void ClearConnectedPort()
    {
        foreach (RoomPort port in topPorts) { port.connectedPorts = new List<ConnectedPort>(); }
        foreach (RoomPort port in botPorts) { port.connectedPorts = new List<ConnectedPort>(); }
        foreach (RoomPort port in rigPorts) { port.connectedPorts = new List<ConnectedPort>(); }
        foreach (RoomPort port in lefPorts) { port.connectedPorts = new List<ConnectedPort>(); }
    }
    #endregion
}

#region Room Port
[Serializable]
public class RoomPort
{
    public string flag;
    public int index;
    public PortDirection direction;
    public List<Vector2Int> ports = new List<Vector2Int>();
    public List<ConnectedPort> connectedPorts = new List<ConnectedPort>();

    public RoomPort(PortDirection direction, List<Vector2Int> ports, int index)
    {
        this.direction = direction;
        this.ports = ports;
        this.index = index;
    }

    public List<SORoom> GetConnectedRoom()
    {
        List<SORoom> scenes = new List<SORoom>();

 //       foreach (ConnectedPort port in connectedPorts)
 //           scenes.Add(port.room);

        return scenes;
    }

    // 5 11인 관문이 있다고 했을 때, 8로 진입 시, 8 / 16, 0.5
    // 들어온 포지션, 0~최대의 합
    public float GetPortPercentage(Vector3 position)
    {
        float percentage = 0.0f;

        if (isHorizontal())
            percentage = (position.y - ports[0].y) / (ports[ports.Count - 1].y - ports[0].y);
        else
            percentage = (position.x - ports[0].x) / (ports[ports.Count - 1].x - ports[0].x);

        if (percentage < 0) percentage = 0;
        if (percentage > 1) percentage = 1;
        return percentage;
    }

    public Vector3 GetPortPosition(float percentage)
    { 
        Vector3 position = new Vector3();

        if (isHorizontal())
            return new Vector3(ports[0].x, (ports[ports.Count - 1].y - ports[0].y) * percentage + ports[0].y);

        return new Vector3((ports[ports.Count - 1].x - ports[0].x) * percentage + ports[0].x, ports[0].y);
    }

    public bool isHorizontal()
    {
        Vector2Int value = ports[0] - ports[1];

        return value.x == 0;
    }
}
#endregion

#region Connected Port
[Serializable]
public class ConnectedPort
{
    public string flag;
    public int flagIndex;
    public SceneField scene;
    public int index;

    public ConnectedPort(SceneField scene, int index, string flagName, int flagIndex)
    {
        Debug.Log(flagName + " " + flagIndex);

        this.scene = scene;
        this.index = index;
        this.flag = flagName;
        this.flagIndex = flagIndex;
    }

    public ConnectedPort(SceneField scene, int index)
    {
        this.scene = scene;
        this.index = index;
    }
}
#endregion

public enum PortDirection
{
    Top,
    Bot,
    Rig,
    Lef
};