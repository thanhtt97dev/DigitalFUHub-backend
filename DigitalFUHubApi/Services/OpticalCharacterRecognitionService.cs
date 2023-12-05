using Comons;
using System.Drawing;
using System.Reflection.Metadata;
using Tesseract;

namespace DigitalFUHubApi.Services
{
	public class OpticalCharacterRecognitionService
	{
		public OpticalCharacterRecognitionService() { }	

		public void ClarifyCaptchaImage(string base64)
		{
//#pragma warning disable CA1416 //bitmap just support for windown
			byte[] bytes = System.Convert.FromBase64String(base64);

			Bitmap image;
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				image = new Bitmap(ms);
			}

			if (image == null) return;

			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					var pixel = image.GetPixel(x, y);
					if (pixel.R < 112 && pixel.G < 112 && pixel.B < 112)
					{
						System.Drawing.Color black = System.Drawing.Color.FromArgb(255, 0, 0, 0);
						image.SetPixel(x, y, black);
					}
					else
					{
						System.Drawing.Color white = System.Drawing.Color.FromArgb(255, 255, 255, 255);
						image.SetPixel(x, y, white);
					}
				}
			}
			string path = Constants.CAPTCHA_IMAGE_FILE_NAME;
			image.Save(path);
//#pragma warning restore CA1416 // 
		}

		public string GetCaptchaInImage()
		{
			string path = Constants.CAPTCHA_IMAGE_FILE_NAME;
			Pix pix = Pix.LoadFromFile(path);

			var orc = new Tesseract.TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndLstm);
			var page = orc.Process(pix);

			return page.GetText();
		}
	}
}
