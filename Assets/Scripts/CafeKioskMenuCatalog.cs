using System.Collections.Generic;

public static class CafeKioskMenuCatalog
{
    public static List<MenuItem> CreateMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem("맥길동 커피", "Coffee", "깔끔한 산미의 기본 커피", 4500),
            new MenuItem("카페 라떼", "Coffee", "우유 거품이 부드러운 라떼", 5200),
            new MenuItem("바닐라 라떼", "Coffee", "달콤한 바닐라 향 라떼", 5800),
            new MenuItem("콜드브루", "Coffee", "차갑게 우린 깊은 커피", 5500),
            new MenuItem("자몽 에이드", "Ade", "상큼한 자몽 에이드", 5900),
            new MenuItem("레몬 에이드", "Ade", "레몬 향이 선명한 에이드", 5600),
            new MenuItem("초콜릿 케이크", "Dessert", "진한 초콜릿 조각 케이크", 6800),
            new MenuItem("버터 크루아상", "Dessert", "바삭한 버터 크루아상", 4300),
            new MenuItem("햄치즈 샌드위치", "Food", "든든한 햄치즈 샌드위치", 7200),
            new MenuItem("치킨 샐러드", "Food", "가볍게 먹는 닭가슴살 샐러드", 7600),
        };
    }
}
