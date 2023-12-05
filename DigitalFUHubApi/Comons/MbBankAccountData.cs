namespace DigitalFUHubApi.Comons
{
	public static class MbBankAccountData
	{
		public const string AccountNo = "0336687454";
		public const string Password = "0646022851b21cdc6f0317075e771bbf";
		public const string DeviceIdCommon = "xp4czbl7-mbib-0000-0000-2023060214073587";
		public const string RefNo = "0336687454-2023062609580818";
		public static string SessionId {get; set;} = string.Empty;

		public const string BasicAuthUserName = "EMBRETAILWEB";
		public const string BasicAuthPassword = "SD234dfg34%#@FG@34sfsdf45843f";
		public const string BasicAuthBase64 = "RU1CUkVUQUlMV0VCOlNEMjM0ZGZnMzQlI0BGR0AzNHNmc2RmNDU4NDNm";

		public const string ApiGetCaptchaImage = "https://online.mbbank.com.vn/api/retail-web-internetbankingms/getCaptchaImage";
		public const string ApiLogin = "https://online.mbbank.com.vn/api/retail_web/internetbanking/doLogin";
		public const string ApiHistoryTransaction = "https://online.mbbank.com.vn/api/retail-web-transactionservice/transaction/getTransactionAccountHistory";
		public const string ApiInquiryAccountName = "https://online.mbbank.com.vn/api/retail_web/transfer/inquiryAccountName";

		public const string DirectoryPathStoreDepositData = "Data/historyDepositTransactionMbBank.json";
		public const string DirectoryPathStoreWithdrawData = "Data/historyWithdrawTransactionMbBank.json";
		public const string DirectoryPathStoreData = "Data/historyTransactionMbBank.json";
	}
}
