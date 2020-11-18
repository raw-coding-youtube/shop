namespace RawCoding.Shop.Domain.Extensions
{
    public static class ValueExtensions
    {
        public static string ToMoney(this int v)
        {
            return $"£{v * 0.01:N2}";
        }
    }
}