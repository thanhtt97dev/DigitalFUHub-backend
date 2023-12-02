using BusinessObject.Entities;
using Comons;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		[Authorize(Roles = "Admin")]
		[HttpGet("getWithdrawBuyListMbBank")]
		public IActionResult GetWithdrawBuyListMbBank()
		{
			try
			{
				var file = Util.GetFile(Constants.MB_BANK_TRANFER_BY_LIST_EXCEL_FILE_PATH);
				if(file == null) return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Faild", true, new {}));
				return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Constants.MB_BANK_TRANFER_BY_LIST_EXCEL_FILE_NAME);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("OrderReportFile")]
		public IActionResult GetOrder()
		{
			try
			{
				var file = Util.GetFile(Constants.ORDER_REPORT_EXCEL_FILE_PATH);
				if (file == null) return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Faild", true, new { }));
				return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Constants.ORDER_REPORT_EXCEL_FILE_NAME);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("WithdrawTransactionReportFile")]
		public IActionResult GetWithdrawTransactionReportFile()
		{
			try
			{
				var file = Util.GetFile(Constants.WITHDRAW_TRANSACTION_EXCEL_FILE_PATH);
				if (file == null) return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Faild", true, new { }));
				return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Constants.WITHDRAW_TRANSACTION_REPORT_EXCEL_FILE_NAME);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}


		[Authorize(Roles = "Admin")]
		[HttpGet("TransactionInternalReportFile")]
		public IActionResult GeTransactionInternalReportFile()
		{
			try
			{
				var file = Util.GetFile(Constants.TRANSACTION_INTERNAL_FILE_PATH);
				if (file == null) return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Faild", true, new { }));
				return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Constants.TRANSACTION_INTERNAL_REPORT_EXCEL_FILE_NAME);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
	}
}
