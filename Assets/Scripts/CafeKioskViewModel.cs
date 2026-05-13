// List<T>, IReadOnlyList<T> 같은 컬렉션 타입을 사용하기 위해 가져옵니다.
using System.Collections.Generic;
// 가격을 "1,000원"처럼 쉼표가 들어간 문자열로 바꾸기 위해 가져옵니다.
using System.Globalization;
// Where, Sum, FirstOrDefault 같은 LINQ 확장 메서드를 사용하기 위해 가져옵니다.
using System.Linq;

// CafeKioskViewModel은 화면에 직접 그리는 코드를 가지지 않고, 키오스크의 상태와 규칙만 관리합니다.
public sealed class CafeKioskViewModel
{
    // 전체 메뉴 목록입니다. 카테고리 필터를 바꿀 때 이 목록에서 보여줄 메뉴를 골라냅니다.
    private readonly List<MenuItem> menuItems;
    // 현재 장바구니에 담긴 항목 목록입니다. 같은 메뉴라도 옵션이 다르면 다른 CartLine으로 저장됩니다.
    private readonly List<CartLine> cart = new();
    // 멤버십 가입, 조회, 스탬프 적립 규칙을 처리하는 서비스입니다.
    private readonly CafeKioskMembershipService membershipService = new();

    // 음료 메뉴를 눌렀을 때, 옵션 선택 창에서 어떤 메뉴를 처리 중인지 기억합니다.
    private MenuItem pendingOptionItem;
    // 결제 완료 때 보여줄 번호표 번호입니다. 결제할 때마다 1씩 증가합니다.
    private int orderNumber = 100;

    // ViewModel이 처음 만들어질 때 기본 상태를 준비합니다.
    public CafeKioskViewModel()
    {
        // 메뉴 카탈로그에서 전체 메뉴 데이터를 가져옵니다.
        menuItems = CafeKioskMenuCatalog.CreateMenu();
        // 화면 상단 카테고리 버튼에 사용할 카테고리 목록입니다.
        Categories = new[] { "All", "Coffee", "Ade", "Dessert", "Food" };
        // 처음에는 전체 메뉴를 보여주도록 "All"을 선택합니다.
        SelectedCategory = "All";
        // 음료 옵션의 기본 온도는 ICE입니다.
        SelectedTemperature = "ICE";
        // 음료 옵션의 기본 사이즈는 Regular입니다.
        SelectedSize = "Regular";
        // 결제 창에서 처음 보여줄 멤버십 안내 문구입니다.
        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
    }

    // 컨트롤러가 카테고리 버튼을 만들 때 읽는 카테고리 목록입니다.
    public IReadOnlyList<string> Categories { get; }
    // 컨트롤러가 장바구니 UI를 다시 그릴 때 읽는 장바구니 목록입니다.
    public IReadOnlyList<CartLine> Cart => cart;
    // 현재 선택된 카테고리입니다. private set이라 외부에서는 SelectCategory로만 바꿀 수 있습니다.
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

    // 현재 선택된 카테고리에 따라 화면에 보여줄 메뉴만 골라서 반환합니다.
    public IReadOnlyList<MenuItem> VisibleMenuItems =>
        // "All"이면 모든 메뉴를 보여주고, 아니면 item.Category가 선택 카테고리와 같은 메뉴만 보여줍니다.
        menuItems.Where(item => SelectedCategory == "All" || item.Category == SelectedCategory).ToList();

    // 카테고리 버튼을 눌렀을 때 호출됩니다.
    public void SelectCategory(string category)
    {
        // 선택된 카테고리를 저장하면 VisibleMenuItems 결과가 달라집니다.
        SelectedCategory = category;
    }

    // 시작 화면에서 "매장" 또는 "포장"을 눌렀을 때 호출됩니다.
    public void SelectOrderMode(string mode)
    {
        // 사용자가 고른 주문 방식을 저장합니다.
        OrderMode = mode;
        // 주문 방식을 골랐으므로 시작 화면은 닫습니다.
        IsStartScreenVisible = false;
        // 화면 하단에 주문 시작 메시지를 보여줍니다.
        StatusText = $"{OrderMode} 주문을 시작합니다.";
    }

    // 메뉴 카드의 "담기" 버튼을 눌렀을 때 호출됩니다.
    public bool StartAddToCart(MenuItem item)
    {
        // 커피나 에이드처럼 옵션이 필요한 음료인지 확인합니다.
        if (IsDrink(item))
        {
            // 옵션 선택이 끝날 때 어떤 메뉴를 담아야 하는지 기억합니다.
            pendingOptionItem = item;
            // 커피는 ICE/HOT를 고를 수 있으므로 기본값을 ICE로 둡니다. 에이드는 온도 선택 없이 ICE로 처리합니다.
            SelectedTemperature = item.Category == "Coffee" ? "ICE" : "";
            // 옵션 창을 열 때마다 사이즈 기본값을 Regular로 초기화합니다.
            SelectedSize = "Regular";
            // 컨트롤러가 옵션 팝업을 보이게 만들 수 있도록 상태를 true로 바꿉니다.
            IsOptionOverlayVisible = true;
            // 사용자에게 옵션 선택이 필요하다는 메시지를 보여줍니다.
            StatusText = $"{item.Name} 옵션을 선택해주세요.";
            // 아직 장바구니에 바로 추가하지 않았으므로 false를 반환합니다.
            return false;
        }

        // 음료가 아닌 메뉴는 옵션 없이 기본 가격 그대로 장바구니에 추가합니다.
        AddToCart(item, "", "", item.Price);
        // 장바구니 UI를 다시 그려야 하므로 true를 반환합니다.
        return true;
    }

    // 옵션 창에서 ICE 또는 HOT 버튼을 눌렀을 때 호출됩니다.
    public void SelectTemperature(string temperature)
    {
        // 선택한 온도를 저장합니다.
        SelectedTemperature = temperature;
        // 화면 하단 상태 메시지를 갱신합니다.
        StatusText = $"{temperature} 선택";
    }

    // 옵션 창에서 Small, Regular, Large 버튼을 눌렀을 때 호출됩니다.
    public void SelectSize(string size)
    {
        // 선택한 사이즈를 저장합니다.
        SelectedSize = size;
        // 사이즈별 추가 금액이 포함된 안내 문구를 상태 메시지로 보여줍니다.
        StatusText = $"{SizeLabel(size)} 선택";
    }

    // 옵션 창에서 "선택 담기" 버튼을 눌렀을 때 호출됩니다.
    public bool ConfirmDrinkOption()
    {
        // 처리 중인 음료가 없다면 더 할 일이 없으므로 팝업만 닫습니다.
        if (pendingOptionItem == null)
        {
            // 컨트롤러가 옵션 팝업을 숨기도록 상태를 false로 바꿉니다.
            IsOptionOverlayVisible = false;
            // 장바구니가 바뀌지 않았으므로 false를 반환합니다.
            return false;
        }

        // 커피는 사용자가 고른 온도를 쓰고, 에이드는 항상 ICE로 저장합니다.
        var temperature = pendingOptionItem.Category == "Coffee" ? SelectedTemperature : "ICE";
        // 기본 가격에 사이즈 추가 금액을 더해 실제 단가를 계산합니다.
        var unitPrice = pendingOptionItem.Price + SizeExtraPrice(SelectedSize);
        // 계산된 옵션과 단가를 사용해 장바구니에 추가합니다.
        AddToCart(pendingOptionItem, temperature, SelectedSize, unitPrice);
        // 옵션 처리가 끝났으므로 임시로 기억한 메뉴를 비웁니다.
        pendingOptionItem = null;
        // 컨트롤러가 옵션 팝업을 숨기도록 상태를 false로 바꿉니다.
        IsOptionOverlayVisible = false;
        // 장바구니가 바뀌었으므로 true를 반환합니다.
        return true;
    }

    // 옵션 창에서 "취소" 버튼을 눌렀을 때 호출됩니다.
    public void CancelDrinkOption()
    {
        // 담으려던 음료 메뉴를 비웁니다.
        pendingOptionItem = null;
        // 옵션 팝업을 닫도록 상태를 false로 바꿉니다.
        IsOptionOverlayVisible = false;
        // 화면 하단에 취소 메시지를 보여줍니다.
        StatusText = "옵션 선택을 취소했습니다.";
    }

    // 장바구니에서 + 또는 - 버튼을 눌렀을 때 호출됩니다.
    public void ChangeQuantity(CartLine line, int delta)
    {
        // delta는 +1 또는 -1이며, 현재 수량에 더합니다.
        line.Quantity += delta;
        // 수량이 0 이하가 되면 장바구니에서 해당 줄을 제거합니다.
        if (line.Quantity <= 0)
        {
            // 더 이상 주문할 수량이 없으므로 목록에서 삭제합니다.
            cart.Remove(line);
        }

        // 수량 조정은 별도 성공 메시지를 남기지 않도록 상태 메시지를 비웁니다.
        StatusText = "";
    }

    // "비우기" 버튼을 눌렀을 때 호출됩니다.
    public void ClearCart()
    {
        // 장바구니의 모든 항목을 삭제합니다.
        cart.Clear();
        // 화면 하단에 장바구니를 비웠다는 메시지를 보여줍니다.
        StatusText = "주문을 비웠습니다.";
    }

    // "결제하기" 버튼을 눌렀을 때 호출됩니다.
    public bool Checkout()
    {
        // 장바구니가 비어 있으면 결제 창을 열면 안 됩니다.
        if (cart.Count == 0)
        {
            // 사용자에게 메뉴를 먼저 담으라고 안내합니다.
            StatusText = "메뉴를 먼저 담아주세요.";
            // 결제 창이 열리지 않았으므로 false를 반환합니다.
            return false;
        }

        // 결제 창에 보여줄 금액을 현재 장바구니 합계로 저장합니다.
        PaymentAmount = CartTotal;
        // 결제 창을 열 때 멤버십 안내 문구를 기본값으로 초기화합니다.
        MemberStatusText = "전화번호를 입력하면 스탬프가 적립됩니다.";
        // 이전 결제의 번호표 문구가 남지 않도록 비웁니다.
        TicketText = "";
        // 컨트롤러가 결제 팝업을 보이게 만들 수 있도록 상태를 true로 바꿉니다.
        IsPaymentOverlayVisible = true;
        // 화면 하단에 결제 방식 선택 안내를 보여줍니다.
        StatusText = "결제 방식을 선택해주세요.";
        // 결제 창이 열렸으므로 true를 반환합니다.
        return true;
    }

    // 결제 창에서 카드, 현금, 모바일페이 중 하나를 눌렀을 때 호출됩니다.
    public void CompletePayment(string method, string memberPhone)
    {
        // 멤버십 스탬프 적립을 위해 구매한 총 메뉴 개수를 계산합니다.
        var purchasedCount = cart.Sum(line => line.Quantity);
        // 전화번호가 있으면 멤버십 적립을 적용하고, 상태 메시지에 붙일 요약 문구를 받습니다.
        var membershipMessage = ApplyMembership(memberPhone, purchasedCount);
        // 번호표 번호를 1 증가시키고, 증가된 값을 이번 주문 번호로 사용합니다.
        var ticketNumber = ++orderNumber;
        // 결제가 끝났으므로 장바구니를 비웁니다.
        cart.Clear();
        // 결제 창에 표시할 번호표 문구를 저장합니다.
        TicketText = $"번호표 {ticketNumber}번";
        // 컨트롤러가 결제 팝업을 숨기도록 상태를 false로 바꿉니다.
        IsPaymentOverlayVisible = false;
        // 주문 방식, 결제 방식, 번호표, 금액, 멤버십 결과를 하나의 상태 메시지로 만듭니다.
        StatusText = $"{OrderMode} · {method} 결제 완료 · 번호표 {ticketNumber}번 · {FormatPrice(PaymentAmount)} {membershipMessage}";
    }

    // 결제 창에서 "돌아가기" 버튼을 눌렀을 때 호출됩니다.
    public void CancelPayment()
    {
        // 결제 팝업을 닫도록 상태를 false로 바꿉니다.
        IsPaymentOverlayVisible = false;
        // 사용자에게 결제가 취소되었다고 알려줍니다.
        StatusText = "결제를 취소했습니다.";
    }

    // 결제 창에서 "회원가입/조회" 버튼을 눌렀을 때 호출됩니다.
    public void RegisterOrLookupMember(string phone)
    {
        // 멤버십 서비스에 전화번호를 전달하고, 결과 문구만 ViewModel 상태로 저장합니다.
        MemberStatusText = membershipService.RegisterOrLookup(phone).StatusText;
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
}
