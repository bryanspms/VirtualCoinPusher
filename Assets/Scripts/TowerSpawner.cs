using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    public static TowerSpawner Instance { get; private set; }

    [Header("Spawn Bounds (Lower Playfield)")]
    public Vector3 playfieldMin = new Vector3(-2.3f, 1.202f, -3.7f);
    public Vector3 playfieldMax = new Vector3(2.3f, 1.202f, -0.55f);

    [Header("Spawn Bounds (Moving Pusher Shelf)")]
    public Vector3 shelfMin = new Vector3(-1.9f, 1.752f, 0.45f);
    public Vector3 shelfMax = new Vector3(1.9f, 1.752f, 1.25f);

    [Header("Prefabs")]
    public GameObject quarterPrefab;
    public GameObject cashPrefab;
    public GameObject goldBarPrefab;
    public GameObject chip10kPrefab;
    public GameObject chip25kPrefab;
    public GameObject whiteDollarChipPrefab;
    public GameObject goldDollarChipPrefab;
    public GameObject fiftyDollarChipPrefab;
    public GameObject earbudsWhitePrefab;
    public GameObject earbudsYellowPrefab;
    public GameObject earbudsGreenPrefab;
    public GameObject earbudsRedPrefab;

    public Transform activeItemsParent;

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetPrefab(ItemType type)
    {
        switch (type)
        {
            case ItemType.Quarter: return quarterPrefab;
            case ItemType.CashStack: return cashPrefab;
            case ItemType.GoldBar1oz: return goldBarPrefab;
            case ItemType.PokerChip10k: return chip10kPrefab;
            case ItemType.PokerChip25k: return chip25kPrefab;
            case ItemType.WhiteDollarChip: return whiteDollarChipPrefab;
            case ItemType.GoldDollarChip: return goldDollarChipPrefab;
            case ItemType.FiftyDollarChip: return fiftyDollarChipPrefab;
            case ItemType.EarbudsWhite: return earbudsWhitePrefab;
            case ItemType.EarbudsYellow: return earbudsYellowPrefab;
            case ItemType.EarbudsGreen: return earbudsGreenPrefab;
            case ItemType.EarbudsRed: return earbudsRedPrefab;
            default: return quarterPrefab;
        }
    }

    public GameObject SpawnSingleItem(ItemType type, Vector3 position, Quaternion rotation, bool sleepOnSpawn = false)
    {
        GameObject prefab = GetPrefab(type);
        if (prefab == null) return null;
        GameObject obj = Instantiate(prefab, position, rotation, activeItemsParent);
        if (sleepOnSpawn && obj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.Sleep();
        }
        return obj;
    }

    public void SpawnTowers(ItemType type, int minTowers, int maxTowers, int minHeight, int maxHeight, bool onMovingShelf = false)
    {
        int towerCount = Random.Range(minTowers, maxTowers + 1);
        Vector3 minB = onMovingShelf ? shelfMin : playfieldMin;
        Vector3 maxB = onMovingShelf ? shelfMax : playfieldMax;
        bool isPokerChip = (type == ItemType.PokerChip10k || type == ItemType.PokerChip25k);

        for (int t = 0; t < towerCount; t++)
        {
            float padX = isPokerChip ? 0.65f : 0.35f;
            float padZ = isPokerChip ? 0.55f : 0.25f;
            float x = Random.Range(minB.x + padX, maxB.x - padX);
            float z = onMovingShelf
                ? Random.Range(0.80f, 1.00f)
                : Random.Range(minB.z + padZ, maxB.z - padZ);
            int stackCount = Random.Range(minHeight, maxHeight + 1);

            if (isPokerChip && Random.value < 0.60f)
            {
                int legs = Random.value < 0.5f ? 3 : 4;
                int upperH = Random.Range(6, 12);
                ItemType? bonusHidden = Random.value < 0.5f ? ItemType.GoldBar1oz : ItemType.CashStack;
                SpawnElaborateMultiLegTower(new Vector3(x, minB.y, z), type, legs, stackCount, upperH, bonusHidden, null, onMovingShelf);
            }
            else
            {
                SpawnSingleTowerAt(new Vector3(x, minB.y, z), type, stackCount, null);
            }
        }
    }

    public void SpawnSingleTowerAt(Vector3 basePos, ItemType type, int stackCount, ItemType? hiddenItem)
    {
        float itemHeight = GetStackHeight(type);
        float currentY = basePos.y;
        int hiddenLayer = hiddenItem.HasValue ? Random.Range(Mathf.Max(2, stackCount / 3), Mathf.Max(3, (stackCount * 2) / 3)) : -1;

        for (int i = 0; i < stackCount; i++)
        {
            if (i == hiddenLayer && hiddenItem.HasValue)
            {
                float hHeight = GetStackHeight(hiddenItem.Value);
                Vector3 hPos = new Vector3(basePos.x, currentY + (hHeight * 0.5f) + 0.002f, basePos.z);
                SpawnSingleItem(hiddenItem.Value, hPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
                currentY += hHeight + 0.003f;
            }
            else
            {
                Vector3 pos = new Vector3(basePos.x, currentY + (itemHeight * 0.5f) + 0.001f, basePos.z);
                SpawnSingleItem(type, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
                currentY += itemHeight + 0.002f;
            }
        }
    }

    /// <summary>
    /// Builds an elaborate 3-leg (tripod) or 4-leg (quad fortress) poker chip structure with
    /// non-overlapping bridging beams, a hollow hidden-item vault in the center, and an upper spire.
    /// All pieces start asleep so the structure remains rock-solid until gameplay affects it.
    /// </summary>
    public void SpawnElaborateMultiLegTower(
        Vector3 basePos,
        ItemType chipType,
        int legCount,
        int lowerLegHeight,
        int upperSpireHeight,
        ItemType? hiddenCoreItem,
        ItemType? hiddenUpperItem,
        bool compactForShelf = false)
    {
        legCount = (legCount <= 3) ? 3 : 4;
        float chipH = GetStackHeight(chipType);
        float stepY = chipH + 0.002f;
        float cashH = GetStackHeight(ItemType.CashStack);

        if (legCount == 4)
        {
            float half = compactForShelf ? 0.32f : 0.36f;
            Vector3[] legOffsets = new Vector3[]
            {
                new Vector3(-half, 0f, -half),
                new Vector3( half, 0f, -half),
                new Vector3( half, 0f,  half),
                new Vector3(-half, 0f,  half)
            };

            // 1. Build 4 outer chip legs
            for (int l = 0; l < 4; l++)
            {
                Vector3 legBase = basePos + legOffsets[l];
                for (int i = 0; i < lowerLegHeight; i++)
                {
                    Vector3 pos = new Vector3(legBase.x, basePos.y + (chipH * 0.5f) + (i * stepY), legBase.z);
                    SpawnSingleItem(chipType, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
                }
            }

            // 2. Hide treasure inside the hollow center core
            SpawnCoreTreasure(basePos, lowerLegHeight, hiddenCoreItem);

            // 3. Two non-overlapping side beams + level cross-beam
            float legTopY = basePos.y + (lowerLegHeight * stepY);
            float bridgeY = legTopY + (cashH * 0.5f) + 0.002f;
            SpawnSingleItem(ItemType.CashStack, new Vector3(basePos.x - half, bridgeY, basePos.z), Quaternion.Euler(0f, 90f, 0f), true);
            SpawnSingleItem(ItemType.CashStack, new Vector3(basePos.x + half, bridgeY, basePos.z), Quaternion.Euler(0f, 90f, 0f), true);

            float crossY = bridgeY + cashH + 0.003f;
            SpawnSingleItem(ItemType.CashStack, new Vector3(basePos.x, crossY, basePos.z), Quaternion.identity, true);

            // 4. Tall upper spire
            float spireBaseY = crossY + (cashH * 0.5f) + 0.002f;
            if (upperSpireHeight > 0)
            {
                SpawnSingleTowerAt(new Vector3(basePos.x, spireBaseY, basePos.z), chipType, upperSpireHeight, hiddenUpperItem);
            }
        }
        else
        {
            // 3-Leg Tripod: 2 front legs + 1 taller back leg so the cross-beam sits completely level with ZERO overlap
            float halfX = compactForShelf ? 0.30f : 0.34f;
            float frontZ = compactForShelf ? -0.26f : -0.30f;
            float backZ = compactForShelf ? 0.28f : 0.32f;

            Vector3 leg0 = basePos + new Vector3(-halfX, 0f, frontZ);
            Vector3 leg1 = basePos + new Vector3( halfX, 0f, frontZ);
            Vector3 leg2 = basePos + new Vector3( 0f,    0f, backZ);

            // Front two legs
            for (int i = 0; i < lowerLegHeight; i++)
            {
                float y = basePos.y + (chipH * 0.5f) + (i * stepY);
                SpawnSingleItem(chipType, new Vector3(leg0.x, y, leg0.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
                SpawnSingleItem(chipType, new Vector3(leg1.x, y, leg1.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
            }

            // Back leg is taller by ~cashH so its top matches the top of the front beam
            int extraChips = Mathf.RoundToInt(cashH / stepY);
            int backLegHeight = lowerLegHeight + extraChips;
            for (int i = 0; i < backLegHeight; i++)
            {
                float y = basePos.y + (chipH * 0.5f) + (i * stepY);
                SpawnSingleItem(chipType, new Vector3(leg2.x, y, leg2.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
            }

            // Hide treasure in center core
            SpawnCoreTreasure(basePos, lowerLegHeight, hiddenCoreItem);

            // Front beam spanning leg0 and leg1
            float frontTopY = basePos.y + (lowerLegHeight * stepY);
            float frontBeamY = frontTopY + (cashH * 0.5f) + 0.002f;
            SpawnSingleItem(ItemType.CashStack, new Vector3(basePos.x, frontBeamY, basePos.z + frontZ), Quaternion.identity, true);

            // Level cross-beam spanning from the front beam back to leg2
            float crossBaseY = Mathf.Max(frontBeamY + (cashH * 0.5f), basePos.y + (backLegHeight * stepY));
            float crossY = crossBaseY + (cashH * 0.5f) + 0.002f;
            SpawnSingleItem(ItemType.CashStack, new Vector3(basePos.x, crossY, basePos.z + (frontZ + backZ) * 0.5f), Quaternion.Euler(0f, 90f, 0f), true);

            // Tall upper spire
            float spireBaseY = crossY + (cashH * 0.5f) + 0.002f;
            if (upperSpireHeight > 0)
            {
                SpawnSingleTowerAt(new Vector3(basePos.x, spireBaseY, basePos.z), chipType, upperSpireHeight, hiddenUpperItem);
            }
        }
    }

    private void SpawnCoreTreasure(Vector3 basePos, int lowerLegHeight, ItemType? hiddenCoreItem)
    {
        if (!hiddenCoreItem.HasValue) return;

        int pedestalChips = Mathf.Clamp(lowerLegHeight / 4, 2, 4);
        float coreY = basePos.y;
        float qH = GetStackHeight(ItemType.Quarter);
        for (int i = 0; i < pedestalChips; i++)
        {
            SpawnSingleItem(ItemType.Quarter, new Vector3(basePos.x, coreY + (qH * 0.5f) + 0.001f, basePos.z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
            coreY += qH + 0.002f;
        }
        float hHeight = GetStackHeight(hiddenCoreItem.Value);
        SpawnSingleItem(hiddenCoreItem.Value, new Vector3(basePos.x, coreY + (hHeight * 0.5f) + 0.002f, basePos.z), Quaternion.Euler(0f, 45f, 0f), true);
    }

    public void DumpItemBurst(List<ItemType> itemsToDrop)
    {
        StartCoroutine(DumpRoutine(itemsToDrop));
    }

    private IEnumerator DumpRoutine(List<ItemType> itemsToDrop)
    {
        foreach (ItemType type in itemsToDrop)
        {
            float x = Random.Range(playfieldMin.x + 0.3f, playfieldMax.x - 0.3f);
            float z = Random.Range(playfieldMin.z + 0.5f, playfieldMax.z);
            Vector3 pos = new Vector3(x, 3.8f + Random.Range(0f, 0.8f), z);
            SpawnSingleItem(type, pos, Random.rotation, false);
            yield return new WaitForSeconds(0.03f);
        }
    }

    public float GetStackHeight(ItemType type)
    {
        switch (type)
        {
            case ItemType.EarbudsWhite:
            case ItemType.EarbudsYellow:
            case ItemType.EarbudsGreen:
            case ItemType.EarbudsRed: return 0.34f;
            case ItemType.GoldBar1oz: return 0.20f;
            case ItemType.CashStack: return 0.14f;
            case ItemType.FiftyDollarChip: return 0.060f;
            case ItemType.WhiteDollarChip:
            case ItemType.GoldDollarChip: return 0.056f;
            case ItemType.PokerChip10k:
            case ItemType.PokerChip25k: return 0.050f;
            default: return 0.040f;
        }
    }

    public void ClearAllActiveItems()
    {
        if (activeItemsParent == null) return;
        for (int i = activeItemsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(activeItemsParent.GetChild(i).gameObject);
        }
    }
}
