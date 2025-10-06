using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*

    EquipmentSlot -> EquipmentManager -> ItemData(EquipmentData)

*/

public class EquipmentSlot : MonoBehaviour
{
    [Header("장비 타입 설정")]
    public EquipmentType equipmentType;

    [Header("장비 UI 설정")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI equipmentNameText;
    [SerializeField] private Sprite emptySlotIcon; // 빈 슬롯 아이콘 추가

    private EquipmentData currentEquipment;

    private void Awake()
    {
        InitializeComponents();
        UpdateUI();
    }

    private void InitializeComponents()
    {
        // 버튼 컴포넌트 안전하게 가져오기
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("Button component is missing!", this);
                return;
            }
        }
        button.onClick.RemoveListener(OnSlotClicked);
        button.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(EquipmentType type)
    {
        equipmentType = type;
        UpdateSlotName();
    }

    #region 장비 슬롯 채우기 / 비우기
    public void SetEquipmentSlot(EquipmentData equipment)
    {
        if (equipment == null)
        {
            Debug.LogWarning("설정하려는 장비가 null입니다.");
            ClearSlot();
            return;
        }

        if (equipment.equipmentType != equipmentType)
        {
            Debug.LogError($"타입 불일치! 슬롯:{equipmentType}, 장비:{equipment.equipmentType}");
            return;
        }

        currentEquipment = equipment;
        InventoryManager.Instance.RemoveItem(currentEquipment);
        UpdateUI();
    }

    public void ClearSlot()
    {
        if (currentEquipment != null)
        {
            currentEquipment = null;
        }
        UpdateUI();
    }

    public void RemoveEquipmentSlot()
    {
        if (currentEquipment == null) return;
        Debug.Log($"{equipmentType} 슬롯에서 {currentEquipment.itemName} 해제");
        currentEquipment = null;

        // 스프라이트 초기화 (null로 설정)
        button.image.sprite = null;
        // 텍스트 초기화
        if (equipmentNameText != null)
        {
            equipmentNameText.text = equipmentType.ToString();
        }
    }
    #endregion

    private void OnSlotClicked()
    {
        if (currentEquipment != null)
        {
            EquipmentManager.Instance.UnequipItem(equipmentType);
        }
    }

    #region UI 업데이트
    void UpdateUI()
    {
        // 버튼 이미지 업데이트
        if (button != null)
        {
            button.image.sprite = currentEquipment?.icon ?? emptySlotIcon;
            button.image.enabled = true; // 항상 활성화 (빈 슬롯도 표시)
        }

        // 이름 텍스트 업데이트
        UpdateSlotName();
    }

    private void UpdateSlotName()
    {
        if (equipmentNameText != null)
        {
            equipmentNameText.text = currentEquipment?.itemName ?? equipmentType.ToString();
        }
    }
    #endregion

    public EquipmentData GetCurrentEquipment() => currentEquipment;
    public bool IsEmpty => currentEquipment == null;

}
