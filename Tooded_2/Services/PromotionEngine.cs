namespace Tooded_2;

/// <summary>Выбирает наибольшую действующую скидку. Скидки не складываются.</summary>
public static class PromotionEngine
{
    public static ShopProduct Apply(ShopProduct product, IEnumerable<Promotion> promotions, DateTimeOffset now)
    {
        decimal originalPrice = product.OriginalPrice ?? product.Price;
        Promotion? bestOffer = null;
        foreach (var offer in promotions)
        {
            if (!offer.Active(now) || !offer.Matches(product) || offer.Percent <= 0 || offer.Percent > 90) continue;
            bool largerDiscount = bestOffer == null || offer.Percent > bestOffer.Percent;
            bool earlierPosition = bestOffer != null && offer.Percent == bestOffer.Percent && offer.Position < bestOffer.Position;
            if (largerDiscount || earlierPosition) bestOffer = offer;
        }

        decimal price = originalPrice;
        if (bestOffer != null)
            price = decimal.Round(originalPrice * (1 - bestOffer.Percent / 100), 2, MidpointRounding.AwayFromZero);

        return product with { OriginalPrice = originalPrice, Price = price, PromotionId = bestOffer?.Id };
    }
}
