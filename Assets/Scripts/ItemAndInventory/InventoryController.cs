using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventory 클래스의 생명주기를 관리하고 그 레퍼런스를 싱글톤으로 전역으로 접근 가능하게 함.
/// 세이브/로드 시의 초기화도 관여
/// </summary>
public class InventoryController : MonoBehaviour
{
    #region 싱글턴
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
    #endregion

    //인벤토리 데이터
    private Inventory inventory;

    // 아이템 증감 이벤트
    public delegate void ItemEvent(ItemCode itemCode, int quantity);
    public ItemEvent OnItemAdded;
    public ItemEvent OnItemRemoved;

    public void AddItem(ItemCode itemCode, int quantity)
    {
        inventory.AddItem(itemCode, quantity);
        OnItemAdded?.Invoke(itemCode, quantity);
    }

    public void RemoveItem(ItemCode itemCode, int quantity)
    {
        inventory.RemoveItem(itemCode, quantity);
        OnItemRemoved?.Invoke(itemCode, quantity);
    }

    public int GetQuantity(ItemCode itemCode)
    {
        return inventory.GetQuantity(itemCode);
    }

    private void Start()
    {
        // TODO: 인벤토리 세이브로드 시스템과 연계되도록 구현
        // LoadInventory();
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
}
