using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskStartScreen
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private readonly Color cream;
    private readonly Color espresso;
    private readonly Color charcoal;
    private readonly Color sage;

    public CafeKioskStartScreen(Transform parent, CafeKioskViewModel viewModel, Font font, Color cream, Color espresso, Color charcoal, Color sage, System.Action onSelectMode)
    {
        this.viewModel = viewModel;
        this.font = font;
        this.cream = cream;
        this.espresso = espresso;
        this.charcoal = charcoal;
        this.sage = sage;

        Build(parent, onSelectMode);
    }

    private void Build(Transform parent, System.Action onSelectMode)
    {
        Root = CafeKioskUIUtility.Panel("Start Order Screen", parent, cream);
        CafeKioskUIUtility.Stretch(Root);

        var title = CafeKioskUIUtility.Label("Megazone Cafe", Root, 56, espresso, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.68f, 1f, 0.82f, 24f, 0f, -24f, 0f);

        var subtitle = CafeKioskUIUtility.Label("주문 방식을 선택해주세요", Root, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(subtitle.rectTransform, 0f, 0.58f, 1f, 0.68f, 24f, 0f, -24f, 0f);

        var choices = CafeKioskUIUtility.Panel("Order Mode Choices", Root, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(choices, 0.16f, 0.28f, 0.84f, 0.55f, 0f, 0f, 0f, 0f);
        CafeKioskUIUtility.AddHorizontalLayout(choices, 22f, TextAnchor.MiddleCenter);

        CafeKioskUIUtility.Button("매장", choices, 34, espresso, Color.white, () => {
            viewModel.SelectOrderMode("매장");
            onSelectMode?.Invoke();
        }, font, 260f, 150f);

        CafeKioskUIUtility.Button("포장", choices, 34, sage, Color.white, () => {
            viewModel.SelectOrderMode("포장");
            onSelectMode?.Invoke();
        }, font, 260f, 150f);
    }

    public void Refresh()
    {
        Debug.Log("Refreshing Start Screen : " + viewModel.IsStartScreenVisible);
        Root.gameObject.SetActive(viewModel.IsStartScreenVisible);
    }
}
