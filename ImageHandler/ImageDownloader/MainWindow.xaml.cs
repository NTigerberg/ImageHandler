using FlickrNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageDownloader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string key = "Enter key here";
		private string secret = "";

		public MainWindow()
		{
			InitializeComponent();
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = dialog.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					tbDirectoryPath.Text = dialog.SelectedPath;
				}

			}
		}

		private void Download_Click(object sender, RoutedEventArgs e)
		{
			Flickr flickr = new Flickr();
			flickr.ApiKey = key;
			string category = tbCategory.Text;
			int perPage = 100;
			int.TryParse(tbPerPage.Text, out perPage);

			var directoryPath = System.IO.Path.Combine(tbDirectoryPath.Text, category);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
			int iterations = 20;
			int.TryParse(tbPages.Text, out iterations);
			int imageCount = 0;
			int totalCount = iterations * perPage;
			for (int i = 1; i <= iterations; i++)
			{
				var options = new PhotoSearchOptions
				{
					Tags = category,
					PerPage = perPage,
					Page = i,
					Extras = PhotoSearchExtras.LargeUrl | PhotoSearchExtras.Tags
				};

				PhotoCollection photos = flickr.PhotosSearch(options);

				foreach (var photo in photos)
				{
					var url = photo.Medium800Url;

					var photoPath = System.IO.Path.Combine(directoryPath, (new Uri(url)).Segments[2]);
					if (!File.Exists(photoPath))
					{
						Console.WriteLine(String.Format("Downloading file ({0}/{1}): {2} ", ++imageCount, totalCount, url));
						using (WebClient client = new WebClient())
						{
							client.DownloadFile(url, photoPath);
						}
					}
					else
					{
						Console.WriteLine(String.Format("Already exists in folder ({0}/{1}): {2} ", ++imageCount, totalCount, url));
					}
				}
			}

			Console.WriteLine("Finished downloading images");
		}

	}
}
