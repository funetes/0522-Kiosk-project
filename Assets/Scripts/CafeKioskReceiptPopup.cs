using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskReceiptPopup
{
    // 팝업 전체를 켜고 끄기 위한 최상위 RectTransform입니다.
    public RectTransform Root { get; private set; }
    // 영수증에 표시할 주문 상태와 결제 결과는 ViewModel에서 읽어옵니다.
    private readonly CafeKioskViewModel viewModel;
    // 런타임에서 만든 UI Text들이 같은 폰트와 색상 체계를 쓰도록 생성자에서 받은 값을 보관합니다.
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
    // 주문 항목 줄들이 동적으로 추가되는 스크롤 영역의 content입니다.
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
        // 반투명 배경을 먼저 만들고, 그 위에 실제 영수증 패널을 올립니다.
        Root = CafeKioskUIUtility.Panel("Receipt Overlay", parent, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        var modal = CafeKioskUIUtility.Panel("Receipt Panel", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.27f, 0.13f, 0.73f, 0.87f, 0f, 0f, 0f, 0f);

        var title = CafeKioskUIUtility.Label("영수증", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.88f, 1f, 0.98f, 24f, 0f, -24f, 0f);

        // 번호표와 결제 금액은 결제 완료 후 RefreshStatus에서 실제 값으로 채웁니다.
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

        // 주문 항목은 개수가 달라질 수 있으므로 스크롤 영역 안에 매번 다시 그립니다.
        var scroll = CafeKioskUIUtility.ScrollArea("Receipt Order List", modal);
        CafeKioskUIUtility.Anchor(scroll.viewport, 0f, 0.28f, 1f, 0.54f, 34f, 0f, -34f, 0f);
        orderList = scroll.content;
        CafeKioskUIUtility.AddVerticalLayout(orderList, 6f, TextAnchor.UpperLeft);

        memberStatusText = CafeKioskUIUtility.Label("", modal, 15, sage, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(memberStatusText.rectTransform, 0f, 0.2f, 1f, 0.27f, 34f, 0f, -34f, 0f);

        statusText = CafeKioskUIUtility.Label("", modal, 14, new Color(0.46f, 0.42f, 0.36f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(statusText.rectTransform, 0f, 0.14f, 1f, 0.2f, 34f, 0f, -34f, 0f);

        // 확인을 누르면 ViewModel에서 영수증 상태를 닫고, 컨트롤러 콜백으로 화면을 갱신합니다.
        var confirm = CafeKioskUIUtility.Button("확인", modal, 22, espresso, Color.white, () => { 
            viewModel.ConfirmReceipt();
            onAction?.Invoke(); 
        }, font, 180f, 54f);
        CafeKioskUIUtility.Anchor(confirm.GetComponent<RectTransform>(), 0.5f, 0.04f, 0.5f, 0.13f, -90f, 0f, 90f, 0f);
    }

    public void Refresh()
    {
        // ViewModel의 표시 상태에 맞춰 팝업을 켜거나 끕니다.
        Root.gameObject.SetActive(viewModel.IsReceiptVisible);
        // 팝업이 열린 직후 최신 결제 결과가 보이도록 매번 텍스트를 갱신합니다.
        RefreshStatus();
    }

    public void RefreshStatus()
    {
        // 아직 번호표가 만들어지지 않은 예외 상황에서도 빈 화면 대신 안내 문구를 보여줍니다.
        TicketText.text = string.IsNullOrWhiteSpace(viewModel.TicketText) ? "번호표 준비 중" : viewModel.TicketText;
        PaymentAmount.text = $"결제 금액 {CafeKioskViewModel.FormatPrice(viewModel.PaymentAmount)}";
        orderModeText.text = string.IsNullOrWhiteSpace(viewModel.OrderMode) ? "주문 방식 -" : $"주문 방식 - {viewModel.OrderMode}";
        memberStatusText.text = viewModel.MemberStatusText;
        statusText.text = viewModel.StatusText;

        RefreshOrderList();
    }

    private void RefreshOrderList()
    {
        // 이전에 그려둔 주문 줄을 지우고 결제 완료 시점에 저장한 주문내역 기준으로 다시 만듭니다.
        CafeKioskUIUtility.ClearChildren(orderList);

        if (viewModel.ReceiptItems.Count == 0)
        {
            // 결제 내역이 비어 있는 경우에도 팝업이 깨지지 않게 처리합니다.
            var empty = CafeKioskUIUtility.Label("표시할 주문 내역이 없습니다.", orderList, 16, charcoal, FontStyle.Normal, TextAnchor.MiddleLeft, font);
            empty.rectTransform.sizeDelta = new Vector2(0f, 34f);
            return;
        }

        foreach (var line in viewModel.ReceiptItems)
        {
            // 한 줄에는 메뉴명/옵션/수량과 해당 줄의 합계 금액을 좌우로 배치합니다.
            var row = CafeKioskUIUtility.Panel(line.DisplayName, orderList, new Color(0.98f, 0.94f, 0.88f));
            row.sizeDelta = new Vector2(0f, 42f);

            var name = CafeKioskUIUtility.Label($"{line.DisplayName} x {line.Quantity}", row, 15, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(name.rectTransform, 0f, 0f, 0.65f, 1f, 10f, 0f, 0f, 0f);

            var price = CafeKioskUIUtility.Label(CafeKioskViewModel.FormatPrice(line.UnitPrice * line.Quantity), row, 15, caramel, FontStyle.Bold, TextAnchor.MiddleRight, font);
            CafeKioskUIUtility.Anchor(price.rectTransform, 0.65f, 0f, 1f, 1f, 0f, 0f, -10f, 0f);
        }

        // 동적으로 만든 주문 줄의 높이가 스크롤 content에 바로 반영되도록 레이아웃을 강제로 갱신합니다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(orderList);
    }
}
