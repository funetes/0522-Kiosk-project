public sealed class MenuItem
{
    public MenuItem(string name, string category, string description, int price)
    {
        Name = name;
        Category = category;
        Description = description;
        Price = price;
    }

    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public int Price { get; }
}

public sealed class CartLine
{
    public CartLine(MenuItem item, string temperature, string size, int unitPrice)
    {
        Item = item;
        Temperature = temperature;
        Size = size;
        UnitPrice = unitPrice;
        Quantity = 0;
    }

    public MenuItem Item { get; }
    public string Temperature { get; }
    public string Size { get; }
    public int UnitPrice { get; }
    public int Quantity { get; set; }

    public string DisplayName
    {
        get
        {
            var options = "";
            if (!string.IsNullOrWhiteSpace(Temperature))
            {
                options += Temperature;
            }

            if (!string.IsNullOrWhiteSpace(Size))
            {
                options += string.IsNullOrWhiteSpace(options) ? Size : $" / {Size}";
            }

            return string.IsNullOrWhiteSpace(options) ? Item.Name : $"{Item.Name} ({options})";
        }
    }
}

public sealed class MemberInfo
{
    public MemberInfo(string phone)
    {
        Phone = phone;
    }

    public string Phone { get; }
    public int Stamps { get; set; }
    public int Coupons { get; set; }
}

public sealed class MembershipResult
{
    public MembershipResult(string statusText, string summaryText)
    {
        StatusText = statusText;
        SummaryText = summaryText;
    }

    public string StatusText { get; }
    public string SummaryText { get; }
}
