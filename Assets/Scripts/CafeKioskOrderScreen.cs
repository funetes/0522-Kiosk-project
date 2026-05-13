using UnityEngine;
using UnityEngine.UI;

// 카페 키오스크의 메인 주문 화면을 담당하는 클래스
public sealed class CafeKioskOrderScreen
{
    public RectTransform Root { get; private set; } // 화면의 최상위 루트 오브젝트
    private readonly CafeKioskViewModel viewModel; // 데이터 및 상태 제어를 위한 뷰모델
    private readonly Font font; // 공통 사용 폰트
    
    // UI 디자인을 위한 컬러 팔레트
    private readonly Color cream;
    private readonly Color espresso;
    private readonly Color charcoal;
    private readonly Color caramel;
    private readonly Color sage;
    private readonly Color paper;

    // 동적으로 내용이 변하는 UI 참조 변수들
    private RectTransform menuGrid; // 메뉴 아이템들이 배치될 그리드
    private RectTransform cartList; // 장바구니 아이템 리스트
    private Text totalText;         // 총 합계 금액 텍스트
    private Text emptyCartText;     // 장바구니가 비었을 때 표시할 안내 문구
    private Text statusText;        // 하단 상태 메시지 (알림 등)

    // 생성자: 필요한 색상 정보와 콜백들을 전달받아 화면을 구축함
    public CafeKioskOrderScreen(Transform parent, CafeKioskViewModel viewModel, Font font, 
        Color cream, Color espresso, Color charcoal, Color caramel, Color sage, Color paper,
        System.Action onRefreshMenu, System.Action onRefreshCart, System.Action onCheckout, System.Action onAction)
    {
        this.viewModel = viewModel;
        this.font = font;
        this.cream = cream;
        this.espresso = espresso;
        this.charcoal = charcoal;
        this.caramel = caramel;
        this.sage = sage;
        this.paper = paper;

        Build(parent, onRefreshMenu, onRefreshCart, onCheckout, onAction);
    }

    // 전체적인 UI 레이아웃을 코드로 생성하고 배치하는 함수
    private void Build(Transform parent, System.Action onRefreshMenu, System.Action onRefreshCart, System.Action onCheckout, System.Action onAction)
    {
        // 배경 패널 생성 및 화면 전체 채우기
        Root = CafeKioskUIUtility.Panel("Order Screen", parent, cream);
        CafeKioskUIUtility.Stretch(Root);

        // 상단 헤더 영역 (브랜드 로직 및 타이틀)
        var header = CafeKioskUIUtility.Panel("Header", Root, espresso);
        CafeKioskUIUtility.Anchor(header, 0f, 0.85f, 1f, 1f, 28f, 18f, -28f, -18f);

        var title = CafeKioskUIUtility.Label("Megazone Cafe", header, 42, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0f, 0.5f, 1f, 26f, 0f, 0f, 0f);

        var subtitle = CafeKioskUIUtility.Label("주문할 메뉴를 선택하세요", header, 20, new Color(1f, 0.88f, 0.7f), FontStyle.Normal, TextAnchor.MiddleRight, font);
        CafeKioskUIUtility.Anchor(subtitle.rectTransform, 0.5f, 0f, 1f, 1f, 0f, 0f, -26f, 0f);

        // 중앙 콘텐츠 영역
        var content = CafeKioskUIUtility.Panel("Content", Root, cream);
        CafeKioskUIUtility.Anchor(content, 0f, 0f, 1f, 0.85f, 28f, 24f, -28f, -12f);

        // [왼쪽] 메뉴 선택 영역
        var left = CafeKioskUIUtility.Panel("Menu Area", content, new Color(0.98f, 0.95f, 0.9f));
        CafeKioskUIUtility.Anchor(left, 0f, 0f, 0.66f, 1f, 0f, 0f, -12f, 0f);

        // 카테고리 탭 버튼 바 생성
        var categories = CafeKioskUIUtility.Panel("Categories", left, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(categories, 0f, 0.88f, 1f, 1f, 14f, 8f, -14f, -8f);
        CafeKioskUIUtility.AddHorizontalLayout(categories, 10, TextAnchor.MiddleLeft);

        foreach (var category in viewModel.Categories)
        {
            var captured = category;
            CafeKioskUIUtility.Button(CafeKioskViewModel.CategoryLabel(category), categories, 18, caramel, Color.white, () =>
            {
                viewModel.SelectCategory(captured); // 뷰모델 상태 변경
                RefreshMenu(onAction);               // 해당 카테고리 메뉴로 갱신
            }, font, 116f);
        }

        // 메뉴 아이템 스크롤 영역
        var scroll = CafeKioskUIUtility.ScrollArea("Menu Scroll", left);
        CafeKioskUIUtility.Anchor(scroll.viewport, 0f, 0f, 1f, 0.88f, 14f, 14f, -14f, -8f);
        menuGrid = scroll.content;
        // 3열 고정 그리드 레이아웃 설정
        CafeKioskUIUtility.AddGrid(menuGrid, new Vector2(245f, 250f), new Vector2(14f, 14f), new RectOffset(0, 0, 0, 0));

        // [오른쪽] 주문 내역(장바구니) 영역
        var right = CafeKioskUIUtility.Panel("Order Area", content, paper);
        CafeKioskUIUtility.Anchor(right, 0.66f, 0f, 1f, 1f, 12f, 0f, 0f, 0f);

        var orderTitle = CafeKioskUIUtility.Label("주문 내역", right, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(orderTitle.rectTransform, 0f, 0.88f, 1f, 1f, 22f, 0f, -22f, -12f);

        // 장바구니 리스트 스크롤 영역
        var cartScroll = CafeKioskUIUtility.ScrollArea("Cart Scroll", right);
        CafeKioskUIUtility.Anchor(cartScroll.viewport, 0f, 0.24f, 1f, 0.88f, 18f, 4f, -18f, -8f);
        cartList = cartScroll.content;
        CafeKioskUIUtility.AddVerticalLayout(cartList, 10, TextAnchor.UpperLeft);

        // 빈 장바구니 안내 텍스트
        emptyCartText = CafeKioskUIUtility.Label("아직 담긴 메뉴가 없습니다.", right, 18, new Color(0.5f, 0.45f, 0.39f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(emptyCartText.rectTransform, 0f, 0.45f, 1f, 0.65f, 20f, 0f, -20f, 0f);

        // 합계 금액 표시
        totalText = CafeKioskUIUtility.Label("합계 0원", right, 30, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(totalText.rectTransform, 0f, 0.14f, 1f, 0.24f, 22f, 0f, -22f, 0f);

        // 하단 액션 버튼 영역 (비우기, 결제하기)
        var actions = CafeKioskUIUtility.Panel("Actions", right, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(actions, 0f, 0.02f, 1f, 0.14f, 18f, 0f, -18f, 0f);
        CafeKioskUIUtility.AddHorizontalLayout(actions, 12, TextAnchor.MiddleCenter);
        
        CafeKioskUIUtility.Button("비우기", actions, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, () => {
            viewModel.ClearCart();
            RefreshCart();
            onAction?.Invoke();
        }, font, 118f);
        
        CafeKioskUIUtility.Button("결제하기", actions, 20, sage, Color.white, () => {
            if (viewModel.Checkout()) onCheckout?.Invoke();
            onAction?.Invoke();
        }, font, 178f);

        // 상태 메시지 텍스트 (알림 등)
        statusText = CafeKioskUIUtility.Label("", right, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(statusText.rectTransform, 0f, 0f, 1f, 0.04f, 20f, 0f, -20f, 0f);
    }

    // 선택된 카테고리에 맞는 메뉴 아이템 리스트를 갱신
    public void RefreshMenu(System.Action onStartAddToCart)
    {
        CafeKioskUIUtility.ClearChildren(menuGrid); // 기존 메뉴 오브젝트 삭제
        var visibleItems = viewModel.VisibleMenuItems;
        
        if (visibleItems.Count == 0)
        {
            CafeKioskUIUtility.Label("표시할 메뉴가 없습니다.", menuGrid, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        }

        // 메뉴 데이터를 기반으로 메뉴 카드 생성
        foreach (var item in visibleItems)
        {
            var card = CafeKioskUIUtility.Panel(item.Name, menuGrid, Color.white);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = card.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            Thumbnail(item, card); // 메뉴 이미지
            CafeKioskUIUtility.Label(item.Name, card, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font); // 이름
            CafeKioskUIUtility.Label(item.Description, card, 15, new Color(0.42f, 0.38f, 0.32f), FontStyle.Normal, TextAnchor.MiddleLeft, font); // 설명
            CafeKioskUIUtility.Label(CafeKioskViewModel.MenuPriceText(item), card, 20, caramel, FontStyle.Bold, TextAnchor.MiddleLeft, font); // 가격
            
            // '담기' 버튼 추가
            CafeKioskUIUtility.Button("담기", card, 17, espresso, Color.white, () => {
                if (viewModel.StartAddToCart(item)) RefreshCart(); // 장바구니 데이터 추가 및 UI 갱신
                onStartAddToCart?.Invoke();
            }, font, 0f, 38f);
        }

        // 그리드의 전체 높이를 동적으로 계산하여 스크롤 범위 갱신
        var grid = menuGrid.GetComponent<GridLayoutGroup>();
        var rowHeight = grid.cellSize.y + grid.spacing.y;
        var rows = Mathf.CeilToInt(Mathf.Max(1, visibleItems.Count) / (float)grid.constraintCount);
        menuGrid.sizeDelta = new Vector2(menuGrid.sizeDelta.x, rows * rowHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuGrid);
    }

    // 장바구니에 담긴 아이템 리스트와 합계 금액 갱신
    public void RefreshCart()
    {
        CafeKioskUIUtility.ClearChildren(cartList); // 기존 장바구니 항목 삭제
        emptyCartText.gameObject.SetActive(!viewModel.HasCartItems); // 빈 상태 메시지 표시 여부

        foreach (var pair in viewModel.Cart)
        {
            // 장바구니의 각 항목(아이템 + 수량) 행 생성
            var row = CafeKioskUIUtility.Panel(pair.Item.Name, cartList, new Color(0.98f, 0.94f, 0.88f));
            row.sizeDelta = new Vector2(0f, 78f);

            var nameLabel = CafeKioskUIUtility.Label(pair.DisplayName, row, 16, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(nameLabel.rectTransform, 0f, 0.45f, 0.54f, 1f, 12f, 0f, 0f, 0f);

            var priceLabel = CafeKioskUIUtility.Label(CafeKioskViewModel.FormatPrice(pair.UnitPrice * pair.Quantity), row, 16, caramel, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(priceLabel.rectTransform, 0f, 0f, 0.54f, 0.48f, 12f, 0f, 0f, 0f);

            // 수량 감소 버튼
            CafeKioskUIUtility.Button("-", row, 18, new Color(0.55f, 0.5f, 0.45f), Color.white, () => {
                viewModel.ChangeQuantity(pair, -1);
                RefreshCart();
                RefreshStatus();
            }, font, 42f, 42f, 0.58f);

            var quantityLabel = CafeKioskUIUtility.Label(pair.Quantity.ToString(), row, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
            CafeKioskUIUtility.Anchor(quantityLabel.rectTransform, 0.72f, 0.22f, 0.82f, 0.78f, 0f, 0f, 0f, 0f);
            
            // 수량 증가 버튼
            CafeKioskUIUtility.Button("+", row, 18, sage, Color.white, () => {
                viewModel.ChangeQuantity(pair, 1);
                RefreshCart();
                RefreshStatus();
            }, font, 42f, 42f, 0.85f);
        }

        // 총 합계 금액 갱신
        totalText.text = $"합계 {CafeKioskViewModel.FormatPrice(viewModel.CartTotal)}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(cartList);
    }

    // 하단 상태 메시지 텍스트 갱신
    public void RefreshStatus()
    {
        if (statusText != null) statusText.text = viewModel.StatusText;
    }

    // 화면 활성화/비활성화 및 전체 상태 갱신
    public void Refresh()
    {
        Root.gameObject.SetActive(viewModel.IsOrderScreenVisible);
        RefreshStatus();
    }

    // 메뉴 카드의 썸네일 이미지를 생성하고 설정하는 보조 함수
    private void Thumbnail(MenuItem item, Transform parent)
    {
        var thumbnailObject = new GameObject($"{item.Name} Thumbnail", typeof(RectTransform), typeof(Image));
        thumbnailObject.transform.SetParent(parent, false);
        var rect = thumbnailObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 74f);

        // 레이아웃 그룹 내에서 크기 고정을 위해 LayoutElement 추가
        var layout = thumbnailObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 74f;
        layout.minHeight = 74f;

        // 팩토리를 통해 스프라이트를 가져와 이미지 컴포넌트에 할당
        var image = thumbnailObject.GetComponent<Image>();
        image.sprite = CafeKioskThumbnailFactory.Get(item);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }
}
