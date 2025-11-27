using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerRef 클래스는 GameObject 최상단에서 존재하며 단순히 Player 관련 클래스에 대한 레퍼런스를 모아놓은 클래스임
/// </summary>
public class PlayerRef : MonoBehaviour
{
    // 싱글턴
    private static PlayerRef _instance;
    public static PlayerRef Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerRef>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<PlayerRef>();
                }
            }
            return _instance;
        }
    }
    public PlayerState state;
    public PlayerController controller;
    public PlayerMovement movement;
    public PlayerGrabCube grabCube;
    
    public PlayerAnimation animation;
    public PlayerAudio sound;

    public Rigidbody2D rb;
    public BoxCollider2D col;
    public PlayerDamageReceiver damageReceiver;

    private void Awake()
    {
        if(Instance == null)
            _instance = this;
        else if (_instance != this)
        {
            return;
        }
        if (state == null) state = GetComponent<PlayerState>();
        if(controller == null) controller = GetComponent<PlayerController>();
        if(movement == null) movement = GetComponent<PlayerMovement>();
        // if(magic == null) magic = GetComponent<PlayerMagic>();
        if (grabCube == null) grabCube = GetComponentInChildren<PlayerGrabCube>();
        if(animation == null) animation = GetComponent<PlayerAnimation>();
        if(sound == null) sound = GetComponent<PlayerAudio>();

        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if(col == null) col = GetComponent<BoxCollider2D>();
        if(damageReceiver == null)  damageReceiver = GetComponent<PlayerDamageReceiver>();
    }
}
