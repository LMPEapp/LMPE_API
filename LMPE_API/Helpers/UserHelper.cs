namespace LMPE_API.Helpers
{
    public static class UserHelper
    {
        public static (long UserId, bool IsAdmin) GetUserIdAndAdmin(System.Security.Claims.ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var isAdminClaim = user.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;

            if (userIdClaim == null || isAdminClaim == null)
                throw new UnauthorizedAccessException("Token invalide");

            long userId = long.Parse(userIdClaim);
            bool isAdmin = bool.Parse(isAdminClaim);

            return (userId, isAdmin);
        }
    }

}
