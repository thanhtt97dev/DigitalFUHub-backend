namespace Comons
{
	public static class Constants
	{

		//MB bank config
		public const string BANK_ID_MB_BANK = "970422";
		public const int NUMBER_DAYS_CAN_UPDATE_BANK_ACCOUNT = 15;
		public const string BANK_TRANSACTION_CODE_KEY = "FU";

		public const string MB_BANK_RESPONE_CODE_SUCCESS = "00";
		public const string MB_BANK_RESPONE_CODE_SESSION_INVALID = "GW200";
		public const string MB_BANK_RESPONE_CODE_SAME_URI_IN_SAME_TIME = "GW485";
		public const string MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND = "MC201";
		public const string MB_BANK_RESPONE_CODE_SEARCH_WITH_TYPE_ERR = "MC231";

		//User config
		public const int ADMIN_USER_ID = 1;
		public const int ADMIN_ROLE = 1;
		public const int CUSTOMER_ROLE = 2;
		public const int SELLER_ROLE = 3;

		//Respone code
		public const string RESPONSE_CODE_SUCCESS = "00";
		public const string RESPONSE_CODE_NOT_ACCEPT = "01";
		public const string RESPONSE_CODE_DATA_NOT_FOUND = "02";
		public const string RESPONSE_CODE_FAILD = "03";
		public const string RESPONSE_CODE_UN_AUTHORIZE = "04";

		public const string RESPONSE_CODE_BANK_WITHDRAW_PAID = "BANK_01";
		public const string RESPONSE_CODE_BANK_WITHDRAW_UNPAY = "BANK_02";
		public const string RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND = "BANK_03";


		//SignalR
		public const string SIGNAL_R_CHAT_HUB = "chat";
		public const string SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE = "ReceiveMessage";


		public const string SIGNAL_R_NOTIFICATION_HUB = "notification";
		public const string SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION = "ReceiveNotification";
		public const string SIGNAL_R_NOTIFICATION_HUB_RECEIVE_ALL_NOTIFICATION = "ReceiveAllNotification";

		//Azure
		public const string AZURE_ROOT_PATH = "https://fptu.blob.core.windows.net";

		// Product Status
		public const int PRODUCT_ACTIVE = 1;
		public const int PRODUCT_BAN = 2;
		public const int PRODUCT_HIDE = 3;

        // Cart Status
        public const string CART_RESPONSE_CODE_SUCCESS = "0";
        public const string CART_RESPONSE_CODE_INVALID_QUANTITY = "1";

        //Order Status
        public const int NUMBER_DAYS_AUTO_CONFIRM_ORDER = 3;
		public const int ORDER_ALL = 0;
		public const int ORDER_WAIT_CONFIRMATION = 1;
		public const int ORDER_CONFIRMED = 2;
		public const int ORDER_COMPLAINT = 3;
		public const int ORDER_SELLER_REFUNDED = 4;
		public const int ORDER_DISPUTE = 5;
		public const int ORDER_REJECT_COMPLAINT = 6;
		public const int ORDER_SELLER_VIOLATES = 7;
		public static int[] ORDER_STATUS = new int[] { ORDER_WAIT_CONFIRMATION, ORDER_CONFIRMED, ORDER_COMPLAINT, ORDER_SELLER_REFUNDED,ORDER_DISPUTE, ORDER_REJECT_COMPLAINT, ORDER_SELLER_VIOLATES };

		//Transaction Type
		public const int TRANSACTION_TYPE_INTERNAL_PAYMENT = 1;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT = 2;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND = 3;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT = 4;
		public static int[] TRANSACTION_STATUS = new int[] { TRANSACTION_TYPE_INTERNAL_PAYMENT, TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT, TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND, TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT};
	}
}
