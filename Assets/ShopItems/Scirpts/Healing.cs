using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion", menuName = "ShopItems/Data/Potions/Healing")]
public class Healing : ItemData
{
    public int healAmount = 30;

    public override void UseInInventory()
    {
        PlayerState player = FindAnyObjectByType<PlayerState>();
        
        if (player == null)
        {
            Debug.LogError("플레이어 체력 시스템 없음!");
            return;
        }

        if (player.currentHP >= player.maxHP)
        {
            Debug.Log("체력이 이미 가득 찼습니다!");
            return;
        }

        player.Heal(healAmount);
        Debug.Log($"회복 물약 사용! (+{healAmount} HP)");
    }
}