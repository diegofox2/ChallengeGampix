using ChallengeGampix.Models;

namespace ChallengeGampix.Services
{
    public class TransactionService
    {
        private readonly Balance _balance;

        public TransactionService()
        {
            _balance = new Balance();
        }

        public BalanceResult ProcessTransactions(Transaction[] transactions)
        {
            return _balance.CalculateBalance(transactions);
        }
    }
}
