using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageHandler.Analyzer;

namespace ImageHandler.API.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var trainer = new Trainer();

			trainer.Train();
		}
	}
}
