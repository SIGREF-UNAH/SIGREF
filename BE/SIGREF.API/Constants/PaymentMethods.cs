using SIGREF.Common.Types;

namespace SIGREF.API.Constants;

public static class PaymentMethods
{
    public static readonly PaymentMethodType[] All =
    {
        PaymentMethodType.Cash,
        // PaymentMethodType.Card,
        //PaymentMethodType.Transfer,
        //PaymentMethodType.Mixed
    };
}