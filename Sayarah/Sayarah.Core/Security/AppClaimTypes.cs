namespace Sayarah.Security
{
    /// <summary>
    /// Used to get Express-specific claim type names.
    /// </summary>
    public static class AppClaimTypes
    {
        /// <summary>
        /// UserType.
        /// </summary>
        public const string UserType = "http://www.al7osamcompany.com/identity/claims/UserType";
        public const string CompanyId = "http://www.al7osamcompany.com/identity/claims/CompanyId";
        public const string BranchId = "http://www.al7osamcompany.com/identity/claims/BranchId";
        public const string ProviderId = "http://www.al7osamcompany.com/identity/claims/ProviderId";
        public const string MainProviderId = "http://www.al7osamcompany.com/identity/claims/MainProviderId";
    }
}
