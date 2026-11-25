using System.Collections.Generic;
using UnityEngine;

public class SpecialItemResolver : MonoBehaviour
{
    public static SpecialItemResolver Instance { get; private set; }

    public Transform leftSideShelf;
    public Transform rightSideShelf;

    void Awake()
    {
        Instance = this;
    }

    public void ExecuteSpecial(ItemType type)
    {
        string msg = "";

        switch (type)
        {
            case ItemType.WhiteDollarChip:
                GameManager.Instance.AddQuarters(500);
                msg = "Redeemed White $1 Chip: +500 Quarters!";
                break;

            case ItemType.GoldDollarChip:
                GameManager.Instance.AddQuarters(1000);
                msg = "Redeemed Gold $1 Chip: +1,000 Quarters!";
                break;

            case ItemType.FiftyDollarChip:
                List<ItemType> dumpList = BankerSlotTracker.Instance.DrainAllLostItems();
                int lostCount = dumpList.Count;

                bool isJackpot = Random.value < 0.20f;
                int bonusQuarters = isJackpot ? Random.Range(45, 75) : Random.Range(10, 22);
                int bonusChips = isJackpot ? Random.Range(12, 24) : Random.Range(2, 6);

                for (int i = 0; i < bonusQuarters; i++) dumpList.Add(ItemType.Quarter);
                for (int i = 0; i < bonusChips; i++)
                {
                    dumpList.Add(Random.value < 0.5f ? ItemType.PokerChip10k : ItemType.PokerChip25k);
                }

                TowerSpawner.Instance.DumpItemBurst(dumpList);
                msg = isJackpot
                    ? $"$50 CHIP JACKPOT! Dumped {lostCount} banker items + {bonusQuarters} quarters & {bonusChips} chips!"
                    : $"Redeemed $50 Chip: Dumped {lostCount} banker items + {bonusQuarters} quarters & {bonusChips} chips!";
                break;

            case ItemType.EarbudsWhite:
                SweepSideShelves();
                msg = "White Earbuds: Swept side shelves into active play!";
                break;

            case ItemType.EarbudsYellow:
                TowerSpawner.Instance.SpawnTowers(ItemType.GoldBar1oz, 1, 3, 3, 7);
                msg = "Yellow Earbuds: Spawned 1-3 towers of 1oz Gold Bars!";
                break;

            case ItemType.EarbudsGreen:
                TowerSpawner.Instance.SpawnTowers(ItemType.PokerChip25k, 1, 3, 9, 15, Random.value < 0.35f);
                msg = "Green Earbuds: Spawned 1-3 towers of 25,000 Poker Chips!";
                break;

            case ItemType.EarbudsRed:
                TowerSpawner.Instance.SpawnTowers(ItemType.PokerChip10k, 1, 3, 9, 15, Random.value < 0.35f);
                msg = "Red Earbuds: Spawned 1-3 towers of 10,000 Poker Chips!";
                break;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowToast(msg);
        }
    }

    private void SweepSideShelves()
    {
        Collider[] hits = Physics.OverlapBox(new Vector3(0f, 2.2f, 0f), new Vector3(4.5f, 2.5f, 4.0f));
        foreach (Collider col in hits)
        {
            CollectibleItem item = col.GetComponentInParent<CollectibleItem>();
            if (item == null) continue;
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb == null) continue;

            if (Mathf.Abs(item.transform.position.x) > 2.15f)
            {
                float dirToCenter = -Mathf.Sign(item.transform.position.x);
                rb.AddForce(new Vector3(dirToCenter * 4.5f, 2.2f, -1.5f), ForceMode.Impulse);
            }
        }
    }
}
