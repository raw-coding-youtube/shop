namespace RawCoding.Shop.UI
{
    public struct ShopConstants
    {
        public struct Policies
        {
            public const string Customer = nameof(Customer);
            public const string ShopManager = nameof(ShopManager);
            public const string Admin = nameof(Admin);
        }
        public struct Schemas
        {
            public const string Guest = "GuestCookie";
        }

        public struct Claims
        {
            public const string Role = nameof(Role);
        }

        public struct Roles
        {
            public const string Guest = nameof(Guest);
            public const string ShopManager = nameof(ShopManager);
            public const string Admin = nameof(Admin);
        }
    }
}