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
    public int CartTotal => cart.Sum(line => line.UnitPrice * line.Quantity);
    public bool HasCartItems => cart.Count > 0;
    public bool IsStartScreenVisible { get; private set; } = true;
    public bool IsOrderScreenVisible => !IsStartScreenVisible;
    public bool IsOptionOverlayVisible { get; private set; }
    public bool IsPaymentOverlayVisible { get; private set; }

    public IReadOnlyList<MenuItem> VisibleMenuItems =>
        menuItems.Where(item => SelectedCategory == "All" || item.Category == SelectedCategory).ToList();

    public void SelectCategory(string category)
    {
        SelectedCategory = category;
    }

    public void SelectOrderMode(string mode)
    {
        OrderMode = mode;
        IsStartScreenVisible = false;
        StatusText = $"{OrderMode} 주문을 시작합니다.";
    }

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

    public void SelectTemperature(string temperature)
    {
        SelectedTemperature = temperature;
        StatusText = $"{temperature} 선택";
    }

    public void SelectSize(string size)
    {
        SelectedSize = size;
        StatusText = $"{SizeLabel(size)} 선택";
    }

    public bool ConfirmDrinkOption()
    {
        if (pendingOptionItem == null)
        {
            IsOptionOverlayVisible = false;
            return false;
        }

        var temperature = pendingOptionItem.Category == "Coffee" ? SelectedTemperature : "ICE";
        var unitPrice = pendingOptionItem.Price + SizeExtraPrice(SelectedSize);
        AddToCart(pendingOptionItem, temperature, SelectedSize, unitPrice);
        pendingOptionItem = null;
        IsOptionOverlayVisible = false;
        return true;
    }

    public void CancelDrinkOption()
    {
        pendingOptionItem = null;
        IsOptionOverlayVisible = false;
        StatusText = "옵션 선택을 취소했습니다.";
    }

    public void ChangeQuantity(CartLine line, int delta)
    {
        line.Quantity += delta;
        if (line.Quantity <= 0)
        {
            cart.Remove(line);
        }

        StatusText = "";
    }

    public void ClearCart()
    {
        cart.Clear();
        StatusText = "주문을 비웠습니다.";
    }

    public bool Checkout()
    {
        if (cart.Count == 0)
        {
            StatusText = "메뉴를 먼저 담아주세요.";
            return false;
        }

        PaymentAmount = CartTotal;
        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
        TicketText = "";
        IsPaymentOverlayVisible = true;
        StatusText = "결제 방식을 선택해주세요.";
        return true;
    }

    public void CompletePayment(string method, string memberPhone)
    {
        var purchasedCount = cart.Sum(line => line.Quantity);
        var membershipMessage = ApplyMembership(memberPhone, purchasedCount);
        var ticketNumber = ++orderNumber;
        cart.Clear();
        TicketText = $"번호표 {ticketNumber}번";
        IsPaymentOverlayVisible = false;
        StatusText = $"{OrderMode} · {method} 결제 완료 · 번호표 {ticketNumber}번 · {FormatPrice(PaymentAmount)} {membershipMessage}";
    }

    public void CancelPayment()
    {
        IsPaymentOverlayVisible = false;
        StatusText = "결제를 취소했습니다.";
    }

    public void RegisterOrLookupMember(string phone)
    {
        MemberStatusText = membershipService.RegisterOrLookup(phone).StatusText;
    }

    public static string CategoryLabel(string category)
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

    public static string MenuPriceText(MenuItem item)
    {
        return IsDrink(item) ? $"{FormatPrice(item.Price)}부터" : FormatPrice(item.Price);
    }

    public static string FormatPrice(int price)
    {
        return price.ToString("N0", CultureInfo.InvariantCulture) + "원";
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
        StatusText = $"{line.DisplayName} 추가";
    }

    private string ApplyMembership(string phone, int purchasedCount)
    {
        var result = membershipService.ApplyPurchase(phone, purchasedCount);
        MemberStatusText = result.StatusText;
        return result.SummaryText;
    }

    private static bool IsDrink(MenuItem item)
    {
        return item.Category == "Coffee" || item.Category == "Ade";
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
}
