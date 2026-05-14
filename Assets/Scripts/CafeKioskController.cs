using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class CafeKioskController : MonoBehaviour
{
    private readonly CafeKioskViewModel viewModel = new();
    private readonly Color cream = new(0.96f, 0.92f, 0.86f);
    private readonly Color charcoal = new(0.12f, 0.11f, 0.1f);
    private readonly Color espresso = new(0.28f, 0.17f, 0.1f);
    private readonly Color caramel = new(0.77f, 0.45f, 0.22f);
    private readonly Color sage = new(0.38f, 0.5f, 0.42f);
    private readonly Color paper = new(1f, 0.98f, 0.94f);
    private Font font;

    private CafeKioskStartScreen startScreenUI;
    private CafeKioskOrderScreen orderScreenUI;
    private CafeKioskOptionPopup optionPopupUI;
    private CafeKioskPaymentPopup paymentPopupUI;
    private CafeKioskAdminPopup adminPopupUI;
    private CafeKioskPasswordPopup passwordPopupUI;

    private void OnEnable() => RebuildInterface();
    private void Update() => EnsureEventSystem();

    private void RebuildInterface()
    {
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RemoveGeneratedChildren();
        EnsureEventSystem();
        BuildInterface();
        orderScreenUI.RefreshMenu(() => RefreshScreens());
        orderScreenUI.RefreshCart();
    }

    private void BuildInterface()
    {
        var canvas = CreateCanvas();
        var root = CafeKioskUIUtility.Panel("Kiosk Root", canvas.transform, cream);
        CafeKioskUIUtility.Stretch(root);

        orderScreenUI = new CafeKioskOrderScreen(root, viewModel, font, cream, espresso, charcoal, caramel, sage, paper, () => RefreshScreens(), () => RefreshScreens(), () => { paymentPopupUI.ResetInput(); paymentPopupUI.RefreshPayment(); RefreshScreens(); }, () => RefreshScreens());
        optionPopupUI = new CafeKioskOptionPopup(root, viewModel, font, paper, charcoal, sage, espresso, caramel, () => RefreshScreens());
        paymentPopupUI = new CafeKioskPaymentPopup(root, viewModel, font, paper, charcoal, sage, espresso, caramel, () => RefreshScreens());
        adminPopupUI = new CafeKioskAdminPopup(root, viewModel, font, paper, charcoal, espresso, () => RefreshScreens());

        passwordPopupUI = new CafeKioskPasswordPopup(root, viewModel, font, paper, charcoal, espresso,
            onAction: () => RefreshScreens(),
            onSuccess: () => { adminPopupUI.Show(); RefreshScreens(); }
        );


        optionPopupUI = new CafeKioskOptionPopup(root, viewModel, font, paper, charcoal, sage, espresso, caramel, 
            () =>
            {
                RefreshScreens();
                
                orderScreenUI.RefreshCart();
            }
        );

        // --- Admin 버튼 생성 ---
        var adminBtn = CafeKioskUIUtility.Button("Admin", root, 14, charcoal, Color.white, () =>
        {
            passwordPopupUI.Show();
            RefreshScreens();
        }, font, 80f, 40f);


        CafeKioskUIUtility.Anchor(adminBtn.GetComponent<RectTransform>(), 0.45f, 0.92f, 0.55f, 0.97f, 0f, 0f, 0f, 0f);

        startScreenUI = new CafeKioskStartScreen(root, viewModel, font, cream, espresso, charcoal, sage, () => RefreshScreens());
        RefreshScreens();
    }

    private void RefreshScreens()
    {
        startScreenUI?.Refresh();
        orderScreenUI?.Refresh();
        optionPopupUI?.Refresh();
        orderScreenUI.RefreshCart();
        paymentPopupUI?.Refresh();
        adminPopupUI?.Refresh();
        passwordPopupUI?.Refresh();
    }

    private Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("Cafe Kiosk Canvas") { transform = { parent = transform } };
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>().gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private void RemoveGeneratedChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
            if (transform.GetChild(i).name == "Cafe Kiosk Canvas") CafeKioskUIUtility.DestroyGeneratedObject(transform.GetChild(i).gameObject);
    }
}