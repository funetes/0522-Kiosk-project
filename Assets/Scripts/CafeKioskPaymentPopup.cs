using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskPaymentPopup
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private readonly Color paper, charcoal, sage, espresso, caramel;

    private Text paymentTotalText, memberStatusText, ticketText;
    private InputField memberPhoneInput;

    public CafeKioskPaymentPopup(Transform parent, CafeKioskViewModel vm, Font f, Color p, Color c, Color s, Color e, Color cr, System.Action onAction)
    {
        viewModel = vm; font = f; paper = p; charcoal = c; sage = s; espresso = e; caramel = cr;
        Build(parent, onAction);
    }

    private void Build(Transform parent, System.Action onAction)
    {
        Root = CafeKioskUIUtility.Panel("Payment Overlay", parent, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        var modal = CafeKioskUIUtility.Panel("Payment Modal", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.28f, 0.18f, 0.72f, 0.82f, 0f, 0f, 0f, 0f);

        var title = CafeKioskUIUtility.Label("결제 방식을 선택하세요", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.78f, 1f, 0.95f, 24f, 0f, -24f, 0f);

        paymentTotalText = CafeKioskUIUtility.Label("결제 금액 0원", modal, 24, caramel, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(paymentTotalText.rectTransform, 0f, 0.66f, 1f, 0.78f, 24f, 0f, -24f, 0f);

        var memberTitle = CafeKioskUIUtility.Label("멤버십 전화번호", modal, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(memberTitle.rectTransform, 0f, 0.58f, 1f, 0.66f, 34f, 0f, -34f, 0f);

        memberPhoneInput = CafeKioskUIUtility.Input("01012345678", modal, charcoal, font);
        CafeKioskUIUtility.Anchor(memberPhoneInput.GetComponent<RectTransform>(), 0f, 0.49f, 0.72f, 0.58f, 34f, 0f, -8f, 0f);

        var join = CafeKioskUIUtility.Button("회원가입/조회", modal, 17, sage, Color.white, () => {
            viewModel.RegisterOrLookupMember(memberPhoneInput.text);
            RefreshPayment();
        }, font, 146f, 48f);
        CafeKioskUIUtility.Anchor(join.GetComponent<RectTransform>(), 0.72f, 0.49f, 1f, 0.58f, 8f, 0f, -34f, 0f);

        memberStatusText = CafeKioskUIUtility.Label("전화번호를 입력하면 스탬프가 적립됩니다.", modal, 15, new Color(0.46f, 0.42f, 0.36f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(memberStatusText.rectTransform, 0f, 0.43f, 1f, 0.49f, 34f, 0f, -34f, 0f);


        var couponBtn = CafeKioskUIUtility.Button("쿠폰 사용 (-2,000원)", modal, 18, caramel, Color.white, () => {
            viewModel.ApplyStampDiscount(memberPhoneInput.text);
            RefreshPayment();
            onAction?.Invoke();
        }, font, 240f, 44f);
        CafeKioskUIUtility.Anchor(couponBtn.GetComponent<RectTransform>(), 0.5f, 0.34f, 0.5f, 0.42f, -120f, 0f, 120f, 0f);

        var methods = CafeKioskUIUtility.Panel("Methods", modal, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(methods, 0f, 0.18f, 1f, 0.33f, 28f, 0f, -28f, 0f);
        CafeKioskUIUtility.AddHorizontalLayout(methods, 14f, TextAnchor.MiddleCenter);

    
        CafeKioskUIUtility.Button("카드", methods, 22, espresso, Color.white, () => CompletePayment("카드", onAction), font, 130f, 70f);
        CafeKioskUIUtility.Button("현금", methods, 22, sage, Color.white, () => CompletePayment("현금", onAction), font, 130f, 70f);
        CafeKioskUIUtility.Button("모바일페이", methods, 20, caramel, Color.white, () => CompletePayment("모바일페이", onAction), font, 170f, 70f);

        ticketText = CafeKioskUIUtility.Label("", modal, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(ticketText.rectTransform, 0f, 0.12f, 1f, 0.18f, 24f, 0f, -24f, 0f);

        var back = CafeKioskUIUtility.Button("돌아가기", modal, 19, new Color(0.42f, 0.38f, 0.34f), Color.white, () => {
            viewModel.CancelPayment();
            onAction?.Invoke();
        }, font, 150f, 46f);
        CafeKioskUIUtility.Anchor(back.GetComponent<RectTransform>(), 0.5f, 0.04f, 0.5f, 0.11f, -75f, 0f, 75f, 0f);
    }

    private void CompletePayment(string m, System.Action a) { viewModel.CompletePayment(m, memberPhoneInput.text); RefreshPayment(); a?.Invoke(); }
    public void RefreshPayment() { paymentTotalText.text = $"결제 금액 {CafeKioskViewModel.FormatPrice(viewModel.PaymentAmount)}"; memberStatusText.text = viewModel.MemberStatusText; ticketText.text = viewModel.TicketText; }
    public void ResetInput() => memberPhoneInput.text = "";
    public void Refresh() => Root.gameObject.SetActive(viewModel.IsPaymentOverlayVisible);
}