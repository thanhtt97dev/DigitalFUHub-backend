using Comons;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Azure;
using System;
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
#pragma warning disable CA1416 //bitmap just support for windown
			byte[] bytes = System.Convert.FromBase64String(base64);

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				using (Bitmap image = new Bitmap(ms))
				{
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
				}
			}
#pragma warning restore CA1416 // 
		}

		public string GetCaptchaInImage()
		{
			using (var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default))
			{
				string path = Constants.CAPTCHA_IMAGE_FILE_NAME;
				Pix pix = Pix.LoadFromFile(path);

				var page = engine.Process(pix);

				return page.GetText();
			}
		}

		public byte[]? GetClarifyCaptchaImage(string base64)
		{
#pragma warning disable CA1416, SC8600 //bitmap just support for windown
			byte[] bytes = System.Convert.FromBase64String(base64);

			using (MemoryStream ms = new MemoryStream(bytes))
			{
				using (Bitmap image = new Bitmap(ms))
				{
					if (image == null) return null;

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

					ImageConverter converter = new ImageConverter();
					return (byte[]?)converter.ConvertTo(image, typeof(byte[]));
				}
			}
#pragma warning restore CA1416, SC8600 // 
		}


		public string GetCaptchaInBase64Image(string base64)
		{
			using (var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default))
			{
				Pix pix = Pix.LoadFromMemory(GetClarifyCaptchaImage(base64));

				var page = engine.Process(pix);

				return page.GetText();
			}
		}
	}
}
