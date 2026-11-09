namespace Tooded_2;
public sealed class CheckoutFlow(IShopStore store, Func<ShopOrder, Task> sendEmail)
{
    private int working;
    public bool InProgress => Volatile.Read(ref working) != 0;
    public CheckoutResult? Result { get; private set; }

    public async Task<CheckoutResult> PayAsync(ShoppingCart cart, string email)
    {
        Enter();
        try
        {
            if (Result != null)
                throw new InvalidOperationException(UiText.Get("ThisOrderHasAlreadyBeenPaid"));
            if (!OrderEmail.ValidAddress(email))
                throw new InvalidOperationException(UiText.Get("EnterAValidEmailAddress"));
            var lines = cart.Lines.ToArray();
            var order = (await Task.Run(() => store.Checkout(lines)))with
            {
                CustomerEmail = email.Trim()
            };
            Result = new(order, false, "SendingConfirmation");
            cart.Clear(); // A mail failure must never repeat checkout or consume stock twice.
            return Result = await Deliver(order);
        }
        finally
        {
            Interlocked.Exchange(ref working, 0);
        }
    }

    public async Task<CheckoutResult> RetryEmailAsync()
    {
        Enter();
        try
        {
            var current = Result ?? throw new InvalidOperationException(UiText.Get("NoOrderIsAvailable"));
            return Result = current.EmailSent ? current : await Deliver(current.Order);
        }
        finally
        {
            Interlocked.Exchange(ref working, 0);
        }
    }

    private void Enter()
    {
        if (Interlocked.CompareExchange(ref working, 1, 0) != 0)
            throw new InvalidOperationException(UiText.Get("OrderAlreadyProcessing"));
    }

    private async Task<CheckoutResult> Deliver(ShopOrder order)
    {
        try
        {
            await sendEmail(order);
            return new(order, true, "TheOrderConfirmationWasSentTo");
        }
        catch (Exception)
        {
            return new(order, false, "EmailDeliveryFailed");
        }
    }
}
