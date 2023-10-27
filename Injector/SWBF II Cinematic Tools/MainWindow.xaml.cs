using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace SWBF_II_Cinematic_Tools
{
	// Token: 0x02000002 RID: 2
	public partial class MainWindow : Window
	{
		// Token: 0x06000001 RID: 1
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

		// Token: 0x06000002 RID: 2
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		// Token: 0x06000003 RID: 3
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		// Token: 0x06000004 RID: 4
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

		// Token: 0x06000005 RID: 5
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

		// Token: 0x06000006 RID: 6
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		// Token: 0x06000007 RID: 7
		[DllImport("kernel32.dll")]
		private static extern int CloseHandle(IntPtr hObject);

		private static readonly IntPtr IntptrZero = (IntPtr)0;
		private static string g_shortName = "SWBF2.patched";
		private static string g_dllPath = "CT_SWBF2.patched.dll";
		private static string g_targetExe = "starwarsbattlefrontii";
		private static bool g_hasTrial = true;
		private static string g_targetExeTrial = "starwarsbattlefrontii_trial";
		private BackgroundWorker bgWorker = new BackgroundWorker();
		private bool isInjected;
		private const string c_dlUrl = "https://github.com/MagixGames/SWBFII-Cinematic-Tools-Patch/releases/latest/download/updateinfo.txt";


		// Token: 0x06000008 RID: 8 RVA: 0x00002048 File Offset: 0x00000248
		private void Inject()
		{
			uint num = 0U;
			Process[] processes = Process.GetProcesses();
			this.SetStatus("Waiting for " + MainWindow.g_targetExe + ".exe");
			string exeDirPath = "";
			while (num == 0U)
			{
				for (int i = 0; i < processes.Length; i++)
				{
					if (processes[i].ProcessName == MainWindow.g_targetExe || (MainWindow.g_hasTrial && processes[i].ProcessName == MainWindow.g_targetExeTrial))
					{
						num = (uint)processes[i].Id;
						exeDirPath = Path.GetDirectoryName(processes[i].MainModule.FileName);
					}
				}
				Thread.Sleep(1000);
                processes = Process.GetProcesses();
            }
			this.SetStatus(MainWindow.g_targetExe + ".exe found");
            Thread.Sleep(500);
			if (!Directory.Exists(exeDirPath + "/Cinematic Tools/"))
			{
				Directory.CreateDirectory(exeDirPath + "/Cinematic Tools");
            }
            if (!File.Exists(exeDirPath + "/Cinematic Tools/CT.log"))
            {
                File.Create(exeDirPath + "/Cinematic Tools/CT.log");
            }
            if (!File.Exists(exeDirPath + "/Cinematic Tools/Offsets.log"))
            {
                File.Create(exeDirPath + "/Cinematic Tools/Offsets.log");
            }
            Thread.Sleep(500);
            string fullPath = Path.GetFullPath(MainWindow.g_dllPath);
			IntPtr intPtr = MainWindow.OpenProcess(1082U, 1, num);
			if (intPtr == MainWindow.IntptrZero)
			{
				MessageBox.Show("OpenProcess failed. GetLastError " + Marshal.GetLastWin32Error(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}

			foreach (ProcessModule module in Process.GetProcessById((int)num).Modules)
			{
				if (module.FileName == fullPath)
				{
					this.isInjected = true;
					return;
				}
			}

			IntPtr procAddress = MainWindow.GetProcAddress(MainWindow.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
			if (procAddress == MainWindow.IntptrZero)
			{
				MessageBox.Show("GetProcAddress failed. GetLastError " + Marshal.GetLastWin32Error(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			IntPtr intPtr2 = MainWindow.VirtualAllocEx(intPtr, (IntPtr)null, (IntPtr)fullPath.Length, 12288U, 64U);
			if (intPtr2 == MainWindow.IntptrZero)
			{
				MessageBox.Show("VirtualAllocEx failed. GetLastError " + Marshal.GetLastWin32Error(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(fullPath);
			if (MainWindow.WriteProcessMemory(intPtr, intPtr2, bytes, (uint)bytes.Length, 0) == 0)
			{
				MessageBox.Show("WriteProcessMemory failed. GetLastError " + Marshal.GetLastWin32Error(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			Thread.Sleep(100);
			if (MainWindow.CreateRemoteThread(intPtr, (IntPtr)null, MainWindow.IntptrZero, procAddress, intPtr2, 0U, (IntPtr)null) == MainWindow.IntptrZero)
			{
				MessageBox.Show("CreateRemoteThread failed. GetLastError " + Marshal.GetLastWin32Error(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			MainWindow.CloseHandle(intPtr);
			Thread.Sleep(1000);

			foreach (ProcessModule module in Process.GetProcessById((int)num).Modules)
			{
				if (module.FileName == fullPath)
				{
					this.isInjected = true;
					return;
				}
			}
			
			this.isInjected = false;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002360 File Offset: 0x00000560
		private void CheckUpdates()
		{
			// For now lets skip this func entirely
			//return;

			uint timeDateStamp = new PeFileHeaderReader(MainWindow.g_dllPath).FileHeader.TimeDateStamp;
			uint num = 0U;
			WebClient webClient = new WebClient();
			StreamReader streamReader;
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
				streamReader = new StreamReader(webClient.OpenRead(c_dlUrl));
				Thread.Sleep(50);
				goto IL_98;
			}
			catch (WebException ex)
			{
				MessageBox.Show("Unable to check updates.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			IL_56:
			string[] array = streamReader.ReadLine().Split(' ');
			if (array.Length >= 3 && !(array[0] != MainWindow.g_shortName))
			{
				uint.TryParse(array[1], out num);
				string text = array[2];
				goto IL_A1;
			}
			IL_98:
			if (streamReader.Peek() >= 0)
			{
				goto IL_56;
			}
			IL_A1:
			streamReader.Close();
			webClient.Dispose();
			if (num == 0U)
			{
				MessageBox.Show("Unable to check updates.\nCould not find latest version.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			if (num > timeDateStamp && MessageBox.Show("There is a new version available. Update now?", "Updates available", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
			{
				Process.Start("Updater.exe", MainWindow.g_shortName + " " + MainWindow.g_dllPath);
				this.Shutdown();
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002480 File Offset: 0x00000680
		public MainWindow()
		{
			this.InitializeComponent();
			this.bgWorker.DoWork += this.BgWorker_DoWork;
			this.bgWorker.RunWorkerAsync();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000024BC File Offset: 0x000006BC
		private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			Thread.Sleep(1000);
			if (File.Exists("UpdaterNew.exe"))
			{
				File.Delete("Updater.exe");
				File.Move("UpdaterNew.exe", "Updater.exe");
			}
			if (!File.Exists(MainWindow.g_dllPath))
			{
				MessageBox.Show(MainWindow.g_dllPath + " could not be found!\nPlease make sure you have exported all the files in the same folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				this.Shutdown();
			}
			if (File.Exists("CT_SWBF2_D.dll"))
			{
				MainWindow.g_dllPath = "CT_SWBF2_D.dll";
			}
			this.CheckUpdates();
			this.Inject();
			if (!this.isInjected)
			{
				MessageBox.Show(MainWindow.g_dllPath + " could not be injected!\nMake sure your anti-virus software isn't blocking the tools and make an exception if necessary.\nYou can also try running the program with administrator privileges", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			this.Shutdown();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002574 File Offset: 0x00000774
		private void SetStatus(string status)
		{
			base.Dispatcher.BeginInvoke(new Action(delegate()
			{
				this.statusLabel.Content = status;
			}), new object[0]);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000025B3 File Offset: 0x000007B3
		private void Shutdown()
		{
			base.Dispatcher.BeginInvoke(new Action(delegate()
			{
				Application.Current.Shutdown();
			}), new object[0]);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000025E6 File Offset: 0x000007E6
		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				base.DragMove();
			}
		}
	}
}
