using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskAdminPopup
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;

    private Text detailsText;
    private Text finalSalesText;

    public CafeKioskAdminPopup(Transform parent, CafeKioskViewModel viewModel, Font font, Color paper, Color charcoal, Color espresso, System.Action onAction)
    {
        this.viewModel = viewModel;
        this.font = font;
        Build(parent, paper, charcoal, espresso, onAction);
    }

    private void Build(Transform parent, Color paper, Color charcoal, Color espresso, System.Action onAction)
    {
        //팝업이 뜰 때 뒷배경을 어둡게 덮어주는 반투명 검은색 배경
        Root = CafeKioskUIUtility.Panel("Admin Overlay", parent, new Color(0f, 0f, 0f, 0.85f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        //글씨들이 적힐 실제 하얀색 팝업창 본체
        var modal = CafeKioskUIUtility.Panel("Admin Modal", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.2f, 0.15f, 0.8f, 0.85f, 0f, 0f, 0f, 0f);

        //팝업창 맨 위쪽에 들어가는 "관리자 정산 페이지" 제목 텍스트
        var title = CafeKioskUIUtility.Label("관리자 정산 페이지", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.82f, 1f, 0.95f, 0f, 0f, 0f, 0f);

        // 결제액, 총 주문 건수, 쿠폰 차감 총액이 표시될 텍스트 영역
        detailsText = CafeKioskUIUtility.Label("", modal, 26, charcoal, FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(detailsText.rectTransform, 0f, 0.45f, 1f, 0.80f, 0f, 0f, 0f, 0f);
        detailsText.lineSpacing = 1.2f;

        // 실제 결제된 금액이 표시될 텍스트 영역
        finalSalesText = CafeKioskUIUtility.Label("", modal, 28, espresso, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(finalSalesText.rectTransform, 0f, 0.25f, 1f, 0.45f, 0f, 0f, 0f, 0f);

        // 창을 닫는 '닫기' 버튼
        var closeBtn = CafeKioskUIUtility.Button("닫기", modal, 20, charcoal, Color.white, () => {
            Hide();
            onAction?.Invoke();
        }, font, 120f, 50f);
        CafeKioskUIUtility.Anchor(closeBtn.GetComponent<RectTransform>(), 0.5f, 0.08f, 0.5f, 0.18f, -60f, 0f, 60f, 0f);
    }

    public void Show()
    {
        Root.gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        Root.gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (!Root.gameObject.activeSelf) return;

        detailsText.text = $"결제액 : {CafeKioskViewModel.FormatPrice(viewModel.TotalOriginalSales)}\n" +
                           $"총 주문 건수 : {viewModel.TotalOrderCount}건\n" +
                           $"쿠폰 차감 총액 : -{CafeKioskViewModel.FormatPrice(viewModel.TotalDiscountAmount)}";

        finalSalesText.text = $"실제 결제된 금액 (최종 실 결제액)\n{CafeKioskViewModel.FormatPrice(viewModel.TotalActualSales)}";
    }
}