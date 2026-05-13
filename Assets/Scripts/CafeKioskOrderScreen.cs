using UnityEngine;
using UnityEngine.UI;

public sealed class CafeKioskOrderScreen
{
    public RectTransform Root { get; private set; }
    private readonly CafeKioskViewModel viewModel;
    private readonly Font font;
    private readonly Color cream;
    private readonly Color espresso;
    private readonly Color charcoal;
    private readonly Color caramel;
    private readonly Color sage;
    private readonly Color paper;

    private RectTransform menuGrid;
    private RectTransform cartList;
    private Text totalText;
    private Text emptyCartText;
    private Text statusText;

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

    private void Build(Transform parent, System.Action onRefreshMenu, System.Action onRefreshCart, System.Action onCheckout, System.Action onAction)
    {
        Root = CafeKioskUIUtility.Panel("Order Screen", parent, cream);
        CafeKioskUIUtility.Stretch(Root);

        var header = CafeKioskUIUtility.Panel("Header", Root, espresso);
        CafeKioskUIUtility.Anchor(header, 0f, 0.85f, 1f, 1f, 28f, 18f, -28f, -18f);

        var title = CafeKioskUIUtility.Label("Megazone Cafe", header, 42, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(title.rectTransform, 0f, 0f, 0.5f, 1f, 26f, 0f, 0f, 0f);

        var subtitle = CafeKioskUIUtility.Label("주문할 메뉴를 선택하세요", header, 20, new Color(1f, 0.88f, 0.7f), FontStyle.Normal, TextAnchor.MiddleRight, font);
        CafeKioskUIUtility.Anchor(subtitle.rectTransform, 0.5f, 0f, 1f, 1f, 0f, 0f, -26f, 0f);

        var content = CafeKioskUIUtility.Panel("Content", Root, cream);
        CafeKioskUIUtility.Anchor(content, 0f, 0f, 1f, 0.85f, 28f, 24f, -28f, -12f);

        // Left Menu Area
        var left = CafeKioskUIUtility.Panel("Menu Area", content, new Color(0.98f, 0.95f, 0.9f));
        CafeKioskUIUtility.Anchor(left, 0f, 0f, 0.66f, 1f, 0f, 0f, -12f, 0f);

        var categories = CafeKioskUIUtility.Panel("Categories", left, new Color(0f, 0f, 0f, 0f));
        CafeKioskUIUtility.Anchor(categories, 0f, 0.88f, 1f, 1f, 14f, 8f, -14f, -8f);
        CafeKioskUIUtility.AddHorizontalLayout(categories, 10, TextAnchor.MiddleLeft);

        foreach (var category in viewModel.Categories)
        {
            var captured = category;
            CafeKioskUIUtility.Button(CafeKioskViewModel.CategoryLabel(category), categories, 18, caramel, Color.white, () =>
            {
                viewModel.SelectCategory(captured);
                RefreshMenu(onAction);
            }, font, 116f);
        }

        var scroll = CafeKioskUIUtility.ScrollArea("Menu Scroll", left);
        CafeKioskUIUtility.Anchor(scroll.viewport, 0f, 0f, 1f, 0.88f, 14f, 14f, -14f, -8f);
        menuGrid = scroll.content;
        CafeKioskUIUtility.AddGrid(menuGrid, new Vector2(245f, 208f), new Vector2(14f, 14f), new RectOffset(0, 0, 0, 0));

        // Right Order Area
        var right = CafeKioskUIUtility.Panel("Order Area", content, paper);
        CafeKioskUIUtility.Anchor(right, 0.66f, 0f, 1f, 1f, 12f, 0f, 0f, 0f);

        var orderTitle = CafeKioskUIUtility.Label("주문 내역", right, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(orderTitle.rectTransform, 0f, 0.88f, 1f, 1f, 22f, 0f, -22f, -12f);

        var cartScroll = CafeKioskUIUtility.ScrollArea("Cart Scroll", right);
        CafeKioskUIUtility.Anchor(cartScroll.viewport, 0f, 0.24f, 1f, 0.88f, 18f, 4f, -18f, -8f);
        cartList = cartScroll.content;
        CafeKioskUIUtility.AddVerticalLayout(cartList, 10, TextAnchor.UpperLeft);

        emptyCartText = CafeKioskUIUtility.Label("아직 담긴 메뉴가 없습니다.", right, 18, new Color(0.5f, 0.45f, 0.39f), FontStyle.Normal, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(emptyCartText.rectTransform, 0f, 0.45f, 1f, 0.65f, 20f, 0f, -20f, 0f);

        totalText = CafeKioskUIUtility.Label("합계 0원", right, 30, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
        CafeKioskUIUtility.Anchor(totalText.rectTransform, 0f, 0.14f, 1f, 0.24f, 22f, 0f, -22f, 0f);

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

        statusText = CafeKioskUIUtility.Label("", right, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        CafeKioskUIUtility.Anchor(statusText.rectTransform, 0f, 0f, 1f, 0.04f, 20f, 0f, -20f, 0f);
    }

    public void RefreshMenu(System.Action onStartAddToCart)
    {
        CafeKioskUIUtility.ClearChildren(menuGrid);
        var visibleItems = viewModel.VisibleMenuItems;
        if (visibleItems.Count == 0)
        {
            CafeKioskUIUtility.Label("표시할 메뉴가 없습니다.", menuGrid, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
        }

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

            Thumbnail(item, card);
            CafeKioskUIUtility.Label(item.Name, card, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Label(item.Description, card, 15, new Color(0.42f, 0.38f, 0.32f), FontStyle.Normal, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Label(CafeKioskViewModel.MenuPriceText(item), card, 20, caramel, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Button("담기", card, 17, espresso, Color.white, () => {
                if (viewModel.StartAddToCart(item)) RefreshCart();
                onStartAddToCart?.Invoke();
            }, font, 0f, 38f);
        }

        var rows = Mathf.CeilToInt(Mathf.Max(1, visibleItems.Count) / 3f);
        menuGrid.sizeDelta = new Vector2(menuGrid.sizeDelta.x, Mathf.Max(1, rows) * 222f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuGrid);
    }

    public void RefreshCart()
    {
        CafeKioskUIUtility.ClearChildren(cartList);
        emptyCartText.gameObject.SetActive(!viewModel.HasCartItems);

        foreach (var pair in viewModel.Cart)
        {
            var row = CafeKioskUIUtility.Panel(pair.Item.Name, cartList, new Color(0.98f, 0.94f, 0.88f));
            row.sizeDelta = new Vector2(0f, 78f);

            var nameLabel = CafeKioskUIUtility.Label(pair.DisplayName, row, 16, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(nameLabel.rectTransform, 0f, 0.45f, 0.54f, 1f, 12f, 0f, 0f, 0f);

            var priceLabel = CafeKioskUIUtility.Label(CafeKioskViewModel.FormatPrice(pair.UnitPrice * pair.Quantity), row, 16, caramel, FontStyle.Bold, TextAnchor.MiddleLeft, font);
            CafeKioskUIUtility.Anchor(priceLabel.rectTransform, 0f, 0f, 0.54f, 0.48f, 12f, 0f, 0f, 0f);

            CafeKioskUIUtility.Button("-", row, 18, new Color(0.55f, 0.5f, 0.45f), Color.white, () => {
                viewModel.ChangeQuantity(pair, -1);
                RefreshCart();
                RefreshStatus();
            }, font, 42f, 42f, 0.58f);

            var quantityLabel = CafeKioskUIUtility.Label(pair.Quantity.ToString(), row, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter, font);
            CafeKioskUIUtility.Anchor(quantityLabel.rectTransform, 0.72f, 0.22f, 0.82f, 0.78f, 0f, 0f, 0f, 0f);
            
            CafeKioskUIUtility.Button("+", row, 18, sage, Color.white, () => {
                viewModel.ChangeQuantity(pair, 1);
                RefreshCart();
                RefreshStatus();
            }, font, 42f, 42f, 0.85f);
        }

        totalText.text = $"합계 {CafeKioskViewModel.FormatPrice(viewModel.CartTotal)}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(cartList);
    }

    public void RefreshStatus()
    {
        if (statusText != null) statusText.text = viewModel.StatusText;
    }

    public void Refresh()
    {
        Root.gameObject.SetActive(viewModel.IsOrderScreenVisible);
        RefreshStatus();
    }

    private void Thumbnail(MenuItem item, Transform parent)
    {
        var thumbnailObject = new GameObject($"{item.Name} Thumbnail", typeof(RectTransform), typeof(Image));
        thumbnailObject.transform.SetParent(parent, false);
        var rect = thumbnailObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 74f);

        var layout = thumbnailObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 74f;
        layout.minHeight = 74f;

        var image = thumbnailObject.GetComponent<Image>();
        image.sprite = CafeKioskThumbnailFactory.Get(item);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }
}
