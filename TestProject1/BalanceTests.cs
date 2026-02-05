using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChallengeGampix.Models;

namespace TestProject1
{
    [TestClass]
    public class BalanceTests
    {
        [TestMethod]
        public void CalculateBalance_EmptyTransactions_ReturnsEmptyResult()
        {
            // Arrange
            var balance = new Balance();
            Transaction[] transactions = [];

            // Act
            var result = balance.CalculateBalance(transactions);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UserBalances.Count);
            Assert.AreEqual(0, result.TopUsers.Count);
        }

        [TestMethod]
        public void CalculateBalance_NullTransactions_ReturnsEmptyResult()
        {
            // Arrange
            var balance = new Balance();

            // Act
            var result = balance.CalculateBalance(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.UserBalances.Count);
            Assert.AreEqual(0, result.TopUsers.Count);
        }

        [TestMethod]
        public void CalculateBalance_SingleDeposit_CalculatesCorrectly()
        {
            // Arrange
            var balance = new Balance();
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100, Timestamp = DateTime.Now }
            };

            // Act
            var result = balance.CalculateBalance(transactions);

            // Assert
            Assert.AreEqual(1, result.UserBalances.Count);
            var userBalance = result.UserBalances[0];
            Assert.AreEqual("user1", userBalance.UserId);
            Assert.AreEqual(100, userBalance.NetBalance);
            Assert.AreEqual(100, userBalance.TotalDeposits);
            Assert.AreEqual(0, userBalance.TotalWithdrawals);

            Assert.AreEqual(1, result.TopUsers.Count);
            var topUser = result.TopUsers[0];
            Assert.AreEqual("user1", topUser.UserId);
            Assert.AreEqual(100, topUser.TotalVolume);
        }

        [TestMethod]
        public void CalculateBalance_DepositAndWithdrawal_CalculatesCorrectly()
        {
            // Arrange
            var balance = new Balance();
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 200, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 50, Timestamp = DateTime.Now }
            };

            // Act
            var result = balance.CalculateBalance(transactions);

            // Assert
            Assert.AreEqual(1, result.UserBalances.Count);
            var userBalance = result.UserBalances[0];
            Assert.AreEqual("user1", userBalance.UserId);
            Assert.AreEqual(150, userBalance.NetBalance);
            Assert.AreEqual(200, userBalance.TotalDeposits);
            Assert.AreEqual(50, userBalance.TotalWithdrawals);

            Assert.AreEqual(1, result.TopUsers.Count);
            var topUser = result.TopUsers[0];
            Assert.AreEqual("user1", topUser.UserId);
            Assert.AreEqual(250, topUser.TotalVolume);
        }

        [TestMethod]
        public void CalculateBalance_NegativeBalance_ThrowsException()
        {
            // Arrange
            var balance = new Balance();
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100, Timestamp = DateTime.Now },
                new Transaction { UserId = "user1", Type = TransactionType.Withdraw, Amount = 150, Timestamp = DateTime.Now }
            };

            // Act & Assert
            try
            {
                balance.CalculateBalance(transactions);
                Assert.Fail("Expected NegativeBalanceException to be thrown.");
            }
            catch (NegativeBalanceException ex)
            {
                Assert.AreEqual("user1", ex.UserId);
                Assert.AreEqual(-50, ex.Balance);
            }
        }

        [TestMethod]
        public void CalculateBalance_MultipleUsers_CalculatesTopUsers()
        {
            // Arrange
            var balance = new Balance();
            var transactions = new[]
            {
                new Transaction { UserId = "user1", Type = TransactionType.Deposit, Amount = 100, Timestamp = DateTime.Now },
                new Transaction { UserId = "user2", Type = TransactionType.Deposit, Amount = 200, Timestamp = DateTime.Now },
                new Transaction { UserId = "user3", Type = TransactionType.Deposit, Amount = 50, Timestamp = DateTime.Now },
                new Transaction { UserId = "user4", Type = TransactionType.Deposit, Amount = 150, Timestamp = DateTime.Now }
            };

            // Act
            var result = balance.CalculateBalance(transactions);

            // Assert
            Assert.AreEqual(4, result.UserBalances.Count);

            Assert.AreEqual(3, result.TopUsers.Count);
            Assert.AreEqual("user2", result.TopUsers[0].UserId);
            Assert.AreEqual(200, result.TopUsers[0].TotalVolume);
            Assert.AreEqual("user4", result.TopUsers[1].UserId);
            Assert.AreEqual(150, result.TopUsers[1].TotalVolume);
            Assert.AreEqual("user1", result.TopUsers[2].UserId);
            Assert.AreEqual(100, result.TopUsers[2].TotalVolume);
        }
    }
}