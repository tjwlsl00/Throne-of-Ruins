using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlot : MonoBehaviour
{
    [Header("UI연결")]
    public Image itemIcon;
    public TextMeshProUGUI stackText;
    public Button slotButton;

    [Header("Slot Data")]
    public int slotIndex;
    public int inventorySlotIndex = -1;
    public ItemData itemData;

    private void Awake()
    {
        // 버튼이 null이면 자동 할당
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }
    }

    // 버튼 클릭 리스너 관리 개선
    private void OnDestroy()
    {
        slotButton.onClick.RemoveAllListeners();
    }

    public void Initialize(int index)
    {
        slotIndex = index;
        Debug.Log($"[QuickSlot] Initialized slot {slotIndex}");

        // 초기화 시 슬롯 비우기
        ClearSlot();
    }

    #region 아이템 등록/삭제
    public void RegisterItem(int invSlotIndex, ItemData data)
    {
        inventorySlotIndex = invSlotIndex;
        itemData = data;

        if (itemIcon != null)
        {
            itemIcon.sprite = data?.icon;
            itemIcon.enabled = data != null;
        }
        UpdateStackText();
    }
    // 등록된 아이템 퀵슬롯에서 삭제 
    public void ClearSlot()
    {
        inventorySlotIndex = -1;
        itemData = null;

        // 아이콘 이미지 직접 null 처리
        if (itemIcon != null)
        {
            itemIcon.sprite = null; // 스프라이트 직접 null 할당
            itemIcon.enabled = false; // 이미지 비활성화
        }

        if (stackText != null)
        {
            stackText.text = "";
        }
    }
    #endregion

    #region 아이템 사용 
    // 아이템 사용 시 null 체크 강화
    public void UseItem()
    {
        if (!isValidSlot())
        {
            ClearSlot();
            return;
        }

        InventorySlot inventorySlot = InventoryManager.Instance.slots[inventorySlotIndex];

        // 아이템 사용 전에 스택 확인
        if (inventorySlot.StackSize <= 0)
        {
            ClearSlot();
            return;
        }

        // 아이템 사용
        InventoryManager.Instance.UseItem(inventorySlotIndex);

        // 사용 후 상태 확인
        if (inventorySlot.IsEmpty || inventorySlot.StackSize <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateStackText();
        }
    }
    #endregion

    private bool isValidSlot()
    {
        if (itemData == null || InventoryManager.Instance == null || inventorySlotIndex < 0 || inventorySlotIndex >= InventoryManager.Instance.slots.Count)
        {
            return false;
        }

        InventorySlot slot = InventoryManager.Instance.slots[inventorySlotIndex];
        return slot != null && !slot.IsEmpty && slot.ItemData == itemData;
    }

    #region 아이템 스택수 업데이트
    public void UpdateStackText()
    {
        if (itemData != null && itemData.isStackable)
        {
            InventorySlot slot = InventoryManager.Instance.slots[inventorySlotIndex];
            stackText.text = slot.StackSize > 1 ? slot.StackSize.ToString() : "";
        }
        else
        {
            stackText.text = "";
        }
    }
    #endregion
}