using UnityEngine;
using System.Collections.Generic;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    // 퀵슬롯 설정
    public int quickSlotSize = 3;
    public List<QuickSlot> quickSlots = new List<QuickSlot>();
    public Transform quickSlotParent;
    public GameObject quickSlotPrefab;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        quickSlotParent.gameObject.SetActive(true);
    }
    public void Start()
    {
        InitializeQuickSlots();
    }

    public void Update()
    {
        for (int i = 0; i < quickSlotSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                UseQuickSlot(i);
            }
        }
    }
    public void InitializeQuickSlots()
    {
        if (quickSlotParent == null || quickSlotPrefab == null)
        {
            Debug.LogError("QuickSlot Parent or Prefab not assigned!");
            return;
        }

        foreach (Transform child in quickSlotParent)
        {
            Destroy(child.gameObject);
        }
        quickSlots.Clear();

        for (int i = 0; i < quickSlotSize; i++)
        {
            GameObject slotObj = Instantiate(quickSlotPrefab, quickSlotParent);
            slotObj.name = $"QuickSlot_{i}"; //이름으로 인덱스 명시화
            QuickSlot slot = slotObj.GetComponent<QuickSlot>();
            slot.Initialize(i);
            quickSlots.Add(slot);
        }
    }

    #region 퀵슬롯 아이템 중복 등록 방지 
    public void RegisterToQuickSlot(int quickSlotIndex, int inventorySlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= quickSlots.Count) return;

        // 인벤토리 슬롯 유효성 검사
        if (inventorySlotIndex < 0 || inventorySlotIndex >= InventoryManager.Instance.slots.Count)
            return;

        InventorySlot inventorySlot = InventoryManager.Instance.slots[inventorySlotIndex];
        if (inventorySlot.IsEmpty) return;

        ItemData newItemData = inventorySlot.ItemData;

        // 아이템 데이터 초기화
        if (inventorySlot.ItemData.isStackable)
        {
            inventorySlot.ItemData.ResetRuntimeStack();
        }

        // 1. 동일한 인벤토리 슬롯이 다른 퀵슬롯에 등록되어 있는지 확인
        foreach (var slot in quickSlots)
        {
            if (slot.inventorySlotIndex == inventorySlotIndex &&
                slot.slotIndex != quickSlotIndex)
            {
                slot.ClearSlot(); // 기존 등록 해제
            }
        }

        // 2. 대상 퀵슬롯에 이미 등록된 아이템이 있는 경우 
        if (quickSlots[quickSlotIndex].itemData != null)
        {
            // 기존에 등록된 아이템 데이터
            ItemData existingItemData = quickSlots[quickSlotIndex].itemData;

            // 다른 아이템인 경우에만 새로 등록
            if (existingItemData != newItemData)
            {
                quickSlots[quickSlotIndex].RegisterItem(inventorySlotIndex, newItemData);
                // 즉시 스택 텍스트 업데이트
                quickSlots[quickSlotIndex].UpdateStackText();
            }
        }
        else
        {
            // 빈 슬롯이면 바로 등록
            quickSlots[quickSlotIndex].RegisterItem(inventorySlotIndex, newItemData);
            // 즉시 스택 텍스트 업데이트
            quickSlots[quickSlotIndex].UpdateStackText();
        }
    }
    #endregion

    #region 퀵슬롯 등록 된 아이템 사용 -> 0되면 삭제 
    // 퀵슬롯에서 등록 된 아이템 사용 
    public void UseQuickSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Count) return;

        QuickSlot targetSlot = quickSlots[slotIndex];
        targetSlot.UseItem();
        Debug.Log($"퀵슬롯 {slotIndex + 1}번 아이템 사용 시도");
        
    }

    // 아이템 다 사용되면 퀵슬롯에서 자동 삭제
    public void ClearSlotsWithItem(ItemData itemData)
    {
        foreach (var slot in quickSlots)
        {
            if (slot.itemData == itemData)
            {
                slot.ClearSlot();
            }
        }
    }
    #endregion
}