using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskReceiptPopup
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private readonly Color paper;
    private readonly Color charcoal;
    private readonly Color sage;
    private readonly Color espresso;
    private readonly Color caramel;
    // 번호표 문구
    private Text TicketText;
    // 현재 결제할 금액
    private Text PaymentAmount;
    private Text orderModeText;
    private Text memberStatusText;
    private Text statusText;
    private RectTransform orderList;

    public CafeKioskReceiptPopup(Transform parent, CafeKioskViewModel viewModel, Font font, Color paper, Color charcoal, Color sage, Color espresso, Color caramel, System.Action onAction)
    {
        this.viewModel = viewModel;
        this.font = font;
        this.paper = paper;
        this.charcoal = charcoal;
        this.sage = sage;
        this.espresso = espresso;
        this.caramel = caramel;

        Build(parent, onAction);
    }

    private void Build(Transform parent, System.Action onAction)
    {
        Root = CafeKioskUIUtility.Panel("Receipt Overlay", parent, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        var modal = CafeKioskUIUtility.Panel("Receipt Panel", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.27f, 0.13f, 0.73f, 0.87f, 0f, 0f, 0f, 0f);

        var title = CafeKioskUIUtility.Label("영수증", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.88f, 1f, 0.98f, 24f, 0f, -24f, 0f);

        TicketText = CafeKioskUIUtility.Label("", modal, 30, espresso, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(TicketText.rectTransform, 0f, 0.78f, 1f, 0.88f, 34f, 0f, -34f, 0f);

        PaymentAmount = CafeKioskUIUtility.Label("", modal, 26, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(PaymentAmount.rectTransform, 0f, 0.69f, 1f, 0.78f, 34f, 0f, -34f, 0f);

        orderModeText = CafeKioskUIUtility.Label("", modal, 18, caramel, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(orderModeText.rectTransform, 0f, 0.62f, 1f, 0.69f, 34f, 0f, -34f, 0f);

        var divider = CafeKioskUIUtility.Panel("Receipt Divider", modal, new Color(0.82f, 0.74f, 0.64f));
        CafeKioskUIUtility.Anchor(divider, 0f, 0.6f, 1f, 0.605f, 34f, 0f, -34f, 0f);

        var orderTitle = CafeKioskUIUtility.Label("주문 내역", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(orderTitle.rectTransform, 0f, 0.54f, 1f, 0.6f, 34f, 0f, -34f, 0f);

        var scroll = CafeKioskUIUtility.ScrollArea("Receipt Order List", modal);
        CafeKioskUIUtility.Anchor(scroll.viewport, 0f, 0.28f, 1f, 0.54f, 34f, 0f, -34f, 0f);
        orderList = scroll.content;
        CafeKioskUIUtility.AddVerticalLayout(orderList, 6f, TextAnchor.UpperLeft);

        memberStatusText = CafeKioskUIUtility.Label("", modal, 15, sage, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(memberStatusText.rectTransform, 0f, 0.2f, 1f, 0.27f, 34f, 0f, -34f, 0f);

        statusText = CafeKioskUIUtility.Label("", modal, 14, new Color(0.46f, 0.42f, 0.36f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(statusText.rectTransform, 0f, 0.14f, 1f, 0.2f, 34f, 0f, -34f, 0f);

        var confirm = CafeKioskUIUtility.Button("확인", modal, 22, espresso, Color.white, () => { 
            viewModel.ConfirmReceipt();
            onAction?.Invoke(); 
        }, font, 180f, 54f);
        CafeKioskUIUtility.Anchor(confirm.GetComponent<RectTransform>(), 0.5f, 0.04f, 0.5f, 0.13f, -90f, 0f, 90f, 0f);
    }

    public void Refresh()
    {
        Root.gameObject.SetActive(viewModel.IsReceiptVisible);
        RefreshStatus();
    }

    public void RefreshStatus()
    {
        TicketText.text = string.IsNullOrWhiteSpace(viewModel.TicketText) ? "번호표 준비 중" : viewModel.TicketText;
        PaymentAmount.text = $"결제 금액 {CafeKioskViewModel.FormatPrice(viewModel.PaymentAmount)}";
        orderModeText.text = string.IsNullOrWhiteSpace(viewModel.OrderMode) ? "주문 방식 -" : $"주문 방식 - {viewModel.OrderMode}";
        memberStatusText.text = viewModel.MemberStatusText;
        statusText.text = viewModel.StatusText;

        RefreshOrderList();
    }

    private void RefreshOrderList()
    {
        CafeKioskUIUtility.ClearChildren(orderList);

        if (viewModel.Cart.Count == 0)
        {
            var empty = CafeKioskUIUtility.Label("표시할 주문 내역이 없습니다.", orderList, 16, charcoal, FontStyle.Normal, TextAnchor.MiddleLeft, font);
            empty.rectTransform.sizeDelta = new Vector2(0f, 34f);
            return;
        }

        foreach (var line in viewModel.Cart)
        {
            var row = CafeKioskUIUtility.Panel(line.DisplayName, orderList, new Color(0.98f, 0.94f, 0.88f));
            row.sizeDelta = new Vector2(0f, 42f);

            var name = CafeKioskUIUtility.Label($"{line.DisplayName} x {line.Quantity}", row, 15, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(name.rectTransform, 0f, 0f, 0.65f, 1f, 10f, 0f, 0f, 0f);

            var price = CafeKioskUIUtility.Label(CafeKioskViewModel.FormatPrice(line.UnitPrice * line.Quantity), row, 15, caramel, FontStyle.Bold, TextAnchor.MiddleRight, font);
            CafeKioskUIUtility.Anchor(price.rectTransform, 0.65f, 0f, 1f, 1f, 0f, 0f, -10f, 0f);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(orderList);
    }
}
