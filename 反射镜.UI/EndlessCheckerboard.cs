using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace 反射镜.UI {
	public class EndlessCheckerboard : Checkerboard {
		static EndlessCheckerboard() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EndlessCheckerboard), new FrameworkPropertyMetadata(typeof(EndlessCheckerboard)));
		}

		public EndlessCheckerboard() {
			Loaded += delegate {
				Columns = 8;
				Rows = 7;
				var gameMap = (GameEditor.GetEditor(this)).EditorCheckerboard.GameMap;

				AddCell(0, 0, new LightSource(LightColor.Red));
				AddCell(1, 0, new LightSource(LightColor.Green));
				AddCell(2, 0, new LightSource(LightColor.Blue));
				AddCell(3, 0, new LightSource(LightColor.Yellow));
				AddCell(4, 0, new LightSource(LightColor.Magenta));
				AddCell(5, 0, new LightSource(LightColor.Cyan));
				AddCell(6, 0, new LightSource(LightColor.White));

				AddCell(0, 1, new TargetLight(LightColor.Red));
				AddCell(1, 1, new TargetLight(LightColor.Green));
				AddCell(2, 1, new TargetLight(LightColor.Blue));
				AddCell(3, 1, new TargetLight(LightColor.Yellow));
				AddCell(4, 1, new TargetLight(LightColor.Magenta));
				AddCell(5, 1, new TargetLight(LightColor.Cyan));
				AddCell(6, 1, new TargetLight(LightColor.White));
				AddCell(7, 1, new BlackLight());

				AddCell(0, 2, new ColorSplitting(LightColor.Red));
				AddCell(1, 2, new ColorSplitting(LightColor.Green));
				AddCell(2, 2, new ColorSplitting(LightColor.Blue));
				AddCell(3, 2, new ColorSplitting(LightColor.Yellow));
				AddCell(4, 2, new ColorSplitting(LightColor.Magenta));
				AddCell(5, 2, new ColorSplitting(LightColor.Cyan));

				AddCell(0, 3, new Wormhole(LightColor.Red, gameMap));
				AddCell(1, 3, new Wormhole(LightColor.Green, gameMap));
				AddCell(2, 3, new Wormhole(LightColor.Blue, gameMap));
				AddCell(3, 3, new Wormhole(LightColor.Yellow, gameMap));
				AddCell(4, 3, new Wormhole(LightColor.Magenta, gameMap));
				AddCell(5, 3, new Wormhole(LightColor.Cyan, gameMap));
				AddCell(6, 3, new Wormhole(LightColor.White, gameMap));

				AddCell(0, 4, new Reflection());
				AddCell(1, 4, new BeveledReflection());
				AddCell(2, 4, new Splitting());
				AddCell(3, 4, new Discolor());
				AddCell(4, 4, new Quantum());
				AddCell(5, 4, new Prism());
				AddCell(6, 4, new OneWay());
				AddCell(7, 4, new TwoWay());

				AddCell(0, 5, new Xor());
				AddCell(1, 5, new And());
				AddCell(2, 5, new Or());
				AddCell(3, 5, new Filter(LightColor.Red));
				AddCell(4, 5, new Filter(LightColor.Green));
				AddCell(5, 5, new Filter(LightColor.Blue));
				AddCell(6, 5, new Obstacle());
				AddCell(7, 5, new Blockade());
			};
		}

		private void AddCell(int col, int row, Cell cell) {
			var mirror = new EditorMirror() { Cell = cell };
			GameEditor.SetEditor(mirror, VisualTreeHelper.GetParent(this) as GameEditor);
			SetColumn(mirror, col);
			SetRow(mirror, row);
			bool isCancel = false;

			mirror.BeginDrag += (sender, e) => {
				if (e.OldCheckerboard == this) {
					isCancel = false;
					AddCell(col, row, mirror.Cell.Clone() as Cell);
				}
			};
			mirror.PreviewEndDrag += (sender, e) => {
				if (e.NewCheckerboard == this) {
					e.Cancel = true;
				}
				if (e.Cancel && e.OldCheckerboard == this) {
					isCancel = true;
				}
			};
			mirror.EndDrag += (sender, e) => {
				if (isCancel) {
					e.OldCheckerboard.Children.Remove(mirror);
				}
			};

			Children.Add(mirror);
		}
	}
}
