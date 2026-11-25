using System.Collections.Generic;
using UnityEngine;

public class HopperEntry
{
    public ItemType itemType;
    public ItemCategory category;
    public string displayName;
    public float dollarValue;
    public bool isSpecialRedeemable;
}

public class HopperManager : MonoBehaviour
{
    public static HopperManager Instance { get; private set; }

    public List<HopperEntry> uncollectedItems = new List<HopperEntry>();

    public int bankedQuarters;
    public float bankedCash;
    public float bankedChips;
    public float bankedGold;
    public int bankedSpecialsCount;
    public float bankedGrandTotal;

    public float TotalMoneyMade
    {
        get
        {
            float uncollectedVal = 0f;
            foreach (var e in uncollectedItems)
            {
                if (!e.isSpecialRedeemable) uncollectedVal += e.dollarValue;
            }
            return bankedGrandTotal + uncollectedVal;
        }
    }

    public string lastEmptySummary = "Hopper has not been emptied yet.";

    void Awake()
    {
        Instance = this;
    }

    public void AddItemToHopper(CollectibleItem item)
    {
        uncollectedItems.Add(new HopperEntry
        {
            itemType = item.itemType,
            category = item.category,
            displayName = item.displayName,
            dollarValue = item.dollarValue,
            isSpecialRedeemable = item.isSpecialRedeemable
        });

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
            UIManager.Instance.RefreshHopperUI();
        }
    }

    public bool HasRedeemableQuarterChips()
    {
        foreach (var e in uncollectedItems)
        {
            if (e.itemType == ItemType.WhiteDollarChip || e.itemType == ItemType.GoldDollarChip)
                return true;
        }
        return false;
    }

    public void RedeemSpecialAtIndex(int index)
    {
        if (index < 0 || index >= uncollectedItems.Count) return;
        HopperEntry entry = uncollectedItems[index];
        if (!entry.isSpecialRedeemable) return;

        uncollectedItems.RemoveAt(index);
        bankedSpecialsCount++;

        if (SpecialItemResolver.Instance != null)
        {
            SpecialItemResolver.Instance.ExecuteSpecial(entry.itemType);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
            UIManager.Instance.RefreshHopperUI();
        }
    }

    public void EmptyHopperAndCollectAll()
    {
        int qCount = 0;
        float cashSum = 0f;
        float chipSum = 0f;
        float goldSum = 0f;
        int specialCount = 0;

        List<HopperEntry> remainingSpecials = new List<HopperEntry>();

        foreach (var entry in uncollectedItems)
        {
            if (entry.isSpecialRedeemable)
            {
                specialCount++;
                remainingSpecials.Add(entry);
                continue;
            }

            switch (entry.category)
            {
                case ItemCategory.Quarter:
                    qCount++;
                    break;
                case ItemCategory.Cash:
                    cashSum += entry.dollarValue;
                    break;
                case ItemCategory.PokerChip:
                    chipSum += entry.dollarValue;
                    break;
                case ItemCategory.GoldBar:
                    goldSum += entry.dollarValue;
                    break;
            }
        }

        uncollectedItems = remainingSpecials;

        if (qCount > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddQuarters(qCount);
        }

        float batchTotal = (qCount * 0.25f) + cashSum + chipSum + goldSum;

        bankedQuarters += qCount;
        bankedCash += cashSum;
        bankedChips += chipSum;
        bankedGold += goldSum;
        bankedGrandTotal += batchTotal;

        lastEmptySummary =
            $"COLLECTED FROM HOPPER:\n" +
            $"Quarters: {qCount} (${qCount * 0.25f:N2}) [Added to Balance]\n" +
            $"Cash: ${cashSum:N2}  |  Poker Chips: ${chipSum:N2}  |  Gold Bars: ${goldSum:N2}\n" +
            $"Special Items Waiting: {specialCount}\n" +
            $"Batch Total: ${batchTotal:N2}  |  All-Time Total: ${bankedGrandTotal:N2}";

        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshHUD();
            UIManager.Instance.RefreshHopperUI();
        }
    }

    public void ResetHopper()
    {
        uncollectedItems.Clear();
        bankedQuarters = 0;
        bankedCash = 0f;
        bankedChips = 0f;
        bankedGold = 0f;
        bankedSpecialsCount = 0;
        bankedGrandTotal = 0f;
        lastEmptySummary = "Hopper cleared.";
    }
}
