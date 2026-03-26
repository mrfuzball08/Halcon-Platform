using Xunit;

public sealed class DomainValidationTests
{
    [Theory]
    [InlineData(nameof(UserRole.ADMIN), true)]
    [InlineData(nameof(UserRole.SALES), true)]
    [InlineData("admin", false)]
    [InlineData("UNKNOWN", false)]
    public void IsValidRole_ReturnsExpectedValue(string input, bool expected)
    {
        var result = DomainValidation.IsValidRole(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(nameof(OrderStatus.ORDERED), true)]
    [InlineData(nameof(OrderStatus.DELIVERED), true)]
    [InlineData("ordered", false)]
    [InlineData("INVALID", false)]
    public void IsValidStatus_ReturnsExpectedValue(string input, bool expected)
    {
        var result = DomainValidation.IsValidStatus(input);

        Assert.Equal(expected, result);
    }
}
