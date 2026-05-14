using System.Collections.Generic;
using System.Linq;

public sealed class CafeKioskMembershipService
{
    private readonly Dictionary<string, MemberInfo> members = new();

    public MembershipResult RegisterOrLookup(string phone)
    {
        phone = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(phone)) return new MembershipResult("전화번호를 입력해주세요.", "");

        if (!members.TryGetValue(phone, out var member))
        {
            member = new MemberInfo(phone);
            members[phone] = member;
            return new MembershipResult("회원가입 완료 · 스탬프 0/10", "");
        }

        return new MembershipResult($"회원 조회 완료 · 스탬프 {member.Stamps}/10 · 쿠폰 {member.Coupons}장", "");
    }

    public MembershipResult ApplyPurchase(string phone, int purchasedCount)
    {
        phone = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(phone)) return new MembershipResult("", "");

        if (!members.TryGetValue(phone, out var member))
        {
            member = new MemberInfo(phone);
            members[phone] = member;
        }

        member.Stamps += purchasedCount;
        var issuedCoupons = member.Stamps / 10;
        if (issuedCoupons > 0)
        {
            member.Coupons += issuedCoupons;
            member.Stamps %= 10;
            return new MembershipResult($"쿠폰 {issuedCoupons}장 발급 · 남은 스탬프 {member.Stamps}/10", $"· 쿠폰 {issuedCoupons}장 발급");
        }

        return new MembershipResult($"스탬프 {member.Stamps}/10 · 보유 쿠폰 {member.Coupons}장", $"· 스탬프 {member.Stamps}/10");
    }

    // -- 쿠폰 존재 여부 확인 --
    public bool HasCoupon(string phone)
    {
        phone = NormalizePhone(phone);
        return members.TryGetValue(phone, out var member) && member.Coupons > 0;
    }

    // -- 쿠폰 차감 --
    public bool UseCoupon(string phone)
    {
        phone = NormalizePhone(phone);
        if (members.TryGetValue(phone, out var member) && member.Coupons > 0)
        {
            member.Coupons--;
            return true;
        }
        return false;
    }

    private static string NormalizePhone(string phone) => new string(phone.Where(char.IsDigit).ToArray());
}