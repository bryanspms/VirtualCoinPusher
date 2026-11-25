using UnityEngine;

public enum ItemCategory
{
    Quarter,
    Cash,
    PokerChip,
    GoldBar,
    SpecialItem
}

public enum ItemType
{
    Quarter,
    CashStack,
    GoldBar1oz,
    PokerChip10k,
    PokerChip25k,
    WhiteDollarChip,
    GoldDollarChip,
    FiftyDollarChip,
    EarbudsWhite,
    EarbudsYellow,
    EarbudsGreen,
    EarbudsRed
}

public class CollectibleItem : MonoBehaviour
{
    public ItemType itemType;
    public ItemCategory category;
    public string displayName;
    public float dollarValue;
    public bool isSpecialRedeemable;

    private bool hasBeenProcessed = false;

    public bool TryMarkProcessed()
    {
        if (hasBeenProcessed) return false;
        hasBeenProcessed = true;
        return true;
    }
}
