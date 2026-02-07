using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class NpcMovement : MonoBehaviour
{
    [Tooltip("이 스크립트가 어떤 NPC의 움직임을 담당하는지.")]
    [SerializeField] private List<CommunicationTarget> character;

    [SerializeField] private bool isMoving = false;
    [SerializeField] private float _startPositionX;
    [SerializeField] private float _goalPositionX;

    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    [SerializeField] private VfxPoolEntity disappearVfx;
    [SerializeField, ReadOnly] private string npcDisappearSaveKey;

    // Start is called before the first frame update
    void Start()
    {
        npcDisappearSaveKey = GetNpcDisappearSaveKey();
        if (FlagManager.Instance.GetFlag(npcDisappearSaveKey) == 1)
        {
            Debug.Log($"[NpcMovement] Hide npc with key {npcDisappearSaveKey}");
            Destroy(gameObject);
        }
        else
            Init();
    }

    void Init()
    {
        Debug.Assert(rb != null, "NpcMovement: rigidbody가 지정되어 있지 않음!");
        rb.isKinematic = true;

        // 맵 하나 당 동일 인물은 하나만 있다는 가정. 덮어쓰는 등의 상황은 고려하지 않음.
        foreach (var ch in character)
        {
            CommunicationManager.Instance.npcMovements.Add(ch, this);
        }
    }

    private void OnDestroy()
    {
        foreach(var ch in character)
        {
            CommunicationManager.Instance.npcMovements.Remove(ch);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleMove();
    }

    void HandleMove()
    {
        if (!isMoving) return;
        if (Mathf.Abs(transform.position.x - _goalPositionX) > 0.2f)
        {
            float dir = (_goalPositionX - transform.position.x) > 0 ? 1 : -1;
            rb.velocity = Vector2.right * dir * _moveSpeed;
            anim.SetBool("isWalking", true);
        }
        else
        {
            isMoving = false;
            rb.velocity = Vector2.zero;
            anim.SetBool("isWalking", false);
        }
    }

    // 리턴 값은 움직이는 데 걸릴 예상 시간
    [Button]
    public float MoveTo(float destWorldPosX)
    {
        _startPositionX = transform.position.x;
        _goalPositionX = destWorldPosX;
        isMoving = true;

        LookAtX(_goalPositionX);
        
        float eta = Mathf.Abs(_goalPositionX - _startPositionX) / _moveSpeed;
        return eta;
    }

    public void TeleportTo(float destWorldPosX)
    {
        Vector3 pos = transform.position;
        pos.x = destWorldPosX;
        transform.position = pos;
    }

    private void LookAtX(float destX)
    {
        LR dir = (destX - _startPositionX) > 0 ? LR.RIGHT : LR.LEFT;
        Vector3 scale = transform.localScale;
        // AnyPortrait로 제작된 NPC 이미지는 기본 왼쪽을 바라보고 있음 (localScale.x > 0일때 왼쪽)
        // 그래서 -1 곱해서 보정 필요
        scale.x = Mathf.Abs(scale.x) * dir.toFloat() * -1;
        transform.localScale = scale;
    }

    public void Disappear()
    {
        npcDisappearSaveKey = GetNpcDisappearSaveKey();
        Debug.Log($"[NpcMovement] Disappear npc with key {npcDisappearSaveKey}");
        
        FlagManager.Instance.SetFlag(npcDisappearSaveKey, 1);
        StartCoroutine(CoDisappear());

        IEnumerator CoDisappear()
        {
            if(disappearVfx)
                VfxManager.Instance.SpawnVfxObject(disappearVfx, transform.position);
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }
    }

    #if UNITY_EDITOR
    [Button]
    public void SetNpcDisappearSaveKey()
    {
        if (Application.isPlaying) return;
        npcDisappearSaveKey = GetNpcDisappearSaveKey();
    }
    #endif

    private string GetNpcDisappearSaveKey()
    {
        // string sceneName = SceneManager.GetActiveScene().name;
        string sceneName = gameObject.scene.name;
        string characterName = character.Count > 0 ? character[0].ToString() : "Anonymous";
        return sceneName + "_" + characterName;
    }
}
