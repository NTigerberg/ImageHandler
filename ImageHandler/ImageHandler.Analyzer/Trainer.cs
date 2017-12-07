using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using System.IO;
using Emgu.CV.Structure;
using Emgu.CV.XFeatures2D;
using Emgu.CV.Flann;
using ImageHandler.Analyzer.Extensions;
using Emgu.CV.ML;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageHandler.Analyzer
{
	public class Trainer
	{
		public void Train()
		{

			//Init
			//**********************************************
			SURF featureDetector = new SURF(1000);
			VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
			
			BFMatcher matcher = new BFMatcher(DistanceType.L2);
			LinearIndexParams ip = new LinearIndexParams();
			SearchParams sp = new SearchParams();
			var descriptorMatcher = new FlannBasedMatcher(ip, sp);
			BOWImgDescriptorExtractor extractor = new BOWImgDescriptorExtractor(featureDetector, descriptorMatcher);
			MCvTermCriteria criteria = new MCvTermCriteria();
			BOWKMeansTrainer bowtrainer = new BOWKMeansTrainer(1000, criteria, 3, Emgu.CV.CvEnum.KMeansInitType.PPCenters);
			//****************************************************

			//Make train set
			//*****************************************************
			List<Tuple<string, Mat>> trainingSet = new List<Tuple<string, Mat>>();
			List<string> categoryNames = new List<string>();
			ReadTrainingDataForCategory(featureDetector, modelKeyPoints, trainingSet, "soccer");
			ReadTrainingDataForCategory(featureDetector, modelKeyPoints, trainingSet, "basketball");
			//**************************************

			#region CreateVocabulary
			//Create vocabulary 
			//*************************************************
			Mat vocabularyDescriptors = new Mat();
			foreach (var mat in trainingSet)
			{
				Mat desc = new Mat();
				featureDetector.DetectAndCompute(mat.Item2, null, modelKeyPoints, desc, false);
				vocabularyDescriptors.PushBack(desc);
			}

			bowtrainer.Add(vocabularyDescriptors);
			Mat vocabulary = new Mat();
			bowtrainer.Cluster(vocabulary);
			//*************************************************
			#endregion


			
			extractor.SetVocabulary(vocabulary);

			//Train
			#region Train
			var positive_data = new Dictionary<string, Mat>();
			var negative_data = new Dictionary<string, Mat>();

			var categories = trainingSet.Select(t => t.Item1).Distinct();

			foreach (var ts in trainingSet)
			{
				var cat = ts.Item1;
				Mat im = ts.Item2;
				Mat feat = new Mat();
				var detected = featureDetector.Detect(im);
				VectorOfKeyPoint kp = new VectorOfKeyPoint(detected);
				extractor.Compute(im, kp, feat);

				//Create positives and negatives
				foreach (var category in categories)
				{
					if (ts.Item1 == category)
					{
						if (!positive_data.ContainsKey(cat))
							positive_data.Add(cat, new Mat());
						positive_data[category].PushBack(feat);
					}
					else
					{
						if (!negative_data.ContainsKey(cat))
							negative_data.Add(cat, new Mat());
						negative_data[cat].PushBack(feat);
					}
				}
			}


			//Train for each category and create SVM
			foreach (var category in categories)
			{
				//Set positives
				var trainData = positive_data[category];
				Mat trainLabels = new Mat(trainData.Rows, 1, Emgu.CV.CvEnum.DepthType.Cv32S, 1);

				for (int i = 0; i < trainLabels.Cols; i++)
				{
					for (int j = 0; j < trainLabels.Rows; j++)
					{
						trainLabels.SetValue(j, i, (int)1);
					}
				}

				trainLabels.SetTo(new MCvScalar(1));


				//Set negatives
				trainData.PushBack(negative_data[category]);
				Mat m = new Mat(negative_data[category].Rows, 1, Emgu.CV.CvEnum.DepthType.Cv32S, 1);
				{
					for (int col = 0; col < m.Cols; col++)
						for (int row = 0; row < m.Rows; row++)
							m.SetValue(row, col, (int)0);
					m.SetTo(new MCvScalar(0));

					trainLabels.PushBack(m);
				}

				SVM svm = new SVM();
				svm.C = 312.5;
				svm.Gamma = 0.50625000000000009;
				svm.SetKernel(SVM.SvmKernelType.Rbf);
				svm.Type = SVM.SvmType.CSvc;

				var success = svm.Train(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainLabels);
				//svm.Save(string.Format(@"svm_{0}.txt", category));
				SaveSVMToFile(svm, string.Format(@"svm_{0}.txt", category));
			}


		
			TestImage(extractor, featureDetector);
			#endregion


			//BFMatcher matcher = new BFMatcher(DistanceType.L2);
			//LinearIndexParams ip = new LinearIndexParams();
			//SearchParams sp = new SearchParams();
			//var descriptorMatcher = new FlannBasedMatcher(ip, sp);


			//BOWImgDescriptorExtractor extractor = new BOWImgDescriptorExtractor(surfCPU, descriptorMatcher);
			//extractor.SetVocabulary(vocabulary);

			//Dictionary<string, Mat> classes_training_data = new Dictionary<string, Mat>();


			//// computing descriptors
			//Ptr<DescriptorExtractor> extractor(new SurfDescriptorExtractor());//  extractor;
			//Mat descriptors;
			//Mat training_descriptors(1,extractor->descriptorSize(),extractor->descriptorType());
			//Mat img;
		}

		public void TestImage(BOWImgDescriptorExtractor extractor, Feature2D featureDetector)
		{
			SURF surfCPU = new SURF(1000);
			List<string> categories = new List<string>();
			categories.Add("soccer");
			categories.Add("basketball");

			foreach (var category in categories)
			{
				var svm = LoadSVMFromFile(string.Format(@"svm_{0}.txt", category));

				using (var srcImage = Image.FromFile(@"testimage2.jpg"))
				{
					var image = ResizeImage(srcImage, 200, 200);
					using (var modelImage = new Emgu.CV.Image<Emgu.CV.Structure.Gray, byte>(image))
					{

						Mat feat = new Mat();
						var detected = featureDetector.Detect(modelImage);
						VectorOfKeyPoint kp = new VectorOfKeyPoint(detected);
						extractor.Compute(modelImage, kp, feat);

						//Mat modelDescriptors = new Mat();
						//VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();

						//surfCPU.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);

						Mat output = new Mat();
						var result = svm.Predict(feat, output);

					}
				}
			}
		}

		public static void SaveSVMToFile(SVM model, String path)
		{
			if (File.Exists(path)) File.Delete(path);
			FileStorage fs = new FileStorage(path, FileStorage.Mode.Write);
			model.Write(fs);
			fs.ReleaseAndGetString();
		}

		public static SVM LoadSVMFromFile(String path)
		{
			SVM svm = new SVM();
			FileStorage fs = new FileStorage(path, FileStorage.Mode.Read);
			svm.Read(fs.GetRoot());
			fs.ReleaseAndGetString();
			return svm;
		}

		private static void ReadTrainingDataForCategory(SURF surfCPU, VectorOfKeyPoint modelKeyPoints, List<Tuple<string, Mat>> trainingSet, string category)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(string.Format(@"C:\TMHackBilder\{0}\filtered", category));
			var directoryFiles = directoryInfo.GetFiles("*.jpg");
			//Mat trainingDescriptors = new Mat();
			int count = 0;
			foreach (var file in directoryFiles)
			{
				using (var srcImage = Image.FromFile(file.FullName))
				{
					var image = ResizeImage(srcImage, 200, 200);
					using (var modelImage = new Emgu.CV.Image<Emgu.CV.Structure.Gray, byte>(image))
					{
						//Mat modelDescriptors = new Mat();
						//surfCPU.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);
						//trainingDescriptors.PushBack(modelDescriptors);
						var mat2 = new Mat();
						modelImage.Mat.ConvertTo(mat2, Emgu.CV.CvEnum.DepthType.Cv8U);
						trainingSet.Add(new Tuple<string, Mat>(category, mat2));
					}
					count++;
					if (count > 5)
					{
						break;
					}
				}
			}
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
	}
}
