using System.Collections.Generic;
using UnityEngine;

public class BankerSlotTracker : MonoBehaviour
{
    public static BankerSlotTracker Instance { get; private set; }

    private List<ItemType> lostItems = new List<ItemType>();
    public int LostItemCount => lostItems.Count;

    void Awake()
    {
        Instance = this;
    }

    public void RecordLostItem(ItemType type)
    {
        lostItems.Add(type);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
        }
    }

    public List<ItemType> DrainAllLostItems()
    {
        List<ItemType> copy = new List<ItemType>(lostItems);
        lostItems.Clear();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
        }
        return copy;
    }

    public void ClearAll()
    {
        lostItems.Clear();
    }
}
