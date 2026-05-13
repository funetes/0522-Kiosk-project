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
    // 전체 배경에 쓰는 크림색입니다.
    private readonly Color cream = new(0.96f, 0.92f, 0.86f);
    // 글자색으로 자주 쓰는 진한 색입니다.
    private readonly Color charcoal = new(0.12f, 0.11f, 0.1f);
    // 헤더와 주요 버튼에 쓰는 커피색입니다.
    private readonly Color espresso = new(0.28f, 0.17f, 0.1f);
    // 가격이나 보조 버튼에 쓰는 캐러멜색입니다.
    private readonly Color caramel = new(0.77f, 0.45f, 0.22f);
    // 결제 버튼이나 긍정 액션에 쓰는 세이지색입니다.
    private readonly Color sage = new(0.38f, 0.5f, 0.42f);
    // 주문 영역, 팝업 패널 배경에 쓰는 종이색입니다.
    private readonly Color paper = new(1f, 0.98f, 0.94f);

    // 모든 Text 컴포넌트가 사용할 폰트입니다.
    private Font font;
    // 메뉴 카드들이 들어가는 GridLayoutGroup의 RectTransform입니다.
    private RectTransform menuGrid;
    // 장바구니 줄들이 들어가는 세로 목록 RectTransform입니다.
    private RectTransform cartList;
    // 장바구니 합계 금액을 보여주는 Text입니다.
    private Text totalText;
    // 장바구니가 비었을 때 보여주는 안내 Text입니다.
    private Text emptyCartText;
    // 화면 하단의 상태 메시지 Text입니다.
    private Text statusText;
    // 메뉴와 장바구니가 있는 주문 화면입니다.
    private RectTransform orderScreen;
    // 처음에 매장/포장을 고르는 시작 화면입니다.
    private RectTransform startScreen;
    // 결제 방식 선택 팝업입니다.
    private RectTransform paymentOverlay;
    // 음료 옵션 선택 팝업입니다.
    private RectTransform optionOverlay;
    // 결제 팝업 안의 결제 금액 Text입니다.
    private Text paymentTotalText;
    // 결제 팝업 안의 멤버십 상태 Text입니다.
    private Text memberStatusText;
    // 결제 팝업 안의 번호표 Text입니다.
    private Text ticketText;
    // 결제 팝업 안에서 전화번호를 입력받는 InputField입니다.
    private InputField memberPhoneInput;

    // 이 컴포넌트가 활성화될 때 Unity가 자동으로 호출합니다.
    private void OnEnable()
    {
        // 화면을 처음부터 다시 생성합니다.
        RebuildInterface();
    }

    // 키오스크 UI 전체를 새로 만드는 메서드입니다.
    private void RebuildInterface()
    {
        // OS에서 한글 폰트 후보를 찾아 동적 폰트를 만듭니다.
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
        // 폰트를 찾지 못한 경우 Unity 기본 런타임 폰트를 사용합니다.
        if (font == null)
        {
            // Unity에 포함된 기본 폰트 리소스를 가져옵니다.
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // 이전에 자동 생성한 Canvas가 있으면 삭제해서 UI가 중복 생성되지 않게 합니다.
        RemoveGeneratedChildren();
        // 버튼 클릭과 입력 처리를 위해 EventSystem이 있는지 확인합니다.
        EnsureEventSystem();
        // Canvas, 화면, 버튼, 팝업 같은 UI 오브젝트를 생성합니다.
        BuildInterface();
        // 현재 ViewModel 상태를 기준으로 메뉴 영역을 그립니다.
        RefreshMenu();
        // 현재 ViewModel 상태를 기준으로 장바구니 영역을 그립니다.
        RefreshCart();
    }

    // 매 프레임 Unity가 호출합니다.
    private void Update()
    {
        // 에디터에서 EventSystem이 삭제되거나 누락되어도 다시 보장합니다.
        EnsureEventSystem();
    }

    // 화면 전체 구조를 만드는 메서드입니다.
    private void BuildInterface()
    {
        // 모든 UI의 최상위 Canvas를 생성합니다.
        var canvas = CreateCanvas();
        // Canvas 바로 아래에 전체 배경 패널을 만듭니다.
        var root = Panel("Kiosk Root", canvas.transform, cream);
        // root 패널을 화면 전체 크기로 늘립니다.
        Stretch(root);

        // 실제 주문 화면을 만듭니다.
        orderScreen = Panel("Order Screen", root, cream);
        // 주문 화면을 root 전체에 꽉 차게 배치합니다.
        Stretch(orderScreen);

        // 주문 화면 상단의 헤더 영역을 만듭니다.
        var header = Panel("Header", orderScreen, espresso);
        // 헤더를 화면 위쪽 15% 영역에 배치하고 여백을 줍니다.
        Anchor(header, 0f, 0.85f, 1f, 1f, 28f, 18f, -28f, -18f);

        // 카페 이름 Text를 만듭니다.
        var title = Label("Megazone Cafe", header, 42, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 제목을 헤더 왼쪽 절반에 배치합니다.
        Anchor(title.rectTransform, 0f, 0f, 0.5f, 1f, 26f, 0f, 0f, 0f);

        // 주문 안내 문구 Text를 만듭니다.
        var subtitle = Label("주문할 메뉴를 선택하세요", header, 20, new Color(1f, 0.88f, 0.7f), FontStyle.Normal, TextAnchor.MiddleRight);
        // 안내 문구를 헤더 오른쪽 절반에 배치합니다.
        Anchor(subtitle.rectTransform, 0.5f, 0f, 1f, 1f, 0f, 0f, -26f, 0f);

        // 헤더 아래의 본문 영역을 만듭니다.
        var content = Panel("Content", orderScreen, cream);
        // 본문을 주문 화면의 아래쪽 85% 영역에 배치합니다.
        Anchor(content, 0f, 0f, 1f, 0.85f, 28f, 24f, -28f, -12f);

        // 왼쪽 메뉴 영역을 만듭니다.
        var left = Panel("Menu Area", content, new Color(0.98f, 0.95f, 0.9f));
        // 왼쪽 영역은 본문 너비의 약 66%를 차지합니다.
        Anchor(left, 0f, 0f, 0.66f, 1f, 0f, 0f, -12f, 0f);

        // 카테고리 버튼들이 들어갈 영역을 만듭니다.
        var categories = Panel("Categories", left, new Color(0f, 0f, 0f, 0f));
        // 카테고리 영역을 왼쪽 패널 상단에 배치합니다.
        Anchor(categories, 0f, 0.88f, 1f, 1f, 14f, 8f, -14f, -8f);
        // 버튼들이 가로로 나란히 배치되도록 HorizontalLayoutGroup을 붙입니다.
        AddHorizontalLayout(categories, 10, TextAnchor.MiddleLeft);

        // ViewModel이 제공하는 카테고리 목록만큼 버튼을 생성합니다.
        foreach (var category in viewModel.Categories)
        {
            // 람다 안에서 현재 category 값을 안전하게 사용하기 위해 별도 변수에 복사합니다.
            var captured = category;
            // 카테고리 버튼을 만들고, 클릭하면 ViewModel의 선택 카테고리를 바꾼 뒤 메뉴를 다시 그립니다.
            Button(CafeKioskViewModel.CategoryLabel(category), categories, 18, caramel, Color.white, () =>
            {
                // ViewModel에 선택된 카테고리를 저장합니다.
                viewModel.SelectCategory(captured);
                // 새 카테고리에 맞게 메뉴 카드들을 다시 생성합니다.
                RefreshMenu();
            }, 116f);
        }

        // 메뉴 카드들을 스크롤할 수 있는 영역을 만듭니다.
        var scroll = ScrollArea("Menu Scroll", left);
        // 스크롤 영역을 카테고리 아래쪽 전체에 배치합니다.
        Anchor(scroll.viewport, 0f, 0f, 1f, 0.88f, 14f, 14f, -14f, -8f);
        // ScrollArea가 만든 content를 메뉴 카드 부모로 저장합니다.
        menuGrid = scroll.content;
        // 메뉴 카드들을 3열 그리드로 배치합니다.
        AddGrid(menuGrid, new Vector2(245f, 208f), new Vector2(14f, 14f), new RectOffset(0, 0, 0, 0));

        // 오른쪽 주문 내역 영역을 만듭니다.
        var right = Panel("Order Area", content, paper);
        // 오른쪽 영역은 본문 너비의 나머지 34%를 차지합니다.
        Anchor(right, 0.66f, 0f, 1f, 1f, 12f, 0f, 0f, 0f);

        // 장바구니 제목 Text를 만듭니다.
        var orderTitle = Label("주문 내역", right, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 제목을 오른쪽 영역 상단에 배치합니다.
        Anchor(orderTitle.rectTransform, 0f, 0.88f, 1f, 1f, 22f, 0f, -22f, -12f);

        // 장바구니 항목을 스크롤할 수 있는 영역을 만듭니다.
        var cartScroll = ScrollArea("Cart Scroll", right);
        // 장바구니 스크롤 영역을 제목 아래와 합계 위 사이에 배치합니다.
        Anchor(cartScroll.viewport, 0f, 0.24f, 1f, 0.88f, 18f, 4f, -18f, -8f);
        // ScrollArea가 만든 content를 장바구니 줄 부모로 저장합니다.
        cartList = cartScroll.content;
        // 장바구니 줄들이 위에서 아래로 쌓이도록 VerticalLayoutGroup을 붙입니다.
        AddVerticalLayout(cartList, 10, TextAnchor.UpperLeft);

        // 장바구니가 비었을 때 보여줄 안내 문구를 만듭니다.
        emptyCartText = Label("아직 담긴 메뉴가 없습니다.", right, 18, new Color(0.5f, 0.45f, 0.39f), FontStyle.Normal, TextAnchor.MiddleCenter);
        // 빈 장바구니 문구를 장바구니 영역 중앙 근처에 배치합니다.
        Anchor(emptyCartText.rectTransform, 0f, 0.45f, 1f, 0.65f, 20f, 0f, -20f, 0f);

        // 합계 금액 Text를 만듭니다.
        totalText = Label("합계 0원", right, 30, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 합계 금액을 오른쪽 영역 하단 버튼 위에 배치합니다.
        Anchor(totalText.rectTransform, 0f, 0.14f, 1f, 0.24f, 22f, 0f, -22f, 0f);

        // 비우기/결제하기 버튼들이 들어갈 영역을 만듭니다.
        var actions = Panel("Actions", right, new Color(0f, 0f, 0f, 0f));
        // 버튼 영역을 오른쪽 영역 맨 아래쪽에 배치합니다.
        Anchor(actions, 0f, 0.02f, 1f, 0.14f, 18f, 0f, -18f, 0f);
        // 두 버튼을 가로로 나란히 배치합니다.
        AddHorizontalLayout(actions, 12, TextAnchor.MiddleCenter);
        // 비우기 버튼을 만들고 ClearCart 메서드에 연결합니다.
        Button("비우기", actions, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, ClearCart, 118f);
        // 결제하기 버튼을 만들고 Checkout 메서드에 연결합니다.
        Button("결제하기", actions, 20, sage, Color.white, Checkout, 178f);

        // 상태 메시지 Text를 만듭니다.
        statusText = Label("", right, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 상태 메시지를 오른쪽 영역 가장 아래에 얇게 배치합니다.
        Anchor(statusText.rectTransform, 0f, 0f, 1f, 0.04f, 20f, 0f, -20f, 0f);

        // 음료 옵션 팝업을 생성합니다.
        CreateOptionOverlay(root);
        // 결제 방식 팝업을 생성합니다.
        CreatePaymentOverlay(root);
        // 시작 화면을 생성합니다.
        CreateStartScreen(root);
        // ViewModel의 화면 표시 상태에 맞춰 시작 화면/주문 화면/팝업 표시 여부를 적용합니다.
        RefreshScreens();
    }

    // 메뉴 영역을 ViewModel의 현재 카테고리 상태에 맞춰 다시 그립니다.
    private void RefreshMenu()
    {
        // 이전에 만들어진 메뉴 카드들을 모두 삭제합니다.
        ClearChildren(menuGrid);
        // ViewModel에서 현재 카테고리에 해당하는 메뉴만 가져옵니다.
        var visibleItems = viewModel.VisibleMenuItems;
        // 보여줄 메뉴가 하나도 없으면 안내 문구를 표시합니다.
        if (visibleItems.Count == 0)
        {
            // 빈 메뉴 상태를 사용자가 알 수 있게 Text를 하나 만듭니다.
            Label("표시할 메뉴가 없습니다.", menuGrid, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        // 보여줄 메뉴마다 카드 UI를 하나씩 만듭니다.
        foreach (var item in visibleItems)
        {
            // 메뉴 하나를 담을 흰색 카드 패널을 만듭니다.
            var card = Panel(item.Name, menuGrid, Color.white);
            // 카드 안의 이미지/이름/설명/가격/버튼을 세로로 쌓기 위해 VerticalLayoutGroup을 붙입니다.
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            // 카드 내부 여백을 설정합니다.
            layout.padding = new RectOffset(14, 14, 12, 12);
            // 카드 안 자식 UI들 사이의 간격을 설정합니다.
            layout.spacing = 4f;
            // 자식 UI의 너비를 레이아웃 그룹이 관리하도록 합니다.
            layout.childControlWidth = true;
            // 자식 UI의 높이는 각 요소가 가진 높이를 쓰도록 합니다.
            layout.childControlHeight = false;
            // 자식 UI가 카드 너비를 채우도록 합니다.
            layout.childForceExpandWidth = true;
            // 자식 UI가 세로로 강제로 늘어나지 않게 합니다.
            layout.childForceExpandHeight = false;

            // 메뉴 썸네일 이미지를 카드에 추가합니다.
            Thumbnail(item, card);
            // 메뉴 이름 Text를 카드에 추가합니다.
            Label(item.Name, card, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
            // 메뉴 설명 Text를 카드에 추가합니다.
            Label(item.Description, card, 15, new Color(0.42f, 0.38f, 0.32f), FontStyle.Normal, TextAnchor.MiddleLeft);
            // 가격 Text를 카드에 추가합니다. 음료는 "부터" 표시가 붙을 수 있습니다.
            Label(CafeKioskViewModel.MenuPriceText(item), card, 20, caramel, FontStyle.Bold, TextAnchor.MiddleLeft);
            // 담기 버튼을 만들고, 클릭하면 이 메뉴를 장바구니 흐름으로 넘깁니다.
            Button("담기", card, 17, espresso, Color.white, () => StartAddToCart(item), 0f, 38f);
        }

        // 3열 그리드 기준으로 필요한 줄 수를 계산합니다.
        var rows = Mathf.CeilToInt(Mathf.Max(1, visibleItems.Count) / 3f);
        // 스크롤 컨텐츠 높이를 줄 수에 맞춰 늘립니다.
        menuGrid.sizeDelta = new Vector2(menuGrid.sizeDelta.x, Mathf.Max(1, rows) * 222f);
        // Unity 레이아웃 시스템에 즉시 다시 계산하라고 요청합니다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuGrid);
    }

    // 장바구니 영역을 ViewModel의 현재 장바구니 상태에 맞춰 다시 그립니다.
    private void RefreshCart()
    {
        // 기존 장바구니 줄 UI를 모두 삭제합니다.
        ClearChildren(cartList);
        // 장바구니가 비어 있으면 빈 안내 문구를 보이고, 아니면 숨깁니다.
        emptyCartText.gameObject.SetActive(!viewModel.HasCartItems);

        // ViewModel의 장바구니 항목마다 UI 줄을 하나씩 만듭니다.
        foreach (var pair in viewModel.Cart)
        {
            // 장바구니 한 줄을 담을 패널을 만듭니다.
            var row = Panel(pair.Item.Name, cartList, new Color(0.98f, 0.94f, 0.88f));
            // 장바구니 줄의 높이를 78픽셀로 고정합니다.
            row.sizeDelta = new Vector2(0f, 78f);

            // 메뉴명과 옵션 표시 Text를 만듭니다.
            var name = Label(pair.DisplayName, row, 16, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
            // 메뉴명을 줄의 왼쪽 위쪽에 배치합니다.
            Anchor(name.rectTransform, 0f, 0.45f, 0.54f, 1f, 12f, 0f, 0f, 0f);

            // 해당 줄의 총액(단가 x 수량) Text를 만듭니다.
            var price = Label(CafeKioskViewModel.FormatPrice(pair.UnitPrice * pair.Quantity), row, 16, caramel, FontStyle.Bold, TextAnchor.MiddleLeft);
            // 가격을 줄의 왼쪽 아래쪽에 배치합니다.
            Anchor(price.rectTransform, 0f, 0f, 0.54f, 0.48f, 12f, 0f, 0f, 0f);

            // 수량 감소 버튼입니다. 누르면 ViewModel에 -1을 전달합니다.
            Button("-", row, 18, new Color(0.55f, 0.5f, 0.45f), Color.white, () => ChangeQuantity(pair, -1), 42f, 42f, 0.58f);
            // 현재 수량을 보여주는 Text입니다.
            var quantity = Label(pair.Quantity.ToString(), row, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
            // 수량 Text를 - 버튼과 + 버튼 사이에 배치합니다.
            Anchor(quantity.rectTransform, 0.72f, 0.22f, 0.82f, 0.78f, 0f, 0f, 0f, 0f);
            // 수량 증가 버튼입니다. 누르면 ViewModel에 +1을 전달합니다.
            Button("+", row, 18, sage, Color.white, () => ChangeQuantity(pair, 1), 42f, 42f, 0.85f);
        }

        // ViewModel의 장바구니 합계를 화면 Text에 반영합니다.
        totalText.text = $"합계 {CafeKioskViewModel.FormatPrice(viewModel.CartTotal)}";
        // 장바구니 줄들의 레이아웃을 즉시 다시 계산합니다.
        LayoutRebuilder.ForceRebuildLayoutImmediate(cartList);
    }

    // 메뉴 카드의 담기 버튼에서 호출됩니다.
    private void StartAddToCart(MenuItem item)
    {
        // ViewModel이 바로 장바구니에 담았다면 true를 반환합니다. 음료 옵션이 필요하면 false입니다.
        if (viewModel.StartAddToCart(item))
        {
            // 장바구니가 바뀌었으므로 장바구니 UI를 다시 그립니다.
            RefreshCart();
        }

        // 옵션 팝업 표시 여부와 상태 메시지를 ViewModel 상태에 맞춰 반영합니다.
        RefreshScreens();
    }

    // 장바구니의 +, - 버튼에서 호출됩니다.
    private void ChangeQuantity(CartLine line, int delta)
    {
        // ViewModel에 어떤 장바구니 줄의 수량을 얼마나 바꿀지 전달합니다.
        viewModel.ChangeQuantity(line, delta);
        // 수량과 합계가 바뀌었으므로 장바구니 UI를 다시 그립니다.
        RefreshCart();
        // 상태 메시지를 ViewModel 값으로 갱신합니다.
        RefreshScreens();
    }

    // 비우기 버튼에서 호출됩니다.
    private void ClearCart()
    {
        // ViewModel의 장바구니를 비웁니다.
        viewModel.ClearCart();
        // 비워진 장바구니 상태를 화면에 반영합니다.
        RefreshCart();
        // 상태 메시지를 화면에 반영합니다.
        RefreshScreens();
    }

    // 결제하기 버튼에서 호출됩니다.
    private void Checkout()
    {
        // ViewModel이 결제 가능하다고 판단하면 결제 팝업을 열 준비를 합니다.
        if (viewModel.Checkout())
        {
            // 새 결제마다 전화번호 입력칸은 비워 둡니다.
            memberPhoneInput.text = "";
            // 결제 금액, 멤버십 문구, 번호표 문구를 갱신합니다.
            RefreshPayment();
        }

        // 결제 팝업 표시 여부와 상태 메시지를 반영합니다.
        RefreshScreens();
    }

    // 결제 팝업의 결제 수단 버튼에서 호출됩니다.
    private void CompletePayment(string method)
    {
        // 선택한 결제 수단과 입력된 전화번호를 ViewModel에 전달해 결제를 완료합니다.
        viewModel.CompletePayment(method, memberPhoneInput.text);
        // 결제 결과 문구와 번호표를 갱신합니다.
        RefreshPayment();
        // 결제 후 장바구니가 비워졌으므로 장바구니 UI를 다시 그립니다.
        RefreshCart();
        // 결제 팝업 닫힘과 상태 메시지를 반영합니다.
        RefreshScreens();
    }

    // 결제 팝업의 돌아가기 버튼에서 호출됩니다.
    private void CancelPayment()
    {
        // ViewModel에 결제를 취소했다고 알립니다.
        viewModel.CancelPayment();
        // 결제 팝업 닫힘과 상태 메시지를 반영합니다.
        RefreshScreens();
    }

    // 결제 팝업의 회원가입/조회 버튼에서 호출됩니다.
    private void RegisterOrLookupMember()
    {
        // 입력칸의 전화번호를 ViewModel에 전달해 멤버십 조회/가입을 처리합니다.
        viewModel.RegisterOrLookupMember(memberPhoneInput.text);
        // 멤버십 결과 문구를 결제 팝업에 반영합니다.
        RefreshPayment();
    }

    // 음료 옵션을 선택하는 팝업 UI를 생성합니다.
    private void CreateOptionOverlay(Transform root)
    {
        // 화면 전체를 덮는 반투명 어두운 배경을 만듭니다.
        optionOverlay = Panel("Drink Option Overlay", root, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        // 팝업 배경을 화면 전체 크기로 늘립니다.
        Stretch(optionOverlay);
        // 처음에는 옵션 팝업을 숨깁니다.
        optionOverlay.gameObject.SetActive(false);

        // 팝업 중앙의 실제 흰색 패널을 만듭니다.
        var modal = Panel("Drink Option Panel", optionOverlay, paper);
        // 모달 패널을 화면 중앙에 적당한 크기로 배치합니다.
        Anchor(modal, 0.27f, 0.13f, 0.73f, 0.87f, 0f, 0f, 0f, 0f);

        // 팝업 제목 Text를 만듭니다.
        var title = Label("음료 옵션 선택", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 제목을 모달 상단에 배치합니다.
        Anchor(title.rectTransform, 0f, 0.83f, 1f, 0.96f, 24f, 0f, -24f, 0f);

        // 온도 선택 섹션 제목을 만듭니다.
        var tempTitle = Label("커피 온도", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 온도 제목을 모달 중상단에 배치합니다.
        Anchor(tempTitle.rectTransform, 0f, 0.7f, 1f, 0.79f, 34f, 0f, -34f, 0f);

        // ICE/HOT 버튼이 들어갈 영역을 만듭니다.
        var temps = Panel("Temperature Options", modal, new Color(0f, 0f, 0f, 0f));
        // 온도 버튼 영역을 온도 제목 아래에 배치합니다.
        Anchor(temps, 0f, 0.58f, 1f, 0.7f, 34f, 0f, -34f, 0f);
        // 온도 버튼 두 개를 가로로 배치합니다.
        AddHorizontalLayout(temps, 12f, TextAnchor.MiddleCenter);
        // ICE 버튼을 만들고 ViewModel의 온도 선택 상태를 바꾸는 메서드에 연결합니다.
        Button("ICE", temps, 22, sage, Color.white, () => SelectTemperature("ICE"), 150f, 54f);
        // HOT 버튼을 만들고 ViewModel의 온도 선택 상태를 바꾸는 메서드에 연결합니다.
        Button("HOT", temps, 22, espresso, Color.white, () => SelectTemperature("HOT"), 150f, 54f);

        // 사이즈 선택 섹션 제목을 만듭니다.
        var sizeTitle = Label("음료 사이즈", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 사이즈 제목을 온도 영역 아래에 배치합니다.
        Anchor(sizeTitle.rectTransform, 0f, 0.47f, 1f, 0.56f, 34f, 0f, -34f, 0f);

        // Small/Regular/Large 버튼이 들어갈 영역을 만듭니다.
        var sizes = Panel("Size Options", modal, new Color(0f, 0f, 0f, 0f));
        // 사이즈 버튼 영역을 사이즈 제목 아래에 배치합니다.
        Anchor(sizes, 0f, 0.29f, 1f, 0.47f, 34f, 0f, -34f, 0f);
        // 사이즈 버튼 세 개를 가로로 배치합니다.
        AddHorizontalLayout(sizes, 12f, TextAnchor.MiddleCenter);
        // Small 버튼을 만들고 사이즈 선택 메서드에 연결합니다.
        Button("Small\n기본", sizes, 17, new Color(0.42f, 0.38f, 0.34f), Color.white, () => SelectSize("Small"), 126f, 74f);
        // Regular 버튼을 만들고 사이즈 선택 메서드에 연결합니다.
        Button("Regular\n+500원", sizes, 17, caramel, Color.white, () => SelectSize("Regular"), 140f, 74f);
        // Large 버튼을 만들고 사이즈 선택 메서드에 연결합니다.
        Button("Large\n+1,000원", sizes, 17, sage, Color.white, () => SelectSize("Large"), 140f, 74f);

        // 선택한 옵션으로 장바구니에 담는 버튼을 만듭니다.
        var add = Button("선택 담기", modal, 22, espresso, Color.white, ConfirmDrinkOption, 180f, 54f);
        // 선택 담기 버튼을 모달 하단 중앙에 배치합니다.
        Anchor(add.GetComponent<RectTransform>(), 0.5f, 0.15f, 0.5f, 0.24f, -90f, 0f, 90f, 0f);

        // 옵션 선택을 취소하는 버튼을 만듭니다.
        var cancel = Button("취소", modal, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, CancelDrinkOption, 130f, 44f);
        // 취소 버튼을 선택 담기 버튼 아래에 배치합니다.
        Anchor(cancel.GetComponent<RectTransform>(), 0.5f, 0.05f, 0.5f, 0.12f, -65f, 0f, 65f, 0f);
    }

    // ICE/HOT 버튼에서 호출됩니다.
    private void SelectTemperature(string temperature)
    {
        // 선택된 온도를 ViewModel에 저장합니다.
        viewModel.SelectTemperature(temperature);
        // 상태 메시지를 화면에 반영합니다.
        RefreshScreens();
    }

    // Small/Regular/Large 버튼에서 호출됩니다.
    private void SelectSize(string size)
    {
        // 선택된 사이즈를 ViewModel에 저장합니다.
        viewModel.SelectSize(size);
        // 상태 메시지를 화면에 반영합니다.
        RefreshScreens();
    }

    // 옵션 팝업의 선택 담기 버튼에서 호출됩니다.
    private void ConfirmDrinkOption()
    {
        // ViewModel이 옵션 적용 후 장바구니에 담았다면 true를 반환합니다.
        if (viewModel.ConfirmDrinkOption())
        {
            // 장바구니가 바뀌었으므로 장바구니 UI를 다시 그립니다.
            RefreshCart();
        }

        // 옵션 팝업 닫힘과 상태 메시지를 반영합니다.
        RefreshScreens();
    }

    // 옵션 팝업의 취소 버튼에서 호출됩니다.
    private void CancelDrinkOption()
    {
        // ViewModel에 옵션 선택을 취소했다고 알립니다.
        viewModel.CancelDrinkOption();
        // 옵션 팝업 닫힘과 상태 메시지를 반영합니다.
        RefreshScreens();
    }

    // 매장/포장 선택 시작 화면을 생성합니다.
    private void CreateStartScreen(Transform root)
    {
        // 시작 화면 패널을 만듭니다.
        startScreen = Panel("Start Order Screen", root, cream);
        // 시작 화면을 전체 화면으로 늘립니다.
        Stretch(startScreen);

        // 카페 이름 제목을 만듭니다.
        var title = Label("Megazone Cafe", startScreen, 56, espresso, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 제목을 시작 화면 위쪽에 배치합니다.
        Anchor(title.rectTransform, 0f, 0.68f, 1f, 0.82f, 24f, 0f, -24f, 0f);

        // 주문 방식 선택 안내 문구를 만듭니다.
        var subtitle = Label("주문 방식을 선택해주세요", startScreen, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 안내 문구를 제목 아래에 배치합니다.
        Anchor(subtitle.rectTransform, 0f, 0.58f, 1f, 0.68f, 24f, 0f, -24f, 0f);

        // 매장/포장 버튼이 들어갈 영역을 만듭니다.
        var choices = Panel("Order Mode Choices", startScreen, new Color(0f, 0f, 0f, 0f));
        // 버튼 영역을 시작 화면 중앙 아래쪽에 배치합니다.
        Anchor(choices, 0.16f, 0.28f, 0.84f, 0.55f, 0f, 0f, 0f, 0f);
        // 두 버튼을 가로로 배치합니다.
        AddHorizontalLayout(choices, 22f, TextAnchor.MiddleCenter);

        // 매장 버튼을 만들고 주문 방식 선택 메서드에 연결합니다.
        Button("매장", choices, 34, espresso, Color.white, () => SelectOrderMode("매장"), 260f, 150f);
        // 포장 버튼을 만들고 주문 방식 선택 메서드에 연결합니다.
        Button("포장", choices, 34, sage, Color.white, () => SelectOrderMode("포장"), 260f, 150f);
    }

    // 매장/포장 버튼에서 호출됩니다.
    private void SelectOrderMode(string mode)
    {
        // 선택한 주문 방식을 ViewModel에 저장합니다.
        viewModel.SelectOrderMode(mode);
        // 시작 화면을 닫고 주문 화면을 여는 상태를 반영합니다.
        RefreshScreens();
    }

    // 결제 방식 선택 팝업 UI를 생성합니다.
    private void CreatePaymentOverlay(Transform root)
    {
        // 화면 전체를 덮는 반투명 어두운 배경을 만듭니다.
        paymentOverlay = Panel("Payment Method Overlay", root, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        // 결제 팝업 배경을 화면 전체 크기로 늘립니다.
        Stretch(paymentOverlay);
        // 처음에는 결제 팝업을 숨깁니다.
        paymentOverlay.gameObject.SetActive(false);

        // 결제 팝업 중앙의 실제 패널을 만듭니다.
        var modal = Panel("Payment Method Panel", paymentOverlay, paper);
        // 결제 모달을 화면 중앙에 배치합니다.
        Anchor(modal, 0.28f, 0.18f, 0.72f, 0.82f, 0f, 0f, 0f, 0f);

        // 결제 팝업 제목 Text를 만듭니다.
        var title = Label("결제 방식을 선택하세요", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 제목을 결제 모달 상단에 배치합니다.
        Anchor(title.rectTransform, 0f, 0.78f, 1f, 0.95f, 24f, 0f, -24f, 0f);

        // 결제 금액 Text를 만들고, 이후 RefreshPayment에서 실제 금액으로 바뀝니다.
        paymentTotalText = Label("결제 금액 0원", modal, 24, caramel, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 결제 금액을 제목 아래에 배치합니다.
        Anchor(paymentTotalText.rectTransform, 0f, 0.66f, 1f, 0.78f, 24f, 0f, -24f, 0f);

        // 멤버십 전화번호 입력 제목을 만듭니다.
        var memberTitle = Label("멤버십 전화번호", modal, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        // 멤버십 제목을 입력칸 위에 배치합니다.
        Anchor(memberTitle.rectTransform, 0f, 0.58f, 1f, 0.66f, 34f, 0f, -34f, 0f);

        // 전화번호 입력칸을 만듭니다.
        memberPhoneInput = Input("01012345678", modal);
        // 입력칸을 모달 왼쪽에 배치합니다.
        Anchor(memberPhoneInput.GetComponent<RectTransform>(), 0f, 0.49f, 0.72f, 0.58f, 34f, 0f, -8f, 0f);

        // 회원가입/조회 버튼을 만듭니다.
        var join = Button("회원가입/조회", modal, 17, sage, Color.white, RegisterOrLookupMember, 146f, 48f);
        // 회원가입/조회 버튼을 입력칸 오른쪽에 배치합니다.
        Anchor(join.GetComponent<RectTransform>(), 0.72f, 0.49f, 1f, 0.58f, 8f, 0f, -34f, 0f);

        // 멤버십 조회 결과나 안내 문구를 보여줄 Text를 만듭니다.
        memberStatusText = Label("전화번호를 입력하면 스탬프가 적립됩니다.", modal, 15, new Color(0.46f, 0.42f, 0.36f), FontStyle.Normal, TextAnchor.MiddleCenter);
        // 멤버십 문구를 입력칸 아래에 배치합니다.
        Anchor(memberStatusText.rectTransform, 0f, 0.43f, 1f, 0.49f, 34f, 0f, -34f, 0f);

        // 결제 수단 버튼들이 들어갈 영역을 만듭니다.
        var methods = Panel("Payment Methods", modal, new Color(0f, 0f, 0f, 0f));
        // 결제 수단 영역을 모달 중하단에 배치합니다.
        Anchor(methods, 0f, 0.25f, 1f, 0.42f, 28f, 0f, -28f, 0f);
        // 결제 수단 버튼들을 가로로 배치합니다.
        AddHorizontalLayout(methods, 14f, TextAnchor.MiddleCenter);

        // 카드 결제 버튼을 만들고 CompletePayment에 "카드" 값을 전달합니다.
        Button("카드", methods, 22, espresso, Color.white, () => CompletePayment("카드"), 130f, 70f);
        // 현금 결제 버튼을 만들고 CompletePayment에 "현금" 값을 전달합니다.
        Button("현금", methods, 22, sage, Color.white, () => CompletePayment("현금"), 130f, 70f);
        // 모바일페이 버튼을 만들고 CompletePayment에 "모바일페이" 값을 전달합니다.
        Button("모바일페이", methods, 20, caramel, Color.white, () => CompletePayment("모바일페이"), 170f, 70f);

        // 결제 완료 후 번호표를 보여줄 Text를 만듭니다.
        ticketText = Label("", modal, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 번호표 Text를 결제 수단 아래에 배치합니다.
        Anchor(ticketText.rectTransform, 0f, 0.16f, 1f, 0.24f, 24f, 0f, -24f, 0f);

        // 결제 팝업을 닫는 돌아가기 버튼을 만듭니다.
        var back = Button("돌아가기", modal, 19, new Color(0.42f, 0.38f, 0.34f), Color.white, CancelPayment, 150f, 46f);
        // 돌아가기 버튼을 모달 하단 중앙에 배치합니다.
        Anchor(back.GetComponent<RectTransform>(), 0.5f, 0.06f, 0.5f, 0.14f, -75f, 0f, 75f, 0f);
    }

    // ViewModel이 가진 화면 표시 상태를 실제 Unity GameObject 활성/비활성 상태로 반영합니다.
    private void RefreshScreens()
    {
        // 시작 화면 오브젝트가 만들어져 있을 때만 처리합니다.
        if (startScreen != null)
        {
            // ViewModel이 시작 화면을 보이라고 하면 활성화하고, 아니면 비활성화합니다.
            startScreen.gameObject.SetActive(viewModel.IsStartScreenVisible);
        }

        // 주문 화면 오브젝트가 만들어져 있을 때만 처리합니다.
        if (orderScreen != null)
        {
            // ViewModel이 주문 화면을 보이라고 하면 활성화하고, 아니면 비활성화합니다.
            orderScreen.gameObject.SetActive(viewModel.IsOrderScreenVisible);
        }

        // 옵션 팝업 오브젝트가 만들어져 있을 때만 처리합니다.
        if (optionOverlay != null)
        {
            // ViewModel의 옵션 팝업 표시 상태를 Unity 오브젝트에 적용합니다.
            optionOverlay.gameObject.SetActive(viewModel.IsOptionOverlayVisible);
        }

        // 결제 팝업 오브젝트가 만들어져 있을 때만 처리합니다.
        if (paymentOverlay != null)
        {
            // ViewModel의 결제 팝업 표시 상태를 Unity 오브젝트에 적용합니다.
            paymentOverlay.gameObject.SetActive(viewModel.IsPaymentOverlayVisible);
        }

        // 상태 메시지 Text가 만들어져 있을 때만 처리합니다.
        if (statusText != null)
        {
            // ViewModel의 상태 메시지를 화면 하단 Text에 적용합니다.
            statusText.text = viewModel.StatusText;
        }
    }

    // 결제 팝업 안에 있는 금액, 멤버십, 번호표 Text를 ViewModel 상태로 갱신합니다.
    private void RefreshPayment()
    {
        // 결제 금액을 "결제 금액 4,500원" 같은 문자열로 표시합니다.
        paymentTotalText.text = $"결제 금액 {CafeKioskViewModel.FormatPrice(viewModel.PaymentAmount)}";
        // 멤버십 조회/가입/적립 결과 문구를 표시합니다.
        memberStatusText.text = viewModel.MemberStatusText;
        // 결제 완료 후 번호표 문구를 표시합니다. 결제 전에는 빈 문자열입니다.
        ticketText.text = viewModel.TicketText;
    }

    // 메뉴 카드의 상단 썸네일 이미지를 만듭니다.
    private void Thumbnail(MenuItem item, Transform parent)
    {
        // RectTransform과 Image 컴포넌트를 가진 새 GameObject를 생성합니다.
        var thumbnailObject = new GameObject($"{item.Name} Thumbnail", typeof(RectTransform), typeof(Image));
        // 새 썸네일 오브젝트를 메뉴 카드 아래 자식으로 붙입니다.
        thumbnailObject.transform.SetParent(parent, false);

        // 위치와 크기를 다루기 위해 RectTransform을 가져옵니다.
        var rect = thumbnailObject.GetComponent<RectTransform>();
        // 레이아웃 그룹 안에서 기본 높이 74픽셀을 가지도록 설정합니다.
        rect.sizeDelta = new Vector2(0f, 74f);

        // LayoutElement는 VerticalLayoutGroup 안에서 이 요소의 선호 높이를 알려줍니다.
        var layout = thumbnailObject.AddComponent<LayoutElement>();
        // 선호 높이를 74픽셀로 지정합니다.
        layout.preferredHeight = 74f;
        // 최소 높이도 74픽셀로 지정해서 너무 작아지지 않게 합니다.
        layout.minHeight = 74f;

        // 실제 이미지를 보여주는 Image 컴포넌트를 가져옵니다.
        var image = thumbnailObject.GetComponent<Image>();
        // 메뉴별 Sprite를 썸네일 팩토리에서 받아 넣습니다.
        image.sprite = CafeKioskThumbnailFactory.Get(item);
        // Sprite를 단순 이미지로 그립니다.
        image.type = Image.Type.Simple;
        // 카드 영역을 꽉 채우기 위해 원본 비율 고정을 끕니다.
        image.preserveAspect = false;
    }

    // 키오스크 UI가 올라갈 Canvas를 생성합니다.
    private Canvas CreateCanvas()
    {
        // Canvas용 GameObject를 새로 만듭니다.
        var canvasObject = new GameObject("Cafe Kiosk Canvas");
        // 이 컨트롤러가 붙은 GameObject 아래에 Canvas를 붙입니다.
        canvasObject.transform.SetParent(transform, false);

        // Canvas 컴포넌트를 추가합니다.
        var canvas = canvasObject.AddComponent<Canvas>();
        // ScreenSpaceOverlay는 카메라 없이 화면 위에 UI를 바로 그리는 모드입니다.
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 다른 UI보다 앞에 보이도록 정렬 순서를 높게 둡니다.
        canvas.sortingOrder = 10;

        // 화면 해상도에 따라 UI 크기를 자동 조절하기 위해 CanvasScaler를 추가합니다.
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        // 기준 해상도를 두고 실제 화면에 맞춰 스케일되게 합니다.
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // UI를 디자인한 기준 해상도입니다.
        scaler.referenceResolution = new Vector2(1280f, 720f);
        // 가로/세로 중간 비율로 스케일을 맞춥니다.
        scaler.matchWidthOrHeight = 0.5f;

        // 버튼 클릭 같은 UI Raycast를 처리하기 위해 GraphicRaycaster를 추가합니다.
        canvasObject.AddComponent<GraphicRaycaster>();
        // 만든 Canvas를 호출한 곳에 돌려줍니다.
        return canvas;
    }

    // Unity UI가 클릭과 입력을 받을 수 있도록 EventSystem을 보장합니다.
    private static void EnsureEventSystem()
    {
        // 씬 안에 이미 EventSystem이 있는지 찾습니다.
        var eventSystem = FindAnyObjectByType<EventSystem>();
        // EventSystem이 없으면 새 GameObject를 만들고 EventSystem 컴포넌트를 추가합니다.
        if (eventSystem == null)
        {
            // UI 입력 처리를 담당하는 EventSystem 오브젝트를 생성합니다.
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        }

        // 구 Input Manager용 StandaloneInputModule이 붙어 있는지 확인합니다.
        var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        // 이 프로젝트는 새 Input System 모듈을 쓰므로 기존 모듈이 있으면 제거합니다.
        if (legacyModule != null)
        {
            // 플레이 모드와 에디터 모드에 맞게 컴포넌트를 삭제합니다.
            DestroyComponent(legacyModule);
        }

        // 새 Input System용 UI 입력 모듈이 없으면 추가합니다.
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            // 버튼 클릭, 포인터 입력, 키보드 입력 등을 새 Input System으로 처리하게 합니다.
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    // 단색 배경을 가진 기본 UI 패널을 만듭니다.
    private RectTransform Panel(string name, Transform parent, Color color)
    {
        // RectTransform과 Image를 가진 GameObject를 만듭니다.
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        // 생성한 패널을 지정된 부모 아래에 붙입니다.
        panel.transform.SetParent(parent, false);
        // 위치와 크기를 조정할 RectTransform을 가져옵니다.
        var rect = panel.GetComponent<RectTransform>();
        // 배경색을 칠할 Image 컴포넌트를 가져옵니다.
        var image = panel.GetComponent<Image>();
        // Image 색을 지정해서 패널 배경색을 만듭니다.
        image.color = color;
        // 위치 배치에 쓸 RectTransform을 반환합니다.
        return rect;
    }

    // Text UI를 만들고 기본 폰트/크기/색/정렬을 설정합니다.
    private Text Label(string text, Transform parent, int size, Color color, FontStyle style, TextAnchor alignment)
    {
        // RectTransform과 Text를 가진 GameObject를 만듭니다.
        var label = new GameObject($"{text} Label", typeof(RectTransform), typeof(Text));
        // 생성한 Text 오브젝트를 지정된 부모 아래에 붙입니다.
        label.transform.SetParent(parent, false);
        // 위치와 크기를 조정할 RectTransform을 가져옵니다.
        var rect = label.GetComponent<RectTransform>();
        // 기본 높이를 폰트 크기보다 조금 크게 잡습니다.
        rect.sizeDelta = new Vector2(0f, size + 12f);

        // 실제 글자를 표시하는 Text 컴포넌트를 가져옵니다.
        var uiText = label.GetComponent<Text>();
        // 화면에 표시할 문자열을 넣습니다.
        uiText.text = text;
        // RebuildInterface에서 준비한 폰트를 적용합니다.
        uiText.font = font;
        // 글자 크기를 지정합니다.
        uiText.fontSize = size;
        // 굵게, 보통 같은 글꼴 스타일을 지정합니다.
        uiText.fontStyle = style;
        // 글자 색을 지정합니다.
        uiText.color = color;
        // Text 영역 안에서 글자가 어디에 정렬될지 지정합니다.
        uiText.alignment = alignment;
        // Rich Text 태그를 해석하지 않게 해서 입력 문자열 그대로 표시합니다.
        uiText.supportRichText = false;
        // 가로로 길면 줄바꿈되도록 합니다.
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        // 세로로 넘치면 잘리도록 합니다.
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        // 나중에 위치 조정이나 텍스트 변경을 할 수 있게 Text 컴포넌트를 반환합니다.
        return uiText;
    }

    // Button UI를 만들고 클릭 이벤트까지 연결합니다.
    private Button Button(string text, Transform parent, int size, Color background, Color foreground, UnityEngine.Events.UnityAction action, float width = 0f, float height = 46f, float anchorX = -1f)
    {
        // 버튼 배경으로 사용할 패널을 만듭니다.
        var buttonRect = Panel($"{text} Button", parent, background);
        // 버튼의 기본 크기를 지정합니다.
        buttonRect.sizeDelta = new Vector2(width, height);

        // Unity Button 컴포넌트를 추가합니다.
        var button = buttonRect.gameObject.AddComponent<Button>();
        // Button이 눌림 색 변화 등을 적용할 대상 그래픽을 배경 Image로 지정합니다.
        button.targetGraphic = buttonRect.GetComponent<Image>();
        // 버튼을 클릭했을 때 실행할 메서드 또는 람다를 등록합니다.
        button.onClick.AddListener(action);

        // 버튼 안에 표시될 글자 Text를 만듭니다.
        var label = Label(text, buttonRect, size, foreground, FontStyle.Bold, TextAnchor.MiddleCenter);
        // 버튼 글자가 버튼 전체 영역을 차지하도록 늘립니다.
        Stretch(label.rectTransform);

        // anchorX가 0 이상이면 이 버튼을 부모 안에서 특정 x 비율 위치에 고정 배치합니다.
        if (anchorX >= 0f)
        {
            // 버튼 중심을 anchorX 위치에 두고 width만큼 좌우로 배치합니다.
            Anchor(buttonRect, anchorX, 0.22f, anchorX, 0.78f, -width * 0.5f, 0f, width * 0.5f, 0f);
        }

        // 만든 Button 컴포넌트를 반환합니다.
        return button;
    }

    // 전화번호 입력용 InputField UI를 만듭니다.
    private InputField Input(string placeholder, Transform parent)
    {
        // 입력칸 배경 패널을 흰색으로 만듭니다.
        var inputRect = Panel("Phone Input", parent, Color.white);
        // 입력칸의 기본 높이를 48픽셀로 지정합니다.
        inputRect.sizeDelta = new Vector2(0f, 48f);

        // Unity InputField 컴포넌트를 추가합니다.
        var input = inputRect.gameObject.AddComponent<InputField>();
        // 숫자만 입력받는 전화번호 입력칸으로 설정합니다.
        input.contentType = InputField.ContentType.IntegerNumber;
        // 전화번호 길이가 너무 길어지지 않게 최대 13자로 제한합니다.
        input.characterLimit = 13;
        // 입력칸이 선택되었을 때 반응할 그래픽 대상을 배경 Image로 지정합니다.
        input.targetGraphic = inputRect.GetComponent<Image>();

        // 실제 사용자가 입력한 문자를 표시할 Text를 만듭니다.
        var text = Label("", inputRect, 21, charcoal, FontStyle.Normal, TextAnchor.MiddleLeft);
        // 입력 Text를 입력칸 전체에 배치하되 좌우 여백을 줍니다.
        Anchor(text.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        // 아무것도 입력하지 않았을 때 보여줄 placeholder Text를 만듭니다.
        var placeholderText = Label(placeholder, inputRect, 19, new Color(0.62f, 0.58f, 0.52f), FontStyle.Normal, TextAnchor.MiddleLeft);
        // placeholder Text도 입력칸 전체에 배치하되 좌우 여백을 줍니다.
        Anchor(placeholderText.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        // InputField가 실제 입력 문자열을 표시할 Text 컴포넌트를 알려줍니다.
        input.textComponent = text;
        // InputField가 비어 있을 때 보여줄 placeholder 컴포넌트를 알려줍니다.
        input.placeholder = placeholderText;
        // 만든 InputField를 반환합니다.
        return input;
    }

    // 스크롤 가능한 영역을 만들고 viewport와 content를 함께 반환합니다.
    private (RectTransform viewport, RectTransform content) ScrollArea(string name, Transform parent)
    {
        // 스크롤에서 보이는 영역인 viewport 오브젝트를 만듭니다.
        var viewportObject = new GameObject($"{name} Viewport", typeof(RectTransform), typeof(RectMask2D));
        // viewport를 지정된 부모 아래에 붙입니다.
        viewportObject.transform.SetParent(parent, false);
        // viewport의 RectTransform을 가져옵니다.
        var viewport = viewportObject.GetComponent<RectTransform>();

        // 실제 스크롤될 자식들을 담는 content 오브젝트를 만듭니다.
        var contentObject = new GameObject($"{name} Content", typeof(RectTransform));
        // content를 viewport 아래에 붙입니다.
        contentObject.transform.SetParent(viewport, false);
        // content의 RectTransform을 가져옵니다.
        var content = contentObject.GetComponent<RectTransform>();
        // content의 왼쪽/오른쪽은 viewport에 맞추고, 위쪽 기준으로 움직이게 합니다.
        content.anchorMin = new Vector2(0f, 1f);
        // content가 viewport 너비를 따라가도록 x anchor를 1까지 둡니다.
        content.anchorMax = new Vector2(1f, 1f);
        // content의 기준점을 위쪽 가운데로 잡아 목록이 위에서 아래로 늘어나게 합니다.
        content.pivot = new Vector2(0.5f, 1f);
        // content의 시작 위치를 viewport의 위쪽에 맞춥니다.
        content.anchoredPosition = Vector2.zero;
        // content의 기본 크기 보정을 0으로 둡니다.
        content.sizeDelta = Vector2.zero;

        // viewport에 ScrollRect 컴포넌트를 붙여 실제 스크롤 기능을 만듭니다.
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        // ScrollRect가 움직일 대상 content를 지정합니다.
        scroll.content = content;
        // ScrollRect가 보여줄 viewport 영역을 지정합니다.
        scroll.viewport = viewport;
        // 좌우 스크롤은 사용하지 않습니다.
        scroll.horizontal = false;
        // 위아래 스크롤은 사용합니다.
        scroll.vertical = true;
        // 스크롤이 영역 밖으로 튕기지 않고 끝에서 멈추게 합니다.
        scroll.movementType = ScrollRect.MovementType.Clamped;
        // 호출한 쪽에서 위치 지정과 자식 추가를 할 수 있도록 viewport와 content를 반환합니다.
        return (viewport, content);
    }

    // 자식 UI들을 가로로 배치하는 HorizontalLayoutGroup을 추가합니다.
    private static void AddHorizontalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        // 지정된 RectTransform의 GameObject에 HorizontalLayoutGroup을 붙입니다.
        var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        // 자식들 사이의 가로 간격을 지정합니다.
        layout.spacing = spacing;
        // 자식 너비는 각 자식의 sizeDelta를 따르게 합니다.
        layout.childControlWidth = false;
        // 자식 높이는 부모 높이에 맞춰 관리합니다.
        layout.childControlHeight = true;
        // 자식들이 남는 너비를 강제로 나눠 갖지 않게 합니다.
        layout.childForceExpandWidth = false;
        // 자식들이 높이를 채우도록 합니다.
        layout.childForceExpandHeight = true;
        // 자식들이 부모 안에서 어디에 모일지 정합니다.
        layout.childAlignment = alignment;
    }

    // 자식 UI들을 세로로 배치하는 VerticalLayoutGroup을 추가합니다.
    private static void AddVerticalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        // 지정된 RectTransform의 GameObject에 VerticalLayoutGroup을 붙입니다.
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        // 자식들 사이의 세로 간격을 지정합니다.
        layout.spacing = spacing;
        // 자식 너비는 부모 너비에 맞춰 관리합니다.
        layout.childControlWidth = true;
        // 자식 높이는 각 자식의 sizeDelta를 따르게 합니다.
        layout.childControlHeight = false;
        // 자식들이 가로 공간을 채우도록 합니다.
        layout.childForceExpandWidth = true;
        // 자식들이 세로 공간을 강제로 나눠 갖지 않게 합니다.
        layout.childForceExpandHeight = false;
        // 자식들이 부모 안에서 어디부터 쌓일지 정합니다.
        layout.childAlignment = alignment;

        // content 높이가 자식들의 총 높이에 맞춰 자동으로 커지게 하기 위해 ContentSizeFitter를 추가합니다.
        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        // 세로 크기를 자식들의 선호 높이 합계에 맞춥니다.
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // 메뉴 카드들을 격자로 배치하는 GridLayoutGroup을 추가합니다.
    private static void AddGrid(RectTransform rect, Vector2 cellSize, Vector2 spacing, RectOffset padding)
    {
        // 지정된 RectTransform의 GameObject에 GridLayoutGroup을 붙입니다.
        var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
        // 각 카드 셀의 크기를 지정합니다.
        grid.cellSize = cellSize;
        // 카드 사이의 가로/세로 간격을 지정합니다.
        grid.spacing = spacing;
        // 그리드 안쪽 여백을 지정합니다.
        grid.padding = padding;
        // 열 개수를 고정하는 방식으로 그리드를 배치합니다.
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        // 메뉴 카드는 한 줄에 3개씩 보여줍니다.
        grid.constraintCount = 3;

        // 그리드 content 높이가 카드 줄 수에 맞춰 자동으로 커지게 합니다.
        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        // 세로 크기를 자식들의 선호 높이 기준으로 맞춥니다.
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // RectTransform의 anchor와 offset을 한 번에 설정하는 헬퍼입니다.
    private static void Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
    {
        // 부모 영역 안에서 왼쪽 아래 기준 anchor를 지정합니다.
        rect.anchorMin = new Vector2(minX, minY);
        // 부모 영역 안에서 오른쪽 위 기준 anchor를 지정합니다.
        rect.anchorMax = new Vector2(maxX, maxY);
        // anchorMin 기준으로 왼쪽/아래 여백을 지정합니다.
        rect.offsetMin = new Vector2(left, bottom);
        // anchorMax 기준으로 오른쪽/위 여백을 지정합니다.
        rect.offsetMax = new Vector2(right, top);
    }

    // RectTransform을 부모 전체에 꽉 차게 만드는 헬퍼입니다.
    private static void Stretch(RectTransform rect)
    {
        // anchor를 0~1 전체로 두고 offset을 모두 0으로 만들어 부모를 꽉 채웁니다.
        Anchor(rect, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
    }

    // 지정된 부모 아래의 자식 GameObject를 모두 삭제합니다.
    private static void ClearChildren(Transform parent)
    {
        // 삭제 중 인덱스가 밀리지 않도록 뒤에서 앞으로 순회합니다.
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            // 각 자식 GameObject를 플레이 모드/에디터 모드에 맞게 삭제합니다.
            DestroyGeneratedObject(parent.GetChild(i).gameObject);
        }
    }

    // 이 컨트롤러가 이전에 만든 Canvas를 찾아 삭제합니다.
    private void RemoveGeneratedChildren()
    {
        // 이 컴포넌트가 붙은 Transform의 자식들을 뒤에서 앞으로 순회합니다.
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            // 현재 검사 중인 자식 Transform을 가져옵니다.
            var child = transform.GetChild(i);
            // 자동 생성 Canvas 이름과 같으면 삭제 대상입니다.
            if (child.name == "Cafe Kiosk Canvas")
            {
                // 기존 Canvas를 삭제해서 새 UI와 중복되지 않게 합니다.
                DestroyGeneratedObject(child.gameObject);
            }
        }
    }

    // GameObject나 컴포넌트 같은 Unity Object를 현재 실행 상태에 맞게 삭제합니다.
    private static void DestroyGeneratedObject(Object target)
    {
        // 플레이 모드에서는 Destroy를 써야 프레임 끝에 안전하게 삭제됩니다.
        if (Application.isPlaying)
        {
            // 런타임 삭제를 예약합니다.
            Destroy(target);
        }
        // 에디터 모드에서는 Destroy가 바로 동작하지 않으므로 DestroyImmediate를 씁니다.
        else
        {
            // 에디터에서 즉시 삭제합니다.
            DestroyImmediate(target);
        }
    }

    // 컴포넌트를 현재 실행 상태에 맞게 삭제합니다.
    private static void DestroyComponent(Component target)
    {
        // 플레이 모드에서는 Destroy를 사용합니다.
        if (Application.isPlaying)
        {
            // 런타임 삭제를 예약합니다.
            Destroy(target);
        }
        // 에디터 모드에서는 DestroyImmediate를 사용합니다.
        else
        {
            // 에디터에서 즉시 삭제합니다.
            DestroyImmediate(target);
        }
    }

}
