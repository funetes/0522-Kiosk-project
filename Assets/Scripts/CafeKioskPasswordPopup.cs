using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskPasswordPopup
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private InputField passwordInput;
    private Text messageText;

    public CafeKioskPasswordPopup(Transform parent, CafeKioskViewModel viewModel, Font font, Color paper, Color charcoal, Color espresso, System.Action onAction, System.Action onSuccess)
    {
        this.viewModel = viewModel;
        this.font = font;
        Build(parent, paper, charcoal, espresso, onAction, onSuccess);
    }

    private void Build(Transform parent, Color paper, Color charcoal, Color espresso, System.Action onAction, System.Action onSuccess)
    {
        Root = CafeKioskUIUtility.Panel("Password Overlay", parent, new Color(0f, 0f, 0f, 0.85f));
        CafeKioskUIUtility.Stretch(Root);
        Root.gameObject.SetActive(false);

        var modal = CafeKioskUIUtility.Panel("Password Modal", Root, paper);
        CafeKioskUIUtility.Anchor(modal, 0.35f, 0.35f, 0.65f, 0.65f, 0f, 0f, 0f, 0f);

        var title = CafeKioskUIUtility.Label("관리자 인증", modal, 24, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0.75f, 1f, 0.95f, 0f, 0f, 0f, 0f);

        messageText = CafeKioskUIUtility.Label("숫자 4자리를 입력하세요", modal, 14, new Color(0.5f, 0.5f, 0.5f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(messageText.rectTransform, 0f, 0.6f, 1f, 0.75f, 0f, 0f, 0f, 0f);

        passwordInput = CafeKioskUIUtility.Input("****", modal, charcoal, font);
        passwordInput.contentType = InputField.ContentType.Password;
        passwordInput.characterLimit = 4;
        CafeKioskUIUtility.Anchor(passwordInput.GetComponent<RectTransform>(), 0.2f, 0.4f, 0.8f, 0.55f, 0f, 0f, 0f, 0f);

        var confirmBtn = CafeKioskUIUtility.Button("확인", modal, 18, espresso, Color.white, () => {
            if (viewModel.CheckAdminPassword(passwordInput.text))
            {
                Hide();
                onSuccess?.Invoke();
            }
            else
            {
               
                messageText.text = "숫자 4자리를 입력해주세요!";
                passwordInput.text = "";
            }
        }, font, 100f, 40f);
        CafeKioskUIUtility.Anchor(confirmBtn.GetComponent<RectTransform>(), 0.55f, 0.15f, 0.85f, 0.3f, 0f, 0f, 0f, 0f);

        var cancelBtn = CafeKioskUIUtility.Button("취소", modal, 18, new Color(0.4f, 0.4f, 0.4f), Color.white, () => {
            Hide();
            onAction?.Invoke();
        }, font, 100f, 40f);
        CafeKioskUIUtility.Anchor(cancelBtn.GetComponent<RectTransform>(), 0.15f, 0.15f, 0.45f, 0.3f, 0f, 0f, 0f, 0f);
    }

    public void Show() { Root.gameObject.SetActive(true); passwordInput.text = ""; messageText.text = "숫자 4자리를 입력하세요"; }
    public void Hide() { Root.gameObject.SetActive(false); }
    public void Refresh() { }
}