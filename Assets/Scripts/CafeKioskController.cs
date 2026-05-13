// Unity의 기본 타입(GameObject, MonoBehaviour, Color, RectTransform 등)을 사용하기 위해 가져옵니다.
using UnityEngine;
// UI 클릭을 처리하는 EventSystem을 직접 만들거나 찾기 위해 가져옵니다.
using UnityEngine.EventSystems;
// 새 Input System용 UI 입력 모듈을 사용하기 위해 가져옵니다.
using UnityEngine.InputSystem.UI;
// Text, Button, Image, Canvas 같은 Unity UI 컴포넌트를 사용하기 위해 가져옵니다.
using UnityEngine.UI;

// ExecuteAlways는 플레이 모드가 아니어도 OnEnable 등이 실행되게 해 줍니다.
// 이 스크립트는 에디터에서도 UI를 자동 생성하기 위해 이 속성을 사용합니다.
[ExecuteAlways]
// MonoBehaviour를 상속해야 Unity GameObject에 컴포넌트로 붙일 수 있습니다.
public sealed class CafeKioskController : MonoBehaviour
{
    // ViewModel은 실제 주문 상태와 비즈니스 규칙을 담당합니다. Controller는 이 값을 읽어서 UI만 갱신합니다.
    private readonly CafeKioskViewModel viewModel = new();
    
    // UI 테마 색상들
    private readonly Color cream = new(0.96f, 0.92f, 0.86f);
    private readonly Color charcoal = new(0.12f, 0.11f, 0.1f);
    private readonly Color espresso = new(0.28f, 0.17f, 0.1f);
    private readonly Color caramel = new(0.77f, 0.45f, 0.22f);
    private readonly Color sage = new(0.38f, 0.5f, 0.42f);
    private readonly Color paper = new(1f, 0.98f, 0.94f);

    private Font font;
    
    // 분리된 UI 컴포넌트들
    private CafeKioskStartScreen startScreenUI;
    private CafeKioskOrderScreen orderScreenUI;
    private CafeKioskOptionPopup optionPopupUI;
    private CafeKioskPaymentPopup paymentPopupUI;

    private void OnEnable()
    {
        RebuildInterface();
    }

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

    private void Update()
    {
        EnsureEventSystem();
    }

    private void BuildInterface()
    {
        var canvas = CreateCanvas();
        var root = CafeKioskUIUtility.Panel("Kiosk Root", canvas.transform, cream);
        CafeKioskUIUtility.Stretch(root);

        // 컴포넌트들 초기화
        orderScreenUI = new CafeKioskOrderScreen(root, viewModel, font, cream, espresso, charcoal, caramel, sage, paper, 
            () => RefreshScreens(), // onRefreshMenu
            () => RefreshScreens(), // onRefreshCart
            () => { paymentPopupUI.ResetInput(); RefreshPayment(); RefreshScreens(); }, // onCheckout
            () => RefreshScreens()  // onAction
        );

        optionPopupUI = new CafeKioskOptionPopup(root, viewModel, font, paper, charcoal, sage, espresso, caramel, 
            () =>
            {
                RefreshScreens();
                // 옵션매뉴가 있는 아이템의 cart를 주문내역에 업데이트하는 함수를 추가.
                orderScreenUI.RefreshCart();
            }
        );

        paymentPopupUI = new CafeKioskPaymentPopup(root, viewModel, font, paper, charcoal, sage, espresso, caramel, 
            () => RefreshScreens()
        );

        startScreenUI = new CafeKioskStartScreen(root, viewModel, font, cream, espresso, charcoal, sage, 
            () => RefreshScreens()
        );

        RefreshScreens();
    }

    private void RefreshScreens()
    {
        startScreenUI?.Refresh();
        orderScreenUI?.Refresh();
        
        optionPopupUI?.Refresh();
        paymentPopupUI?.Refresh();
    }

    private void RefreshPayment()
    {
        paymentPopupUI?.RefreshPayment();
    }

    private Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("Cafe Kiosk Canvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        var eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        var legacyModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (legacyModule != null) CafeKioskUIUtility.DestroyComponent(legacyModule);

        if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private void RemoveGeneratedChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "Cafe Kiosk Canvas")
            {
                CafeKioskUIUtility.DestroyGeneratedObject(child.gameObject);
            }
        }
    }

}
