using Microsoft.AspNetCore.Mvc;
using ChallengeGampix.Models;
using ChallengeGampix.Services;

namespace ChallengeGampix.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost(Name = "ProcessTransactions")]
        public IActionResult ProcessTransactions([FromBody] Transaction[] transactions)
        {
            if (transactions == null || transactions.Length == 0)
            {
                return BadRequest("No transactions provided");
            }

            try
            {
                var result = _transactionService.ProcessTransactions(transactions);
                return Ok(result);
            }
            catch (NegativeBalanceException ex)
            {
                return BadRequest(new 
                { 
                    Error = "Negative balance detected"
                });
            }
        }
    }
}
