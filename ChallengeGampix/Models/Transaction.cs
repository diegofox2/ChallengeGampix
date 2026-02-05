namespace ChallengeGampix.Models
{
    public class Transaction
    {
        public required string UserId { get; set; }
        public required TransactionType Type { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Timestamp { get; set; }
    }

    public enum TransactionType
    {
        Deposit,
        Withdraw
    }
}
