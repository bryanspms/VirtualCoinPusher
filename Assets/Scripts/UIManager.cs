using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD References")]
    public Text quartersText;
    public Text moneyMadeText;
    public Text hopperCountText;
    public Text bankerCountText;
    public Text boardModeBtnText;
    public Text dropModeBtnText;
    public Text shelfToggleBtnText;
    public Text toastText;

    [Header("HUD Buttons")]
    public Button boardModeBtn;
    public Button dropModeBtn;
    public Button shelfToggleBtn;
    public Button hopperViewBtn;

    [Header("Hopper Panel")]
    public GameObject hopperPanel;
    public Transform hopperListContent;
    public Text hopperSummaryText;
    public Button filterAllBtn;
    public Button filterQuartersBtn;
    public Button filterCashBtn;
    public Button filterChipsBtn;
    public Button filterSpecialsBtn;
    public Button emptyHopperBtn;
    public Button closeHopperBtn;

    [Header("Start Prompt Panel")]
    public GameObject startPromptPanel;
    public InputField startingQuartersInput;
    public Button startGameBtn;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text gameOverMessageText;
    public Button restartBtn;

    private string currentFilter = "All";
    private float toastClearTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        FixAllLayouts();

        if (boardModeBtn != null) boardModeBtn.onClick.AddListener(OnToggleBoardMode);
        if (dropModeBtn != null) dropModeBtn.onClick.AddListener(OnToggleDropMode);
        if (shelfToggleBtn != null) shelfToggleBtn.onClick.AddListener(OnToggleShelf);
        if (hopperViewBtn != null) hopperViewBtn.onClick.AddListener(OnToggleHopperView);

        if (filterAllBtn != null) filterAllBtn.onClick.AddListener(() => SetFilter("All"));
        if (filterQuartersBtn != null) filterQuartersBtn.onClick.AddListener(() => SetFilter("Quarters"));
        if (filterCashBtn != null) filterCashBtn.onClick.AddListener(() => SetFilter("Cash"));
        if (filterChipsBtn != null) filterChipsBtn.onClick.AddListener(() => SetFilter("Chips"));
        if (filterSpecialsBtn != null) filterSpecialsBtn.onClick.AddListener(() => SetFilter("Specials"));

        if (emptyHopperBtn != null) emptyHopperBtn.onClick.AddListener(OnEmptyHopperClicked);
        if (closeHopperBtn != null) closeHopperBtn.onClick.AddListener(OnToggleHopperView);
        if (restartBtn != null) restartBtn.onClick.AddListener(OnRestartClicked);
        if (startGameBtn != null) startGameBtn.onClick.AddListener(OnStartGameConfirmed);

        RefreshHUD();
    }

    private void FixAllLayouts()
    {
        if (hopperPanel == null) return;
        Canvas canvas = hopperPanel.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        foreach (var lg in canvas.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>(true))
        {
            lg.childControlWidth = true;
            lg.childControlHeight = true;
            if (lg is VerticalLayoutGroup)
            {
                lg.childForceExpandWidth = true;
                lg.childForceExpandHeight = false;
            }
            else
            {
                lg.childForceExpandWidth = false;
                lg.childForceExpandHeight = false;
            }
        }

        Transform title = hopperPanel.transform.Find("HopperTitle");
        if (title != null && title.TryGetComponent<LayoutElement>(out var titleLe))
        {
            titleLe.preferredHeight = 36f;
        }

        ScrollRect sr = hopperPanel.GetComponentInChildren<ScrollRect>(true);
        if (sr != null)
        {
            sr.scrollSensitivity = 35f;
        }
    }

    void Update()
    {
        if (toastText != null && !string.IsNullOrEmpty(toastText.text) && Time.time > toastClearTime)
        {
            toastText.text = "";
        }
    }

    public void ShowToast(string message)
    {
        if (toastText != null)
        {
            toastText.text = message;
            toastClearTime = Time.time + 4.5f;
        }
    }

    public void RefreshHUD()
    {
        if (GameManager.Instance != null && quartersText != null)
            quartersText.text = $"Quarters: {GameManager.Instance.quartersRemaining}";

        if (HopperManager.Instance != null && moneyMadeText != null)
            moneyMadeText.text = $"Money Made: ${HopperManager.Instance.TotalMoneyMade:N2}";

        if (HopperManager.Instance != null && hopperCountText != null)
            hopperCountText.text = $"Hopper: {HopperManager.Instance.uncollectedItems.Count}";

        if (BankerSlotTracker.Instance != null && bankerCountText != null)
            bankerCountText.text = $"Banker: {BankerSlotTracker.Instance.LostItemCount}";

        if (GameManager.Instance != null && boardModeBtnText != null)
            boardModeBtnText.text = $"Board: {GameManager.Instance.currentBoardMode}";

        if (GameManager.Instance != null && GameManager.Instance.coinDropper != null && dropModeBtnText != null)
            dropModeBtnText.text = $"Drop: {GameManager.Instance.coinDropper.currentDropMode}";

        if (PusherController.Instance != null && shelfToggleBtnText != null)
            shelfToggleBtnText.text = PusherController.Instance.IsPaused ? "Start Shelf" : "Stop Shelf";
    }

    public void OnToggleBoardMode()
    {
        if (GameManager.Instance == null) return;
        var next = GameManager.Instance.currentBoardMode == GameManager.BoardMode.Deluxe
            ? GameManager.BoardMode.QuartersOnly
            : GameManager.BoardMode.Deluxe;
        var drop = GameManager.Instance.coinDropper.currentDropMode;
        GameManager.Instance.StartNewGame(next, drop);
    }

    public void OnToggleDropMode()
    {
        if (GameManager.Instance == null || GameManager.Instance.coinDropper == null) return;
        var next = GameManager.Instance.coinDropper.currentDropMode == CoinDropper.DropMode.Standard
            ? CoinDropper.DropMode.Plinko
            : CoinDropper.DropMode.Standard;
        GameManager.Instance.coinDropper.ApplyDropMode(next);
        RefreshHUD();
    }

    public void OnToggleShelf()
    {
        if (PusherController.Instance != null)
        {
            PusherController.Instance.TogglePaused();
        }
    }

    public void OnToggleHopperView()
    {
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ToggleView();
        }
    }

    public void SetHopperPanelVisible(bool visible)
    {
        if (hopperPanel != null) hopperPanel.SetActive(visible);
        if (visible) RefreshHopperUI();
    }

    public void SetFilter(string filterName)
    {
        currentFilter = filterName;
        RefreshHopperUI();
    }

    public void OnEmptyHopperClicked()
    {
        if (HopperManager.Instance != null)
        {
            HopperManager.Instance.EmptyHopperAndCollectAll();
        }
    }

    public void OnStartGameConfirmed()
    {
        int chosen = 2000;
        if (startingQuartersInput != null && int.TryParse(startingQuartersInput.text, out int parsed) && parsed > 0)
        {
            chosen = parsed;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginSessionWithQuarters(chosen);
        }
    }

    public void SetStartPromptVisible(bool visible)
    {
        if (startPromptPanel != null) startPromptPanel.SetActive(visible);
        if (visible && startingQuartersInput != null && GameManager.Instance != null)
        {
            startingQuartersInput.text = GameManager.Instance.startingQuarters.ToString();
        }
    }

    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame(
                GameManager.Instance.currentBoardMode,
                GameManager.Instance.coinDropper.currentDropMode,
                true);
        }
    }

    public void SetGameOverVisible(bool visible, bool hasRedeemableChips = false)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(visible);
        if (gameOverMessageText != null)
        {
            gameOverMessageText.text = hasRedeemableChips
                ? "OUT OF QUARTERS!\nYou still have redeemable $1 Chips in your Hopper.\nOpen the Hopper (H) to redeem them, or Restart."
                : "GAME OVER\nYou ran out of quarters!";
        }
    }

    public void RefreshHopperUI()
    {
        if (hopperListContent == null || HopperManager.Instance == null) return;

        for (int i = hopperListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(hopperListContent.GetChild(i).gameObject);
        }

        Font uiFont = GetBuiltinFont();
        var items = HopperManager.Instance.uncollectedItems;

        for (int i = 0; i < items.Count; i++)
        {
            HopperEntry entry = items[i];
            if (!MatchesFilter(entry)) continue;

            int capturedIndex = i;
            GameObject row = new GameObject("Row_" + i, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(hopperListContent, false);
            row.GetComponent<Image>().color = entry.isSpecialRedeemable
                ? new Color(0.24f, 0.22f, 0.14f, 0.95f)
                : new Color(0.16f, 0.18f, 0.23f, 0.92f);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 38f;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 5, 5);
            hlg.spacing = 10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblObj.transform.SetParent(row.transform, false);
            Text lbl = lblObj.GetComponent<Text>();
            lbl.font = uiFont;
            lbl.fontSize = 15;
            lbl.color = entry.isSpecialRedeemable ? new Color(1f, 0.92f, 0.55f) : Color.white;
            lbl.alignment = TextAnchor.MiddleLeft;
            lbl.text = entry.isSpecialRedeemable
                ? $"[SPECIAL] {entry.displayName}"
                : $"{entry.displayName} (${entry.dollarValue:N2})";
            var lblLe = lblObj.AddComponent<LayoutElement>();
            lblLe.flexibleWidth = 1f;
            lblLe.preferredHeight = 28f;

            if (entry.isSpecialRedeemable)
            {
                GameObject btnObj = new GameObject("RedeemBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObj.transform.SetParent(row.transform, false);
                btnObj.GetComponent<Image>().color = new Color(0.2f, 0.72f, 0.35f, 1f);
                var btnLe = btnObj.AddComponent<LayoutElement>();
                btnLe.preferredWidth = 105f;
                btnLe.preferredHeight = 28f;

                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => HopperManager.Instance.RedeemSpecialAtIndex(capturedIndex));

                GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                btnTxtObj.transform.SetParent(btnObj.transform, false);
                Text btnTxt = btnTxtObj.GetComponent<Text>();
                btnTxt.font = uiFont;
                btnTxt.fontSize = 14;
                btnTxt.fontStyle = FontStyle.Bold;
                btnTxt.color = Color.white;
                btnTxt.alignment = TextAnchor.MiddleCenter;
                btnTxt.text = "REDEEM";
                RectTransform rt = btnTxtObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        if (hopperSummaryText != null)
        {
            hopperSummaryText.text = HopperManager.Instance.lastEmptySummary;
        }
    }

    private bool MatchesFilter(HopperEntry entry)
    {
        switch (currentFilter)
        {
            case "Quarters": return entry.category == ItemCategory.Quarter;
            case "Cash": return entry.category == ItemCategory.Cash;
            case "Chips": return entry.category == ItemCategory.PokerChip;
            case "Specials": return entry.isSpecialRedeemable;
            default: return true;
        }
    }

    public static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
