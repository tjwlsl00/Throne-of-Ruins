using UnityEngine;

[CreateAssetMenu(fileName = "New Mana Potion", menuName = "ShopItems/Data/Potions/Mana")]
public class Mana : ItemData
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

        if (player.currentMP >= player.maxMP)
        {
            Debug.Log("체력이 이미 가득 찼습니다!");
            return;
        }

        player.HealMana(healAmount);
        Debug.Log($"회복 물약 사용! (+{healAmount} HP)");
    }
    
}