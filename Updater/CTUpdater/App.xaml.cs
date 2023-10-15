using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;

namespace CTUpdater
{
	// Token: 0x02000002 RID: 2
	public partial class App : Application
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow mainWindow = new MainWindow();
			if (e.Args.Length == 2)
			{
				mainWindow.g_argGame = e.Args[0];
				mainWindow.g_argDll = e.Args[1];
			}
			mainWindow.Show();
		}
	}
}
