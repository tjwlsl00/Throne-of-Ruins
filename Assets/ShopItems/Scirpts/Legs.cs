using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment_Legs", menuName = "ShopItems/Data/Equipments/Legs")]
public class Legs : EquipmentData
{
    [SerializeField] private float extraHP = 5f;
    public PlayerState playerState;

    public override void ApplyEffect(PlayerState playerState, bool isEquipping)
    {
        if (playerState == null)
        {
            playerState = PlayerState.Instance;
            if (playerState == null)
            {
                Debug.Log("PlayerState 인스턴스 null");
                return;
            }
        }

        float hpChange = isEquipping ? extraHP : -extraHP;
        playerState.ModifyMaxHP((int)hpChange);
    }
}
