using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using 反射镜.UI;

namespace 反射镜.Editor {
	public class EditorCommands {
		public DelegateCommand<EditorMirror> Fixed => new DelegateCommand<EditorMirror>(mirror => {
			mirror.Freedom = false;
		});

		public DelegateCommand<EditorMirror> CancelFixed => new DelegateCommand<EditorMirror>(mirror => {
			mirror.Freedom = true;
		});

		public DelegateCommand<EditorMirror> RotateDirection315 => new DelegateCommand<EditorMirror>(mirror => {
			mirror.RotateDirection(Direction._315);
		}, mirror => mirror.Cell.CanRotate && mirror.Freedom);

		public DelegateCommand<EditorMirror> RotateDirection180 => new DelegateCommand<EditorMirror>(mirror => {
			mirror.RotateDirection(Direction._180);
		}, mirror => mirror.Cell.CanRotate && mirror.Freedom);

		public DelegateCommand<EditorMirror> ResetDirection => new DelegateCommand<EditorMirror>(mirror => {
			mirror.ResetDirection();
		}, mirror => mirror.Cell.CanRotate && mirror.Freedom);

		public DelegateCommand<EditorMirror> AllTypeFixed => new DelegateCommand<EditorMirror>(mirror => {
			if (GameEditor.GetEditor(mirror) is GameEditor editor && editor.EditorCheckerboard is EditorCheckerboard checkerboard) {
				using (checkerboard.FreezeMap()) {
					var type = mirror.Cell.GetType();
					foreach (EditorMirror child in checkerboard.Children) {
						if (child.Cell.GetType() != type) continue;
						child.Freedom = false;
					}
				}
			}
		});

		public DelegateCommand<EditorMirror> AllTypeCancelFixed => new DelegateCommand<EditorMirror>(mirror => {
			if (GameEditor.GetEditor(mirror) is GameEditor editor && editor.EditorCheckerboard is EditorCheckerboard checkerboard) {
				using (checkerboard.FreezeMap()) {
					var type = mirror.Cell.GetType();
					foreach (EditorMirror child in checkerboard.Children) {
						if (child.Cell.GetType() != type) continue;
						child.Freedom = true;
					}
				}
			}
		});

		public DelegateCommand<EditorMirror> AllTypeSelect => new DelegateCommand<EditorMirror>(mirror => {
			if (GameEditor.GetEditor(mirror) is GameEditor editor && editor.EditorCheckerboard is EditorCheckerboard checkerboard) {
				var type = mirror.Cell.GetType();
				editor.Select(checkerboard.Children.OfType<EditorMirror>().Where(m => m.Cell.GetType() == type));
			}
		});


		public DelegateCommand<EditorCheckerboard> SelectedFixed => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				var @fixed = Fixed;
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (@fixed.CanExecute(mirror)) @fixed.Execute(mirror);
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedCancelFixed => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				var cancelFixed = CancelFixed;
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (cancelFixed.CanExecute(mirror)) cancelFixed.Execute(mirror);
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedRotateDirection45 => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (mirror.Freedom && mirror.Cell.CanRotate) {
							mirror.RotateDirection(Direction._45);
						}
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedRotateDirection315 => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				var rotate = RotateDirection315;
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (rotate.CanExecute(mirror)) {
							rotate.Execute(mirror);
						}
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedRotateDirection180 => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				var rotate = RotateDirection180;
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (rotate.CanExecute(mirror)) {
							rotate.Execute(mirror);
						}
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedResetDirection => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				var rotate = ResetDirection;
				foreach (EditorMirror mirror in checkerboard.Children) {
					if (mirror.IsSelected) {
						if (rotate.CanExecute(mirror)) {
							rotate.Execute(mirror);
						}
					}
				}
			}
		});

		public DelegateCommand<EditorCheckerboard> SelectedRemoveAll => new DelegateCommand<EditorCheckerboard>(checkerboard => {
			using (checkerboard.FreezeMap()) {
				foreach (var mirror in checkerboard.Children.OfType<EditorMirror>().Where(m => m.IsSelected).ToArray()) {
					checkerboard.Children.Remove(mirror);
				}
			}
		});


		public DelegateCommand<EditorMirror> BrushMode => new DelegateCommand<EditorMirror>(mirror => {
			if (GameEditor.GetEditor(mirror) is GameEditor editor) {
				editor.BrushMode = true;
				editor.MirrorBrush = mirror.Clone() as EditorMirror;
			}
		});

		public DelegateCommand<GameEditor> EncodeToClipboardForEdit => new DelegateCommand<GameEditor>(editor => {
			try {
				var data = editor.EditorCheckerboard.EncodeToBytes();
				Clipboard.SetText(Convert.ToBase64String(data), TextDataFormat.Text);
			} catch (COMException e) when (e.ErrorCode == unchecked((int)0x800401D0)) {
			} catch (Exception e) {
				MessageBox.Show(e.Message, "编码错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});

		public DelegateCommand<GameEditor> EncodeToClipboardForPlay => new DelegateCommand<GameEditor>(editor => {
			try {
				var cloneMap = editor.EditorCheckerboard.GameMap.Clone();
				var freedomCells = new List<Cell>();
				foreach (var cell in cloneMap.ForEachCell()) {
					if (cell.Cell != null && cell.Cell.Freedom) {
						freedomCells.Add(cell.Cell);
						cell.Cell = null;
					}
				}
				var data = cloneMap.Encode(freedomCells);
				Clipboard.SetText(Convert.ToBase64String(data), TextDataFormat.Text);
			} catch (COMException e) when (e.ErrorCode == unchecked((int)0x800401D0)) {
			} catch (Exception e) {
				MessageBox.Show(e.Message, "编码错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});

		public DelegateCommand<GameEditor> DecodeFromClipboard => new DelegateCommand<GameEditor>(editor => {
			try {
				var data = Convert.FromBase64String(Clipboard.GetText(TextDataFormat.Text));
				if (editor.EditorCheckerboard.Children.Count > 0) {
					var result = MessageBox.Show("确定使用新咒语？注意：这会覆盖当前的编辑！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
					if (result == MessageBoxResult.No) return;
				}
				editor.EditorCheckerboard.DecodeFromBytes(data);
			} catch (Exception e) {
				MessageBox.Show($@"这不是有效的咒语。
“我阅读了无数的魔法书，从来没见过这种咒语。” —— 匿名魔法师

{e.Message}", "解码错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});
	}
}
