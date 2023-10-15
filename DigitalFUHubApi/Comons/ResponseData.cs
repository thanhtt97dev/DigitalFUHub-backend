namespace DigitalFUHubApi.Comons
{
	public class ResponseData
	{
		public ResponseData()
		{
			Status = new Status();
			Result = new();
		}

		public ResponseData(Status status, object result)
		{
			Status = status;
			Result = result;
		}
		public ResponseData(string responseCode, string message, bool ok, object result)
		{
			Status = new Status { ResponseCode = responseCode, Message = message, Ok = ok };
			Result = result;
		}

		public Status Status { get; set; }
		public Object Result { get; set; }
	}
}
