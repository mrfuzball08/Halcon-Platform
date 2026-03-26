public static class DomainValidation
{
    public static bool IsValidRole(string? role)
    {
        return Enum.TryParse<UserRole>(role, ignoreCase: false, out _);
    }

    public static bool IsValidStatus(string? status)
    {
        return Enum.TryParse<OrderStatus>(status, ignoreCase: false, out _);
    }
}
