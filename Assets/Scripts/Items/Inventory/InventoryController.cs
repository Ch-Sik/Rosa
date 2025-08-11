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

    // 인벤토리 데이터
    [SerializeField]
    private Inventory inventory;
    // SO_Item 데이터 관리자
    private ItemDataManager itemDatas;

    // 아이템 증감 이벤트
    public delegate void ItemEvent(ItemCode itemCode, int quantity);
    public ItemEvent OnItemAdded;
    public ItemEvent OnItemRemoved;

    private void Start()
    {
        inventory = new Inventory();
        itemDatas = new ItemDataManager();

        // TODO: 인벤토리 세이브로드 시스템과 연계되도록 구현
        // LoadInventory();
    }

    public void AddItem(ItemCode item, int quantity)
    {
        if (quantity <= 0)
        {
            Debug.Log("quantity는 0보다 큰 값이어야 함");
            return;
        }
        // 아이템 최대 소지 갯수 정보 가져오기
        int maxQuantity = itemDatas.GetItemData(item).maxQuantity;
        if (inventory.GetQuantity(item) + quantity >= maxQuantity)
        {
            inventory.SetItem(item, maxQuantity);
        }
        else
        {
            inventory.AddItem(item, quantity);
        }
        OnItemAdded?.Invoke(item, quantity);
    }

    public void RemoveItem(ItemCode item, int quantity)
    {
        if(!inventory.RemoveItem(item, quantity))
        {
            Debug.Log($"현재 소지 수({inventory.GetQuantity(item)})가 {quantity}보다 적어서 그만큼 아이템을 없앨 수 없음");
            return;
        }
        OnItemRemoved?.Invoke(item, quantity);
    }

    public int GetQuantity(ItemCode itemCode)
    {
        return inventory.GetQuantity(itemCode);
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
