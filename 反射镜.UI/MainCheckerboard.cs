using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using 反射镜;

namespace 反射镜.UI {
	public class MainCheckerboard : Checkerboard {
		static MainCheckerboard() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MainCheckerboard), new FrameworkPropertyMetadata(typeof(MainCheckerboard)));
		}

		public bool IsMissionComplete {
			get { return (bool)GetValue(IsMissionCompleteProperty); }
			protected set { SetValue(IsMissionCompletePropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey IsMissionCompletePropertyKey =
			DependencyProperty.RegisterReadOnly("IsMissionComplete", typeof(bool), typeof(MainCheckerboard), new FrameworkPropertyMetadata(false, OnMissionCompleteChanged));

		public static readonly DependencyProperty IsMissionCompleteProperty = IsMissionCompletePropertyKey.DependencyProperty;

		private static void OnMissionCompleteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is MainCheckerboard @this && (bool)e.NewValue) {
				@this.RaiseEvent(new RoutedEventArgs(MissionCompleteEvent, @this));
			}
		}

		public static readonly RoutedEvent MissionCompleteEvent = EventManager.RegisterRoutedEvent("MissionComplete", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(Chessman));

		public event RoutedEventHandler MissionComplete {
			add { AddHandler(MissionCompleteEvent, value); }
			remove { RemoveHandler(MissionCompleteEvent, value); }
		}

		public static readonly RoutedEvent MapChangedEvent = EventManager.RegisterRoutedEvent("MapChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(Chessman));

		public event RoutedEventHandler MapChanged {
			add { AddHandler(MapChangedEvent, value); }
			remove { RemoveHandler(MapChangedEvent, value); }
		}

		public class MapFreezer : IDisposable {
			MainCheckerboard checkerboard;

			public bool FreezeCell { get; }

			internal MapFreezer(MainCheckerboard checkerboard, bool freezeCell) {
				if (checkerboard.freezer != null) throw new InvalidOperationException();
				this.checkerboard = checkerboard;
				checkerboard.freezer = this;
				FreezeCell = freezeCell;
			}

			void IDisposable.Dispose() {
				Unfreeze();
			}

			public void Unfreeze() {
				checkerboard.freezer = null;
				checkerboard.RaiseEvent(new RoutedEventArgs(MapChangedEvent, checkerboard));
				checkerboard.InvalidateVisual();
			}
		}

		private Map gameMap;
		private Dictionary<(int X1, int Y1, int X2, int Y2), LightColor> lines;
		private MapFreezer freezer;

		public Map GameMap => gameMap;

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);

			if (e.Property == ColumnsProperty || e.Property == RowsProperty) {
				var cols = Columns;
				var rows = Rows;
				if (cols > 0 && rows > 0) {
					if (gameMap == null) {
						gameMap = new Map(cols, rows);
					} else {
						gameMap.Resize(cols, rows);
					}
					lines = new Dictionary<(int X1, int Y1, int X2, int Y2), LightColor>(cols * rows * 12);
				}
			}
		}

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);
			if (gameMap == null) return;
			gameMap.Refresh();
			IsMissionComplete = gameMap.MissionComplete;

			lines.Clear();
			var w = gameMap.Width;
			var h = gameMap.Height;
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					foreach (var (color, dir) in gameMap.Lights[x, y]) {
						var x1 = x;
						var y1 = y;
						var (x2, y2) = ShortLight.Go(dir, x1, y1);
						if (x2 < x1 || (x1 == x2 && y2 < y1)) {
							(x1, y1) = (x2, y2);
							(x2, y2) = (x, y);
						}
						lines.TryGetValue((x1, y1, x2, y2), out var lightColor);
						lines[(x1, y1, x2, y2)] = lightColor | color;
					}
				}
			}

			var clipRect = new RectangleGeometry(new Rect(new Size(ActualWidth, ActualHeight)));
			clipRect.Freeze();
			dc.PushClip(clipRect);
			foreach (var kv in lines) {
				var cellSize = CellSize;
				var offset = cellSize / 2;
				var p1 = new Point(kv.Key.X1 * cellSize + offset, kv.Key.Y1 * cellSize + offset);
				var p2 = new Point(kv.Key.X2 * cellSize + offset, kv.Key.Y2 * cellSize + offset);
				dc.DrawLine(MirrorDrawing.GetPen(kv.Value), p1, p2);
			}
			dc.Pop();
		}

		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved) {
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
			if (gameMap == null) return;

			switch (visualAdded) {
				case Mirror mirror:
					if (freezer == null || !freezer.FreezeCell) {
						var x = (int)mirror.GetValue(ColumnProperty);
						var y = (int)mirror.GetValue(RowProperty);
						gameMap.AddCell(x, y, mirror.Cell);
					}
					if (freezer == null) RaiseEvent(new RoutedEventArgs(MapChangedEvent, this));
					break;
			}
			switch (visualRemoved) {
				case Mirror mirror:
					if (freezer == null || !freezer.FreezeCell) {
						gameMap.RemoveCell(mirror.Cell);
					}
					if (freezer == null) RaiseEvent(new RoutedEventArgs(MapChangedEvent, this));
					break;
			}
			InvalidateVisual();
		}

		public virtual byte[] EncodeToBytes(IReadOnlyCollection<Cell> freedomCells = null) {
			return gameMap.Encode(freedomCells);
		}

		public virtual Mirror CreateMirror(Cell cell) => new Mirror() { Cell = cell };

		public virtual IReadOnlyList<Mirror> DecodeFromBytes(byte[] data) {
			var freedomCells = gameMap.Decode(data);
			using (FreezeMap(true)) {
				Children.Clear();
				for (int y = 0; y < gameMap.Height; y++) {
					for (int x = 0; x < gameMap.Width; x++) {
						if (gameMap.Cells[x, y] is Cell cell) {
							var mirror = CreateMirror(cell);
							mirror.Freedom = cell.Freedom;
							SetColumn(mirror, x);
							SetRow(mirror, y);
							Children.Add(mirror);
						}
					}
				}
			}
			if (freedomCells != null) {
				return freedomCells.Select(cell => CreateMirror(cell)).ToList();
			} else {
				return null;
			}
		}

		public MapFreezer FreezeMap(bool freezeCell = false) => new MapFreezer(this, freezeCell);
	}
}
