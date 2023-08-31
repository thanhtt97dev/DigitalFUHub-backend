using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Utilities;

namespace ServerAPI.Controllers
{
    [Route("api/export")]
    [ApiController]
    public class ExportReportController : ControllerBase
    {
        private readonly ReportService _exportService;
 

        public ExportReportController(ReportService exportService)
        {
            _exportService = exportService;
        }

        #region Get report user (sample)
        [HttpPost("user")]
        public IActionResult ExportUser (int id)
        {
            try
            {

                var fileContents = _exportService.ExportUserToExcel(id);

                if (fileContents == null || fileContents.Length == 0)
                    return NotFound("Unable to create Excel file");

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",  "export_user.xlsx");

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
		#endregion

	}
}
