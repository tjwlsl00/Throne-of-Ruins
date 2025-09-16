using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentShopManager : MonoBehaviour
{
    public GameObject shopPanel;
    public Transform slotParent;
    public GameObject shopSlotPrefab;
    public List<ItemData> shopItems; // 에디터에서 할당
    private List<Transform> activeSlots = new List<Transform>();
    private GridLayoutGroup gridLayout;

    void Start()
    {
        shopPanel.SetActive(false);
        gridLayout = slotParent.GetComponent<GridLayoutGroup>();
        CreateSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ToggleShop();
    }

    public void ToggleShop() => shopPanel.SetActive(!shopPanel.activeSelf);

    void CreateSlots()
    {
        // 기존 슬롯 정리
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear();

        foreach (var item in shopItems)
        {
            // 아이템 데이터가 null인 경우 건너뛰기
            if (item == null) continue;

            GameObject slotObj = Instantiate(shopSlotPrefab, slotParent);
            ShopSlot slot = slotObj.GetComponent<ShopSlot>();

            // 슬롯 초기화
            slot.SetSlot(item);
            activeSlots.Add(slotObj.transform);

            // 즉시 활성화 
            slotObj.SetActive(true);
            ForceEnableSlotComponents(slotObj);
        }
        StartCoroutine(DelayedLayoutUpdate());
    }

    // 슬롯 만든후 1프레임 대기 -> 정렬 
    IEnumerator DelayedLayoutUpdate()
    {
        yield return null;
        UpdateLayout();
    }

    // 정렬 하는 동안 시스템 잠시 온오프 
    void UpdateLayout()
    {
        // 레이아웃 그룹 일시 비활성화(갱신 최적화)
        if (gridLayout != null)
            gridLayout.enabled = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotParent.GetComponent<RectTransform>());

        // 레이아웃 그룹 일시 활성화
        if (gridLayout != null)
            gridLayout.enabled = true;
    }

    void ForceEnableSlotComponents(GameObject slot)
    {
        Graphic[] graphics = slot.GetComponentsInChildren<Graphic>(true);
        // 슬롯 안의 모든 그림, 글씨를 켜는 부분 
        foreach (var graphic in graphics)
        {
            graphic.enabled = true;
            graphic.SetAllDirty();
        }

        Button[] buttons = slot.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            button.enabled = true;
        }

        LayoutElement[] layoutElements = slot.GetComponentsInChildren<LayoutElement>(true);
        foreach (var element in layoutElements)
        {
            element.enabled = true;
        }
    }
}