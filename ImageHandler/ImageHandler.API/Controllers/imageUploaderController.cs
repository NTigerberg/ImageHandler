using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ImageHandler.API.Controllers
{
	[EnableCors(origins: "http://localhost:4200,http://127.0.0.1:4200", headers: "*", methods: "*")]
	[RoutePrefix("api/imageUploader")]
	public class imageUploaderController
	{
		/// <summary>
		/// Get user applications
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("test")]
		public HttpResponseMessage UserApplications()
		{
			return Request.CreateResponse(HttpStatusCode.OK, "Test");
		}
	}
}