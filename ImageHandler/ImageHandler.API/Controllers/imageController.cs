using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ImageHandler.API.Controllers
{
	public class ImageEntity
	{
		public string Name { get; set; }
		public string Url { get; set; }
		public string Category { get; set; }

	}
	public static class ImageDataBase
	{
		public static List<ImageEntity> Database = InitDatabase();

		private static List<ImageEntity> InitDatabase()
		{
			List<ImageEntity> images = new List<ImageEntity>();
			images.AddRange(LoadCategory("soccer"));
			images.AddRange(LoadCategory("hockey"));
			return images;
		}
		private static List<ImageEntity> LoadCategory(string category)
		{
			List<ImageEntity> images = new List<ImageEntity>();
			var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/" + category);
			DirectoryInfo di = new DirectoryInfo(path);
			var files = di.GetFiles();
			foreach (var file in files)
			{
				var url = String.Format("/Images/{0}/{1}", category, file.Name);
				images.Add(new ImageEntity() { Name = file.Name, Category = category, Url = url });
			}
			return images;
		}
		public static void AddImage(ImageEntity image)
		{
			if (Database.Any(i => i.Name == image.Name))
				return;

			Database.Add(image);
		}

		internal static List<ImageEntity> GetImagesByCategory(string category)
		{
			return Database.Where(i => i.Category == category).ToList();
		}
	}



	[EnableCors(origins: "http://localhost:4200,http://127.0.0.1:4200", headers: "*", methods: "*")]
	[RoutePrefix("api/image")]
	public class ImageController : ApiController
	{
		/// <summary>
		/// Get user applications
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Route("upload")]
		public IHttpActionResult Upload()
		{
			//Simulate computation time
			Thread.Sleep(1000);


			int iUploadedCnt = 0;

			// DEFINE THE PATH WHERE WE WANT TO SAVE THE FILES.
			string sPath = "";
			sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/uploadedFiles/");

			System.Web.HttpFileCollection hfc = System.Web.HttpContext.Current.Request.Files;

			// CHECK THE FILE COUNT.
			for (int iCnt = 0; iCnt <= hfc.Count - 1; iCnt++)
			{
				System.Web.HttpPostedFile hpf = hfc[iCnt];

				if (hpf.ContentLength > 0)
				{
					// CHECK IF THE SELECTED FILE(S) ALREADY EXISTS IN FOLDER. (AVOID DUPLICATE)
					if (!File.Exists(sPath + Path.GetFileName(hpf.FileName)))
					{

						//Check name and return result
						if (hpf.FileName.ToLower().Contains("soccer"))
						{
							AnalysisResult result = GetAnalysisResult("soccer", new List<string>() { "Fotboll", "Sport" });

							// SAVE THE FILES IN THE FOLDER.
							//sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/soccer/");
							//hpf.SaveAs(sPath + Path.GetFileName(hpf.FileName));
							//iUploadedCnt = iUploadedCnt + 1;
							//ImageDataBase.AddImage(new ImageEntity() { Name = hpf.FileName, Category = "soccer", Url = String.Format("/Images/{0}/{1}", "soccer", hpf.FileName) });

							return Ok(result);
						}
						else
							if (hpf.FileName.ToLower().Contains("hockey"))
						{
							AnalysisResult result = GetAnalysisResult("hockey", new List<string>() { "Ishockey", "Sport", "Is" });

							//sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/hockey/");
							//hpf.SaveAs(sPath + Path.GetFileName(hpf.FileName));
							//iUploadedCnt = iUploadedCnt + 1;
							//ImageDataBase.AddImage(new ImageEntity() { Name = hpf.FileName, Category = "hockey", Url = String.Format("/Images/{0}/{1}", "hockey", hpf.FileName) });

							return Ok(result);
						}
						else
						{
							return BadRequest("Could not match image");
						}



					}
				}
			}

			// RETURN A MESSAGE (OPTIONAL).
			if (iUploadedCnt > 0)
			{
				return Ok(new AnalysisResult() { Message = iUploadedCnt + " Files Uploaded Successfully" });
			}
			else
			{
				return BadRequest("Upload Failed");
			}
		}

		private static AnalysisResult GetAnalysisResult(string category, List<string> tags)
		{
			var result = new AnalysisResult()
			{
				Message = "Analysis successful"
			};
			result.Tags.AddRange(tags);

			List<ImageEntity> images = ImageDataBase.GetImagesByCategory(category).ToList();
			foreach (var image in images)
			{
				result.Images.Add(image.Url);
			}
			return result;
		}
	}
}