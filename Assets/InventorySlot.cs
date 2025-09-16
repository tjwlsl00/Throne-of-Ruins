using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
/*
    로직 순서 
    InventorySlot -> InventoryManager -> ItemData
*/
public class InventorySlot : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public TextMeshProUGUI itemStackText;

    public ItemData ItemData { get; private set; }
    public bool IsEmpty => ItemData == null;
    public int StackSize = 1;
    public int SlotIndex { get; private set; }

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (itemStackText == null) itemStackText = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(OnSlotClicked);
        if (ItemClickPanel != null)
        {
            ItemClickPanel.SetActive(false);
        }
    }

    #region 재시작마다 아이템 개수 stackSize 1로 초기화 
    public void SetItem(ItemData item)
    {
        ItemData = item;
        // 새 아이템 추가 시 스택 크기를 1 또는 기본값으로 초기화
        StackSize = item.isStackable ? item.currentStack : 1;
        UpdateUI();
    }
    #endregion

    #region 스택 증가, 감소
    // 스택 증가 
    public void AddToStack(int amount = 1)
    {
        if (ItemData.isStackable)
        {
            StackSize = Mathf.Clamp(StackSize + amount, 1, ItemData.maxStack);
            UpdateUI();
        }
    }

    // 스택 감소 
    public void RemoveFromStack(int amount = 1)
    {
        if (ItemData.isStackable)
        {
            StackSize = Mathf.Max(0, StackSize - amount);
            UpdateUI();
            if (StackSize <= 0)
            {
                Clear();
                // 패널 강제 닫기 추가(ItemClickedPanel)
                if (ItemClickPanel != null)
                {
                    ItemClickPanel.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region UI업데이트
    public void UpdateUI()
    {
        // Null 체크 강화
        if (ItemData != null && ItemData.icon != null && itemStackText != null)
        {
            button.image.sprite = ItemData.icon;
            button.image.enabled = true; // 이미지 활성화
            itemStackText.enabled = true;
            itemStackText.text = ItemData.isStackable ? StackSize.ToString() : "";
        }
        else
        {
            button.image.sprite = null;
            button.image.enabled = false; // 이미지 비활성화
            itemStackText.text = "";
            itemStackText.enabled = false;
        }
        button.interactable = !IsEmpty;
    }
    #endregion

    // 스택 0 되면 슬롯 비우기
    public void Clear()
    {
        ItemData = null;
        StackSize = 0;
        UpdateUI();
        // Clear 호출 시에도 패널 닫기
        if (ItemClickPanel != null)
        {
            ItemClickPanel.SetActive(false);
        }
    }

    #region 아이템 사용
    // 슬롯 클릭 패널 연동
    public GameObject ItemClickPanel;
    public Button UseItemButton;
    public Button QuickSlotButton;

    private void OnSlotClicked()
    {
        if (IsEmpty) return;

        if (ItemData is EquipmentData equipment)
        {
            EquipmentManager.Instance.EquipItem(equipment);
        }
        else if (ItemClickPanel != null)
        {
            ItemClickPanel.SetActive(true);

            // 기존 리스너 제거 후 새로 추가
            UseItemButton.onClick.RemoveAllListeners();
            QuickSlotButton.onClick.RemoveAllListeners();

            // ItemClickPanel활성화 1. 아이템 사용  2. 퀵슬롯 등록
            UseItemButton.onClick.AddListener(() =>
            {
                InventoryManager.Instance.UseItem(transform.GetSiblingIndex());
                ItemClickPanel.SetActive(false);
            });

            QuickSlotButton.onClick.AddListener(() =>
            {
                QuickSlotSelectionUI.Instance.Show(transform.GetSiblingIndex());
                ItemClickPanel.SetActive(false);
            });
        }

    }
    #endregion
}