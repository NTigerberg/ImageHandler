using System.Collections.Generic;

namespace ImageHandler.API.Controllers
{
	public class AnalysisResult
	{
		public string Message { get; set; }
		public List<string> Tags { get; set; }
		public List<string> Images { get; set; }
		public AnalysisResult()
		{
			Tags = new List<string>();
			Images = new List<string>();
		}
	}
}