using UnityEngine;

public abstract class EquipmentData : ItemData
{
    [Header("장비 정보")]
    public EquipmentType equipmentType;
    public bool isEquipped;

    // 모든 장비 아이템 동일한 로직
    public sealed override void UseInInventory()
    {
        EquipmentManager.Instance.ToggleEquip(this);
    }

    /*
        장비 별 개별 효과 -> playerState 정보, isEquipping bool정보 
    */
    public abstract void ApplyEffect(PlayerState playerState, bool isEquipping);
}

#region 아이템 타입
public enum EquipmentType
{
    Head = 0,
    Chest = 1,
    Legs = 2,
    Weapon = 3,
    Accessory = 4,
    Special = 5
}
#endregion

