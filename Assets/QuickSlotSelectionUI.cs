using UnityEngine;
using UnityEngine.UI;

public class QuickSlotSelectionUI : MonoBehaviour
{
    public static QuickSlotSelectionUI Instance { get; private set; }

    public GameObject panel;
    public Button[] quickSlotButtons;

    private int selectedInventorySlotIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        panel.SetActive(false);
    }

    // UI 표시 전 유효성 검사 추가
    public void Show(int inventorySlotIndex)
    {
        if (InventoryManager.Instance == null ||
            inventorySlotIndex < 0 ||
            inventorySlotIndex >= InventoryManager.Instance.slots.Count ||
            InventoryManager.Instance.slots[inventorySlotIndex].IsEmpty)
        {
            Debug.LogWarning("Invalid inventory slot selected");
            return;
        }

        selectedInventorySlotIndex = inventorySlotIndex;
        panel.SetActive(true);

        // 퀵슬롯 버튼 상태 업데이트
        for (int i = 0; i < quickSlotButtons.Length; i++)
        {
            // 버튼에 현재 등록된 아이템 미리보기 표시
            if (i < QuickSlotManager.Instance.quickSlots.Count)
            {
                var slot = QuickSlotManager.Instance.quickSlots[i];
                quickSlotButtons[i].GetComponent<Image>().sprite =
                    slot.itemData != null ? slot.itemData.icon : null;
            }

            int index = i;
            quickSlotButtons[i].onClick.RemoveAllListeners();
            quickSlotButtons[i].onClick.AddListener(() => OnQuickSlotSelected(index));
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void OnQuickSlotSelected(int quickSlotIndex)
    {
        Debug.Log($"선택된 퀵슬롯 인덱스: {quickSlotIndex}"); //로그 추가
        InventoryManager.Instance.RegisterItemToQuickSlot(selectedInventorySlotIndex, quickSlotIndex);
        Hide();
    }
}