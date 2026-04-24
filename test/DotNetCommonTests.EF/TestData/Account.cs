namespace DotNetCommonTests.EF.TestData;

public enum AccountType
{
    Checking,
    Savings,
    CreditLine,
    CreditCard,
    Mortgage
}

public class Account
{
    public int AccountId { get; set; }
    public int CustomerId { get; set; }
    public string? AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public DateOnly BalanceDate { get; set; }
}