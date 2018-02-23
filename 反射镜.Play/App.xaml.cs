using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace 反射镜.Play {
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application {
		public static readonly string ConfigFile = "maps.json";

		private bool isStartup = false;

		void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			//if (e.Exception is COMException comException && comException.ErrorCode == unchecked((int)0x800401D0))///OpenClipboard HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))
			//	e.Handled = true;
			MessageBox.Show(e.Exception.Message, "未知错误", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
			if (!isStartup) Current.Shutdown();
		}

		protected override void OnStartup(StartupEventArgs e) {
			MainWindow main = new MainWindow();
			try {
				if (e.Args.Length > 0) {
					if (e.Args[0] == "-user") {
						main.IsBuiltInMission = false;
					} else {
						main.LoadUserMap(e.Args[0]);
					}
				} else if (File.Exists(ConfigFile)) {
					main.Loaded += delegate {
						main.LoadMissions(File.ReadAllText(ConfigFile));
					};
				} else {
					main.IsBuiltInMission = false;
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "启动时错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			main.Show();
			isStartup = true;
		}
	}
}
