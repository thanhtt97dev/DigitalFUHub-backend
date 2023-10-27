using DataAccess.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransactionInternalsController : ControllerBase
	{
		private readonly ITransactionInternalRepository transactionRepository;

		public TransactionInternalsController(ITransactionInternalRepository transactionRepository)
		{
			this.transactionRepository = transactionRepository;
		}
	}
}
