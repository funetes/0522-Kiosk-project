// List<T>, IReadOnlyList<T> 같은 컬렉션 타입을 사용하기 위해 가져옵니다.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public sealed class CafeKioskViewModel
{
    private readonly List<MenuItem> menuItems;
    private readonly List<CartLine> cart = new();
    private readonly CafeKioskMembershipService membershipService = new();

    public event Action OnPendingOptionItemSet;

    // 음료 메뉴를 눌렀을 때, 옵션 선택 창에서 어떤 메뉴를 처리 중인지 기억합니다.
    public MenuItem PendingOptionItem { get; private set; }

    // 결제 완료 때 보여줄 번호표 번호입니다. 결제할 때마다 1씩 증가합니다.
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
        // 화면 상단 카테고리 버튼에 사용할 카테고리 목록입니다.
        Categories = new[] { "All", "Coffee", "Ade", "Dessert", "Food", "뒤로가기" };
        // 처음에는 전체 메뉴를 보여주도록 "All"을 선택합니다.
        SelectedCategory = "All";
        SelectedTemperature = "ICE";
        SelectedSize = "Regular";
        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
    }

    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<CartLine> Cart => cart;
    public string SelectedCategory { get; private set; }
    // 음료 옵션 창에서 선택된 온도입니다.
    public string SelectedTemperature { get; private set; }
    // 음료 옵션 창에서 선택된 사이즈입니다.
    public string SelectedSize { get; private set; }
    // "매장" 또는 "포장" 중 사용자가 고른 주문 방식입니다.
    public string OrderMode { get; private set; } = "";
    // 화면 하단 상태 메시지에 보여줄 문구입니다.
    public string StatusText { get; private set; } = "";
    // 결제 창의 멤버십 상태 문구입니다.
    public string MemberStatusText { get; private set; }
    // 결제 완료 후 결제 창 안에 보여줄 번호표 문구입니다.
    public string TicketText { get; private set; } = "";
    // 현재 결제할 금액입니다. 결제 창을 열 때 장바구니 합계로 고정합니다.
    public int PaymentAmount { get; private set; }
    // 장바구니 전체 합계입니다. 매번 현재 장바구니를 기준으로 계산합니다.
    public int CartTotal => cart.Sum(line => line.UnitPrice * line.Quantity);
    // 장바구니가 비어 있는지 UI가 쉽게 판단하도록 bool로 제공합니다.
    public bool HasCartItems => cart.Count > 0;
    // 시작 화면이 보이는지 나타냅니다. 처음에는 주문 방식을 골라야 하므로 true입니다.
    public bool IsStartScreenVisible { get; private set; } = true;
    // 주문 화면은 시작 화면이 사라진 뒤 보이면 되므로 시작 화면 상태의 반대입니다.
    public bool IsOrderScreenVisible => !IsStartScreenVisible;
    // 음료 옵션 팝업이 열려 있는지 나타냅니다.
    public bool IsOptionOverlayVisible { get; private set; }
    // 결제 팝업이 열려 있는지 나타냅니다.
    public bool IsPaymentOverlayVisible { get; private set; }
    // 영수증 팝업이 열려 있는지 나타냅니다.
    public bool IsReceiptVisible { get; private set; }

    public int CurrentDiscount { get; private set; } = 0;
    public bool IsUsingCoupon { get; private set; } = false;

    public IReadOnlyList<MenuItem> VisibleMenuItems => menuItems.Where(item => SelectedCategory == "All" || item.Category == SelectedCategory).ToList();


    // 카테고리 버튼을 눌렀을 때 호출됩니다.
    public void SelectCategory(string category)
    {
        // 선택된 카테고리를 저장하면 VisibleMenuItems 결과가 달라집니다.

        if (category == "뒤로가기")
        {
            BackToStartScreen();
            return;
        }
        SelectedCategory = category;

    }
    // --- 기본 로직 ---
    public void SelectOrderMode(string mode) { OrderMode = mode; IsStartScreenVisible = false; StatusText = $"{OrderMode} 주문을 시작합니다."; }
    public void SelectTemperature(string temp) => SelectedTemperature = temp;
    public void SelectSize(string size) => SelectedSize = size;
    public bool StartAddToCart(MenuItem item)
    {
        if (IsDrink(item))
        {

            // 옵션 선택이 끝날 때 어떤 메뉴를 담아야 하는지 기억합니다.
            PendingOptionItem = item;

            OnPendingOptionItemSet();
            // 커피는 ICE/HOT를 고를 수 있으므로 기본값을 ICE로 둡니다. 에이드는 온도 선택 없이 ICE로 처리합니다.

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
        // 처리 중인 음료가 없다면 더 할 일이 없으므로 팝업만 닫습니다.
        if (PendingOptionItem == null)
        {
            // 컨트롤러가 옵션 팝업을 숨기도록 상태를 false로 바꿉니다.
            IsOptionOverlayVisible = false;
            // 장바구니가 바뀌지 않았으므로 false를 반환합니다.
            return false;
        }

        // 커피는 사용자가 고른 온도를 쓰고, 에이드는 항상 ICE로 저장합니다.
        var temperature = PendingOptionItem.Category == "Coffee" ? SelectedTemperature : "ICE";
        // 기본 가격에 사이즈 추가 금액을 더해 실제 단가를 계산합니다.
        var unitPrice = PendingOptionItem.Price + SizeExtraPrice(SelectedSize);
        // 계산된 옵션과 단가를 사용해 장바구니에 추가합니다.
        AddToCart(PendingOptionItem, temperature, SelectedSize, unitPrice);
        // 옵션 처리가 끝났으므로 임시로 기억한 메뉴를 비웁니다.
        PendingOptionItem = null;
        // 컨트롤러가 옵션 팝업을 숨기도록 상태를 false로 바꿉니다.

        IsOptionOverlayVisible = false;
        return true;
    }

    public void ChangeQuantity(CartLine line, int delta) { line.Quantity += delta; if (line.Quantity <= 0) cart.Remove(line); }

    public void ClearCart() => cart.Clear();

    // 옵션 창에서 "취소" 버튼을 눌렀을 때 호출됩니다.
    public void CancelDrinkOption()
    {
        // 담으려던 음료 메뉴를 비웁니다.
        PendingOptionItem = null;
        // 옵션 팝업을 닫도록 상태를 false로 바꿉니다.
        IsOptionOverlayVisible = false;
        // 화면 하단에 취소 메시지를 보여줍니다.
        StatusText = "옵션 선택을 취소했습니다.";
    }

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
        // 멤버십 스탬프 적립을 위해 구매한 총 메뉴 개수를 계산합니다.
        var purchasedCount = cart.Sum(line => line.Quantity);
        // 전화번호가 있으면 멤버십 적립을 적용하고, 상태 메시지에 붙일 요약 문구를 받습니다.
        var membershipMessage = ApplyMembership(memberPhone, purchasedCount);
        // 번호표 번호를 1 증가시키고, 증가된 값을 이번 주문 번호로 사용합니다.
        var ticketNumber = ++orderNumber;
        // 결제 창에 표시할 번호표 문구를 저장합니다.
        TicketText = $"번호표 {ticketNumber}번";
        // 컨트롤러가 결제 팝업을 숨기도록 상태를 false로 바꿉니다.
        IsPaymentOverlayVisible = false;
        // 주문 방식, 결제 방식, 번호표, 금액, 멤버십 결과를 하나의 상태 메시지로 만듭니다.
        StatusText = $"{OrderMode} · {method} 결제 완료 · 번호표 {ticketNumber}번 · {FormatPrice(PaymentAmount)} {membershipMessage}";
        // 영수증 popup을 보여줍니다.
        IsReceiptVisible = true;
    }

    public void CancelPayment() { IsPaymentOverlayVisible = false; CurrentDiscount = 0; IsUsingCoupon = false; StatusText = "결제를 취소했습니다."; }
    public void RegisterOrLookupMember(string phone) => MemberStatusText = membershipService.RegisterOrLookup(phone).StatusText;

    // --- 관리자 보안 로직 ---
    public bool CheckAdminPassword(string input)
    {
        return input.Length == 4 && input.All(char.IsDigit);
    }

    // 내부 카테고리 코드("Coffee")를 화면 표시용 한글("커피")로 바꿉니다.
    public static string CategoryLabel(string category)
    {
        // category 값에 따라 대응되는 한글 표시명을 반환합니다.
        return category switch
        {
            // 전체 메뉴를 뜻하는 카테고리입니다.
            "All" => "전체",
            // 커피 메뉴 카테고리입니다.
            "Coffee" => "커피",
            // 에이드 메뉴 카테고리입니다.
            "Ade" => "에이드",
            // 디저트 메뉴 카테고리입니다.
            "Dessert" => "디저트",
            // 음식 메뉴 카테고리입니다.
            "Food" => "푸드",
            // 새 카테고리가 추가되었는데 매핑이 없으면 원래 문자열을 그대로 보여줍니다.
            _ => category,
        };
    }

    // 메뉴 가격 표시 문구를 만듭니다.
    public static string MenuPriceText(MenuItem item)
    {
        // 음료는 사이즈 추가금이 있을 수 있으므로 "부터"를 붙이고, 나머지는 기본 가격만 보여줍니다.
        return IsDrink(item) ? $"{FormatPrice(item.Price)}부터" : FormatPrice(item.Price);
    }

    // 정수 가격을 "4,500원" 같은 화면 표시용 문자열로 바꿉니다.
    public static string FormatPrice(int price)
    {
        // InvariantCulture를 쓰면 기기 언어 설정과 무관하게 쉼표 포맷이 안정적으로 적용됩니다.
        return price.ToString("N0", CultureInfo.InvariantCulture) + "원";
    }

    // 장바구니에 실제로 한 줄을 추가하거나, 이미 같은 옵션이 있으면 수량만 늘립니다.
    private void AddToCart(MenuItem item, string temperature, string size, int unitPrice)
    {
        // 같은 메뉴, 같은 온도, 같은 사이즈, 같은 단가인 줄이 이미 있는지 찾습니다.
        var line = cart.FirstOrDefault(entry => entry.Item == item && entry.Temperature == temperature && entry.Size == size && entry.UnitPrice == unitPrice);
        // 같은 조건의 줄이 없다면 새 CartLine을 만들어야 합니다.
        if (line == null)
        {
            // 새 장바구니 줄을 만들고, 생성자에서는 Quantity가 0으로 시작합니다.
            line = new CartLine(item, temperature, size, unitPrice);
            // 새 줄을 장바구니 목록에 추가합니다.
            cart.Add(line);
        }

        // 새 줄이든 기존 줄이든 수량을 1개 늘립니다.
        line.Quantity++;
        // 사용자가 무엇을 담았는지 화면 하단에 표시합니다.
        StatusText = $"{line.DisplayName} 추가";
    }

    // 결제 완료 시 멤버십 적립을 적용합니다.
    private string ApplyMembership(string phone, int purchasedCount)
    {
        // 멤버십 서비스에 전화번호와 구매 수량을 전달합니다.
        var result = membershipService.ApplyPurchase(phone, purchasedCount);
        // 결제 창 안에 보여줄 멤버십 상태 문구를 저장합니다.
        MemberStatusText = result.StatusText;
        // 결제 완료 상태 메시지 뒤에 붙일 짧은 요약 문구를 반환합니다.
        return result.SummaryText;
    }

    // 이 메뉴가 옵션이 필요한 음료인지 판단합니다.
    private static bool IsDrink(MenuItem item)
    {
        // 현재 규칙상 Coffee와 Ade 카테고리만 음료로 봅니다.
        return item.Category == "Coffee" || item.Category == "Ade";
    }

    // 사이즈별 추가 금액을 계산합니다.
    private static int SizeExtraPrice(string size)
    {
        // 선택된 size 문자열에 맞는 추가 금액을 반환합니다.
        return size switch
        {
            // Small은 추가 금액이 없습니다.
            "Small" => 0,
            // Regular는 500원이 추가됩니다.
            "Regular" => 500,
            // Large는 1,000원이 추가됩니다.
            "Large" => 1000,
            // 알 수 없는 값이 들어오면 안전하게 추가 금액 0원으로 처리합니다.
            _ => 0,
        };
    }

    // 사이즈 선택 상태 메시지에 보여줄 문구를 만듭니다.
    private static string SizeLabel(string size)
    {
        // 내부 size 값에 따라 사용자에게 보여줄 설명 문구를 반환합니다.
        return size switch
        {
            // Small은 기본 가격임을 보여줍니다.
            "Small" => "Small",
            // Regular는 500원 추가임을 보여줍니다.
            "Regular" => "Regular +500원",
            // Large는 1,000원 추가임을 보여줍니다.
            "Large" => "Large +1,000원",
            // 알 수 없는 값이면 원래 문자열을 그대로 보여줍니다.
            _ => size,
        };
    }

    private void BackToStartScreen()
    {
        cart.Clear();
        // 시작화면 다시 보이기
        IsStartScreenVisible = true;
        SelectedCategory = "All";

        // 상태메시지
        StatusText = "주문 방식 선택 화면으로 돌아갑니다.";
    }
    // 영수증 팝업에서 확인을 눌렀을때 실행됩니다.
    public void ConfirmReceipt()
    {
        // 결제가 끝났으므로 장바구니를 비웁니다.
        cart.Clear();
        // 시작화면을 보여줍니다.
        IsStartScreenVisible = true;
        // 영수증 화면 popup을 닫습니다.
        IsReceiptVisible = false;
    }
}

