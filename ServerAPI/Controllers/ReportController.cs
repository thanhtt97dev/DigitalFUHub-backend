using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _repository;

        public ReportController(IReportRepository repository)
        {
            _repository = repository;
        }

        #region Get report user (sample)
        [HttpPost("user")]
        public IActionResult ReportUser ([FromBody] ExportRequestDTO exportRequestDTO)
        {
            try
            {

                var fileContents = _repository.ReportUser(exportRequestDTO.Id);

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user_report.xlsx");

            } catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
		#endregion

	}
}
