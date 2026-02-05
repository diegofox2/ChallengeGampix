namespace ChallengeGampix.Models
{
    public class NegativeBalanceException : Exception
    {
        public string UserId { get; }
        public decimal Balance { get; }

        public NegativeBalanceException(string userId, decimal balance) 
            : base($"User '{userId}' has a negative balance of {balance}")
        {
            UserId = userId;
            Balance = balance;
        }
    }
}
