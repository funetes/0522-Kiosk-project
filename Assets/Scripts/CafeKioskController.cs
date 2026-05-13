using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[ExecuteAlways]
public sealed class CafeKioskController : MonoBehaviour
{
    private readonly List<MenuItem> menuItems = CafeKioskMenuCatalog.CreateMenu();
    private readonly List<CartLine> cart = new();
    private readonly CafeKioskMembershipService membershipService = new();
    private readonly Color cream = new(0.96f, 0.92f, 0.86f);
    private readonly Color charcoal = new(0.12f, 0.11f, 0.1f);
    private readonly Color espresso = new(0.28f, 0.17f, 0.1f);
    private readonly Color caramel = new(0.77f, 0.45f, 0.22f);
    private readonly Color sage = new(0.38f, 0.5f, 0.42f);
    private readonly Color paper = new(1f, 0.98f, 0.94f);

    private Font font;
    private RectTransform menuGrid;
    private RectTransform cartList;
    private Text totalText;
    private Text emptyCartText;
    private Text statusText;
    private RectTransform orderScreen;
    private RectTransform startScreen;
    private RectTransform paymentOverlay;
    private RectTransform optionOverlay;
    private Text paymentTotalText;
    private Text memberStatusText;
    private Text ticketText;
    private InputField memberPhoneInput;
    private string selectedCategory = "All";
    private MenuItem pendingOptionItem;
    private string selectedTemperature = "ICE";
    private string selectedSize = "Regular";
    private string orderMode = "";
    private int paymentAmount;
    private int orderNumber = 100;

    private void OnEnable()
    {
        RebuildInterface();
    }

    private void RebuildInterface()
    {
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 18);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        RemoveGeneratedChildren();
        EnsureEventSystem();
        BuildInterface();
        RefreshMenu();
        RefreshCart();
    }

    private void Update()
    {
        EnsureEventSystem();
    }

    private void BuildInterface()
    {
        var canvas = CreateCanvas();
        var root = Panel("Kiosk Root", canvas.transform, cream);
        Stretch(root);

        orderScreen = Panel("Order Screen", root, cream);
        Stretch(orderScreen);

        var header = Panel("Header", orderScreen, espresso);
        Anchor(header, 0f, 0.85f, 1f, 1f, 28f, 18f, -28f, -18f);

        var title = Label("Megazone Cafe", header, 42, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(title.rectTransform, 0f, 0f, 0.5f, 1f, 26f, 0f, 0f, 0f);

        var subtitle = Label("주문할 메뉴를 선택하세요", header, 20, new Color(1f, 0.88f, 0.7f), FontStyle.Normal, TextAnchor.MiddleRight);
        Anchor(subtitle.rectTransform, 0.5f, 0f, 1f, 1f, 0f, 0f, -26f, 0f);

        var content = Panel("Content", orderScreen, cream);
        Anchor(content, 0f, 0f, 1f, 0.85f, 28f, 24f, -28f, -12f);

        var left = Panel("Menu Area", content, new Color(0.98f, 0.95f, 0.9f));
        Anchor(left, 0f, 0f, 0.66f, 1f, 0f, 0f, -12f, 0f);

        var categories = Panel("Categories", left, new Color(0f, 0f, 0f, 0f));
        Anchor(categories, 0f, 0.88f, 1f, 1f, 14f, 8f, -14f, -8f);
        AddHorizontalLayout(categories, 10, TextAnchor.MiddleLeft);

        foreach (var category in new[] { "All", "Coffee", "Ade", "Dessert", "Food" })
        {
            var captured = category;
            Button(CategoryLabel(category), categories, 18, caramel, Color.white, () =>
            {
                selectedCategory = captured;
                RefreshMenu();
            }, 116f);
        }

        var scroll = ScrollArea("Menu Scroll", left);
        Anchor(scroll.viewport, 0f, 0f, 1f, 0.88f, 14f, 14f, -14f, -8f);
        menuGrid = scroll.content;
        AddGrid(menuGrid, new Vector2(245f, 208f), new Vector2(14f, 14f), new RectOffset(0, 0, 0, 0));

        var right = Panel("Order Area", content, paper);
        Anchor(right, 0.66f, 0f, 1f, 1f, 12f, 0f, 0f, 0f);

        var orderTitle = Label("주문 내역", right, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(orderTitle.rectTransform, 0f, 0.88f, 1f, 1f, 22f, 0f, -22f, -12f);

        var cartScroll = ScrollArea("Cart Scroll", right);
        Anchor(cartScroll.viewport, 0f, 0.24f, 1f, 0.88f, 18f, 4f, -18f, -8f);
        cartList = cartScroll.content;
        AddVerticalLayout(cartList, 10, TextAnchor.UpperLeft);

        emptyCartText = Label("아직 담긴 메뉴가 없습니다.", right, 18, new Color(0.5f, 0.45f, 0.39f), FontStyle.Normal, TextAnchor.MiddleCenter);
        Anchor(emptyCartText.rectTransform, 0f, 0.45f, 1f, 0.65f, 20f, 0f, -20f, 0f);

        totalText = Label("합계 0원", right, 30, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(totalText.rectTransform, 0f, 0.14f, 1f, 0.24f, 22f, 0f, -22f, 0f);

        var actions = Panel("Actions", right, new Color(0f, 0f, 0f, 0f));
        Anchor(actions, 0f, 0.02f, 1f, 0.14f, 18f, 0f, -18f, 0f);
        AddHorizontalLayout(actions, 12, TextAnchor.MiddleCenter);
        Button("비우기", actions, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, ClearCart, 118f);
        Button("결제하기", actions, 20, sage, Color.white, Checkout, 178f);

        statusText = Label("", right, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(statusText.rectTransform, 0f, 0f, 1f, 0.04f, 20f, 0f, -20f, 0f);

        CreateOptionOverlay(root);
        CreatePaymentOverlay(root);
        CreateStartScreen(root);
        orderScreen.gameObject.SetActive(false);
    }

    private void RefreshMenu()
    {
        ClearChildren(menuGrid);
        var visibleItems = menuItems.Where(item => selectedCategory == "All" || item.Category == selectedCategory).ToList();
        if (visibleItems.Count == 0)
        {
            Label("표시할 메뉴가 없습니다.", menuGrid, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        }

        foreach (var item in visibleItems)
        {
            var card = Panel(item.Name, menuGrid, Color.white);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Thumbnail(item, card);
            Label(item.Name, card, 22, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
            Label(item.Description, card, 15, new Color(0.42f, 0.38f, 0.32f), FontStyle.Normal, TextAnchor.MiddleLeft);
            Label(MenuPriceText(item), card, 20, caramel, FontStyle.Bold, TextAnchor.MiddleLeft);
            Button("담기", card, 17, espresso, Color.white, () => StartAddToCart(item), 0f, 38f);
        }

        var rows = Mathf.CeilToInt(Mathf.Max(1, visibleItems.Count) / 3f);
        menuGrid.sizeDelta = new Vector2(menuGrid.sizeDelta.x, Mathf.Max(1, rows) * 222f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(menuGrid);
    }

    private void RefreshCart()
    {
        ClearChildren(cartList);
        var hasItems = cart.Count > 0;
        emptyCartText.gameObject.SetActive(!hasItems);

        foreach (var pair in cart.ToList())
        {
            var row = Panel(pair.Item.Name, cartList, new Color(0.98f, 0.94f, 0.88f));
            row.sizeDelta = new Vector2(0f, 78f);

            var name = Label(pair.DisplayName, row, 16, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
            Anchor(name.rectTransform, 0f, 0.45f, 0.54f, 1f, 12f, 0f, 0f, 0f);

            var price = Label(FormatPrice(pair.UnitPrice * pair.Quantity), row, 16, caramel, FontStyle.Bold, TextAnchor.MiddleLeft);
            Anchor(price.rectTransform, 0f, 0f, 0.54f, 0.48f, 12f, 0f, 0f, 0f);

            Button("-", row, 18, new Color(0.55f, 0.5f, 0.45f), Color.white, () => ChangeQuantity(pair, -1), 42f, 42f, 0.58f);
            var quantity = Label(pair.Quantity.ToString(CultureInfo.InvariantCulture), row, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
            Anchor(quantity.rectTransform, 0.72f, 0.22f, 0.82f, 0.78f, 0f, 0f, 0f, 0f);
            Button("+", row, 18, sage, Color.white, () => ChangeQuantity(pair, 1), 42f, 42f, 0.85f);
        }

        totalText.text = $"합계 {FormatPrice(cart.Sum(pair => pair.UnitPrice * pair.Quantity))}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(cartList);
    }

    private void StartAddToCart(MenuItem item)
    {
        if (IsDrink(item))
        {
            pendingOptionItem = item;
            selectedTemperature = item.Category == "Coffee" ? "ICE" : "";
            selectedSize = "Regular";
            optionOverlay.gameObject.SetActive(true);
            statusText.text = $"{item.Name} 옵션을 선택해주세요.";
            return;
        }

        AddToCart(item, "", "", item.Price);
    }

    private void AddToCart(MenuItem item, string temperature, string size, int unitPrice)
    {
        var line = cart.FirstOrDefault(entry => entry.Item == item && entry.Temperature == temperature && entry.Size == size && entry.UnitPrice == unitPrice);
        if (line == null)
        {
            line = new CartLine(item, temperature, size, unitPrice);
            cart.Add(line);
        }

        line.Quantity++;
        statusText.text = $"{line.DisplayName} 추가";
        RefreshCart();
    }

    private void ChangeQuantity(CartLine line, int delta)
    {
        line.Quantity += delta;
        if (line.Quantity <= 0)
        {
            cart.Remove(line);
        }

        statusText.text = "";
        RefreshCart();
    }

    private void ClearCart()
    {
        cart.Clear();
        statusText.text = "주문을 비웠습니다.";
        RefreshCart();
    }

    private void Checkout()
    {
        if (cart.Count == 0)
        {
            statusText.text = "메뉴를 먼저 담아주세요.";
            return;
        }

        paymentAmount = cart.Sum(pair => pair.UnitPrice * pair.Quantity);
        paymentTotalText.text = $"결제 금액 {FormatPrice(paymentAmount)}";
        memberPhoneInput.text = "";
        memberStatusText.text = "전화번호를 입력하면 스탬프가 적립됩니다.";
        ticketText.text = "";
        paymentOverlay.gameObject.SetActive(true);
        statusText.text = "결제 방식을 선택해주세요.";
    }

    private void CompletePayment(string method)
    {
        var purchasedCount = cart.Sum(pair => pair.Quantity);
        var membershipMessage = ApplyMembership(purchasedCount);
        var ticketNumber = ++orderNumber;
        cart.Clear();
        ticketText.text = $"번호표 {ticketNumber}번";
        paymentOverlay.gameObject.SetActive(false);
        statusText.text = $"{orderMode} · {method} 결제 완료 · 번호표 {ticketNumber}번 · {FormatPrice(paymentAmount)} {membershipMessage}";
        RefreshCart();
    }

    private void CancelPayment()
    {
        paymentOverlay.gameObject.SetActive(false);
        statusText.text = "결제를 취소했습니다.";
    }

    private string ApplyMembership(int purchasedCount)
    {
        var result = membershipService.ApplyPurchase(memberPhoneInput.text, purchasedCount);
        memberStatusText.text = result.StatusText;
        return result.SummaryText;
    }

    private void RegisterOrLookupMember()
    {
        memberStatusText.text = membershipService.RegisterOrLookup(memberPhoneInput.text).StatusText;
    }

    private void CreateOptionOverlay(Transform root)
    {
        optionOverlay = Panel("Drink Option Overlay", root, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        Stretch(optionOverlay);
        optionOverlay.gameObject.SetActive(false);

        var modal = Panel("Drink Option Panel", optionOverlay, paper);
        Anchor(modal, 0.27f, 0.13f, 0.73f, 0.87f, 0f, 0f, 0f, 0f);

        var title = Label("음료 옵션 선택", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, 0f, 0.83f, 1f, 0.96f, 24f, 0f, -24f, 0f);

        var tempTitle = Label("커피 온도", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(tempTitle.rectTransform, 0f, 0.7f, 1f, 0.79f, 34f, 0f, -34f, 0f);

        var temps = Panel("Temperature Options", modal, new Color(0f, 0f, 0f, 0f));
        Anchor(temps, 0f, 0.58f, 1f, 0.7f, 34f, 0f, -34f, 0f);
        AddHorizontalLayout(temps, 12f, TextAnchor.MiddleCenter);
        Button("ICE", temps, 22, sage, Color.white, () => SelectTemperature("ICE"), 150f, 54f);
        Button("HOT", temps, 22, espresso, Color.white, () => SelectTemperature("HOT"), 150f, 54f);

        var sizeTitle = Label("음료 사이즈", modal, 20, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(sizeTitle.rectTransform, 0f, 0.47f, 1f, 0.56f, 34f, 0f, -34f, 0f);

        var sizes = Panel("Size Options", modal, new Color(0f, 0f, 0f, 0f));
        Anchor(sizes, 0f, 0.29f, 1f, 0.47f, 34f, 0f, -34f, 0f);
        AddHorizontalLayout(sizes, 12f, TextAnchor.MiddleCenter);
        Button("Small\n기본", sizes, 17, new Color(0.42f, 0.38f, 0.34f), Color.white, () => SelectSize("Small"), 126f, 74f);
        Button("Regular\n+500원", sizes, 17, caramel, Color.white, () => SelectSize("Regular"), 140f, 74f);
        Button("Large\n+1,000원", sizes, 17, sage, Color.white, () => SelectSize("Large"), 140f, 74f);

        var add = Button("선택 담기", modal, 22, espresso, Color.white, ConfirmDrinkOption, 180f, 54f);
        Anchor(add.GetComponent<RectTransform>(), 0.5f, 0.15f, 0.5f, 0.24f, -90f, 0f, 90f, 0f);

        var cancel = Button("취소", modal, 18, new Color(0.42f, 0.38f, 0.34f), Color.white, CancelDrinkOption, 130f, 44f);
        Anchor(cancel.GetComponent<RectTransform>(), 0.5f, 0.05f, 0.5f, 0.12f, -65f, 0f, 65f, 0f);
    }

    private void SelectTemperature(string temperature)
    {
        selectedTemperature = temperature;
        statusText.text = $"{temperature} 선택";
    }

    private void SelectSize(string size)
    {
        selectedSize = size;
        statusText.text = $"{SizeLabel(size)} 선택";
    }

    private void ConfirmDrinkOption()
    {
        if (pendingOptionItem == null)
        {
            optionOverlay.gameObject.SetActive(false);
            return;
        }

        var temperature = pendingOptionItem.Category == "Coffee" ? selectedTemperature : "ICE";
        var unitPrice = pendingOptionItem.Price + SizeExtraPrice(selectedSize);
        AddToCart(pendingOptionItem, temperature, selectedSize, unitPrice);
        pendingOptionItem = null;
        optionOverlay.gameObject.SetActive(false);
    }

    private void CancelDrinkOption()
    {
        pendingOptionItem = null;
        optionOverlay.gameObject.SetActive(false);
        statusText.text = "옵션 선택을 취소했습니다.";
    }

    private void CreateStartScreen(Transform root)
    {
        startScreen = Panel("Start Order Screen", root, cream);
        Stretch(startScreen);

        var title = Label("Megazone Cafe", startScreen, 56, espresso, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, 0f, 0.68f, 1f, 0.82f, 24f, 0f, -24f, 0f);

        var subtitle = Label("주문 방식을 선택해주세요", startScreen, 28, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(subtitle.rectTransform, 0f, 0.58f, 1f, 0.68f, 24f, 0f, -24f, 0f);

        var choices = Panel("Order Mode Choices", startScreen, new Color(0f, 0f, 0f, 0f));
        Anchor(choices, 0.16f, 0.28f, 0.84f, 0.55f, 0f, 0f, 0f, 0f);
        AddHorizontalLayout(choices, 22f, TextAnchor.MiddleCenter);

        Button("매장", choices, 34, espresso, Color.white, () => SelectOrderMode("매장"), 260f, 150f);
        Button("포장", choices, 34, sage, Color.white, () => SelectOrderMode("포장"), 260f, 150f);
    }

    private void SelectOrderMode(string mode)
    {
        orderMode = mode;
        startScreen.gameObject.SetActive(false);
        orderScreen.gameObject.SetActive(true);
        statusText.text = $"{orderMode} 주문을 시작합니다.";
    }

    private void CreatePaymentOverlay(Transform root)
    {
        paymentOverlay = Panel("Payment Method Overlay", root, new Color(0.05f, 0.04f, 0.03f, 0.72f));
        Stretch(paymentOverlay);
        paymentOverlay.gameObject.SetActive(false);

        var modal = Panel("Payment Method Panel", paymentOverlay, paper);
        Anchor(modal, 0.28f, 0.18f, 0.72f, 0.82f, 0f, 0f, 0f, 0f);

        var title = Label("결제 방식을 선택하세요", modal, 32, charcoal, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, 0f, 0.78f, 1f, 0.95f, 24f, 0f, -24f, 0f);

        paymentTotalText = Label("결제 금액 0원", modal, 24, caramel, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(paymentTotalText.rectTransform, 0f, 0.66f, 1f, 0.78f, 24f, 0f, -24f, 0f);

        var memberTitle = Label("멤버십 전화번호", modal, 18, charcoal, FontStyle.Bold, TextAnchor.MiddleLeft);
        Anchor(memberTitle.rectTransform, 0f, 0.58f, 1f, 0.66f, 34f, 0f, -34f, 0f);

        memberPhoneInput = Input("01012345678", modal);
        Anchor(memberPhoneInput.GetComponent<RectTransform>(), 0f, 0.49f, 0.72f, 0.58f, 34f, 0f, -8f, 0f);

        var join = Button("회원가입/조회", modal, 17, sage, Color.white, RegisterOrLookupMember, 146f, 48f);
        Anchor(join.GetComponent<RectTransform>(), 0.72f, 0.49f, 1f, 0.58f, 8f, 0f, -34f, 0f);

        memberStatusText = Label("전화번호를 입력하면 스탬프가 적립됩니다.", modal, 15, new Color(0.46f, 0.42f, 0.36f), FontStyle.Normal, TextAnchor.MiddleCenter);
        Anchor(memberStatusText.rectTransform, 0f, 0.43f, 1f, 0.49f, 34f, 0f, -34f, 0f);

        var methods = Panel("Payment Methods", modal, new Color(0f, 0f, 0f, 0f));
        Anchor(methods, 0f, 0.25f, 1f, 0.42f, 28f, 0f, -28f, 0f);
        AddHorizontalLayout(methods, 14f, TextAnchor.MiddleCenter);

        Button("카드", methods, 22, espresso, Color.white, () => CompletePayment("카드"), 130f, 70f);
        Button("현금", methods, 22, sage, Color.white, () => CompletePayment("현금"), 130f, 70f);
        Button("모바일페이", methods, 20, caramel, Color.white, () => CompletePayment("모바일페이"), 170f, 70f);

        ticketText = Label("", modal, 17, sage, FontStyle.Bold, TextAnchor.MiddleCenter);
        Anchor(ticketText.rectTransform, 0f, 0.16f, 1f, 0.24f, 24f, 0f, -24f, 0f);

        var back = Button("돌아가기", modal, 19, new Color(0.42f, 0.38f, 0.34f), Color.white, CancelPayment, 150f, 46f);
        Anchor(back.GetComponent<RectTransform>(), 0.5f, 0.06f, 0.5f, 0.14f, -75f, 0f, 75f, 0f);
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
        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        }

        var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            DestroyComponent(legacyModule);
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private RectTransform Panel(string name, Transform parent, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        var image = panel.GetComponent<Image>();
        image.color = color;
        return rect;
    }

    private Text Label(string text, Transform parent, int size, Color color, FontStyle style, TextAnchor alignment)
    {
        var label = new GameObject($"{text} Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(parent, false);
        var rect = label.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, size + 12f);

        var uiText = label.GetComponent<Text>();
        uiText.text = text;
        uiText.font = font;
        uiText.fontSize = size;
        uiText.fontStyle = style;
        uiText.color = color;
        uiText.alignment = alignment;
        uiText.supportRichText = false;
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        return uiText;
    }

    private Button Button(string text, Transform parent, int size, Color background, Color foreground, UnityEngine.Events.UnityAction action, float width = 0f, float height = 46f, float anchorX = -1f)
    {
        var buttonRect = Panel($"{text} Button", parent, background);
        buttonRect.sizeDelta = new Vector2(width, height);

        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonRect.GetComponent<Image>();
        button.onClick.AddListener(action);

        var label = Label(text, buttonRect, size, foreground, FontStyle.Bold, TextAnchor.MiddleCenter);
        Stretch(label.rectTransform);

        if (anchorX >= 0f)
        {
            Anchor(buttonRect, anchorX, 0.22f, anchorX, 0.78f, -width * 0.5f, 0f, width * 0.5f, 0f);
        }

        return button;
    }

    private InputField Input(string placeholder, Transform parent)
    {
        var inputRect = Panel("Phone Input", parent, Color.white);
        inputRect.sizeDelta = new Vector2(0f, 48f);

        var input = inputRect.gameObject.AddComponent<InputField>();
        input.contentType = InputField.ContentType.IntegerNumber;
        input.characterLimit = 13;
        input.targetGraphic = inputRect.GetComponent<Image>();

        var text = Label("", inputRect, 21, charcoal, FontStyle.Normal, TextAnchor.MiddleLeft);
        Anchor(text.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        var placeholderText = Label(placeholder, inputRect, 19, new Color(0.62f, 0.58f, 0.52f), FontStyle.Normal, TextAnchor.MiddleLeft);
        Anchor(placeholderText.rectTransform, 0f, 0f, 1f, 1f, 14f, 0f, -14f, 0f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        return input;
    }

    private (RectTransform viewport, RectTransform content) ScrollArea(string name, Transform parent)
    {
        var viewportObject = new GameObject($"{name} Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObject.transform.SetParent(parent, false);
        var viewport = viewportObject.GetComponent<RectTransform>();

        var contentObject = new GameObject($"{name} Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewport, false);
        var content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        return (viewport, content);
    }

    private static void AddHorizontalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        layout.childAlignment = alignment;
    }

    private static void AddVerticalLayout(RectTransform rect, float spacing, TextAnchor alignment)
    {
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = alignment;

        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void AddGrid(RectTransform rect, Vector2 cellSize, Vector2 spacing, RectOffset padding)
    {
        var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.padding = padding;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }

    private static void Stretch(RectTransform rect)
    {
        Anchor(rect, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);
    }

    private static void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyGeneratedObject(parent.GetChild(i).gameObject);
        }
    }

    private void RemoveGeneratedChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "Cafe Kiosk Canvas")
            {
                DestroyGeneratedObject(child.gameObject);
            }
        }
    }

    private static void DestroyGeneratedObject(Object target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private static void DestroyComponent(Component target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private static string CategoryLabel(string category)
    {
        return category switch
        {
            "All" => "전체",
            "Coffee" => "커피",
            "Ade" => "에이드",
            "Dessert" => "디저트",
            "Food" => "푸드",
            _ => category,
        };
    }

    private static bool IsDrink(MenuItem item)
    {
        return item.Category == "Coffee" || item.Category == "Ade";
    }

    private static string MenuPriceText(MenuItem item)
    {
        return IsDrink(item) ? $"{FormatPrice(item.Price)}부터" : FormatPrice(item.Price);
    }

    private static int SizeExtraPrice(string size)
    {
        return size switch
        {
            "Small" => 0,
            "Regular" => 500,
            "Large" => 1000,
            _ => 0,
        };
    }

    private static string SizeLabel(string size)
    {
        return size switch
        {
            "Small" => "Small",
            "Regular" => "Regular +500원",
            "Large" => "Large +1,000원",
            _ => size,
        };
    }

    private static string FormatPrice(int price)
    {
        return price.ToString("N0", CultureInfo.InvariantCulture) + "원";
    }

}
