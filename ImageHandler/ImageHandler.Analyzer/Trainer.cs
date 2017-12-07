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

namespace ImageHandler.Analyzer
{
	public class Trainer
	{
		public void Train()
		{

			SURF surfCPU = new SURF(1000);
			#region CreateTrainSet
			VectorOfKeyPoint modelKeyPoints = new VectorOfKeyPoint();
			Matrix<int> indices;
			Matrix<byte> mask;
			int k = 2;
			double uniquenessThreshold = 0.8;

			//Break this out to read all files


			List<Tuple<string, Mat>> trainingSet = new List<Tuple<string, Mat>>();
			ReadTrainingDataForCategory(surfCPU, modelKeyPoints, trainingSet, "soccer");
			ReadTrainingDataForCategory(surfCPU, modelKeyPoints, trainingSet, "basketball");
			#endregion

			#region CreateVocabulary
			MCvTermCriteria criteria = new MCvTermCriteria();
			BOWKMeansTrainer bowtrainer = new BOWKMeansTrainer(1000, criteria, 3, Emgu.CV.CvEnum.KMeansInitType.PPCenters);
			Mat trainingDescriptors = new Mat();
			foreach (var mat in trainingSet)
			{
				trainingDescriptors.PushBack(mat.Item2);
			}

			bowtrainer.Add(trainingDescriptors);
			Mat vocabulary = new Mat();
			bowtrainer.Cluster(vocabulary);
			#endregion

			//Train
			#region Train
			var positive_data = new Dictionary<string, Mat>();
			var negative_data = new Dictionary<string, Mat>();

			var categories = trainingSet.Select(t => t.Item1).Distinct();

			//Create positives and negatives
			foreach (var category in categories)
			{
				positive_data.Add(category, new Mat());

				if (!negative_data.ContainsKey(category))
					negative_data.Add(category, new Mat());

				foreach (var set in trainingSet.Where(t => t.Item1 == category))
				{
					positive_data[category].PushBack(set.Item2);
				}
				foreach (var set in trainingSet.Where(t => t.Item1 != category))
				{
					if (!negative_data.ContainsKey(set.Item1))
						negative_data.Add(set.Item1, new Mat());

					negative_data[set.Item1].PushBack(set.Item2);
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
				svm.Save(@"svm.txt");
			}


			#endregion


			BFMatcher matcher = new BFMatcher(DistanceType.L2);
			LinearIndexParams ip = new LinearIndexParams();
			SearchParams sp = new SearchParams();
			var descriptorMatcher = new FlannBasedMatcher(ip, sp);


			BOWImgDescriptorExtractor extractor = new BOWImgDescriptorExtractor(surfCPU, descriptorMatcher);
			extractor.SetVocabulary(vocabulary);

			Dictionary<string, Mat> classes_training_data = new Dictionary<string, Mat>();


			//// computing descriptors
			//Ptr<DescriptorExtractor> extractor(new SurfDescriptorExtractor());//  extractor;
			//Mat descriptors;
			//Mat training_descriptors(1,extractor->descriptorSize(),extractor->descriptorType());
			//Mat img;
		}

		private static void ReadTrainingDataForCategory(SURF surfCPU, VectorOfKeyPoint modelKeyPoints, List<Tuple<string, Mat>> trainingSet, string category)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(string.Format(@"C:\TMHackBilder\{0}\filtered", category));
			var directoryFiles = directoryInfo.GetFiles("*.jpg");
			//Mat trainingDescriptors = new Mat();
			int count = 0;
			foreach (var file in directoryFiles)
			{
				using (var modelImage = new Emgu.CV.Image<Emgu.CV.Structure.Gray, byte>(file.FullName))
				{
					Mat modelDescriptors = new Mat();
					surfCPU.DetectAndCompute(modelImage, null, modelKeyPoints, modelDescriptors, false);
					//trainingDescriptors.PushBack(modelDescriptors);
					trainingSet.Add(new Tuple<string, Mat>(category, modelDescriptors));
				}
				count++;
				if (count > 5)
				{
					break;
				}
			}
		}
	}
}
