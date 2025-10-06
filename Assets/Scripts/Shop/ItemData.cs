using UnityEngine;

public class ItemData : ScriptableObject
{
    [Header("아이템 기본 정보")]
    public Sprite icon;
    public string itemName;
    public int price;
    public bool isStackable = false;
    public int maxStack;

    [Tooltip("기본 스택 크기")]
    public int defaultStackSize = 1;

    // 런타임용 스택 (저장되지 않음)
    [System.NonSerialized]
    private int _runtimeStack;
    
    public int currentStack
    {
        get => _runtimeStack;
        set => _runtimeStack = Mathf.Clamp(value, 0, maxStack);
    }

    [Header("사용 설정")]
    public bool canUseInInventory = false;

    private void OnEnable()
    {
        // 에디터에서 생성될 때 초기화
        ResetRuntimeStack();
    }

    public void ResetRuntimeStack()
    {
        _runtimeStack = defaultStackSize;
    }

    // 아이템 사용 효과 (오버라이드 가능)
    public virtual void UseInInventory()
    {
        if (!canUseInInventory) return;
    }
}
