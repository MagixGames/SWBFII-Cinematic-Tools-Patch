using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CTUpdater
{
	// Token: 0x02000003 RID: 3
	public partial class MainWindow : Window
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020C0 File Offset: 0x000002C0
		public MainWindow()
		{
			this.InitializeComponent();
			if (this.g_argDll == "")
			{
				string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
				for (int i = 0; i < files.Length; i++)
				{
					string text = files[i].Split(new char[]
					{
						'\\'
					}).Last<string>();
					if (Regex.IsMatch(text, "^CT_.*dll$"))
					{
						this.g_argGame = text.Substring(3, text.Length - 7);
						this.g_argDll = text;
						break;
					}
				}
			}
			if (this.g_argDll == "" || !File.Exists(this.g_argDll))
			{
				MessageBox.Show("Could not find " + this.g_argDll + " to check updates", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				Application.Current.Shutdown();
			}
			this.UpdateWorker.DoWork += this.UpdateWorker_DoWork;
			this.UpdateWorker.RunWorkerAsync();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000021E7 File Offset: 0x000003E7
		private void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			this.CheckUpdate();
			Thread.Sleep(1000);
			this.Shutdown();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002200 File Offset: 0x00000400
		private void SetStatus(string status)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate()
			{
				this.statusLabel.Content = status;
			}), new object[0]);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000223F File Offset: 0x0000043F
		private void Shutdown()
		{
			base.Dispatcher.BeginInvoke(new Action(delegate()
			{
				Application.Current.Shutdown();
			}), new object[0]);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002274 File Offset: 0x00000474
		private void CheckUpdate()
		{
			PeFileHeaderReader peFileHeaderReader = new PeFileHeaderReader(this.g_argDll);
			string zipAddress = "";
			uint timeDateStamp = peFileHeaderReader.FileHeader.TimeDateStamp;
			uint num = 0U;
			this.SetStatus("Checking updates for " + this.g_argGame);
			StreamReader streamReader;
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				streamReader = new StreamReader(this.webClient.OpenRead(c_dlUrl));
				Thread.Sleep(100);
				goto IL_AB;
			}
			catch (WebException ex)
			{
				MessageBox.Show("Unable to check updates.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			IL_6F:
			string[] array = streamReader.ReadLine().Split(new char[]
			{
				' '
			});
			if (!(array[0] != this.g_argGame))
			{
				uint.TryParse(array[1], out num);
				zipAddress = array[2];
				goto IL_B4;
			}
			IL_AB:
			if (streamReader.Peek() >= 0)
			{
				goto IL_6F;
			}
			IL_B4:
			if (num == 0U)
			{
				MessageBox.Show("Unable to find latest version in the version file.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			if (num <= timeDateStamp)
			{
				this.SetStatus("Files up to date");
				return;
			}
			this.FetchUpdate(zipAddress);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002374 File Offset: 0x00000574
		private void FetchUpdate(string zipAddress)
		{
			this.webClient.DownloadProgressChanged += this.WebClient_DownloadProgressChanged;
			this.webClient.DownloadFileCompleted += this.WebClient_DownloadFileCompleted;
			this.webClient.DownloadFileAsync(new Uri(zipAddress), this.g_argGame + ".zip");
			while (!this.downloadCompleted)
			{
				Thread.Sleep(1000);
			}
			this.SetStatus("Download completed");
			Thread.Sleep(1000);
			this.SetStatus("Extracting files");
			Thread.Sleep(1000);
			string text = "";
			ZipArchive zipArchive = ZipFile.OpenRead(this.g_argGame + ".zip");
			try
			{
				foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
				{
					if (zipArchiveEntry.FullName.Contains("Updater"))
					{
						zipArchiveEntry.ExtractToFile("./NewUpdater.exe", true);
					}
					else
					{
						if (zipArchiveEntry.FullName.Contains(".exe"))
						{
							text = zipArchiveEntry.FullName;
						}
						string directoryName = Path.GetDirectoryName(Path.Combine("./", zipArchiveEntry.FullName));
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						if (zipArchiveEntry.Name != "")
						{
							zipArchiveEntry.ExtractToFile("./" + zipArchiveEntry.FullName, true);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			zipArchive.Dispose();
			File.Delete(this.g_argGame + ".zip");
			this.SetStatus("Update successful");
			if (text != "")
			{
				Process.Start(text);
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002548 File Offset: 0x00000748
		private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			this.downloadCompleted = true;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002554 File Offset: 0x00000754
		private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			this.SetStatus("Downloading update (" + e.ProgressPercentage.ToString() + "%)");
		}

		// Token: 0x04000001 RID: 1
		public string g_argGame = "";

		// Token: 0x04000002 RID: 2
		public string g_argDll = "";
		public const string c_dlUrl = "https://github.com/MagixGames/SWBFII-Cinematic-Tools-Patch/releases/latest/download/updateinfo.txt";

		// Token: 0x04000003 RID: 3
		private BackgroundWorker UpdateWorker = new BackgroundWorker();

		// Token: 0x04000004 RID: 4
		private bool downloadCompleted;

		// Token: 0x04000005 RID: 5
		private WebClient webClient = new WebClient();
	}
}
