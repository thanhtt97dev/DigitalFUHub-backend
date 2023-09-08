using DataAccess.IRepositories;
using DataAccess.Repositories;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportRepository _reportRepository;

        public ReportsController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;   
        }

        #region Get report user (sample)
        [Authorize]
        [HttpPost("user")]
        public async Task<IActionResult> UserInfo ([FromBody] UserReportRequestDTO userReportRequestDTO)
        {
            try
            {
				var fileContents = await _reportRepository.GetReportUserInfoToExcel(userReportRequestDTO.Id);

				if (fileContents == null || fileContents.Length == 0) return NotFound();

				return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user_report.xlsx");
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
		#endregion

	}
}
