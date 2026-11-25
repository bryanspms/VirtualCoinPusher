using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_6000_0_OR_NEWER
using PhysMat = UnityEngine.PhysicsMaterial;
#else
using PhysMat = UnityEngine.PhysicMaterial;
#endif

[InitializeOnLoad]
public static class CoinPusherSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/CoinPusher.unity";

    static CoinPusherSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(ScenePath) && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                BuildScene();
            }
        };
    }

    [MenuItem("Tools/Build Coin Pusher Scene")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play Mode first before running Tools -> Build Coin Pusher Scene.");
            return;
        }
        EnsureFolder("Assets/Textures");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Physics Material
#if UNITY_6000_0_OR_NEWER
        string physMatPath = "Assets/Materials/CoinPhysMat.physicsMaterial";
#else
        string physMatPath = "Assets/Materials/CoinPhysMat.physicMaterial";
#endif
        PhysMat coinPhysMat = AssetDatabase.LoadAssetAtPath<PhysMat>(physMatPath);
        if (coinPhysMat == null)
        {
            coinPhysMat = new PhysMat("CoinPhysMat");
            AssetDatabase.CreateAsset(coinPhysMat, physMatPath);
        }
        coinPhysMat.dynamicFriction = 0.22f;
        coinPhysMat.staticFriction = 0.25f;
        coinPhysMat.bounciness = 0.05f;
        EditorUtility.SetDirty(coinPhysMat);

        // Generate Procedural Textures
        Texture2D texQuarter = ArcadeTextureGenerator.CreateQuarterTexture("Assets/Textures/Quarter.png");
        Texture2D texCash = ArcadeTextureGenerator.CreateCashTexture("Assets/Textures/Cash.png");
        Texture2D texGoldBar = ArcadeTextureGenerator.CreateGoldBarTexture("Assets/Textures/GoldBar.png");
        Texture2D texChip10k = ArcadeTextureGenerator.CreatePokerChipTexture("Assets/Textures/Chip10k.png", new Color(0.78f, 0.12f, 0.14f), Color.white, new Color(0.95f, 0.92f, 0.86f));
        Texture2D texChip25k = ArcadeTextureGenerator.CreatePokerChipTexture("Assets/Textures/Chip25k.png", new Color(0.08f, 0.56f, 0.24f), new Color(1f, 0.85f, 0.2f), new Color(0.95f, 0.95f, 0.92f));
        Texture2D texWhiteChip = ArcadeTextureGenerator.CreatePokerChipTexture("Assets/Textures/WhiteChip.png", new Color(0.94f, 0.94f, 0.94f), new Color(0.15f, 0.42f, 0.88f), Color.white);
        Texture2D texGoldChip = ArcadeTextureGenerator.CreatePokerChipTexture("Assets/Textures/GoldChip.png", new Color(0.96f, 0.76f, 0.16f), Color.white, new Color(1f, 0.92f, 0.55f));
        Texture2D texFiftyChip = ArcadeTextureGenerator.CreatePokerChipTexture("Assets/Textures/FiftyChip.png", new Color(0.45f, 0.12f, 0.72f), new Color(1f, 0.82f, 0.2f), new Color(0.95f, 0.90f, 1f));
        Texture2D texDeck = ArcadeTextureGenerator.CreateBrushedMetalTexture("Assets/Textures/PlayfieldDeck.png", new Color(0.12f, 0.15f, 0.20f), true);
        Texture2D texSteel = ArcadeTextureGenerator.CreateBrushedMetalTexture("Assets/Textures/BrushedSteel.png", new Color(0.60f, 0.64f, 0.70f), false);
        Texture2D texCabinet = ArcadeTextureGenerator.CreateBrushedMetalTexture("Assets/Textures/CabinetWood.png", new Color(0.10f, 0.11f, 0.15f), false);

        // Materials
        Material matCabinet = CreateMat("Cabinet", Color.white, 0.25f, 0.45f, texCabinet);
        Material matDeck = CreateMat("PlayfieldDeck", Color.white, 0.45f, 0.65f, texDeck);
        Material matPusher = CreateMat("Pusher", Color.white, 0.78f, 0.72f, texSteel);
        Material matChrome = CreateMat("Chrome", new Color(0.92f, 0.94f, 0.98f), 0.95f, 0.92f);
        Material matGlass = CreateGlassMat("ArcadeGlass", new Color(0.75f, 0.90f, 1.0f, 0.14f));
        Material matNeonCyan = CreateEmissiveMat("NeonCyan", new Color(0.1f, 0.85f, 1f), 2.4f);
        Material matNeonGold = CreateEmissiveMat("NeonGold", new Color(1f, 0.72f, 0.12f), 2.2f);
        Material matNeonRed = CreateEmissiveMat("NeonRed", new Color(1f, 0.2f, 0.25f), 2.0f);

        Material matQuarter = CreateMat("Quarter", Color.white, 0.90f, 0.78f, texQuarter);
        Material matCash = CreateMat("Cash", Color.white, 0.05f, 0.20f, texCash);
        Material matGold = CreateMat("Gold", Color.white, 0.95f, 0.85f, texGoldBar);
        Material matChip10k = CreateMat("Chip10k", Color.white, 0.15f, 0.45f, texChip10k);
        Material matChip25k = CreateMat("Chip25k", Color.white, 0.15f, 0.45f, texChip25k);
        Material matWhiteChip = CreateMat("WhiteChip", Color.white, 0.20f, 0.55f, texWhiteChip);
        Material matGoldChip = CreateMat("GoldChip", Color.white, 0.82f, 0.78f, texGoldChip);
        Material matFiftyChip = CreateMat("FiftyChip", Color.white, 0.35f, 0.62f, texFiftyChip);

        Material matEarWhite = CreateMat("EarbudsWhite", new Color(0.96f, 0.96f, 0.98f), 0.15f, 0.88f);
        Material matEarYellow = CreateMat("EarbudsYellow", new Color(0.98f, 0.84f, 0.12f), 0.15f, 0.88f);
        Material matEarGreen = CreateMat("EarbudsGreen", new Color(0.12f, 0.80f, 0.34f), 0.15f, 0.88f);
        Material matEarRed = CreateMat("EarbudsRed", new Color(0.88f, 0.14f, 0.16f), 0.15f, 0.88f);
        Material matLedGreen = CreateEmissiveMat("EarbudLED", new Color(0.2f, 1f, 0.35f), 2.0f);

        // Ambient & Arcade Lighting Rig
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.24f, 0.26f, 0.32f);

        GameObject lightRig = new GameObject("ArcadeLighting");

        // Overhead warm key light
        GameObject keyObj = new GameObject("KeyLight");
        keyObj.transform.SetParent(lightRig.transform);
        Light keyLight = keyObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = new Color(1.0f, 0.96f, 0.88f);
        keyLight.intensity = 1.15f;
        keyLight.shadows = LightShadows.Soft;
        keyObj.transform.rotation = Quaternion.Euler(52f, -18f, 0f);

        // Overhead cabinet spotlight directly onto coins
        CreatePointLight("PlayfieldOverheadLight", lightRig.transform, new Vector3(0f, 4.8f, -0.4f), new Color(1f, 0.94f, 0.80f), 14f, 11f);
        // Subtle neon accent fill lights
        CreatePointLight("LeftNeonLight", lightRig.transform, new Vector3(-2.6f, 3.2f, -1.6f), new Color(0.25f, 0.85f, 1.0f), 4.5f, 7f);
        CreatePointLight("RightNeonLight", lightRig.transform, new Vector3(2.6f, 3.2f, -1.6f), new Color(1.0f, 0.72f, 0.20f), 4.5f, 7f);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 46f;
        camObj.AddComponent<AudioListener>();
        CameraController camCtrl = camObj.AddComponent<CameraController>();
        camObj.transform.position = camCtrl.playfieldPos;
        camObj.transform.eulerAngles = camCtrl.playfieldEuler;

        // Machine Root
        GameObject machine = new GameObject("CoinPusherMachine");

        // Playfield Decks
        CreateBox("LowerPlayfield", machine.transform, new Vector3(0f, 1.0f, -1.2f), new Vector3(5.2f, 0.4f, 6.0f), matDeck, coinPhysMat);
        CreateBox("FrontChromeLip", machine.transform, new Vector3(0f, 1.18f, -4.22f), new Vector3(5.25f, 0.08f, 0.08f), matChrome, coinPhysMat);

        // Arcade Cabinet Frame & Walls
        CreateBox("BackWall", machine.transform, new Vector3(0f, 3.8f, 2.35f), new Vector3(6.2f, 5.6f, 0.3f), matCabinet, coinPhysMat);
        CreateBox("LeftOuterWall", machine.transform, new Vector3(-3.15f, 3.0f, -1.0f), new Vector3(0.32f, 5.2f, 7.2f), matCabinet, coinPhysMat);
        CreateBox("RightOuterWall", machine.transform, new Vector3(3.15f, 3.0f, -1.0f), new Vector3(0.32f, 5.2f, 7.2f), matCabinet, coinPhysMat);
        CreateBox("TopMarqueeHeader", machine.transform, new Vector3(0f, 6.2f, 0.8f), new Vector3(6.6f, 0.7f, 3.4f), matCabinet, coinPhysMat);
        CreateDecorBox("MarqueeNeonBar", machine.transform, new Vector3(0f, 5.95f, -0.92f), new Vector3(5.8f, 0.14f, 0.08f), matNeonGold);

        // Glowing LED Pillars & Chrome Trim
        CreateDecorBox("LeftNeonStrip", machine.transform, new Vector3(-2.96f, 3.4f, -4.45f), new Vector3(0.10f, 4.6f, 0.10f), matNeonCyan);
        CreateDecorBox("RightNeonStrip", machine.transform, new Vector3(2.96f, 3.4f, -4.45f), new Vector3(0.10f, 4.6f, 0.10f), matNeonGold);
        CreateDecorBox("LeftBankerGlow", machine.transform, new Vector3(-2.85f, 1.02f, -1.3f), new Vector3(0.12f, 0.06f, 5.6f), matNeonRed);
        CreateDecorBox("RightBankerGlow", machine.transform, new Vector3(2.85f, 1.02f, -1.3f), new Vector3(0.12f, 0.06f, 5.6f), matNeonRed);

        // Physical Front Hopper Tray (visual arcade base)
        CreateDecorBox("CabinetBaseFront", machine.transform, new Vector3(0f, -0.15f, -3.3f), new Vector3(6.5f, 1.9f, 2.2f), matCabinet);
        CreateDecorBox("HopperTrayBack", machine.transform, new Vector3(0f, 0.25f, -4.55f), new Vector3(5.4f, 0.9f, 0.12f), matPusher);
        CreateDecorBox("HopperTrayBottom", machine.transform, new Vector3(0f, -0.22f, -5.05f), new Vector3(5.4f, 0.12f, 1.1f), matChrome);
        CreateDecorBox("HopperTrayFrontLip", machine.transform, new Vector3(0f, 0.05f, -5.58f), new Vector3(5.4f, 0.55f, 0.12f), matChrome);
        CreateDecorBox("HopperNeonAccent", machine.transform, new Vector3(0f, 0.34f, -5.58f), new Vector3(5.2f, 0.06f, 0.14f), matNeonCyan);

        // Subtle Transparent Front Glass Pane (Visual only, no collider so it doesn't interfere with view/raycasts)
        CreateDecorBox("FrontArcadeGlass", machine.transform, new Vector3(0f, 3.7f, -4.52f), new Vector3(5.8f, 4.4f, 0.04f), matGlass);

        // Side Shelves (cleared by White Earbuds)
        GameObject leftShelf = CreateBox("LeftSideShelf", machine.transform, new Vector3(-2.65f, 1.65f, -1.3f), new Vector3(0.6f, 0.15f, 4.6f), matPusher, coinPhysMat);
        GameObject rightShelf = CreateBox("RightSideShelf", machine.transform, new Vector3(2.65f, 1.65f, -1.3f), new Vector3(0.6f, 0.15f, 4.6f), matPusher, coinPhysMat);

        // Reciprocating Pusher Shelf (deep enough to stay sealed against BackWall, flush with LowerPlayfield)
        GameObject pusher = CreateBox("PusherShelf", machine.transform, new Vector3(0f, 1.47f, 2.10f), new Vector3(5.18f, 0.56f, 3.6f), matPusher, coinPhysMat);
        GameObject lip = CreateDecorBox("PusherChromeLip", pusher.transform, Vector3.zero, Vector3.one, matChrome);
        lip.transform.localPosition = new Vector3(0f, 0f, -0.505f);
        lip.transform.localScale = new Vector3(1.005f, 0.28f, 0.02f);
        PusherController pc = pusher.AddComponent<PusherController>();
        pc.strokeDistance = 1.45f;

        // Plinko Pegboard + Invisible Front Retention Wall
        GameObject plinkoRoot = new GameObject("PlinkoBoard");
        plinkoRoot.transform.SetParent(machine.transform);
        for (int row = 0; row < 4; row++)
        {
            int count = (row % 2 == 0) ? 7 : 6;
            float spacing = 0.72f;
            float startX = -((count - 1) * spacing) * 0.5f;
            float y = 4.6f - (row * 0.55f);
            for (int c = 0; c < count; c++)
            {
                GameObject peg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                peg.name = $"Peg_{row}_{c}";
                peg.transform.SetParent(plinkoRoot.transform);
                peg.transform.position = new Vector3(startX + c * spacing, y, 1.92f);
                peg.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                peg.transform.localScale = new Vector3(0.14f, 0.25f, 0.14f);
                peg.GetComponent<Renderer>().sharedMaterial = matChrome;
            }
        }
        plinkoRoot.SetActive(false);

        // Glass chute between drop zone and moving shelf (gap = 0.54 units, well under quarter circumference ~1.13)
        CreateBox("DropChuteGlass", machine.transform, new Vector3(0f, 3.72f, 1.64f), new Vector3(5.4f, 3.76f, 0.04f), matGlass, coinPhysMat);

        // Drop Indicator (Glowing Arcade Coin Slot Marker)
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        indicator.name = "DropIndicator";
        indicator.transform.SetParent(machine.transform);
        indicator.transform.localScale = new Vector3(0.42f, 0.12f, 0.16f);
        Object.DestroyImmediate(indicator.GetComponent<Collider>());
        indicator.GetComponent<Renderer>().sharedMaterial = matNeonGold;

        // Trigger Zones
        CreateTrigger("WinHopperZone", machine.transform, new Vector3(0f, 0.1f, -4.9f), new Vector3(6.2f, 1.2f, 1.5f), ZoneTrigger.ZoneKind.WinHopper);
        CreateTrigger("BankerSlotLeft", machine.transform, new Vector3(-2.85f, 0.2f, -1.4f), new Vector3(0.55f, 1.5f, 6.2f), ZoneTrigger.ZoneKind.BankerSlot);
        CreateTrigger("BankerSlotRight", machine.transform, new Vector3(2.85f, 0.2f, -1.4f), new Vector3(0.55f, 1.5f, 6.2f), ZoneTrigger.ZoneKind.BankerSlot);

        // Create Prefabs
        GameObject pfQuarter = CreateItemPrefab("Quarter", PrimitiveType.Cylinder, new Vector3(0.36f, 0.020f, 0.36f), 0.25f, matQuarter, coinPhysMat, ItemType.Quarter, ItemCategory.Quarter, "Quarter", 0.25f, false);
        GameObject pfCash = CreateItemPrefab("CashStack", PrimitiveType.Cube, new Vector3(0.75f, 0.14f, 0.38f), 0.45f, matCash, coinPhysMat, ItemType.CashStack, ItemCategory.Cash, "$20 Cash Stack", 20f, false);
        GameObject pfGoldBar = CreateItemPrefab("GoldBar1oz", PrimitiveType.Cube, new Vector3(0.65f, 0.20f, 0.34f), 1.2f, matGold, coinPhysMat, ItemType.GoldBar1oz, ItemCategory.GoldBar, "1oz Gold Bar", 2500f, false);
        GameObject pfChip10k = CreateItemPrefab("PokerChip10k", PrimitiveType.Cylinder, new Vector3(0.50f, 0.025f, 0.50f), 0.40f, matChip10k, coinPhysMat, ItemType.PokerChip10k, ItemCategory.PokerChip, "10,000 Poker Chip", 10000f, false);
        GameObject pfChip25k = CreateItemPrefab("PokerChip25k", PrimitiveType.Cylinder, new Vector3(0.50f, 0.025f, 0.50f), 0.40f, matChip25k, coinPhysMat, ItemType.PokerChip25k, ItemCategory.PokerChip, "25,000 Poker Chip", 25000f, false);
        GameObject pfWhiteChip = CreateItemPrefab("WhiteDollarChip", PrimitiveType.Cylinder, new Vector3(0.54f, 0.028f, 0.54f), 0.45f, matWhiteChip, coinPhysMat, ItemType.WhiteDollarChip, ItemCategory.SpecialItem, "White $1 Chip (+500 Q)", 125f, true);
        GameObject pfGoldChip = CreateItemPrefab("GoldDollarChip", PrimitiveType.Cylinder, new Vector3(0.54f, 0.028f, 0.54f), 0.45f, matGoldChip, coinPhysMat, ItemType.GoldDollarChip, ItemCategory.SpecialItem, "Gold $1 Chip (+1000 Q)", 250f, true);
        GameObject pfFiftyChip = CreateItemPrefab("FiftyDollarChip", PrimitiveType.Cylinder, new Vector3(0.58f, 0.030f, 0.58f), 0.55f, matFiftyChip, coinPhysMat, ItemType.FiftyDollarChip, ItemCategory.SpecialItem, "$50 Chip (Banker Dump)", 50f, true);

        // Earbud Cases (Solid closed case with lid seam & glowing LED indicator)
        GameObject pfEarWhite = CreateEarbudCasePrefab("EarbudsWhite", matEarWhite, matChrome, matLedGreen, coinPhysMat, ItemType.EarbudsWhite, "White Earbuds (Sweep Shelves)");
        GameObject pfEarYellow = CreateEarbudCasePrefab("EarbudsYellow", matEarYellow, matChrome, matLedGreen, coinPhysMat, ItemType.EarbudsYellow, "Yellow Earbuds (Gold Towers)");
        GameObject pfEarGreen = CreateEarbudCasePrefab("EarbudsGreen", matEarGreen, matChrome, matLedGreen, coinPhysMat, ItemType.EarbudsGreen, "Green Earbuds (25k Chip Towers)");
        GameObject pfEarRed = CreateEarbudCasePrefab("EarbudsRed", matEarRed, matChrome, matLedGreen, coinPhysMat, ItemType.EarbudsRed, "Red Earbuds (10k Chip Towers)");

        // Managers Root
        GameObject managers = new GameObject("GameManagers");
        GameObject activeItemsParent = new GameObject("ActiveItems");

        TowerSpawner spawner = managers.AddComponent<TowerSpawner>();
        spawner.activeItemsParent = activeItemsParent.transform;
        spawner.quarterPrefab = pfQuarter;
        spawner.cashPrefab = pfCash;
        spawner.goldBarPrefab = pfGoldBar;
        spawner.chip10kPrefab = pfChip10k;
        spawner.chip25kPrefab = pfChip25k;
        spawner.whiteDollarChipPrefab = pfWhiteChip;
        spawner.goldDollarChipPrefab = pfGoldChip;
        spawner.fiftyDollarChipPrefab = pfFiftyChip;
        spawner.earbudsWhitePrefab = pfEarWhite;
        spawner.earbudsYellowPrefab = pfEarYellow;
        spawner.earbudsGreenPrefab = pfEarGreen;
        spawner.earbudsRedPrefab = pfEarRed;

        managers.AddComponent<BankerSlotTracker>();
        managers.AddComponent<HopperManager>();

        SpecialItemResolver resolver = managers.AddComponent<SpecialItemResolver>();
        resolver.leftSideShelf = leftShelf.transform;
        resolver.rightSideShelf = rightShelf.transform;

        CoinDropper dropper = managers.AddComponent<CoinDropper>();
        dropper.plinkoBoardObject = plinkoRoot;
        dropper.dropIndicator = indicator.transform;

        GameManager gm = managers.AddComponent<GameManager>();
        gm.coinDropper = dropper;

        BuildUI(managers);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Virtual Coin Pusher (v3 Realistic Arcade) scene built successfully! Press Play.");
    }

    private static void BuildUI(GameObject managers)
    {
        Font uiFont = UIManager.GetBuiltinFont();

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        UIManager ui = managers.AddComponent<UIManager>();

        // Top HUD Bar
        GameObject topBar = CreatePanel("TopHUD", canvasObj.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -64), Vector2.zero, new Color(0.06f, 0.08f, 0.12f, 0.90f));
        var hlg = topBar.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(14, 14, 10, 10);
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ui.quartersText = CreateText("QuartersTxt", topBar.transform, "Quarters: 2000", uiFont, 18, new Color(1f, 0.88f, 0.25f), 155, 40);
        ui.moneyMadeText = CreateText("MoneyMadeTxt", topBar.transform, "Money Made: $0.00", uiFont, 18, new Color(0.35f, 1f, 0.55f), 205, 40);
        ui.hopperCountText = CreateText("HopperTxt", topBar.transform, "Hopper: 0", uiFont, 17, Color.white, 110, 40);
        ui.bankerCountText = CreateText("BankerTxt", topBar.transform, "Banker: 0", uiFont, 17, new Color(1f, 0.60f, 0.60f), 110, 40);

        ui.boardModeBtn = CreateButton("BoardModeBtn", topBar.transform, "Board: Deluxe", uiFont, 175, 40, new Color(0.18f, 0.34f, 0.58f));
        ui.boardModeBtnText = ui.boardModeBtn.GetComponentInChildren<Text>();

        ui.dropModeBtn = CreateButton("DropModeBtn", topBar.transform, "Drop: Standard", uiFont, 155, 40, new Color(0.16f, 0.46f, 0.48f));
        ui.dropModeBtnText = ui.dropModeBtn.GetComponentInChildren<Text>();

        ui.shelfToggleBtn = CreateButton("ShelfToggleBtn", topBar.transform, "Start Shelf", uiFont, 130, 40, new Color(0.68f, 0.22f, 0.26f));
        ui.shelfToggleBtnText = ui.shelfToggleBtn.GetComponentInChildren<Text>();

        ui.hopperViewBtn = CreateButton("HopperViewBtn", topBar.transform, "View Hopper (H)", uiFont, 155, 40, new Color(0.80f, 0.48f, 0.10f));

        // Toast Banner
        GameObject toastObj = CreatePanel("ToastBanner", canvasObj.transform, new Vector2(0.15f, 0.80f), new Vector2(0.85f, 0.88f), Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0));
        ui.toastText = CreateText("ToastTxt", toastObj.transform, "", uiFont, 20, new Color(0.4f, 1f, 0.55f), 800, 40);
        ui.toastText.alignment = TextAnchor.MiddleCenter;
        StretchRect(ui.toastText.GetComponent<RectTransform>());

        // Hopper Panel
        GameObject hopperPanel = CreatePanel("HopperPanel", canvasObj.transform, new Vector2(0.10f, 0.08f), new Vector2(0.90f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.10f, 0.14f, 0.96f));
        ui.hopperPanel = hopperPanel;

        var vlg = hopperPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(18, 18, 16, 16);
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        CreateText("HopperTitle", hopperPanel.transform, "WINNINGS HOPPER (Redeem Specials Anytime or Empty All)", uiFont, 22, new Color(1f, 0.86f, 0.25f), 700, 36);

        // Filter Row
        GameObject filterRow = new GameObject("FilterRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        filterRow.transform.SetParent(hopperPanel.transform, false);
        var flg = filterRow.GetComponent<HorizontalLayoutGroup>();
        flg.spacing = 8;
        flg.childControlWidth = true;
        flg.childControlHeight = true;
        flg.childForceExpandWidth = false;
        flg.childForceExpandHeight = false;
        filterRow.AddComponent<LayoutElement>().preferredHeight = 38;

        ui.filterAllBtn = CreateButton("Filter_All", filterRow.transform, "All", uiFont, 85, 36, new Color(0.24f, 0.28f, 0.38f));
        ui.filterQuartersBtn = CreateButton("Filter_Quarters", filterRow.transform, "Quarters", uiFont, 100, 36, new Color(0.24f, 0.28f, 0.38f));
        ui.filterCashBtn = CreateButton("Filter_Cash", filterRow.transform, "Cash", uiFont, 85, 36, new Color(0.24f, 0.28f, 0.38f));
        ui.filterChipsBtn = CreateButton("Filter_Chips", filterRow.transform, "Chips", uiFont, 85, 36, new Color(0.24f, 0.28f, 0.38f));
        ui.filterSpecialsBtn = CreateButton("Filter_Specials", filterRow.transform, "Specials", uiFont, 100, 36, new Color(0.24f, 0.28f, 0.38f));
        ui.emptyHopperBtn = CreateButton("EmptyHopperBtn", filterRow.transform, "EMPTY HOPPER", uiFont, 165, 36, new Color(0.18f, 0.68f, 0.32f));
        ui.closeHopperBtn = CreateButton("CloseHopperBtn", filterRow.transform, "Back to Game", uiFont, 140, 36, new Color(0.68f, 0.24f, 0.24f));

        // Scrollable List Area
        GameObject scrollObj = new GameObject("ScrollList", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObj.transform.SetParent(hopperPanel.transform, false);
        scrollObj.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.92f);
        var scrollLe = scrollObj.AddComponent<LayoutElement>();
        scrollLe.flexibleHeight = 1f;
        scrollLe.minHeight = 220f;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        StretchRect(viewport.GetComponent<RectTransform>());

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        var clg = content.GetComponent<VerticalLayoutGroup>();
        clg.spacing = 6;
        clg.padding = new RectOffset(8, 8, 8, 8);
        clg.childControlWidth = true;
        clg.childControlHeight = true;
        clg.childForceExpandWidth = true;
        clg.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.viewport = viewport.GetComponent<RectTransform>();
        sr.content = crt;
        sr.horizontal = false;
        sr.scrollSensitivity = 35f;
        ui.hopperListContent = content.transform;

        // Summary Box
        ui.hopperSummaryText = CreateText("SummaryTxt", hopperPanel.transform, "Empty the hopper to bank winnings and see itemized totals.", uiFont, 16, Color.white, 750, 115);

        hopperPanel.SetActive(false);

        // Game Over Panel
        GameObject goPanel = CreatePanel("GameOverPanel", canvasObj.transform, new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.70f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.06f, 0.08f, 0.96f));
        var golg = goPanel.AddComponent<VerticalLayoutGroup>();
        golg.padding = new RectOffset(20, 20, 20, 20);
        golg.spacing = 16;
        golg.childAlignment = TextAnchor.MiddleCenter;
        golg.childControlWidth = true;
        golg.childControlHeight = true;
        golg.childForceExpandWidth = true;
        golg.childForceExpandHeight = false;

        ui.gameOverMessageText = CreateText("GameOverMsg", goPanel.transform, "GAME OVER", uiFont, 22, Color.white, 480, 110);
        ui.gameOverMessageText.alignment = TextAnchor.MiddleCenter;

        ui.restartBtn = CreateButton("RestartBtn", goPanel.transform, "Restart Game", uiFont, 200, 46, new Color(0.2f, 0.65f, 0.32f));

        ui.gameOverPanel = goPanel;
        goPanel.SetActive(false);

        // Start Prompt Panel (asks player how many starting quarters they want)
        GameObject startPanel = CreatePanel("StartPromptPanel", canvasObj.transform, new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.11f, 0.17f, 0.97f));
        var slg = startPanel.AddComponent<VerticalLayoutGroup>();
        slg.padding = new RectOffset(24, 24, 24, 24);
        slg.spacing = 16;
        slg.childAlignment = TextAnchor.MiddleCenter;
        slg.childControlWidth = true;
        slg.childControlHeight = true;
        slg.childForceExpandWidth = true;
        slg.childForceExpandHeight = false;

        Text promptTitle = CreateText("StartPromptTitle", startPanel.transform, "VIRTUAL COIN PUSHER", uiFont, 24, new Color(1f, 0.88f, 0.25f), 460, 38);
        promptTitle.alignment = TextAnchor.MiddleCenter;

        Text promptSub = CreateText("StartPromptSub", startPanel.transform, "How many quarters would you like to start with?\n(The moving shelf starts OFF — click 'Start Shelf' when ready.)", uiFont, 16, Color.white, 460, 48);
        promptSub.alignment = TextAnchor.MiddleCenter;

        // InputField for Starting Quarters
        GameObject inputObj = new GameObject("StartingQuartersInput", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(LayoutElement));
        inputObj.transform.SetParent(startPanel.transform, false);
        inputObj.GetComponent<Image>().color = new Color(0.16f, 0.20f, 0.28f, 1f);
        var inLe = inputObj.GetComponent<LayoutElement>();
        inLe.preferredWidth = 220;
        inLe.preferredHeight = 44;

        GameObject inTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        inTextObj.transform.SetParent(inputObj.transform, false);
        Text inText = inTextObj.GetComponent<Text>();
        inText.font = uiFont;
        inText.fontSize = 22;
        inText.color = Color.white;
        inText.alignment = TextAnchor.MiddleCenter;
        inText.supportRichText = false;
        StretchRect(inTextObj.GetComponent<RectTransform>());

        InputField inputField = inputObj.GetComponent<InputField>();
        inputField.textComponent = inText;
        inputField.contentType = InputField.ContentType.IntegerNumber;
        inputField.text = "2000";

        ui.startingQuartersInput = inputField;
        ui.startGameBtn = CreateButton("StartGameBtn", startPanel.transform, "Start Game", uiFont, 220, 46, new Color(0.18f, 0.68f, 0.34f));
        ui.startPromptPanel = startPanel;
        startPanel.SetActive(true);
    }

    private static void CreatePointLight(string name, Transform parent, Vector3 pos, Color color, float intensity, float range)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        Light l = obj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        obj.GetComponent<Image>().color = color;
        return obj;
    }

    private static Text CreateText(string name, Transform parent, string text, Font font, int size, Color color, float prefWidth, float prefHeight)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        Text t = obj.GetComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleLeft;
        var le = obj.GetComponent<LayoutElement>();
        le.preferredWidth = prefWidth;
        le.preferredHeight = prefHeight;
        return t;
    }

    private static Button CreateButton(string name, Transform parent, string label, Font font, float width, float height, Color bg)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<Image>().color = bg;
        var le = obj.GetComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(obj.transform, false);
        Text t = txtObj.GetComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 15;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        StretchRect(txtObj.GetComponent<RectTransform>());

        return obj.GetComponent<Button>();
    }

    private static void StretchRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, PhysMat physMat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        obj.GetComponent<Collider>().sharedMaterial = physMat;
        return obj;
    }

    private static GameObject CreateDecorBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static void CreateTrigger(string name, Transform parent, Vector3 pos, Vector3 scale, ZoneTrigger.ZoneKind kind)
    {
        GameObject obj = new GameObject(name, typeof(BoxCollider), typeof(ZoneTrigger));
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        BoxCollider bc = obj.GetComponent<BoxCollider>();
        bc.size = scale;
        bc.isTrigger = true;
        obj.GetComponent<ZoneTrigger>().zoneKind = kind;
    }

    private static GameObject CreateItemPrefab(string name, PrimitiveType prim, Vector3 scale, float mass, Material mat, PhysMat physMat,
        ItemType type, ItemCategory cat, string displayName, float dollarValue, bool isSpecial)
    {
        GameObject obj = GameObject.CreatePrimitive(prim);
        obj.name = name;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;

        if (prim == PrimitiveType.Cylinder)
        {
            Collider oldCol = obj.GetComponent<Collider>();
            if (oldCol != null) Object.DestroyImmediate(oldCol);
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.convex = true;
            mc.sharedMaterial = physMat;
        }
        else
        {
            obj.GetComponent<Collider>().sharedMaterial = physMat;
        }

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        CollectibleItem item = obj.AddComponent<CollectibleItem>();
        item.itemType = type;
        item.category = cat;
        item.displayName = displayName;
        item.dollarValue = dollarValue;
        item.isSpecialRedeemable = isSpecial;

        string path = $"Assets/Prefabs/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
        return prefab;
    }

    private static GameObject CreateEarbudCasePrefab(string name, Material caseMat, Material seamMat, Material ledMat, PhysMat physMat, ItemType type, string displayName)
    {
        GameObject root = new GameObject(name);
        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.size = new Vector3(0.46f, 0.34f, 0.24f);
        bc.sharedMaterial = physMat;

        // Rounded capsule-like body (single solid rigidbody unit)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "CaseBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.44f, 0.32f, 0.22f);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().sharedMaterial = caseMat;

        // Lid seam line
        GameObject seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seam.name = "LidSeam";
        seam.transform.SetParent(root.transform, false);
        seam.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        seam.transform.localScale = new Vector3(0.446f, 0.018f, 0.226f);
        Object.DestroyImmediate(seam.GetComponent<Collider>());
        seam.GetComponent<Renderer>().sharedMaterial = seamMat;

        // Tiny charging LED dot on front
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        led.name = "StatusLED";
        led.transform.SetParent(root.transform, false);
        led.transform.localPosition = new Vector3(0f, -0.02f, -0.112f);
        led.transform.localScale = Vector3.one * 0.032f;
        Object.DestroyImmediate(led.GetComponent<Collider>());
        led.GetComponent<Renderer>().sharedMaterial = ledMat;

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 0.6f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        CollectibleItem item = root.AddComponent<CollectibleItem>();
        item.itemType = type;
        item.category = ItemCategory.SpecialItem;
        item.displayName = displayName;
        item.dollarValue = 0f;
        item.isSpecialRedeemable = true;

        string path = $"Assets/Prefabs/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Material CreateMat(string name, Color color, float metallic = 0.1f, float smoothness = 0.35f, Texture2D mainTex = null)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }
        m.color = color;
        if (mainTex != null)
        {
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", mainTex);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", mainTex);
        }
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static Material CreateEmissiveMat(string name, Color color, float intensity)
    {
        Material m = CreateMat(name, color, 0.1f, 0.8f);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * intensity);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static Material CreateGlassMat(string name, Color color)
    {
        Material m = CreateMat(name, color, 0.1f, 0.95f);
        // Support both Built-In Standard & URP Lit transparency
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f); // URP Transparent
            m.SetFloat("_Blend", 0f);
        }
        else if (m.HasProperty("_Mode"))
        {
            m.SetFloat("_Mode", 3f); // Standard Transparent
        }
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        EditorUtility.SetDirty(m);
        return m;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
