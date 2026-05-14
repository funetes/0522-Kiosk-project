using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public sealed class CafeKioskViewModel
{
    private readonly List<MenuItem> menuItems;
    private readonly List<CartLine> cart = new();
    private readonly CafeKioskMembershipService membershipService = new();
    private MenuItem pendingOptionItem;
    private int orderNumber = 100;

    // --- 관리자 정산 데이터 ---
    private readonly List<OrderRecord> orderHistory = new();
    public IReadOnlyList<OrderRecord> OrderHistory => orderHistory;
    public int TotalOriginalSales => orderHistory.Sum(record => record.OriginalAmount);
    public int TotalDiscountAmount => orderHistory.Sum(record => record.DiscountAmount);
    public int TotalActualSales => orderHistory.Sum(record => record.TotalAmount);
    public int TotalOrderCount => orderHistory.Count;

    public CafeKioskViewModel()
    {
        menuItems = CafeKioskMenuCatalog.CreateMenu();
        Categories = new[] { "All", "Coffee", "Ade", "Dessert", "Food" };
        SelectedCategory = "All";
        SelectedTemperature = "ICE";
        SelectedSize = "Regular";
        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
    }

    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<CartLine> Cart => cart;
    public string SelectedCategory { get; private set; }
    public string SelectedTemperature { get; private set; }
    public string SelectedSize { get; private set; }
    public string OrderMode { get; private set; } = "";
    public string StatusText { get; private set; } = "";
    public string MemberStatusText { get; private set; }
    public string TicketText { get; private set; } = "";
    public int PaymentAmount { get; private set; }
    public int CurrentDiscount { get; private set; } = 0;
    public bool IsUsingCoupon { get; private set; } = false;

    public int CartTotal => cart.Sum(line => line.UnitPrice * line.Quantity);
    public bool HasCartItems => cart.Count > 0;
    public bool IsStartScreenVisible { get; private set; } = true;
    public bool IsOrderScreenVisible => !IsStartScreenVisible;
    public bool IsOptionOverlayVisible { get; private set; }
    public bool IsPaymentOverlayVisible { get; private set; }

    public IReadOnlyList<MenuItem> VisibleMenuItems => menuItems.Where(item => SelectedCategory == "All" || item.Category == SelectedCategory).ToList();

    // --- 기본 로직 ---
    public void SelectCategory(string category) => SelectedCategory = category;
    public void SelectOrderMode(string mode) { OrderMode = mode; IsStartScreenVisible = false; StatusText = $"{OrderMode} 주문을 시작합니다."; }
    public void SelectTemperature(string temp) => SelectedTemperature = temp;
    public void SelectSize(string size) => SelectedSize = size;

    public bool StartAddToCart(MenuItem item)
    {
        if (IsDrink(item))
        {
            pendingOptionItem = item;
            SelectedTemperature = item.Category == "Coffee" ? "ICE" : "";
            SelectedSize = "Regular";
            IsOptionOverlayVisible = true;
            StatusText = $"{item.Name} 옵션을 선택해주세요.";
            return false;
        }
        AddToCart(item, "", "", item.Price);
        return true;
    }

    public bool ConfirmDrinkOption()
    {
        if (pendingOptionItem == null) return false;
        var unitPrice = pendingOptionItem.Price + (SelectedSize == "Regular" ? 500 : SelectedSize == "Large" ? 1000 : 0);
        AddToCart(pendingOptionItem, SelectedTemperature, SelectedSize, unitPrice);
        pendingOptionItem = null;
        IsOptionOverlayVisible = false;
        return true;
    }

    public void CancelDrinkOption() { pendingOptionItem = null; IsOptionOverlayVisible = false; }
    public void ChangeQuantity(CartLine line, int delta) { line.Quantity += delta; if (line.Quantity <= 0) cart.Remove(line); }
    public void ClearCart() => cart.Clear();

    // --- 결제 및 쿠폰 로직 ---
    public bool Checkout()
    {
        if (cart.Count == 0) return false;
        CurrentDiscount = 0;
        IsUsingCoupon = false;
        PaymentAmount = CartTotal;

        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
        TicketText = "";

        IsPaymentOverlayVisible = true;
        StatusText = "결제 방식을 선택해주세요.";
        return true;
    }

    public void ApplyStampDiscount(string phone)
    {
        if (CurrentDiscount > 0) return;
        if (membershipService.HasCoupon(phone))
        {
            IsUsingCoupon = true;
            CurrentDiscount = 2000;
            PaymentAmount = System.Math.Max(0, CartTotal - CurrentDiscount);
            StatusText = "쿠폰 사용! 2,000원이 할인되었습니다.";
        }
        else StatusText = "사용 가능한 쿠폰이 없습니다.";
    }

    public void CompletePayment(string method, string memberPhone)
    {
        if (IsUsingCoupon) membershipService.UseCoupon(memberPhone);

        var record = new OrderRecord
        {
            OrderNumber = ++orderNumber,
            OrderMode = this.OrderMode,
            PaymentMethod = method,
            OriginalAmount = CartTotal,
            DiscountAmount = CurrentDiscount,
            TotalAmount = PaymentAmount,
            OrderTime = System.DateTime.Now,
            PurchasedItems = new List<CartLine>(cart)
        };
        orderHistory.Add(record);

        ApplyMembership(memberPhone, cart.Sum(l => l.Quantity));
        cart.Clear();
        CurrentDiscount = 0;
        IsUsingCoupon = false;
        TicketText = $"번호표 {record.OrderNumber}번";
        IsPaymentOverlayVisible = false;
        StatusText = "결제가 완료되었습니다.";
    }

    public void CancelPayment() { IsPaymentOverlayVisible = false; CurrentDiscount = 0; IsUsingCoupon = false; StatusText = "결제를 취소했습니다."; }
    public void RegisterOrLookupMember(string phone) => MemberStatusText = membershipService.RegisterOrLookup(phone).StatusText;

    // --- 관리자 보안 로직 ---
    public bool CheckAdminPassword(string input)
    {
        return input.Length == 4 && input.All(char.IsDigit);
    }

    public static string CategoryLabel(string category) => category switch { "All" => "전체", "Coffee" => "커피", "Ade" => "에이드", "Dessert" => "디저트", "Food" => "푸드", _ => category };
    public static string MenuPriceText(MenuItem item) => IsDrink(item) ? $"{FormatPrice(item.Price)}부터" : FormatPrice(item.Price);
    private static bool IsDrink(MenuItem item) => item.Category == "Coffee" || item.Category == "Ade";
    public static string FormatPrice(int price) => price.ToString("N0", CultureInfo.InvariantCulture) + "원";
    private void AddToCart(MenuItem item, string temp, string size, int price)
    {
        var line = cart.FirstOrDefault(e => e.Item == item && e.Temperature == temp && e.Size == size);
        if (line == null) cart.Add(new CartLine(item, temp, size, price) { Quantity = 1 });
        else line.Quantity++;
    }
    private void ApplyMembership(string phone, int count) => MemberStatusText = membershipService.ApplyPurchase(phone, count).StatusText;
}