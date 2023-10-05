using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class FileUploadRequestDTO
	{
		public IFormFile FileUpload { get; set; } = null!;
		public int UserId { get;set; }
		public bool IsPublic { get;set; }
	}
}
