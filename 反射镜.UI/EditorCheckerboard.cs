using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace 反射镜.UI {
	public class EditorCheckerboard : MainCheckerboard {
		static EditorCheckerboard() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EditorCheckerboard), new FrameworkPropertyMetadata(typeof(EditorCheckerboard)));
		}

		public EditorCheckerboard() {
			MouseLeftButtonDown += delegate {
				GameEditor.GetEditor(this)?.TryCancelSelect();
			};
		}

		public override Mirror CreateMirror(Cell cell) {
			var mirror = new EditorMirror { Cell = cell };
			GameEditor.SetEditor(mirror, GameEditor.GetEditor(this));
			return mirror;
		}

		public override IReadOnlyList<Mirror> DecodeFromBytes(byte[] data) {
			var freedomCells = GameMap.Decode(data);
			var freedomIndex = 0;
			using (FreezeMap(true)) {
				Children.Clear();
				for (int y = 0; y < GameMap.Height; y++) {
					for (int x = 0; x < GameMap.Width; x++) {
						if (GameMap.Cells[x, y] is Cell cell) {
							var mirror = CreateMirror(cell);
							mirror.Freedom = cell.Freedom;
							SetColumn(mirror, x);
							SetRow(mirror, y);
							Children.Add(mirror);
						} else if (freedomCells != null && freedomIndex < freedomCells.Count) {
							GameMap.AddCell(x, y, freedomCells[freedomIndex++]);
							var mirror = CreateMirror(GameMap.Cells[x, y]);
							SetColumn(mirror, x);
							SetRow(mirror, y);
							Children.Add(mirror);
						}
					}
				}
			}
			return null;
		}
	}
}
