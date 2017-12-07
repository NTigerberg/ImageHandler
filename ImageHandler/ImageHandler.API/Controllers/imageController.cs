using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
		public string Upload()
		{
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
						// SAVE THE FILES IN THE FOLDER.
						hpf.SaveAs(sPath + Path.GetFileName(hpf.FileName));
						iUploadedCnt = iUploadedCnt + 1;
					}
				}
			}

			// RETURN A MESSAGE (OPTIONAL).
			if (iUploadedCnt > 0)
			{
				return iUploadedCnt + " Files Uploaded Successfully";
			}
			else
			{
				return "Upload Failed";
			}
		}
	}
}