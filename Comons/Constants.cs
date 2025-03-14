﻿using System.Net;

namespace Comons
{
	public static class Constants
	{

		//config hosting
		public const string FRONT_END_ENDPOINT_USER = "http://localhost:3000";
		//public const string FRONT_END_ENDPOINT_USER = "https://digitalfuhub.id.vn";

		//MB bank config
		public const string BANK_ID_MB_BANK = "970422";
		public const int NUMBER_DAYS_CAN_UPDATE_BANK_ACCOUNT = 3;
		public const string BANK_TRANSACTION_CODE_KEY = "FU";

		public const string MB_BANK_REQUEST_HEADER_AUTHORIZATION = "Authorization";
		public const string MB_BANK_REQUEST_HEADER_REF_NO = "Refno";
		public const string MB_BANK_REQUEST_HEADER_DEVICE_ID = "Deviceid";

		public const string MB_BANK_RESPONE_CODE_SUCCESS = "00";
		public const string MB_BANK_RESPONE_CODE_SESSION_INVALID = "GW200";
		public const string MB_BANK_RESPONE_CODE_SAME_URI_IN_SAME_TIME = "GW485";
		public const string MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND = "MC201";
		public const string MB_BANK_RESPONE_CODE_NO_SUITABLE_ACCOUNTS = "MC4217";
		public const string MB_BANK_RESPONE_CODE_SEARCH_WITH_TYPE_ERR = "MC231";

		public const string MB_BANK_BASIC_AUTH_USERNAME = "EMBRETAILWEB"; //BasicAuthUserName
		public const string MB_BANK_BASIC_AUTH_PASSWORD = "SD234dfg34%#@FG@34sfsdf45843f"; //BasicAuthPassword
		public const string MB_BANK_BASIC_AUTH_BASE64 = "RU1CUkVUQUlMV0VCOlNEMjM0ZGZnMzQlI0BGR0AzNHNmc2RmNDU4NDNm"; //BasicAuthBase64

		public const string MB_BANK_API_GET_CAPTCHA_IMAGE = "https://online.mbbank.com.vn/api/retail-web-internetbankingms/getCaptchaImage";
		public const string MB_BANK_API_LOGIN = "https://online.mbbank.com.vn/api/retail_web/internetbanking/doLogin";
		public const string MB_BANK_API_HISTORY_TRANSACTION = "https://online.mbbank.com.vn/api/retail-transactionms/transactionms/get-account-transaction-history";
		public const string MB_BANK_API_INQUIRY_ACCOUNT_NAME = "https://online.mbbank.com.vn/api/retail_web/transfer/inquiryAccountName";

		public const string MB_BANK_DIRECTORY_PATH_STORE_ACCOUNT_DATA = "Data/mbBankAccountData.json";
		public const string MB_BANK_DIRECTORY_PATH_STORE_DEPOSIT_DATA = "Data/historyDepositTransactionMbBank.json";
		public const string MB_BANK_DIRECTORY_PATH_STORE_WITHDRAW_DATA = "Data/historyWithdrawTransactionMbBank.json";
		public const string MB_BANK_DIRECTORY_PATH_STORE_TRANSACTION_DATA = "Data/historyTransactionMbBank.json";



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

		public const string RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_INSUFFICIENT_BALANCE = "WITHDRAW_REQUEST_01";
		public const string RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_EXCEEDED_REQUESTS_CREATED = "WITHDRAW_REQUEST_02";
		public const string RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_EXCEEDED_AMOUNT_A_DAY = "WITHDRAW_REQUEST_03";
		public const string RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_CHANGED_BEFORE = "WITHDRAW_REQUEST_04";
		public const string RESPONSE_CODE_BANK_SELLER_REQUEST_WITHDRAW_ACCOUNT_BALLANCE_REQUIRED = "WITHDRAW_REQUEST_04";

		public const string RESPONSE_CODE_BANK_CUSTOMER_REQUEST_DEPOSIT_EXCEEDED_REQUESTS_CREATED = "DEPOSIT_REQUEST_01";

		public const string RESPONSE_CODE_PRODUCT_ACTIVE = "PRODUCT_01";
		public const string RESPONSE_CODE_PRODUCT_BAN = "PRODUCT_02";
		public const string RESPONSE_CODE_PRODUCT_REMOVE = "PRODUCT_03";
		public const string RESPONSE_CODE_PRODUCT_HIDE = "PRODUCT_04";

		public const string RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY = "ORDER_01";
		public const string RESPONSE_CODE_ORDER_COUPON_NOT_EXISTED = "ORDER_02";
		public const string RESPONSE_CODE_ORDER_INSUFFICIENT_BALANCE = "ORDER_03";
		public const string RESPONSE_CODE_ORDER_NOT_ELIGIBLE = "ORDER_04";
		public const string RESPONSE_CODE_ORDER_PRODUCT_VARIANT_NOT_IN_SHOP = "ORDER_05";
		public const string RESPONSE_CODE_ORDER_PRODUCT_HAS_BEEN_BANED = "ORDER_06";
		public const string RESPONSE_CODE_ORDER_CUSTOMER_BUY_THEIR_OWN_PRODUCT = "ORDER_07";
		public const string RESPONSE_CODE_ORDER_COUPON_INVALID_PRODUCT_APPLY = "ORDER_08";
		public const string RESPONSE_CODE_ORDER_SELLER_LOCK_TRANSACTION = "ORDER_09";

		public const string RESPONSE_CODE_CART_SUCCESS = "CART_00";
		public const string RESPONSE_CODE_CART_INVALID_QUANTITY = "CART_01";
		public const string RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY = "CART_02";
		public const string RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP = "CART_03";

		public const string RESPONSE_CODE_RESET_PASSWORD_NOT_CONFIRM = "RS_01";
		public const string RESPONSE_CODE_RESET_PASSWORD_SIGNIN_GOOGLE = "RS_02";
		public const string RESPONSE_CODE_RESET_PASSWORD_ACCOUNT_BANNED = "RS_03";

		public const string RESPONSE_CODE_CONFIRM_PASSWORD_IS_CONFIRMED = "CF_01";

		public const string RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE = "ORDER_STATUS_01";

		public const string RESPONSE_CODE_FEEDBACK_ORDER_UN_COMFIRM = "FEEDBACK_01";

		public const string RESPONSE_CODE_USER_USERNAME_PASSWORD_NOT_ACTIVE = "USER_ACCOUNT_01";
		public const string RESPONSE_CODE_USER_USERNAME_ALREADY_EXISTS = "USER_ACCOUNT_02";
		public const string RESPONSE_CODE_USER_PASSWORD_OLD_INCORRECT = "USER_ACCOUNT_03";

		public const string RESPONSE_CODE_NOT_FEEDBACK_AGAIN = "FB_01";

		public const string RESPONSE_CODE_SHOP_BANNED = "SHOP_01";
		public const string RESPONSE_CODE_BALANCE_NOT_ENOUGH = "SHOP_02";



		//SignalR
		public const string SIGNAL_R_CHAT_HUB = "chatHub";
		public const string SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE = "ReceiveMessage";

		public const string SIGNAL_R_NOTIFICATION_HUB = "notificationHub";
		public const string SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION = "ReceiveNotification";

		public const string SIGNAL_R_USER_ONLINE_STATUS_HUB = "userOnlineStatusHub";
		public const string SIGNAL_R_USER_ONLINE_STATUS_HUB_RECEIVE_ONLINE_STATUS = "ReceiveUserOnlineStatus";

		//User config
		public const string ADMIN_NAME = "Quản trị viên";
		public const int ADMIN_USER_ID = 1;
		public const int ADMIN_ROLE = 1;
		public const int CUSTOMER_ROLE = 2;
		public const int SELLER_ROLE = 3;

		public const int MIN_SELLER_REGISTRATION_FEE = 10000;

		//Azure
		private const string AZURE = "Azure";
		public const string AZURE_COMPUTER_VISION_ENDPOINT = $"{AZURE}:ComputerVisonEndpoint";
		public const string AZURE_COMPUTER_VISION_SUBSCRIPTION_KEY = $"{AZURE}:ComputerVisonSubcriptionKey";
		public const string AZURE_STORAGE_END_POINT = $"{AZURE}:StorageEndPoint";
		public const string AZURE_STORAGE_CONNECTION_STRING = $"{AZURE}:StorageConnectionString";
		public const string AZURE_STORAGE_CONTAINER_NAME = $"{AZURE}:StorageContainerName";

		// Product Status
		public const int PRODUCT_STATUS_ALL = 0;
		public const int PRODUCT_STATUS_ACTIVE = 1;
		public const int PRODUCT_STATUS_BAN = 2;
		public const int PRODUCT_STATUS_REMOVE = 3;
		public const int PRODUCT_STATUS_HIDE = 4;
		public static int[] PRODUCT_STATUS = new int[] { PRODUCT_STATUS_ALL, PRODUCT_STATUS_ACTIVE, PRODUCT_STATUS_BAN, PRODUCT_STATUS_REMOVE, PRODUCT_STATUS_HIDE };

		// Withdraw transaction Status
		public const int WITHDRAW_TRANSACTION_ALL = 0;
		public const int WITHDRAW_TRANSACTION_IN_PROCESSING = 1;
		public const int WITHDRAW_TRANSACTION_PAID = 2;
		public const int WITHDRAW_TRANSACTION_REJECT = 3;
		public const int WITHDRAW_TRANSACTION_CANCEL = 4;
		public static int[] WITHDRAW_TRANSACTION_STATUS = new int[] { WITHDRAW_TRANSACTION_IN_PROCESSING, WITHDRAW_TRANSACTION_PAID, WITHDRAW_TRANSACTION_REJECT, WITHDRAW_TRANSACTION_CANCEL };

		//Order Status
		public const int MAX_NUMBER_ORDER_CAN_VIOLATES_OF_A_SHOP = 3;
		public const int NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER = 3;
		public const int NUMBER_DAYS_AUTO_UPDATE_STATUS_SELLER_REFUNDED_ORDER = NUMBER_DAYS_AUTO_UPDATE_STAUTS_CONFIRM_ORDER + 3;
		public const int ORDER_ALL = 0;
		public const int ORDER_STATUS_WAIT_CONFIRMATION = 1;
		public const int ORDER_STATUS_CONFIRMED = 2;
		public const int ORDER_STATUS_COMPLAINT = 3;
		public const int ORDER_STATUS_SELLER_REFUNDED = 4;
		public const int ORDER_STATUS_DISPUTE = 5;
		public const int ORDER_STATUS_REJECT_COMPLAINT = 6;
		public const int ORDER_STATUS_SELLER_VIOLATES = 7;
		public const int ORDER_STATUS_IN_PROSSESS = 8;
		public const int ORDER_STATUS_FAILED = 9;
		public static int[] ORDER_STATUS = new int[] { ORDER_STATUS_WAIT_CONFIRMATION, ORDER_STATUS_CONFIRMED, ORDER_STATUS_COMPLAINT, ORDER_STATUS_SELLER_REFUNDED, ORDER_STATUS_DISPUTE, ORDER_STATUS_REJECT_COMPLAINT, ORDER_STATUS_SELLER_VIOLATES, ORDER_STATUS_IN_PROSSESS, ORDER_STATUS_FAILED };

		//Transaction internal Type
		public const int TRANSACTION_INTERNAL_TYPE_PAYMENT = 1;
		public const int TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT = 2;
		public const int TRANSACTION_INTERNAL_TYPE_RECEIVE_REFUND = 3;
		public const int TRANSACTION_INTERNAL_TYPE_RECEIVE_PROFIT = 4;
		public const int TRANSACTION_INTERNAL_TYPE_SELLER_REGISTRATION_FEE = 5;
		public static int[] TRANSACTION_INTERNAL_STATUS_TYPE = new int[] { TRANSACTION_INTERNAL_TYPE_PAYMENT, TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT, TRANSACTION_INTERNAL_TYPE_RECEIVE_REFUND, TRANSACTION_INTERNAL_TYPE_RECEIVE_PROFIT, TRANSACTION_INTERNAL_TYPE_SELLER_REGISTRATION_FEE };

		//Transaction coin Type
		public const int TRANSACTION_COIN_TYPE_RECEIVE = 1;
		public const int TRANSACTION_COIN_TYPE_USE = 2;
		public const int TRANSACTION_COIN_TYPE_REFUND = 3;
		public static int[] TRANSACTION_COIN_STATUS_TYPE = new int[] { TRANSACTION_COIN_TYPE_RECEIVE, TRANSACTION_COIN_TYPE_USE, TRANSACTION_COIN_TYPE_REFUND };

		//Message Type
		public const string MESSAGE_TYPE_CONVERSATION_TEXT = "0";
		public const string MESSAGE_TYPE_CONVERSATION_IMAGE = "1";

		public const bool USER_CONVERSATION_TYPE_UN_READ = false;
		public const bool USER_CONVERSATION_TYPE_IS_READ = true;

		//Coupon Type
		public const int COUPON_TYPE_ALL_PRODUCTS = 1;
		public const int COUPON_TYPE_ALL_PRODUCTS_OF_SHOP = 2;
		public const int COUPON_TYPE_SPECIFIC_PRODUCTS = 3;

		// Coupon Status
		public const int COUPON_STATUS_ALL = 0;
		public const int COUPON_STATUS_COMING_SOON = 1;
		public const int COUPON_STATUS_ONGOING = 2;
		public const int COUPON_STATUS_FINISHED = 3;

		//report product status
		public const int REPORT_PRODUCT_STATUS_ALL = 0;
		public const int REPORT_PRODUCT_STATUS_VERIFYING = 1;
		public const int REPORT_PRODUCT_STATUS_REJECT = 2;
		public const int REPORT_PRODUCT_STATUS_ACCEPT = 3;
		public static int[] REPORT_PRODUCT_STATUS = new int[] { REPORT_PRODUCT_STATUS_ALL, REPORT_PRODUCT_STATUS_VERIFYING, REPORT_PRODUCT_STATUS_REJECT, REPORT_PRODUCT_STATUS_ACCEPT };

		//Feedback
		public const int NUMBER_DAYS_CAN_MAKE_FEEDBACK = 7;
		public const int FEEDBACK_TYPE_ALL = 0;
		public const int FEEDBACK_TYPE_1_STAR = 1;
		public const int FEEDBACK_TYPE_2_STAR = 2;
		public const int FEEDBACK_TYPE_3_STAR = 3;
		public const int FEEDBACK_TYPE_4_STAR = 4;
		public const int FEEDBACK_TYPE_5_STAR = 5;
		public const int FEEDBACK_TYPE_HAVE_COMMENT = 6;
		public const int FEEDBACK_TYPE_HAVE_MEDIA = 7;
		public static int[] FEEDBACK_TYPES = new int[] { FEEDBACK_TYPE_ALL, FEEDBACK_TYPE_1_STAR, FEEDBACK_TYPE_2_STAR, FEEDBACK_TYPE_3_STAR, FEEDBACK_TYPE_4_STAR, FEEDBACK_TYPE_5_STAR, FEEDBACK_TYPE_HAVE_COMMENT, FEEDBACK_TYPE_HAVE_MEDIA };

		//URL front-end
		public const string FRONT_END_HISTORY_ORDER_URL = "/history/order/";
		public const string FRONT_END_SELLER_PRODUCT_URL = "/seller/product/";
		public const string FRONT_END_SELLER_ORDER_DETAIL_URL = "/seller/order/";
		public const string FRONT_END_SELLER_DASHBOARD_URL = "/seller/statistic/";
		public const string FRONT_END_ADMIN_ORDER_DETAIL_URL = "/admin/order/";

		// Pagination
		public const int PAGE_SIZE = 10;
		public const int PAGE_SIZE_FEEDBACK = 5;
		public const int PAGE_SIZE_NOTIFICATION = 5;
		public const int PAGE_SIZE_PRODUCT_SHOP_DETAIL_CUSTOMER = 30;
		public const int PAGE_SIZE_PRODUCT_WISH_LIST = 30;
		public const int PAGE_SIZE_PRODUCT_HOME_PAGE = 48;
		public const int PAGE_SIZE_SEARCH_PRODUCT = 20;
		public const int PAGE_SIZE_SEARCH_SHOP = 10;
		public const int PAGE_SIZE_SLIDER = 20;     //ADMIN
		public const int LIMIT_SEARCH_HINT = 10;

		//withdraw, deposit transaction
		public static int[] DEPOSIT_TRANSACTION_STATUS = new int[] { 0, DEPOSIT_TRANSACTION_STATUS_UNPAY, DEPOSIT_TRANSACTION_STATUS_PAIDED, DEPOSIT_TRANSACTION_STATUS_EXPIRED };
		public const int DEPOSIT_TRANSACTION_STATUS_UNPAY = 1;
		public const int DEPOSIT_TRANSACTION_STATUS_PAIDED = 2;
		public const int DEPOSIT_TRANSACTION_STATUS_EXPIRED = 3;

		public const int NUMBER_DAY_DEPOSIT_REQUEST_EXPIRED = 3;
		public const int NUMBER_WITH_DRAW_REQUEST_CAN_MAKE_A_DAY = 20;
		public const int ACCOUNT_BALANCE_REQUIRED_FOR_SELLER = 300000;
		public const int MAX_PRICE_CAN_WITHDRAW = 3000000;
		//public const int MIN_PRICE_CAN_WITHDRAW = 500000;
		public const int MIN_PRICE_CAN_WITHDRAW = 50000;
		public const int NUMBER_DEPOSIT_REQUEST_CAN_MAKE_A_DAY = 100;
		public const int MAX_PRICE_CAN_DEPOSIT = 10000000;
		public const int MIN_PRICE_CAN_DEPOSIT = 10000;

		public const string REGEX_USERNAME_SIGN_UP = "^(?=[a-z])[a-z\\d]{6,12}$";
		public const string REGEX_PASSWORD_SIGN_UP = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,16}$";

		public const long MIN_PERCENT_PRODUCT_VARIANT_DISCOUNT = 0;
		public const int MAX_PERCENT_PRODUCT_VARIANT_DISCOUNT = 50;
		public const long MIN_PRICE_PRODUCT_VARIANT = 1000;
		public const long MAX_PRICE_PRODUCT_VARIANT = 100000000;

		public const string REGEX_COUPON_CODE = "^[a-zA-Z0-9]{4,10}$";
		public const long MIN_PRICE_OF_MIN_ORDER_TOTAL_VALUE = 1000;
		public const long MAX_PRICE_OF_MIN_ORDER_TOTAL_VALUE = 100000000;
		public const long MIN_PRICE_DISCOUNT_COUPON = 1000;
		public const float MAX_PERCENTAGE_PRICE_DISCOUNT_COUPON = 0.7f;
		public const float MAX_PRICE_DISCOUNT_COUPON = 100000000;

		public const int ALL_CATEGORY = 0;
		public const int SORTED_BY_DATETIME = 1;
		public const int SORTED_BY_SALE = 2;
		public const int SORTED_BY_PRICE_ASC = 3;
		public const int SORTED_BY_PRICE_DESC = 4;

		public const int STATISTIC_BY_MONTH = 0;
		public const int STATISTIC_BY_YEAR = 1;

		public const long UPLOAD_FILE_SIZE_LIMIT = 2 * 1024 * 1024;

		public const int STATUS_ALL_SLIDER_FOR_FILTER = 0;
		public const int STATUS_ACTIVE_SLIDER_FOR_FILTER = 1;
		public const int STATUS_UN_ACTIVE_SLIDER_FOR_FILTER = 2;

		//files
		public const string EXECL_FOLDER_PATH = "Files/Excels/";

		public const string MB_BANK_TRANFER_BY_LIST_EXCEL_FILE_NAME = "Chuyenkhoantheobangke.xlsx";
		public const string MB_BANK_TRANFER_BY_LIST_EXCEL_FILE_PATH = $"{EXECL_FOLDER_PATH}{MB_BANK_TRANFER_BY_LIST_EXCEL_FILE_NAME}";

		public const string ORDER_REPORT_EXCEL_FILE_NAME = "OrdersReport.xlsx";
		public const string ORDER_REPORT_EXCEL_FILE_PATH = $"{EXECL_FOLDER_PATH}{ORDER_REPORT_EXCEL_FILE_NAME}";

		public const string WITHDRAW_TRANSACTION_REPORT_EXCEL_FILE_NAME = "WithdrawTransactionReport.xlsx";
		public const string WITHDRAW_TRANSACTION_EXCEL_FILE_PATH = $"{EXECL_FOLDER_PATH}{WITHDRAW_TRANSACTION_REPORT_EXCEL_FILE_NAME}";

		public const string TRANSACTION_INTERNAL_REPORT_EXCEL_FILE_NAME = "TransactionInternalReport.xlsx";
		public const string TRANSACTION_INTERNAL_FILE_PATH = $"{EXECL_FOLDER_PATH}{TRANSACTION_INTERNAL_REPORT_EXCEL_FILE_NAME}";

		public const string TRANSACTION_COIN_REPORT_EXCEL_FILE_NAME = "TransactionCoinReport.xlsx";
		public const string TRANSACTION_COIN_FILE_PATH = $"{EXECL_FOLDER_PATH}{TRANSACTION_COIN_REPORT_EXCEL_FILE_NAME}";

		public const string DEPOSIT_TRANSACTION_REPORT_EXCEL_FILE_NAME = "DepositTransactionReport.xlsx";
		public const string DEPOSIT_TRANSACTION_FILE_PATH = $"Files/Excels/{DEPOSIT_TRANSACTION_REPORT_EXCEL_FILE_NAME}";

		public const string CAPTCHA_IMAGE_FILE_NAME = "captcha.png";

		public enum FinanceTransactionType
		{
			Order = 1,
			Deposit = 2,
			Withdraw = 3,
			RegisterSeller = 4,
		}
	}

}

