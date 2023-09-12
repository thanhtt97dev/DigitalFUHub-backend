using DataAccess.IRepositories;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportRepository _repository;

        public ReportsController(IReportRepository repository)
        {
            _repository = repository;
        }

        #region Get report user (sample)
        [HttpPost("user")]
        public async Task<IActionResult> ReportUser ([FromBody] UserReportRequestDTO reportRequestDTO)
        {
            try
            {

                var fileContents = await _repository.ReportUser(reportRequestDTO.Id);

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user_report.xlsx");

            } catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
		#endregion

	}
}
