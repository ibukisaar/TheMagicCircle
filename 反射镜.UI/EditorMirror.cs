using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace 反射镜.UI {
	public class EditorMirror : Mirror {
		static EditorMirror() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(EditorMirror), new FrameworkPropertyMetadata(typeof(EditorMirror)));
		}

		public bool IsSelected {
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register("IsSelected", typeof(bool), typeof(Chessman), new PropertyMetadata(false, OIsSelectedChanged));

		private static void OIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is EditorMirror @this) {
				@this.InvalidateVisual();
			}
		}

		public GameEditor Editor {
			get => GetValue(GameEditor.EditorProperty) as GameEditor;
			set => SetValue(GameEditor.EditorProperty, value);
		}

		protected override void OnBeginDrag(ChessmanPreviewDragEventArgs e) {
			if (!IsSelected) {
				base.OnBeginDrag(e);
			} else if (GameEditor.GetEditor(this) is GameEditor canvas) {
				var hiddenCheckerboard = new Checkerboard() {
					BorderBrush = null,
					Background = null,
					CellSize = e.OldCheckerboard.CellSize,
					Columns = e.OldCheckerboard.Columns,
					Rows = e.OldCheckerboard.Rows,
				};
				if (e.OldCheckerboard is EditorCheckerboard oldCheckerboard) {
					using (oldCheckerboard.FreezeMap()) {
						foreach (var child in oldCheckerboard.Children.OfType<EditorMirror>().Where(m => m.IsSelected && m.Freedom).ToArray()) {
							oldCheckerboard.Children.Remove(child);
							hiddenCheckerboard.Children.Add(child);
						}
					}
					Canvas.SetLeft(hiddenCheckerboard, Canvas.GetLeft(oldCheckerboard));
					Canvas.SetTop(hiddenCheckerboard, Canvas.GetTop(oldCheckerboard));
					canvas.Children.Add(hiddenCheckerboard);
				}
			}
		}

		protected override void OnDrag(ChessmanPreviewDragEventArgs e) {
			if (!IsSelected) {
				base.OnDrag(e);
			} else if (GameEditor.GetEditor(this) is GameEditor canvas) {
				var hiddenCheckerboard = Parent as Checkerboard;
				var startX = Canvas.GetLeft(e.OldCheckerboard);
				var startY = Canvas.GetTop(e.OldCheckerboard);
				Canvas.SetLeft(hiddenCheckerboard, startX + e.PointRelativeToCanvas.X - e.StartPointRelativeToCanvas.X);
				Canvas.SetTop(hiddenCheckerboard, startY + e.PointRelativeToCanvas.Y - e.StartPointRelativeToCanvas.Y);
			}
		}

		protected override void OnEndDrag(ChessmanDragEventArgs e) {
			if (!IsSelected) {
				base.OnEndDrag(e);
			} else if (GameEditor.GetEditor(this) is GameEditor editor) {
				var hiddenCheckerboard = Parent as Checkerboard;
				if (e.NewCheckerboard is EditorCheckerboard && e.OldCheckerboard is EditorCheckerboard oldCheckerboard) {
					bool pass = false;
					var startX = Canvas.GetLeft(e.OldCheckerboard);
					var startY = Canvas.GetTop(e.OldCheckerboard);
					var endX = Canvas.GetLeft(hiddenCheckerboard);
					var endY = Canvas.GetTop(hiddenCheckerboard);
					var offsetX = (int)Math.Round((endX - startX) / hiddenCheckerboard.CellSize);
					var offsetY = (int)Math.Round((endY - startY) / hiddenCheckerboard.CellSize);
					var cells = editor.EditorCheckerboard?.GameMap?.Cells;
					if (cells != null) {
						pass = true;
						foreach (EditorMirror child in hiddenCheckerboard.Children) {
							var newX = Checkerboard.GetColumn(child) + offsetX;
							var newY = Checkerboard.GetRow(child) + offsetY;
							if (newX < 0) { pass = false; break; }
							if (newX >= hiddenCheckerboard.Columns) { pass = false; break; }
							if (newY < 0) { pass = false; break; }
							if (newY >= hiddenCheckerboard.Rows) { pass = false; break; }
							if (cells[newX, newY] != null) { pass = false; break; }
						}
					}

					using (oldCheckerboard.FreezeMap()) {
						if (pass) {
							foreach (var child in hiddenCheckerboard.Children.OfType<EditorMirror>().ToArray()) {
								var newX = Checkerboard.GetColumn(child) + offsetX;
								var newY = Checkerboard.GetRow(child) + offsetY;
								Checkerboard.SetColumn(child, newX);
								Checkerboard.SetRow(child, newY);
								hiddenCheckerboard.Children.Remove(child);
								e.OldCheckerboard.Children.Add(child);
							}
						} else {
							foreach (var child in hiddenCheckerboard.Children.OfType<EditorMirror>().ToArray()) {
								hiddenCheckerboard.Children.Remove(child);
								e.OldCheckerboard.Children.Add(child);
							}
						}
					}
				} else {
					foreach (var child in hiddenCheckerboard.Children.OfType<EditorMirror>().ToArray()) {
						hiddenCheckerboard.Children.Remove(child);
					}
				}
				editor.Children.Remove(hiddenCheckerboard);
			}
		}

		protected override void OnCancelDrag(ChessmanDragEventArgs e) {
			if (!IsSelected) {
				base.OnCancelDrag(e);
			} else if (GameEditor.GetEditor(this) is GameEditor editor) {
				var hiddenCheckerboard = Parent as Checkerboard;
				if (e.OldCheckerboard is EditorCheckerboard oldCheckerboard
						&& e.NewCheckerboard is EditorCheckerboard newCheckerboard
						&& oldCheckerboard == newCheckerboard) {
					using (newCheckerboard.FreezeMap()) {
						foreach (var child in hiddenCheckerboard.Children.OfType<EditorMirror>().ToArray()) {
							hiddenCheckerboard.Children.Remove(child);
							oldCheckerboard.Children.Add(child);
						}
					}
				} else {
					foreach (var child in hiddenCheckerboard.Children.OfType<EditorMirror>().ToArray()) {
						hiddenCheckerboard.Children.Remove(child);
					}
				}

				editor.Children.Remove(hiddenCheckerboard);
			}
		}

		public EditorMirror() {
			EndDrag += (sender, e) => {
				if (e.NewCheckerboard is EndlessCheckerboard) {
					e.NewCheckerboard.Children.Remove(this);
				}
			};

			CancelDrag += (sender, e) => {
				if (e.NewCheckerboard == null || e.NewCheckerboard is EndlessCheckerboard) {
					e.OldCheckerboard.Children.Remove(this);
				}
			};

			LeftClick += delegate {
				if (GameEditor.GetEditor(this) is GameEditor editor) {
					if (editor.IsAttachedSelect) {
						IsSelected = !IsSelected;
						editor.RaiseSelectedChanged(IsSelected);
					} else {
						editor.CancelSelect();
						if (Cell == null || !Cell.CanRotate || !Freedom) return;
						RotateDirection(Direction._45);
					}
				}
			};

			RightClick += delegate {
				if (Editor is GameEditor editor && (editor.BrushMode || editor.HasSelection)) return;
				if (IsSelected || ContextMenu != null || Cell == null || !Cell.CanRotate || !Freedom) return;
				RotateDirection(Direction._45.Neg());
			};

			MouseDown += (sender, e) => {
				if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed) {
					if (Parent is EditorCheckerboard) {
						Freedom = !Freedom;
					}
				}
			};
		}

		private Geometry GetSelectBorder() {
			const double borderRange = 0.2;
			var geometry = new StreamGeometry();
			using (var sgc = geometry.Open()) {
				sgc.BeginFigure(new Point(0, 0), true, true);
				sgc.LineTo(new Point(borderRange, 0), true, false);
				sgc.LineTo(new Point(1 - borderRange, 0), false, false);
				sgc.LineTo(new Point(1, 0), true, false);
				sgc.LineTo(new Point(1, borderRange), true, false);
				sgc.LineTo(new Point(1, 1 - borderRange), false, false);
				sgc.LineTo(new Point(1, 1), true, false);
				sgc.LineTo(new Point(1 - borderRange, 1), true, false);
				sgc.LineTo(new Point(borderRange, 1), false, false);
				sgc.LineTo(new Point(0, 1), true, false);
				sgc.LineTo(new Point(0, 1 - borderRange), true, false);
				sgc.LineTo(new Point(0, borderRange), false, false);
			}
			geometry.Transform = new TransformGroup() {
				Children = new TransformCollection(
					new Transform[] { new ScaleTransform(CellSize - 2, CellSize - 2), new TranslateTransform(1, 1) }
				)
			};
			geometry.Freeze();
			return geometry;
		}

		private Geometry selectBorder = null;

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);

			if (selectBorder == null) selectBorder = GetSelectBorder();
			if (!Freedom) {
				var pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 80, 80)), 1);
				pen.Freeze();
				var brush = new SolidColorBrush(Color.FromArgb(40, 255, 80, 80));
				brush.Freeze();
				dc.DrawRectangle(brush, null, new Rect(0, 0, CellSize, CellSize));
				dc.DrawGeometry(null, pen, selectBorder);
			}

			if (IsSelected) {
				var pen = new Pen(new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)), 1);
				pen.Freeze();
				dc.DrawGeometry(null, pen, selectBorder);
			}
		}
	}
}
