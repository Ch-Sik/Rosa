using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventory 클래스와 InventoryUI 클래스를 총괄하며, 아이템을 추가하고 뺄 수 있는 클래스
/// </summary>

//InventoryController가 굳이 싱글턴일 이유는 없다. 나중에 싱글턴으로 객체리퍼를 담아놔도 좋을 것이지만, 테스트를 위해 싱글턴 처리했다.
public class InventoryController : MonoBehaviour
{
    private static InventoryController instance;
    public static InventoryController Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //아이템 획득 알림 스크립트
    public ItemEventController eventController;
    //인벤토리 데이터
    public Inventory inventory;

    //인벤토리 오픈 시퀀스 데이터
    private Sequence openEvent;

    private void Start()
    {
        LoadInventory();
    }

    //저장된 데이터로부터 인벤토리를 로드함
    private void LoadInventory()
    {
        /*
        if(인벤토리 세이브 데이터를 로드할 수 있다면,)
            Inventory = new Inventroy(데이터);
        else
         */
        inventory = new Inventory();
    }

    [Button]
    public void AddItem(ItemCode itemCode, int quantity)
    { 
        inventory.AddItem(itemCode, quantity);
    }

    [Button]
    public bool RemoveItem(ItemCode itemCode, int quantity)
    {
        if (!inventory.RemoveItem(itemCode, quantity))
            return false;

        return true;
    }

    public void OnOpen() { }
    public void OnClose() { }
}
