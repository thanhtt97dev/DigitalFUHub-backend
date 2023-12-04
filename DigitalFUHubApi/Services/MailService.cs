using BusinessObject.Entities;
using Comons;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Text;
using static QRCoder.PayloadGenerator;

namespace DigitalFUHubApi.Services
{
	public class MailService
	{
		private readonly IConfiguration configuration;

		public MailService(IConfiguration _configuration)
		{
			configuration = _configuration;
		}

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			string? mail = configuration["SendMail:Email"];
			string? password = configuration["SendMail:Password"];
			if(string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password)) return;

			SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(mail, password),
			};

			MailMessage mailMessage = new MailMessage(from: mail, to: email);
			mailMessage.Subject = subject;
			mailMessage.Body = message;
			mailMessage.IsBodyHtml = true;
			await client.SendMailAsync(mailMessage);
		}

		public async Task SendMailWarningSellerViolate(Order order ,int totalOrderViolate)
		{
			var subject = "DigitalFUHub: CẢNH BÁO: ĐƠN HÀNG VI PHẠM";
			var body = 
				$"<p>" +
				$"Chào {order.Shop.User.Fullname},<br/>Tôi là Nguyễn Văn Hiếu, đại diện cho Sàn Thương Mại Điện Tử DigitalFuHub. <br/>" +
				$"Chúng tôi đã phát hiện một đơn hàng từ bạn (Mã Đơn Hàng: {order.OrderId}) vi phạm chính sách của chúng tôi, và điều này làm tăng tổng số lượng đơn hàng vi phạm của bạn lên đến {totalOrderViolate}. <br/> " +
				$"Chúng tôi muốn nhấn mạnh rằng chúng tôi đánh giá cao mối quan hệ đối tác với bạn, nhưng chúng tôi cần sự hợp tác để giải quyết vấn đề này. Nếu số lượng đơn hàng vi phạm của bạn đạt hoặc vượt quá {Constants.MAX_NUMBER_ORDER_CAN_VIOLATES_OF_A_SHOP}, " +
				$"chúng tôi sẽ buộc phải xem xét quyết định khóa shop của bạn. <br/> Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ, xin đừng ngần ngại liên hệ với chúng tôi qua địa chỉ email này. <br/> " +
				$" Chúng tôi hy vọng nhận được sự hồi đáp tích cực từ phía bạn để chúng ta có thể giải quyết vấn đề một cách hiệu quả và duy trì mối quan hệ đối tác tích cực. <br/> " +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";

			await SendEmailAsync(order.Shop.User.Email, subject, body);
		}

		public async Task SendMailBanShop(Order order, int totalOrderViolate)
		{
			var subject = $"DigitalFUHub: THÔNG BÁO KHÓA CỦA HÀNG {order.Shop.ShopName}";
			var body =
					$"<h2>Thông Báo: Khóa Shop</h2>" +
					$"<p>Chào ${order.Shop.User.Fullname}," +
					$"Chúng tôi thông báo rằng tài khoản của bạn đã bị khóa do vi phạm chính sách của chúng tôi.<br/>" +
					$"Shop của bạn đã thực hiện bán các đơn hàng vi phạm quá số lần quy định!<br/>" +
					$"Do vậy chúng tôi sẽ thực hiện khóa tài khoản của bạn." +
					$"Số dư trong tài khoản của bạn chúng tôi sẽ dùng để thực hiện khắc phục hậu quả <br/>" +
					$"Số dư còn lại chúng tôi sẽ trả lại bạn trong thời gian sớm nhất!<br/>" +
					$"Trân trọng,<br/>" +
					$"Nguyễn Văn Hiếu <br/>" +
					$"Sàn Thương Mại Điện Tử DigitalFuHub " +
					$"<p/>";

			await SendEmailAsync(order.Shop.User.Email, subject, body);
		}

		public async Task SendMailOrderRefundMoneyForCustomer(Order order)
		{
			var subject = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - HOÀN TIỀN";
			var body = 
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đã Hoàn Tiền</h2>" +
				$"<p> " +
				$"Chào ${order.User.Fullname}, <br/>" +
				$"Xin chúc mừng! Chúng tôi xin thông báo rằng đơn hàng của bạn (Mã Đơn Hàng: #{order.OrderId}) đã được chuyển sang trạng thái \"Đã Hoàn Tiền\" thành công.<br/>" +
				$"Thông tin chi tiết về giao dịch như sau:" +
				$"<ul>" +
				$"<li>Mã Đơn Hàng: {order.OrderId}</li>" +
				$"<li>Ngày Đặt Hàng: {order.OrderDate}</li>" +
				$"<li>Tổng Số Xu Hoàn Trả: {order.TotalCoinDiscount}</li>" +
				$"<li>Tổng Số Tiền Hoàn Trả: {order.TotalPayment}</li>" +
				$"<br>" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.User.Email, subject, body);
		}

		public async Task SendMailRejectComplantForCustomer(Order order)
		{
			var subject = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - HOÀN TIỀN";
			var body =
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đã Hoàn Tiền</h2>" +
				$"<p> " +
				$"Chào ${order.User.Fullname}, <br/>" +
				$"Sau qúa trình điều tra và tìm hiều chúng tôi nhận định rằng đơn hàng này hoàn toàn hợp lệ với chính sách và điều khoản của sàn.<br/>" +
				$"Rất tiếc đơn hàng này chúng tôi không thể hoàn trả tiền lại cho bạn!" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.User.Email, subject, body);
		}

		public async Task SendMailOrderComfirm(Order order)
		{
			//send mail for customer
			var subject = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - XÁC NHẬN";
			var body =
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đã Xác Nhận</h2>" +
				$"<p> " +
				$"Chào ${order.User.Fullname}, <br/>" +
				$"Đơn hàng mã số #{order.OrderId} đã được bạn xác nhận thành công.<br/>" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.User.Email, subject, body);

			//send mail for seller
			var subjectForSeller = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - XÁC NHẬN";
			var bodyForSeller =
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đã Xác Nhận</h2>" +
				$"<p> " +
				$"Chào ${order.Shop.ShopName}, <br/>" +
				$"Đơn hàng mã số #{order.OrderId} đã được khách hàng xác nhận thành công.<br/>" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.Shop.User.Email, subjectForSeller, bodyForSeller);
		}

		public async Task SendMailOrderComplain(Order order)
		{
			//send mail for customer
			var subject = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - KHẾU NẠI";
			var body =
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đang Khiếu nại</h2>" +
				$"<p> " +
				$"Chào ${order.User.Fullname}, <br/>" +
				$"Đơn hàng mã số #{order.OrderId} đã được chuyển sang trạng thái khiếu nại.<br/>" +
				$"Người bán sẽ liên hệ với bạn trong thời gian sớm nhất để giải quyết đơn hàng này cho bạn.<br/>" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.User.Email, subject, body);

			//send mail for seller
			var subjectForSeller = $"DigitalFUHub: ĐƠN HÀNG #{order.OrderId} - KHẾU NẠI";
			var bodyForSeller =
				$" <h2>Thông Báo: Trạng Thái Đơn Hàng Đang Khiếu nại</h2>" +
				$"<p> " +
				$"Chào ${order.Shop.ShopName}, <br/>" +
				$"Đơn hàng mã số #{order.OrderId} đã được khách hàng chuyển sang trạng thái khiếu nại.<br/>" +
				$"Bạn vui lòng liên hệ với khác hàng trong khoảng thời gian sớm nhất.<br/>" +
				$"Nếu có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi<br/>" +
				$"Chân thành cảm ơn sự tin tưởng và sự hợp tác của bạn. Chúng tôi hy vọng sẽ tiếp tục phục vụ bạn trong tương lai.<br/>" +
				$"Trân trọng,<br/>" +
				$"Nguyễn Văn Hiếu <br/>" +
				$"Sàn Thương Mại Điện Tử DigitalFuHub " +
				$"<p/>";
			await SendEmailAsync(order.Shop.User.Email, subjectForSeller, bodyForSeller);
		}
	}
}
