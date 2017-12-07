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
	[EnableCors(origins: "http://localhost:4200,http://127.0.0.1:4200", headers: "*", methods: "*")]
	[RoutePrefix("api/image")]
	public class ImageController: ApiController
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
							var result = new AnalysisResult()
							{
								Message = "Analysis successful"
							};
							result.Tags.Add("Fotboll");
							result.Tags.Add("Sport");

							var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/soccer");
							DirectoryInfo di = new DirectoryInfo(path);
							var files = di.GetFiles();
							foreach (var file in files)
							{
								result.Images.Add("/Images/soccer/" + file.Name);
							}

							return Ok(result);
						}
						else
							if (hpf.FileName.ToLower().Contains("hockey"))
						{
							var result = new AnalysisResult()
							{
								Message = "Analysis successful"
							};
							result.Tags.Add("Ishockey");
							result.Tags.Add("Sport");
							result.Tags.Add("Is");

							var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/hockey");
							DirectoryInfo di = new DirectoryInfo(path);
							var files = di.GetFiles();
							foreach (var file in files)
							{
								result.Images.Add("/Images/hockey/" + file.Name);
							}
							return Ok(result);
						}

						// SAVE THE FILES IN THE FOLDER.
						hpf.SaveAs(sPath + Path.GetFileName(hpf.FileName));
						iUploadedCnt = iUploadedCnt + 1;
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
	}
}