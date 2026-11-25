using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    public enum ZoneKind { WinHopper, BankerSlot }
    public ZoneKind zoneKind;

    private void OnTriggerEnter(Collider other)
    {
        CollectibleItem item = other.GetComponentInParent<CollectibleItem>();
        if (item == null) return;
        if (!item.TryMarkProcessed()) return;

        if (zoneKind == ZoneKind.WinHopper)
        {
            if (HopperManager.Instance != null)
            {
                HopperManager.Instance.AddItemToHopper(item);
            }
        }
        else if (zoneKind == ZoneKind.BankerSlot)
        {
            if (BankerSlotTracker.Instance != null)
            {
                BankerSlotTracker.Instance.RecordLostItem(item.itemType);
            }
        }

        if (item.itemType == ItemType.FiftyDollarChip && GameManager.Instance != null)
        {
            GameManager.Instance.NotifyFiftyChipRemovedFromBoard();
        }

        Destroy(item.gameObject);
    }
}
