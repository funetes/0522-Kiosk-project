using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskOptionPopup
{
    public RectTransform Root { get; private set; }

    private Button hotButton = null;

    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private readonly Color paper;
    private readonly Color charcoal;
    private readonly Color sage;
    private readonly Color espresso;
    private readonly Color caramel;

    public CafeKioskOptionPopup(Transform parent, CafeKioskViewModel viewModel, Font font, Color paper, Color charcoal, Color sage, Color espresso, Color caramel, System.Action onAction)
    {
        this.viewModel = viewModel;
        this.viewModel.OnPendingOptionItemSet += OnPendingOptionItemSet;

        this.font = font;
        this.paper = paper;
        this.charcoal = charcoal;
        this.sage = sage;
        this.espresso = espresso;
        this.caramel = caramel;

        Build2(parent, onAction);
    }

    private void Build(Transform parent, System.Action onAction)
    {
        Root = CafeKioskUIUtility.Panel("Drink Option Overlay", parent, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        var modal = CafeKioskUIUtility.Panel("Drink Option Panel", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.27f, 0.13f, 0.73f, 0.87f, 0f, 0f, 0f, 0f);

        var title = CafeKioskUIUtility.Label("음료 옵션 선택", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.83f, 1f, 0.96f, 24f, 0f, -24f, 0f);

        var tempTitle = CafeKioskUIUtility.Label("커피 온도", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(tempTitle.rectTransform, 0f, 0.7f, 1f, 0.79f, 34f, 0f, -34f, 0f);

        var temps = CafeKioskUIUtility.Panel("Temperature Options", modal, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(temps, 0f, 0.58f, 1f, 0.7f, 34f, 0f, -34f, 0f);
        CafeKioskUIUtility.AddHorizontalLayout(temps, 12f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("ICE", temps, 22, sage, Color.white, () => { viewModel.SelectTemperature("ICE"); onAction?.Invoke(); }, font, 150f, 54f);
        CafeKioskUIUtility.Button("HOT", temps, 22, espresso, Color.white, () => { viewModel.SelectTemperature("HOT"); onAction?.Invoke(); }, font, 150f, 54f);

        var sizeTitle = CafeKioskUIUtility.Label("음료 사이즈", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(sizeTitle.rectTransform, 0f, 0.47f, 1f, 0.56f, 34f, 0f, -34f, 0f);

        var sizes = CafeKioskUIUtility.Panel("Size Options", modal, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(sizes, 0f, 0.29f, 1f, 0.47f, 34f, 0f, -34f, 0f);
        CafeKioskUIUtility.AddHorizontalLayout(sizes, 12f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("Small\n기본", sizes, 17, new Color(0.42f, 0.38f, 0.34f), Color.white, () => { viewModel.SelectSize("Small"); onAction?.Invoke(); }, font, 126f, 74f);
        CafeKioskUIUtility.Button("Regular\n+500원", sizes, 17, caramel, Color.white, () => { viewModel.SelectSize("Regular"); onAction?.Invoke(); }, font, 140f, 74f);
        CafeKioskUIUtility.Button("Large\n+1,000원", sizes, 17, sage, Color.white, () => { viewModel.SelectSize("Large"); onAction?.Invoke(); }, font, 140f, 74f);

        var add = CafeKioskUIUtility.Button("선택 담기", modal, 22, espresso, Color.white, () =>
        {
            if (viewModel.ConfirmDrinkOption()) onAction?.Invoke();
        }, font, 180f, 54f);
        CafeKioskUIUtility.Anchor(add.GetComponent<RectTransform>(), 0.5f, 0.15f, 0.5f, 0.24f, -90f, 0f, 90f, 0f);

        var cancel = CafeKioskUIUtility.Button("취소", modal, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, () => { viewModel.CancelDrinkOption(); onAction?.Invoke(); }, font, 130f, 44f);
        CafeKioskUIUtility.Anchor(cancel.GetComponent<RectTransform>(), 0.5f, 0.05f, 0.5f, 0.12f, -65f, 0f, 65f, 0f);
    }

    private void Build2(Transform parent, System.Action onAction)
    {
        Root = CafeKioskUIUtility.Panel("Drink Option Overlay", parent, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        // 모달 창 중앙 배치 및 가로 500 고정 (세로는 ContentSizeFitter가 제어)
        var modal = CafeKioskUIUtility.Panel("Drink Option Panel", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.5f, 0.5f, 0.5f, 0.5f, -250f, 0f, 250f, 0f);

        // VerticalLayoutGroup을 추가하여 자식 UI들을 수직으로 자동 정렬
        var layout = modal.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 16f;
        layout.padding = new UnityEngine.RectOffset(40, 40, 40, 40);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        // 내용물 전체 높이에 맞춰 모달 창 높이를 자동 조절
        var fitter = modal.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        var title = CafeKioskUIUtility.Label("음료 옵션 선택", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);

        var titleSpacer = CafeKioskUIUtility.Panel("Spacer", modal, new Color(0, 0, 0, 0));
        titleSpacer.sizeDelta = new Vector2(0f, 8f);

        var tempTitle = CafeKioskUIUtility.Label("커피 온도", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);

        var temps = CafeKioskUIUtility.Panel("Temperature Options", modal, new Color(0f, 0f, 0f, 0f));
        temps.sizeDelta = new Vector2(0f, 54f);
        CafeKioskUIUtility.AddHorizontalLayout(temps, 12f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("ICE", temps, 22, sage, Color.white, () => { viewModel.SelectTemperature("ICE"); onAction?.Invoke(); }, font, 150f, 54f);
        hotButton = CafeKioskUIUtility.Button("HOT", temps, 22, espresso, Color.white, () => { viewModel.SelectTemperature("HOT"); onAction?.Invoke(); }, font, 150f, 54f);

        var sizeTitle = CafeKioskUIUtility.Label("음료 사이즈", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);

        var sizes = CafeKioskUIUtility.Panel("Size Options", modal, new Color(0f, 0f, 0f, 0f));
        sizes.sizeDelta = new Vector2(0f, 74f);
        CafeKioskUIUtility.AddHorizontalLayout(sizes, 12f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("Small\n기본", sizes, 17, new Color(0.42f, 0.38f, 0.34f), Color.white, () => { viewModel.SelectSize("Small"); onAction?.Invoke(); }, font, 126f, 74f);
        CafeKioskUIUtility.Button("Regular\n+500원", sizes, 17, caramel, Color.white, () => { viewModel.SelectSize("Regular"); onAction?.Invoke(); }, font, 140f, 74f);
        CafeKioskUIUtility.Button("Large\n+1,000원", sizes, 17, sage, Color.white, () => { viewModel.SelectSize("Large"); onAction?.Invoke(); }, font, 140f, 74f);

        var actionSpacer = CafeKioskUIUtility.Panel("Spacer", modal, new Color(0, 0, 0, 0));
        actionSpacer.sizeDelta = new Vector2(0f, 24f);

        var addContainer = CafeKioskUIUtility.Panel("Add Container", modal, new Color(0f, 0f, 0f, 0f));
        addContainer.sizeDelta = new Vector2(0f, 54f);
        CafeKioskUIUtility.AddHorizontalLayout(addContainer, 0f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("선택 담기", addContainer, 22, espresso, Color.white, () =>
        {
            if (viewModel.ConfirmDrinkOption()) onAction?.Invoke();
        }, font, 180f, 54f);

        var cancelContainer = CafeKioskUIUtility.Panel("Cancel Container", modal, new Color(0f, 0f, 0f, 0f));
        cancelContainer.sizeDelta = new Vector2(0f, 44f);
        CafeKioskUIUtility.AddHorizontalLayout(cancelContainer, 0f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("취소", cancelContainer, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, () => { viewModel.CancelDrinkOption(); onAction?.Invoke(); }, font, 130f, 44f);
    }

    public void Refresh()
    {
        Root.gameObject.SetActive(viewModel.IsOptionOverlayVisible);
    }

    private void OnPendingOptionItemSet()
    {
        if (viewModel.PendingOptionItem is MenuItem item)
        {
            Debug.Log(nameof(OnPendingOptionItemSet) + "category : " + item.Category);
        }


    }
}
