using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using 反射镜.UI;

namespace 反射镜.Play {
	public class PlayCommands {
		public DelegateCommand<GameView> EncodeToClipboard => new DelegateCommand<GameView>(view => {
			try {
				var data = view.EncodeMap();
				Clipboard.SetText(Convert.ToBase64String(data), TextDataFormat.Text);
			} catch (COMException e) when (e.ErrorCode == unchecked((int)0x800401D0)) {
			} catch (Exception e) {
				MessageBox.Show(e.Message, "编码错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});

		public DelegateCommand<GameView> DecodeFromClipboard => new DelegateCommand<GameView>(view => {
			try {
				if (view.PlayCheckerboard.Children.Count > 0) {
					var result = MessageBox.Show("这会破坏当前的游戏进度，是否继续？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (result == MessageBoxResult.No) return;
				}
				var data = Convert.FromBase64String(Clipboard.GetText(TextDataFormat.Text));
				view.DecodeMap(data);
			} catch (Exception e) {
				MessageBox.Show($@"这不是有效的咒语。
“我阅读了无数的魔法书，从来没见过这种咒语。” —— 匿名魔法师

{e.Message}", "解码错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});

		public DelegateCommand<MainWindow> NextMission => new DelegateCommand<MainWindow>(main => {
			main.NextMission();
		});

		public DelegateCommand<MainWindow> PrevMission => new DelegateCommand<MainWindow>(main => {
			main.PrevMission();
		});
	}
}
