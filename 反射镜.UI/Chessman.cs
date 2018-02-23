using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace 反射镜.UI {
	public class Chessman : Control {
		static Chessman() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Chessman), new FrameworkPropertyMetadata(typeof(Chessman)));
		}

		public bool Freedom {
			get { return (bool)GetValue(FreedomProperty); }
			set { SetValue(FreedomProperty, value); }
		}

		public static readonly DependencyProperty FreedomProperty =
			DependencyProperty.Register("Freedom", typeof(bool), typeof(Chessman), new PropertyMetadata(true, OnVisualChanged));

		public static readonly RoutedEvent LeftClickEvent = EventManager.RegisterRoutedEvent("LeftClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(Chessman));

		public event RoutedEventHandler LeftClick {
			add { AddHandler(LeftClickEvent, value); }
			remove { RemoveHandler(LeftClickEvent, value); }
		}

		public static readonly RoutedEvent RightClickEvent = EventManager.RegisterRoutedEvent("RightClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(Chessman));

		public event RoutedEventHandler RightClick {
			add { AddHandler(RightClickEvent, value); }
			remove { RemoveHandler(RightClickEvent, value); }
		}

		public static readonly RoutedEvent BeginDragEvent = EventManager.RegisterRoutedEvent("BeginDrag", RoutingStrategy.Direct, typeof(ChessmanPreviewDragEventArgsHandler), typeof(Chessman));

		public event ChessmanPreviewDragEventArgsHandler BeginDrag {
			add { AddHandler(BeginDragEvent, value); }
			remove { RemoveHandler(BeginDragEvent, value); }
		}

		public static readonly RoutedEvent DragEvent = EventManager.RegisterRoutedEvent("Drag", RoutingStrategy.Direct, typeof(ChessmanPreviewDragEventArgsHandler), typeof(Chessman));

		public event ChessmanPreviewDragEventArgsHandler Drag {
			add { AddHandler(DragEvent, value); }
			remove { RemoveHandler(DragEvent, value); }
		}

		public static readonly RoutedEvent PreviewEndDragEvent = EventManager.RegisterRoutedEvent("PreviewEndDrag", RoutingStrategy.Direct, typeof(ChessmanDragEventHandler), typeof(Chessman));

		public event ChessmanDragEventHandler PreviewEndDrag {
			add { AddHandler(PreviewEndDragEvent, value); }
			remove { RemoveHandler(PreviewEndDragEvent, value); }
		}

		public static readonly RoutedEvent EndDragEvent = EventManager.RegisterRoutedEvent("EndDrag", RoutingStrategy.Direct, typeof(ChessmanDragEventHandler), typeof(Chessman));

		public event ChessmanDragEventHandler EndDrag {
			add { AddHandler(EndDragEvent, value); }
			remove { RemoveHandler(EndDragEvent, value); }
		}

		public static readonly RoutedEvent CancelDragEvent = EventManager.RegisterRoutedEvent("CancelDrag", RoutingStrategy.Direct, typeof(ChessmanDragEventHandler), typeof(Chessman));

		public event ChessmanDragEventHandler CancelDrag {
			add { AddHandler(CancelDragEvent, value); }
			remove { RemoveHandler(CancelDragEvent, value); }
		}

		public double CellSize {
			get { return (double)GetValue(CellSizeProperty); }
			internal protected set { SetValue(CellSizePropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey CellSizePropertyKey =
			DependencyProperty.RegisterReadOnly("CellSize", typeof(double), typeof(Mirror), new PropertyMetadata(28d, OnCellSizeChanged));

		public static readonly DependencyProperty CellSizeProperty = CellSizePropertyKey.DependencyProperty;

		private static void OnCellSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Mirror @this) {
				@this.InvalidateVisual();
				@this.OnCellSizeChanged((double)e.OldValue, (double)e.NewValue);
			}
		}

		protected virtual void OnCellSizeChanged(double oldValue, double newValue) {

		}

		private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Mirror @this) {
				@this.InvalidateVisual();
			}
		}

		private void HookMouseLeftButton(object sender, MouseEventArgs e) {
			e.Handled = true;
			if (e.LeftButton == MouseButtonState.Released) {
				e.Handled = false;
			}
		}

		protected virtual void OnBeginDrag(ChessmanPreviewDragEventArgs e) {
			var parent = Parent as Checkerboard;
			var canvas = parent.Canvas;
			parent.Children.Remove(this);
			Width = parent.CellSize;
			Height = parent.CellSize;
			canvas.Children.Add(this);
		}

		protected virtual void OnDrag(ChessmanPreviewDragEventArgs e) {
			Canvas.SetLeft(this, e.PointRelativeToCanvas.X - Width / 2);
			Canvas.SetTop(this, e.PointRelativeToCanvas.Y - Height / 2);
		}

		protected virtual void OnEndDrag(ChessmanDragEventArgs e) {
			var canvas = Parent as Canvas;
			SetValue(Checkerboard.ColumnProperty, e.NewColumn);
			SetValue(Checkerboard.RowProperty, e.NewRow);
			canvas.Children.Remove(this);
			ClearValue(WidthProperty);
			ClearValue(HeightProperty);
			e.NewCheckerboard.Children.Add(this);
			CellSize = e.NewCheckerboard.CellSize;
		}

		protected virtual void OnCancelDrag(ChessmanDragEventArgs e) {
			var canvas = Parent as Canvas;
			canvas.Children.Remove(this);
			ClearValue(WidthProperty);
			ClearValue(HeightProperty);
			e.OldCheckerboard.Children.Add(this);
		}


		public Chessman() {
			bool isMove = false;
			bool leftClick = false;
			bool enabled = false;
			bool captureMouse = false;
			Point start = new Point();
			Checkerboard parent = null;
			Canvas canvas = null;

			MouseLeftButtonDown += (sender, e) => {
				if (!(captureMouse = Freedom)) return;
				parent = VisualTreeHelper.GetParent(this) as Checkerboard;
				if (parent == null) return;
				canvas = parent.Canvas;
				if (canvas == null) return;
				PreviewMouseDown += HookMouseLeftButton;
				PreviewMouseUp += HookMouseLeftButton;
				if (captureMouse) CaptureMouse();
				start = e.GetPosition(canvas);
				enabled = true;
				leftClick = true;
				e.Handled = true;
			};

			MouseMove += (sender, e) => {
				if (!enabled) return;
				var curr = e.GetPosition(canvas);
				if (Freedom && (isMove || Math.Abs(curr.X - start.X) > 4 || Math.Abs(curr.Y - start.Y) > 4)) {
					var oldColumn = (int)GetValue(Checkerboard.ColumnProperty);
					var oldRow = (int)GetValue(Checkerboard.RowProperty);
					ChessmanPreviewDragEventArgs eventArgs;
					if (!isMove) {
						// begin drag
						isMove = true;
						eventArgs = new ChessmanPreviewDragEventArgs(BeginDragEvent, this, parent, oldColumn, oldRow) {
							StartPointRelativeToCanvas = start,
							PointRelativeToCanvas = curr
						};
						RaiseEvent(eventArgs);
						if (eventArgs.Cancel) {
							enabled = false;
							return;
						}
						OnBeginDrag(eventArgs);
					}
					// drag
					eventArgs = new ChessmanPreviewDragEventArgs(DragEvent, this, parent, oldColumn, oldRow) {
						StartPointRelativeToCanvas = start,
						PointRelativeToCanvas = curr
					};
					RaiseEvent(eventArgs);
					OnDrag(eventArgs);

					e.Handled = true;
				}
			};

			MouseLeftButtonUp += (sender, e) => {
				if (!enabled) return;
				enabled = false;
				if (isMove) {
					// end drag
					isMove = false;
					bool hitSuccess = false;
					ChessmanDragEventArgs eventArgs = null;
					var oldColumn = (int)GetValue(Checkerboard.ColumnProperty);
					var oldRow = (int)GetValue(Checkerboard.RowProperty);

					VisualTreeHelper.HitTest(canvas,
						d => d is Checkerboard ? HitTestFilterBehavior.ContinueSkipChildren : HitTestFilterBehavior.ContinueSkipSelf,
						r => {
							if (r.VisualHit is Checkerboard checkerboard) {
								var curr = e.GetPosition(checkerboard);
								var columns = checkerboard.Columns;
								var rows = checkerboard.Rows;
								var cellSize = checkerboard.CellSize;
								var newColumn = (int)(curr.X / cellSize);
								var newRow = (int)(curr.Y / cellSize);

								if (newColumn >= 0 && newColumn < columns && newRow >= 0 && newRow < rows) {
									eventArgs = new ChessmanDragEventArgs(PreviewEndDragEvent, this, parent, oldColumn, oldRow, checkerboard, newColumn, newRow);
									RaiseEvent(eventArgs);
									if (!eventArgs.Cancel) {
										hitSuccess = true;
										
										eventArgs = new ChessmanDragEventArgs(EndDragEvent, this, parent, oldColumn, oldRow, eventArgs.NewCheckerboard, eventArgs.NewColumn, eventArgs.NewRow);
										OnEndDrag(eventArgs);
										RaiseEvent(eventArgs);
									} else {
										System.Diagnostics.Debug.Print("cancel");
									}
								}
							}
							return HitTestResultBehavior.Stop;
						}, new PointHitTestParameters(e.GetPosition(canvas)));

					if (!hitSuccess) {
						if (eventArgs != null) {
							eventArgs = new ChessmanDragEventArgs(CancelDragEvent, this, parent, oldColumn, oldRow, eventArgs.NewCheckerboard, eventArgs.NewColumn, eventArgs.NewRow);
						} else {
							eventArgs = new ChessmanDragEventArgs(CancelDragEvent, this, parent, oldColumn, oldRow, null, 0, 0);
						}
						OnCancelDrag(eventArgs);
						RaiseEvent(eventArgs);
					}
				} else {
					if (leftClick) {
						// click
						leftClick = false;
						RaiseEvent(new RoutedEventArgs(LeftClickEvent, this));
					}
				}
				if (captureMouse) ReleaseMouseCapture();
				PreviewMouseDown -= HookMouseLeftButton;
				PreviewMouseUp -= HookMouseLeftButton;
				e.Handled = true;
			};


			bool rightClick = false;
			MouseRightButtonDown += delegate {
				rightClick = true;
			};
			MouseLeave += delegate {
				rightClick = false;
				leftClick = false;
			};
			MouseRightButtonUp += delegate {
				if (rightClick) {
					rightClick = false;
					RaiseEvent(new RoutedEventArgs(RightClickEvent, this));
				}
			};

			Loaded += delegate {
				if (VisualTreeHelper.GetParent(this) is Checkerboard checkerboard) {
					CellSize = checkerboard.CellSize;
				}
			};
		}
	}
}
