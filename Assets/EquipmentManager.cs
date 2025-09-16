using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    // 싱글톤
    public static EquipmentManager Instance { get; private set; }
    #region  내부 변수 
    public GameObject slotPrefab;
    public Transform slotsParent;
    [Header("장비 슬롯 UI")]
    public EquipmentSlot[] manualSlots = new EquipmentSlot[6];
    [Header("현재 장착 장비")]
    public EquipmentData[] currentEquipment = new EquipmentData[6];
    private Dictionary<EquipmentType, EquipmentSlot> equipmentSlots = new Dictionary<EquipmentType, EquipmentSlot>();
    public PlayerState playerState;
    // 사운드 
    public AudioSource audioSource;
    public AudioClip EquipSound;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSlots();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 인덱스 할당 -> 슬롯에 고유번호 적용
    private void InitializeSlots()
    {
        if (slotPrefab != null && slotsParent != null)
        {
            CreateSlotsFromPrefab();
        }
        else
        {
            Debug.LogError("장비 슬롯 생성 방식이 설정되지 않았습니다!");
        }
    }

    #region 장비 슬롯 생성(프리팹)
    private void CreateSlotsFromPrefab()
    {
        // 새로운 슬롯들을 생성하고 초기화하는 기존 로직
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            EquipmentSlot slot = slotObj.GetComponent<EquipmentSlot>();
            slot.Initialize(type);
            equipmentSlots[type] = slot;

            // 첫 6개 슬롯은 manualSlots 배열에도 할당 (호환성 유지)
            int index = (int)type;
            if (index < manualSlots.Length)
            {
                manualSlots[index] = slot;
            }
        }
        // 모든 슬롯 생성이 완료된 후, 씬에 원래 있던 원본 slotPrefab 인스턴스를 비활성화
        if (slotPrefab != null)
        {
            slotPrefab.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 장비 토글 로직 
    public bool ToggleEquip(EquipmentData equipment)
    {
        EquipmentType type = equipment.equipmentType;
        int slotIndex = (int)type;

        // 1. 이미 장착 된 경우 해제
        if (currentEquipment[slotIndex] == equipment)
        {
            return UnequipItem(type);
        }

        // 2. 다른 장비가 장착된 경우 교체 
        else if (currentEquipment[slotIndex] != null)
        {
            if (!UnequipItem(type)) return false;
        }

        // 3. 새 장비 장착
        return EquipItem(equipment);
    }
    #endregion

    #region 장비 장착 로직
    public bool EquipItem(EquipmentData equipment)
    {
        // 1. 유효성 검사
        if (equipment == null || !equipmentSlots.ContainsKey(equipment.equipmentType))
        {
            Debug.LogWarning("장비 데이터 또는 슬롯이 유효하지 않습니다.");
            return false;
        }

        EquipmentType type = equipment.equipmentType;
        EquipmentSlot targetSlot = equipmentSlots[type];

        // 장비 설정 로직
        targetSlot.SetEquipmentSlot(equipment);
        currentEquipment[(int)type] = equipment;
        equipment.isEquipped = true;

        // 능력치 적용 
        equipment.ApplyEffect(playerState, true);
        Debug.Log($"{equipment.itemName} 장착 완료 ({type})");

        // 사운드 
        audioSource.clip = EquipSound;
        audioSource.Play();

        return true;
    }
    #endregion

    #region 장비 해제 로직
    public bool UnequipItem(EquipmentType type)
    {
        int slotIndex = (int)type;
        if (currentEquipment[slotIndex] == null) return true;

        EquipmentData oldEquipment = currentEquipment[slotIndex];

        // 능력치 해제
        oldEquipment.ApplyEffect(playerState, false);

        // 기존 해제 로직(인벤토리에 해제 장비 추가)
        if (InventoryManager.Instance.AddItem(oldEquipment))
        {
            if (equipmentSlots.TryGetValue(type, out EquipmentSlot slot))
            {
                slot.RemoveEquipmentSlot();
            }

            currentEquipment[slotIndex] = null;
            oldEquipment.isEquipped = false;
            return true;
        }

        Debug.LogWarning("인벤토리에 공간이 부족합니다.");
        
        // 사운드 
        audioSource.clip = EquipSound;
        audioSource.Play();
        
        return false;
    }
    #endregion

}
