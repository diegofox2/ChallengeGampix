using ChallengeGampix.Models;

namespace ChallengeGampix.Tests
{
    public class BalanceTests
    {
        private readonly Balance _balance;

        public BalanceTests()
        {
            _balance = new Balance();
        }

        [Fact]
        public void CalculateBalance_WithNullTransactions_ReturnsEmptyResult()
        {
            // Arrange
            Transaction[]? transactions = null;

            // Act
            var result = _balance.CalculateBalance(transactions!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.UserBalances);
            Assert.Empty(result.TopUsers);
        }

        [Fact]
        public void CalculateBalance_WithEmptyTransactions_ReturnsEmptyResult()
        {
            // Arrange
            var transactions = Array.Empty<Transaction>();

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.UserBalances);
            Assert.Empty(result.TopUsers);
        }

        [Fact]
        public void CalculateBalance_WithSingleDeposit_CalculatesCorrectBalance()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction
                {
                    UserId = "user1",
                    Type = TransactionType.Deposit,
                    Amount = 100m,
                    Timestamp = DateTime.Now
                }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Single(result.UserBalances);
            Assert.Equal("user1", result.UserBalances[0].UserId);
            Assert.Equal(100m, result.UserBalances[0].NetBalance);
            Assert.Equal(100m, result.UserBalances[0].TotalDeposits);
            Assert.Equal(0m, result.UserBalances[0].TotalWithdrawals);
        }

        [Fact]
        public void CalculateBalance_WithSingleWithdrawal_CalculatesCorrectBalance()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction
                {
                    UserId = "user1",
                    Type = TransactionType.Withdraw,
                    Amount = 50m,
                    Timestamp = DateTime.Now
                }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Single(result.UserBalances);
            Assert.Equal("user1", result.UserBalances[0].UserId);
            Assert.Equal(-50m, result.UserBalances[0].NetBalance);
            Assert.Equal(0m, result.UserBalances[0].TotalDeposits);
            Assert.Equal(50m, result.UserBalances[0].TotalWithdrawals);
        }

        [Fact]
        public void CalculateBalance_WithMultipleTransactionsSameUser_CalculatesCorrectBalance()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 200m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 50m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Single(result.UserBalances);
            Assert.Equal("user1", result.UserBalances[0].UserId);
            Assert.Equal(250m, result.UserBalances[0].NetBalance); // 100 + 200 - 50
            Assert.Equal(300m, result.UserBalances[0].TotalDeposits);
            Assert.Equal(50m, result.UserBalances[0].TotalWithdrawals);
        }

        [Fact]
        public void CalculateBalance_WithMultipleUsers_CalculatesCorrectBalances()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 200m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 30m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 150m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Equal(3, result.UserBalances.Count);
            
            var user1Balance = result.UserBalances.FirstOrDefault(u => u.UserId == "user1");
            Assert.NotNull(user1Balance);
            Assert.Equal(70m, user1Balance.NetBalance); // 100 - 30
            Assert.Equal(100m, user1Balance.TotalDeposits);
            Assert.Equal(30m, user1Balance.TotalWithdrawals);

            var user2Balance = result.UserBalances.FirstOrDefault(u => u.UserId == "user2");
            Assert.NotNull(user2Balance);
            Assert.Equal(200m, user2Balance.NetBalance);
            
            var user3Balance = result.UserBalances.FirstOrDefault(u => u.UserId == "user3");
            Assert.NotNull(user3Balance);
            Assert.Equal(150m, user3Balance.NetBalance);
        }

        [Fact]
        public void CalculateBalance_TopUsers_ReturnsTop3ByVolume()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 50m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 500m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 300m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user4", Type = TransactionType.Deposit, Amount = 200m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user5", Type = TransactionType.Deposit, Amount = 50m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Equal(3, result.TopUsers.Count);
            Assert.Equal("user2", result.TopUsers[0].UserId);
            Assert.Equal(500m, result.TopUsers[0].TotalVolume);
            Assert.Equal("user3", result.TopUsers[1].UserId);
            Assert.Equal(300m, result.TopUsers[1].TotalVolume);
            Assert.Equal("user4", result.TopUsers[2].UserId);
            Assert.Equal(200m, result.TopUsers[2].TotalVolume);
        }

        [Fact]
        public void CalculateBalance_TopUsers_CombinesDepositsAndWithdrawals()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 200m, Timestamp = DateTime.Now }, // Total: 300
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 250m, Timestamp = DateTime.Now }  // Total: 250
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Equal(2, result.TopUsers.Count);
            Assert.Equal("user1", result.TopUsers[0].UserId);
            Assert.Equal(300m, result.TopUsers[0].TotalVolume); // 100 + 200
            Assert.Equal("user2", result.TopUsers[1].UserId);
            Assert.Equal(250m, result.TopUsers[1].TotalVolume);
        }

        [Fact]
        public void CalculateBalance_WithLessThan3Users_ReturnsAllUsers()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 200m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Equal(2, result.TopUsers.Count);
        }

        [Fact]
        public void CalculateBalance_WithNegativeBalance_ThrowsException()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 50m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 100m, Timestamp = DateTime.Now }
            };

            // Act & Assert
            var exception = Assert.Throws<NegativeBalanceException>(() => _balance.CalculateBalance(transactions));
            Assert.Equal("user1", exception.UserId);
            Assert.Equal(-50m, exception.Balance);
        }

        [Fact]
        public void CalculateBalance_WithDecimalAmounts_MaintainsPrecision()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100.50m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 25.25m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Single(result.UserBalances);
            Assert.Equal(75.25m, result.UserBalances[0].NetBalance);
            Assert.Equal(100.50m, result.UserBalances[0].TotalDeposits);
            Assert.Equal(25.25m, result.UserBalances[0].TotalWithdrawals);
        }

        [Fact]
        public void CalculateBalance_WithComplexScenario_CalculatesCorrectly()
        {
            // Arrange - Escenario complejo con múltiples usuarios y transacciones
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 1000m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 250m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 500m, Timestamp = DateTime.Now },
                
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 2000m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Withdraw, Amount = 1500m, Timestamp = DateTime.Now },
                
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 750m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 750m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user3", Type = TransactionType.Withdraw, Amount = 200m, Timestamp = DateTime.Now },
                
                new Transaction { UserId = "user4", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert - Verifica balances
            Assert.Equal(4, result.UserBalances.Count);
            
            var user1 = result.UserBalances.First(u => u.UserId == "user1");
            Assert.Equal(1250m, user1.NetBalance); // 1000 - 250 + 500
            Assert.Equal(1500m, user1.TotalDeposits);
            Assert.Equal(250m, user1.TotalWithdrawals);
            
            var user2 = result.UserBalances.First(u => u.UserId == "user2");
            Assert.Equal(500m, user2.NetBalance); // 2000 - 1500
            
            var user3 = result.UserBalances.First(u => u.UserId == "user3");
            Assert.Equal(1300m, user3.NetBalance); // 750 + 750 - 200
            
            // Assert - Verifica Top 3 por volumen
            Assert.Equal(3, result.TopUsers.Count);
            Assert.Equal("user2", result.TopUsers[0].UserId);
            Assert.Equal(3500m, result.TopUsers[0].TotalVolume); // 2000 + 1500
            Assert.Equal("user1", result.TopUsers[1].UserId);
            Assert.Equal(1750m, result.TopUsers[1].TotalVolume); // 1000 + 250 + 500
            Assert.Equal("user3", result.TopUsers[2].UserId);
            Assert.Equal(1700m, result.TopUsers[2].TotalVolume); // 750 + 750 + 200
        }

        [Fact]
        public void CalculateBalance_WithMultipleUsersOneNegative_ThrowsException()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 1000m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 50m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Withdraw, Amount = 100m, Timestamp = DateTime.Now }, // Negativo
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 500m, Timestamp = DateTime.Now }
            };

            // Act & Assert
            var exception = Assert.Throws<NegativeBalanceException>(() => _balance.CalculateBalance(transactions));
            Assert.Equal("user2", exception.UserId);
            Assert.Equal(-50m, exception.Balance);
        }

        [Fact]
        public void CalculateBalance_WithZeroBalance_DoesNotThrowException()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 100m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Single(result.UserBalances);
            Assert.Equal(0m, result.UserBalances[0].NetBalance);
        }

        [Fact]
        public void CalculateBalance_WithAllPositiveBalances_DoesNotThrowException()
        {
            // Arrange
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 50m, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 200m, Timestamp = DateTime.Now }
            };

            // Act
            var result = _balance.CalculateBalance(transactions);

            // Assert
            Assert.Equal(2, result.UserBalances.Count);
            Assert.All(result.UserBalances, ub => Assert.True(ub.NetBalance >= 0));
        }
    }
}
