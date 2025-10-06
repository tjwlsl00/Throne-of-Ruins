using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("UI 설정")]
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("인벤토리 설정")]
    public int inventorySize;
    // 플레이어 돈 
    public TextMeshProUGUI PlayerMoneyText;

    [Header("효과음 설정")]
    public AudioClip inventoryEffect;
    private AudioSource audioSource;

    void Awake()
    {
        // 인스턴스 설정 전에 null 체크 추가
        if (slotPrefab == null)
        {
            Debug.LogError("slotPrefab이 할당되지 않았습니다!");
            return;
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 인벤토리 처음 시작 시 비활성화
        InitializeInventory();
        inventoryPanel.SetActive(false);

        // 오디오 연결
        audioSource = GetComponent<AudioSource>();

        // 플레이어 돈 소비량
        UpdatePlayerMoney();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel != null)
            {
                ToggleInventory();
            }
        }
        // 플레이어 돈 업데이트
        UpdatePlayerMoney();
    }

    #region 인벤토리 토글
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
        }

        audioSource.clip = inventoryEffect;
        audioSource.Play();
    }
    #endregion

    #region 슬롯 정리 및 생성(프리팹)
    void InitializeInventory()
    {
        // 기존 슬롯 정리
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // 새 슬롯 생성
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            Image slotImage = slot.GetComponent<Image>();
            Button slotBtn = slot.GetComponentInChildren<Button>();
            TextMeshProUGUI slotCount = slot.GetComponentInChildren<TextMeshProUGUI>();


            if (slotImage != null)
            {
                slotImage.enabled = true;
            }
            if (slotBtn != null)
            {
                slotBtn.enabled = true;
            }
            if (slotCount != null)
            {
                slotCount.enabled = true;
            }

            slots.Add(slot);
        }
        Debug.Log("InitializeInventory() 호출됨, 슬롯 개수: " + slots.Count); // 디버그 로그 추가
    }
    #endregion

    #region 아이템 추가 
    public bool AddItem(ItemData item)
    {
        if (item.isStackable)
        {
            item.ResetRuntimeStack();

            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty && slot.ItemData == item && slot.StackSize < item.maxStack)
                {
                    if (item.currentStack <= item.maxStack)
                    {
                        item.currentStack++;
                        slot.AddToStack();
                        return true;
                    }
                    else
                    {
                        Debug.Log("아이템 소지 개수 초과");
                    }
                }
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item);
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }
    #endregion

    #region 아이템 사용 및 제거
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        InventorySlot slot = slots[slotIndex];
        if (slot == null || slot.IsEmpty || !slot.ItemData.canUseInInventory) return;

        // 스택 가능 아이템인 경우(포션 아이템)
        if (slot.ItemData.isStackable)
        {
            // 스택 감소 (1개만 사용)
            slot.RemoveFromStack(1);

            // 아이템 사용 효과 적용
            if (slot.StackSize > 0)
            {
                slot.ItemData.UseInInventory();
            }
            // 스택이 0이 되면 슬롯 클리어 
            else if (slot.StackSize <= 0)
            {
                QuickSlotManager.Instance.ClearSlotsWithItem(slot.ItemData);
                slot.Clear();
            }
        }
        else //스택 불가 아이템(장비)
        {
            slot.ItemData.UseInInventory();
            slot.Clear();
        }
    }

    public void ResetAllItemStacks()
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && !slot.IsEmpty && slot.ItemData.isStackable)
            {
                slot.StackSize = 1;
                slot.UpdateUI();
            }
        }
    }
    public void RemoveItem(ItemData item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.ItemData == item)
            {
                slot.Clear();
                break;
            }
        }
    }
    #endregion

    #region 퀵슬롯 관련 기능
    public void RegisterItemToQuickSlot(int inventorySlotIndex, int quickSlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= slots.Count) return;
        if (slots[inventorySlotIndex].IsEmpty) return;

        QuickSlotManager.Instance.RegisterToQuickSlot(quickSlotIndex, inventorySlotIndex);
    }

    public ItemData GetItemDataFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return null;
        return slots[slotIndex].ItemData;
    }
    #endregion

    #region 장비 장착(인벤토리 슬롯에서 제거)
    public void EquipItem(EquipmentData equipment)
    {
        if (!equipment.isEquipped)
        {
            equipment.isEquipped = true;
            RemoveItem(equipment);
        }
    }
    #endregion

    // 플레이어 돈 소비량 업데이트 
    public void UpdatePlayerMoney()
    {
        int currentMoney = PlayerState.Instance.money;
        PlayerMoneyText.text = currentMoney.ToString();
    }

}
