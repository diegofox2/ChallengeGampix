namespace ChallengeGampix.Models
{
    public class Balance
    {
        public BalanceResult CalculateBalance(Transaction[] transactions)
        {
            if (transactions == null || transactions.Length == 0)
            {
                return new BalanceResult
                {
                    UserBalances = [],
                    TopUsers = []
                };
            }

            var userBalances = transactions
                .GroupBy(t => t.UserId)
                .Select(g => new UserBalance
                {
                    UserId = g.Key,
                    NetBalance = g.Sum(t => t.Type == TransactionType.Deposit ? t.Amount : -t.Amount),
                    TotalDeposits = g.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount),
                    TotalWithdrawals = g.Where(t => t.Type == TransactionType.Withdraw).Sum(t => t.Amount)
                })
                .ToList();

            var negativeBalance = userBalances.FirstOrDefault(ub => ub.NetBalance < 0);
            if (negativeBalance != null)
            {
                throw new NegativeBalanceException(negativeBalance.UserId, negativeBalance.NetBalance);
            }

            var topUsers = transactions
                .GroupBy(t => t.UserId)
                .Select(g => new TopUser
                {
                    UserId = g.Key,
                    TotalVolume = g.Sum(t => t.Amount)
                })
                .OrderByDescending(u => u.TotalVolume)
                .Take(3)
                .ToList();

            return new BalanceResult
            {
                UserBalances = userBalances,
                TopUsers = topUsers
            };
        }
    }

    public class BalanceResult
    {
        public required List<UserBalance> UserBalances { get; set; }
        public required List<TopUser> TopUsers { get; set; }
    }

    public class UserBalance
    {
        public required string UserId { get; set; }
        public required decimal NetBalance { get; set; }
        public required decimal TotalDeposits { get; set; }
        public required decimal TotalWithdrawals { get; set; }
    }

    public class TopUser
    {
        public required string UserId { get; set; }
        public required decimal TotalVolume { get; set; }
    }
}
