using Tooded_2.Shop;

namespace Tooded_2;

public static class TsekiKoostaja
{
    public static string LooHtml(List<OstuRida> ostud, string tsekiNr, DateTime aeg, string klient)
    {
        var lines = ostud.Select(row => new CartLine(new ShopProduct
        { Id = row.ToodeId, Name = row.Nimetus, Price = row.Hind }, row.Kogus)).ToArray();
        return OrderEmail.Html(new DemoOrder(tsekiNr, new DateTimeOffset(aeg), lines) { CustomerEmail = klient });
    }
}
