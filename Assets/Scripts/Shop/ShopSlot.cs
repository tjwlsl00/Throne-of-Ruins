using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI itemPriceText;
    [SerializeField] Button itembuyBtn;
    private ItemData currentItem;
    public purchaseUI purchaseUI;

    void Start()
    {
        itembuyBtn.onClick.AddListener(BuyItem);
    }

    public void SetSlot(ItemData item)
    {
        currentItem = item;

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.gameObject.SetActive(item.icon != null); // 아이콘이 없으면 이미지 비활성화
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }

        if (itemPriceText != null)
        {
            itemPriceText.text = "$" + item.price.ToString();
        }

    }

    public void BuyItem()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager 인스턴스가 없습니다!");
            return;
        }

        if (currentItem == null)
        {
            Debug.LogError("현재 아이템 데이터가 null입니다!");
            return;
        }

        // 현재 아이템 가격 가져오기
        // 아이템 플레이어 돈 확인 후, 살 수 있으면 
        // 아이템 추가 
        int ItemPrice = currentItem.price;

        if (PlayerState.Instance.money >= ItemPrice)
        {
            PlayerState.Instance.RemoveMoney(ItemPrice);
            Debug.Log("플레이어 돈 차감:" + ItemPrice);

            //인벤토리 추가 로직
            InventoryManager.Instance.AddItem(currentItem);
            Debug.Log("구매 성공");

            purchaseUI.ShowpurchasePanel();
            return;

        }
        else
        {
            purchaseUI.ShowFailPanel();
            Debug.Log("플레이어 돈이 부족합니다.");
            return;
        }
    }
}
