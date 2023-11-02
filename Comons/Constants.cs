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

		//Respone code
		public const string RESPONSE_CODE_SUCCESS = "00";
		public const string RESPONSE_CODE_NOT_ACCEPT = "01";
		public const string RESPONSE_CODE_DATA_NOT_FOUND = "02";
		public const string RESPONSE_CODE_FAILD = "03";
		public const string RESPONSE_CODE_UN_AUTHORIZE = "04";


		public const string RESPONSE_CODE_BANK_WITHDRAW_PAID = "BANK_01";
		public const string RESPONSE_CODE_BANK_WITHDRAW_UNPAY = "BANK_02";
		public const string RESPONSE_CODE_BANK_WITHDRAW_REJECT = "BANK_03";
		public const string RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND = "BANK_04";

        public const string RESPONSE_CODE_PRODUCT_ACTIVE = "PRODUCT_01";
        public const string RESPONSE_CODE_PRODUCT_BAN = "PRODUCT_02";
        public const string RESPONSE_CODE_PRODUCT_REMOVE = "PRODUCT_03";
        public const string RESPONSE_CODE_PRODUCT_HIDE = "PRODUCT_04";

        public const string RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY = "ORDER_01";
		public const string RESPONSE_CODE_ORDER_COUPON_USED = "ORDER_02";
		public const string RESPONSE_CODE_ORDER_INSUFFICIENT_BALANCE = "ORDER_03";
		public const string RESPONSE_CODE_ORDER_NOT_ELIGIBLE = "ORDER_04";
		public const string RESPONSE_CODE_ORDER_PRODUCT_VARIANT_NOT_IN_SHOP = "ORDER_05";
		public const string RESPONSE_CODE_ORDER_PRODUCT_HAS_BEEN_BANED = "ORDER_06";
		public const string RESPONSE_CODE_ORDER_CUSTOMER_BUY_THEIR_OWN_PRODUCT = "ORDER_07";

		public const string RESPONSE_CODE_CART_SUCCESS = "CART_00";
		public const string RESPONSE_CODE_CART_INVALID_QUANTITY = "CART_01";
		public const string RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY = "CART_02";
		public const string RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP = "CART_03";

		public const string RESPONSE_CODE_RESET_PASSWORD_NOT_CONFIRM = "RS_01";
		public const string RESPONSE_CODE_RESET_PASSWORD_SIGNIN_GOOGLE = "RS_02";

		public const string RESPONSE_CODE_CONFIRM_PASSWORD_IS_CONFIRMED = "CF_01";

		public const string RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE = "ORDER_STATUS_01";

		public const string RESPONSE_CODE_FEEDBACK_ORDER_UN_COMFIRM = "FEEDBACK_01";


		//SignalR
		public const string SIGNAL_R_CHAT_HUB = "chatHub";
		public const string SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE = "ReceiveMessage";

		public const string SIGNAL_R_NOTIFICATION_HUB = "notificationHub";
		public const string SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION = "ReceiveNotification";
		public const string SIGNAL_R_NOTIFICATION_HUB_RECEIVE_ALL_NOTIFICATION = "ReceiveAllNotification";

		public const string SIGNAL_R_USER_ONLINE_STATUS_HUB = "userOnlineStatusHub";
		public const string SIGNAL_R_USER_ONLINE_STATUS_HUB_RECEIVE_ONLINE_STATUS = "ReceiveUserOnlineStatus";

		//User config
		public const int ADMIN_USER_ID = 1;
		public const int ADMIN_ROLE = 1;
		public const int CUSTOMER_ROLE = 2;
		public const int SELLER_ROLE = 3;

		//Azure
		public const string AZURE_ROOT_PATH = "https://fptu.blob.core.windows.net";

		// Product Status
		public const int PRODUCT_STATUS_ACTIVE = 1;
		public const int PRODUCT_STATUS_BAN = 2;
		public const int PRODUCT_STATUS_REMOVE = 3;
		public const int PRODUCT_STATUS_HIDE = 4;

		// Withdraw transaction Status
		public const int WITHDRAW_TRANSACTION_ALL = 0;
		public const int WITHDRAW_TRANSACTION_IN_PROCESSING = 1;
		public const int WITHDRAW_TRANSACTION_PAID = 2;
		public const int WITHDRAW_TRANSACTION_REJECT = 3;
		public static int[] WITHDRAW_TRANSACTION_STATUS = new int[] { WITHDRAW_TRANSACTION_IN_PROCESSING, WITHDRAW_TRANSACTION_PAID, WITHDRAW_TRANSACTION_REJECT };

		//Order Status
		public const int NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER = 3;
		public const int NUMBER_DAYS_AUTO_UPDATE_STATUS_SELLER_REFUNDED_ORDER = NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER + 3;
		public const int ORDER_ALL = 0;
		public const int ORDER_STATUS_WAIT_CONFIRMATION = 1;
		public const int ORDER_STATUS_CONFIRMED = 2;
		public const int ORDER_STATUS_COMPLAINT = 3;
		public const int ORDER_STATUS_SELLER_REFUNDED = 4;
		public const int ORDER_DISPUTE = 5;
		public const int ORDER_STATUS_REJECT_COMPLAINT = 6;
		public const int ORDER_STATUS_SELLER_VIOLATES = 7;
		public static int[] ORDER_STATUS = new int[] { ORDER_STATUS_WAIT_CONFIRMATION, ORDER_STATUS_CONFIRMED, ORDER_STATUS_COMPLAINT, ORDER_STATUS_SELLER_REFUNDED, ORDER_DISPUTE, ORDER_STATUS_REJECT_COMPLAINT, ORDER_STATUS_SELLER_VIOLATES };

		//Transaction internal Type
		public const int TRANSACTION_TYPE_INTERNAL_PAYMENT = 1;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT = 2;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND = 3;
		public const int TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT = 4;
		public static int[] TRANSACTION_STATUS = new int[] { TRANSACTION_TYPE_INTERNAL_PAYMENT, TRANSACTION_TYPE_INTERNAL_RECEIVE_PAYMENT, TRANSACTION_TYPE_INTERNAL_RECEIVE_REFUND, TRANSACTION_TYPE_INTERNAL_RECEIVE_PROFIT };

		//Transaction coin Type
		public const int TRANSACTION_COIN_TYPE_RECEIVE = 1;
		public const int TRANSACTION_COIN_TYPE_USE = 2;
		public const int TRANSACTION_COIN_TYPE_REFUND = 3;

		//Message Type
		public const string MESSAGE_TYPE_CONVERSATION_TEXT = "0";
		public const string MESSAGE_TYPE_CONVERSATION_IMAGE = "1";

		public const int USER_CONVERSATION_TYPE_INITIAL = 1;
		public const int USER_CONVERSATION_TYPE_UN_READ = 2;
		public const int USER_CONVERSATION_TYPE_IS_READ = 3;

		//Coupon Type
		public const int COUPON_TYPE_ALL_PRODUCTS = 1; //admin
		public const int COUPON_TYPE_ALL_PRODUCTS_OF_SHOP = 2;
		public const int COUPON_TYPE_SPECIFIC_PRODUCTS = 3; 

		//Feedback
		public const int NUMBER_DAYS_CAN_MAKE_FEEDBACK = 7;

		//URL front-end
		public const string FRONT_END_HISTORY_ORDER_URL = "/history/order/";
	}
}

