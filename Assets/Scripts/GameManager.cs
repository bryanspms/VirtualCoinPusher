using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum BoardMode { QuartersOnly, Deluxe }

    public BoardMode currentBoardMode = BoardMode.Deluxe;
    public int startingQuarters = 2000;
    public int quartersRemaining;
    public bool isGameOver = false;
    public bool isWaitingForStart = true;

    public CoinDropper coinDropper;

    void Awake()
    {
        Instance = this;
        Physics.defaultSolverIterations = 16;
        Physics.defaultSolverVelocityIterations = 4;
    }

    void Start()
    {
        StartNewGame(currentBoardMode, coinDropper != null ? coinDropper.currentDropMode : CoinDropper.DropMode.Standard, true);
    }

    public void BeginSessionWithQuarters(int quarters)
    {
        startingQuarters = Mathf.Max(1, quarters);
        quartersRemaining = startingQuarters;
        isWaitingForStart = false;
        isGameOver = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetStartPromptVisible(false);
            UIManager.Instance.RefreshHUD();
        }
    }

    public void StartNewGame(BoardMode boardMode, CoinDropper.DropMode dropMode, bool promptForQuarters = false)
    {
        isWaitingForStart = promptForQuarters;
        currentBoardMode = boardMode;
        quartersRemaining = startingQuarters;
        isGameOver = false;

        if (coinDropper != null) coinDropper.ApplyDropMode(dropMode);
        // Start with the moving shelf OFF to keep towers stable
        if (PusherController.Instance != null) PusherController.Instance.SetPaused(true);
        if (TowerSpawner.Instance != null) TowerSpawner.Instance.ClearAllActiveItems();
        if (HopperManager.Instance != null) HopperManager.Instance.ResetHopper();
        if (BankerSlotTracker.Instance != null) BankerSlotTracker.Instance.ClearAll();
        if (CameraController.Instance != null) CameraController.Instance.SetPlayfieldView();

        SeedInitialBoard();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetGameOverVisible(false);
            UIManager.Instance.SetStartPromptVisible(promptForQuarters);
            UIManager.Instance.RefreshHUD();
            UIManager.Instance.RefreshHopperUI();
        }
    }

    private void SeedInitialBoard()
    {
        // Define tower exclusion zones (x, z, radius) so random quarters NEVER overlap tower bases
        List<Vector3> exclusionZones = new List<Vector3>();
        if (currentBoardMode == BoardMode.Deluxe)
        {
            exclusionZones.Add(new Vector3(-1.45f, -2.95f, 0.90f)); // Front-Left 4-leg
            exclusionZones.Add(new Vector3( 1.45f, -2.95f, 0.90f)); // Front-Right 3-leg
            exclusionZones.Add(new Vector3( 0.00f, -1.75f, 0.90f)); // Center-Mid Citadel
            exclusionZones.Add(new Vector3( 0.00f,  0.90f, 0.85f)); // Shelf Multi-leg
            exclusionZones.Add(new Vector3(-1.55f,  0.85f, 0.45f)); // Shelf Left Tower
            exclusionZones.Add(new Vector3( 1.55f,  0.85f, 0.45f)); // Shelf Right Tower
            exclusionZones.Add(new Vector3(-1.60f, -1.45f, 0.55f)); // Gold Stack
            exclusionZones.Add(new Vector3( 1.60f, -1.45f, 0.55f)); // Cash Stack
        }

        // Also keep quarters clear of the 2 front-edge White $1 chips
        exclusionZones.Add(new Vector3(-0.90f, -4.02f, 0.45f));
        exclusionZones.Add(new Vector3( 0.90f, -4.02f, 0.45f));

        // Seed base layer of quarters cleanly around the towers (non-overlapping, asleep on spawn)
        int spawned = 0;
        int attempts = 0;
        List<Vector2> placedQuarters = new List<Vector2>();

        while (spawned < 100 && attempts < 1200)
        {
            attempts++;
            float x = Random.Range(-2.20f, 2.20f);
            float z = Random.Range(-3.75f, 1.25f);

            // Avoid the step seam between lower playfield and moving pusher shelf
            if (z > -0.45f && z < 0.40f) continue;

            bool blocked = false;
            foreach (Vector3 zone in exclusionZones)
            {
                float dx = x - zone.x;
                float dz = z - zone.y;
                if (dx * dx + dz * dz < zone.z * zone.z)
                {
                    blocked = true;
                    break;
                }
            }
            if (blocked) continue;

            // Avoid overlapping other base quarters
            foreach (Vector2 q in placedQuarters)
            {
                float dx = x - q.x;
                float dz = z - q.y;
                if (dx * dx + dz * dz < 0.38f * 0.38f)
                {
                    blocked = true;
                    break;
                }
            }
            if (blocked) continue;

            placedQuarters.Add(new Vector2(x, z));
            float y = z >= 0.40f ? 1.773f : 1.223f;
            TowerSpawner.Instance.SpawnSingleItem(ItemType.Quarter, new Vector3(x, y, z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), true);
            spawned++;
        }

        // Seed cash on side shelves so White Earbuds have items to sweep into play
        TowerSpawner.Instance.SpawnSingleItem(ItemType.CashStack, new Vector3(-2.65f, 1.80f, -1.4f), Quaternion.identity, true);
        TowerSpawner.Instance.SpawnSingleItem(ItemType.CashStack, new Vector3(2.65f, 1.80f, -1.4f), Quaternion.identity, true);

        // At least 2 White $1 Poker Chips perched precariously over the front edge
        TowerSpawner.Instance.SpawnSingleItem(ItemType.WhiteDollarChip, new Vector3(-0.9f, 1.232f, -4.02f), Quaternion.identity, true);
        TowerSpawner.Instance.SpawnSingleItem(ItemType.WhiteDollarChip, new Vector3(0.9f, 1.232f, -4.02f), Quaternion.identity, true);

        // Special & high-value items to hide inside the elaborate chip towers
        List<ItemType> specialPool = new List<ItemType>
        {
            ItemType.GoldDollarChip,
            ItemType.FiftyDollarChip,
            ItemType.EarbudsWhite,
            ItemType.EarbudsYellow,
            ItemType.EarbudsGreen,
            ItemType.EarbudsRed,
            ItemType.GoldBar1oz,
            ItemType.GoldBar1oz,
            ItemType.CashStack
        };

        for (int i = 0; i < specialPool.Count; i++)
        {
            int r = Random.Range(i, specialPool.Count);
            ItemType tmp = specialPool[i];
            specialPool[i] = specialPool[r];
            specialPool[r] = tmp;
        }

        int poolIdx = 0;
        ItemType? NextHidden() => (poolIdx < specialPool.Count) ? specialPool[poolIdx++] : (ItemType?)null;

        if (currentBoardMode == BoardMode.Deluxe)
        {
            // 1. Elaborate 4-Leg Mega Fortress (Front-Left Lower Playfield)
            TowerSpawner.Instance.SpawnElaborateMultiLegTower(
                new Vector3(-1.45f, 1.202f, -2.95f),
                Random.value < 0.5f ? ItemType.PokerChip10k : ItemType.PokerChip25k,
                4,
                Random.Range(12, 18),
                Random.Range(8, 13),
                NextHidden(),
                NextHidden(),
                false);

            // 2. Elaborate 3-Leg Tall Tripod (Front-Right Lower Playfield)
            TowerSpawner.Instance.SpawnElaborateMultiLegTower(
                new Vector3(1.45f, 1.202f, -2.95f),
                Random.value < 0.5f ? ItemType.PokerChip10k : ItemType.PokerChip25k,
                3,
                Random.Range(13, 19),
                Random.Range(8, 13),
                NextHidden(),
                NextHidden(),
                false);

            // 3. Elaborate 4-Leg Grand Citadel (Center-Mid Lower Playfield)
            TowerSpawner.Instance.SpawnElaborateMultiLegTower(
                new Vector3(0.00f, 1.202f, -1.75f),
                Random.value < 0.5f ? ItemType.PokerChip10k : ItemType.PokerChip25k,
                Random.value < 0.5f ? 4 : 3,
                Random.Range(14, 20),
                Random.Range(9, 14),
                NextHidden(),
                NextHidden(),
                false);

            // 4. Elaborate Multi-Leg Tower right on the MOVING Pusher Shelf (Center)
            TowerSpawner.Instance.SpawnElaborateMultiLegTower(
                new Vector3(0.00f, 1.752f, 0.90f),
                Random.value < 0.5f ? ItemType.PokerChip10k : ItemType.PokerChip25k,
                Random.value < 0.5f ? 3 : 4,
                Random.Range(9, 14),
                Random.Range(6, 10),
                NextHidden(),
                null,
                true);

            // 5. Tall Single Chip Towers flanking the moving shelf, hiding remaining treasures
            TowerSpawner.Instance.SpawnSingleTowerAt(
                new Vector3(-1.55f, 1.752f, 0.85f),
                ItemType.PokerChip10k,
                Random.Range(11, 16),
                NextHidden());

            TowerSpawner.Instance.SpawnSingleTowerAt(
                new Vector3(1.55f, 1.752f, 0.85f),
                ItemType.PokerChip25k,
                Random.Range(11, 16),
                NextHidden());

            // Extra gold bar & cash stacks on the lower playfield
            TowerSpawner.Instance.SpawnSingleTowerAt(new Vector3(-1.60f, 1.202f, -1.45f), ItemType.GoldBar1oz, Random.Range(3, 6), null);
            TowerSpawner.Instance.SpawnSingleTowerAt(new Vector3(1.60f, 1.202f, -1.45f), ItemType.CashStack, Random.Range(3, 6), null);
        }
        else
        {
            // QuartersOnly mode still places the special items on the board ready to win
            for (int i = 0; i < specialPool.Count; i++)
            {
                float x = Random.Range(-1.8f, 1.8f);
                float z = Random.Range(-3.1f, -0.9f);
                TowerSpawner.Instance.SpawnSingleItem(specialPool[i], new Vector3(x, 1.45f, z), Quaternion.identity, true);
            }
        }
    }

    public void NotifyFiftyChipRemovedFromBoard()
    {
        // No-op: items do not respawn automatically during gameplay
    }

    public bool ConsumeQuarter()
    {
        if (quartersRemaining <= 0)
        {
            CheckGameOver();
            return false;
        }

        quartersRemaining--;
        if (UIManager.Instance != null) UIManager.Instance.RefreshHUD();

        if (quartersRemaining <= 0)
        {
            CheckGameOver();
        }
        return true;
    }

    public void AddQuarters(int amount)
    {
        quartersRemaining += amount;
        if (quartersRemaining > 0 && isGameOver)
        {
            isGameOver = false;
            if (UIManager.Instance != null) UIManager.Instance.SetGameOverVisible(false);
        }
        if (UIManager.Instance != null) UIManager.Instance.RefreshHUD();
    }

    public void CheckGameOver()
    {
        if (quartersRemaining > 0) return;

        bool canRedeem = HopperManager.Instance != null && HopperManager.Instance.HasRedeemableQuarterChips();
        isGameOver = true;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetGameOverVisible(true, canRedeem);
        }
    }
}
