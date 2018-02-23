using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace 反射镜.Editor {
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application {
		void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			//if (e.Exception is COMException comException && comException.ErrorCode == unchecked((int)0x800401D0))///OpenClipboard HRESULT:0x800401D0 (CLIPBRD_E_CANT_OPEN))
			//	e.Handled = true;
			MessageBox.Show(e.Exception.Message, "未知错误", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}
	}
}
