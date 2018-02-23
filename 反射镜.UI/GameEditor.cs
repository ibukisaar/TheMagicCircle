using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;

namespace 反射镜.UI {
	public class GameEditor : Canvas {

		static GameEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(GameEditor), new FrameworkPropertyMetadata(typeof(GameEditor)));
		}

		public static readonly DependencyProperty EditorProperty =
			DependencyProperty.RegisterAttached("Editor", typeof(GameEditor), typeof(GameEditor), new FrameworkPropertyMetadata(null));

		public static GameEditor GetEditor(UIElement editorMirror) => (GameEditor)editorMirror.GetValue(EditorProperty);
		public static void SetEditor(UIElement editorMirror, GameEditor value) => editorMirror.SetValue(EditorProperty, value);

		public EditorCheckerboard EditorCheckerboard {
			get { return (EditorCheckerboard)GetValue(EditorCheckerboardProperty); }
			set { SetValue(EditorCheckerboardProperty, value); }
		}

		public static readonly DependencyProperty EditorCheckerboardProperty =
			DependencyProperty.Register("EditorCheckerboard", typeof(EditorCheckerboard), typeof(GameEditor), new PropertyMetadata(null));

		public EndlessCheckerboard EndlessCheckerboard {
			get { return (EndlessCheckerboard)GetValue(EndlessCheckerboardProperty); }
			set { SetValue(EndlessCheckerboardProperty, value); }
		}

		public static readonly DependencyProperty EndlessCheckerboardProperty =
			DependencyProperty.Register("EndlessCheckerboard", typeof(EndlessCheckerboard), typeof(GameEditor), new PropertyMetadata(null));

		public bool CanSelect {
			get { return (bool)GetValue(CanSelectProperty); }
			set { SetValue(CanSelectProperty, value); }
		}

		public static readonly DependencyProperty CanSelectProperty =
			DependencyProperty.Register("CanSelect", typeof(bool), typeof(GameEditor), new PropertyMetadata(true, OnSelectDependencyPropertyChanged));

		public bool IsSelected {
			get { return (bool)GetValue(IsSelectedProperty); }
			protected set { SetValue(IsSelectedPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey IsSelectedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(GameEditor), new PropertyMetadata(false, OnSelectDependencyPropertyChanged));

		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		public bool IsAttachedSelect {
			get { return (bool)GetValue(IsAttachedSelectProperty); }
			set { SetValue(IsAttachedSelectProperty, value); }
		}

		public static readonly DependencyProperty IsAttachedSelectProperty =
			DependencyProperty.Register("IsAttachedSelect", typeof(bool), typeof(GameEditor), new PropertyMetadata(false));

		public Border Selector {
			get { return (Border)GetValue(SelectorProperty); }
			set { SetValue(SelectorProperty, value); }
		}

		public static readonly DependencyProperty SelectorProperty =
			DependencyProperty.Register("Selector", typeof(Border), typeof(GameEditor), new PropertyMetadata(null));

		private static void OnSelectDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GameEditor @this) {
				if (e.Property == CanSelectProperty) {
					if ((bool)e.NewValue == false) {
						@this.IsSelected = false;
						@this.HasSelection = false;
					}
				} else if (e.Property == IsSelectedProperty) {
					if (@this.Selector != null) {
						if ((bool)e.NewValue == false) {
							@this.Children.Remove(@this.Selector);
						} else {
							@this.Children.Add(@this.Selector);
						}
					}
					if ((bool)e.OldValue == false && (bool)e.NewValue == true) {
						@this.HasSelection = false;
					}
				}
			}
		}

		public bool HasSelection {
			get { return (bool)GetValue(HasSelectionProperty); }
			protected set { SetValue(HasSelectionPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey HasSelectionPropertyKey =
			DependencyProperty.RegisterReadOnly("HasSelection", typeof(bool), typeof(GameEditor), new PropertyMetadata(false));

		public static readonly DependencyProperty HasSelectionProperty = HasSelectionPropertyKey.DependencyProperty;

		public bool BrushMode {
			get { return (bool)GetValue(BrushModeProperty); }
			set { SetValue(BrushModeProperty, value); }
		}

		public static readonly DependencyProperty BrushModeProperty =
			DependencyProperty.Register("BrushMode", typeof(bool), typeof(GameEditor), new PropertyMetadata(false, OnBrushModeChanged));

		private static void OnBrushModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GameEditor @this) {
				if (e.Property == BrushModeProperty) {
					if (@this.MirrorBrush is EditorMirror mirrorBrush) {
						mirrorBrush.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden;
					}
				}
			}
		}

		public EditorMirror MirrorBrush {
			get { return (EditorMirror)GetValue(MirrorBrushProperty); }
			set { SetValue(MirrorBrushProperty, value); }
		}

		public static readonly DependencyProperty MirrorBrushProperty =
			DependencyProperty.Register("MirrorBrush", typeof(EditorMirror), typeof(GameEditor), new PropertyMetadata(null, OnMirrorBrushChanged));

		private static void OnMirrorBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is GameEditor @this) {
				if (e.Property == MirrorBrushProperty) {
					if (e.NewValue is EditorMirror mirrorBrush) {
						if (@this.EditorCheckerboard is EditorCheckerboard editorCheckerboard) {
							mirrorBrush.CellSize = editorCheckerboard.CellSize;
							mirrorBrush.Visibility = Visibility.Hidden;
							mirrorBrush.RightClick += CancelBrushMode;
							mirrorBrush.MouseDown += ApplyBrush;
							mirrorBrush.MouseMove += ApplyBrush;
							@this.Children.Add(mirrorBrush);
						}
					}
					if (e.OldValue is EditorMirror oldMirrorBrush) {
						@this.Children.Remove(oldMirrorBrush);
						oldMirrorBrush.RightClick -= CancelBrushMode;
						oldMirrorBrush.MouseDown -= ApplyBrush;
						oldMirrorBrush.MouseMove -= ApplyBrush;
					}
				}
			}
		}

		private static void ApplyBrush(object sender, MouseEventArgs e) {
			if (sender is EditorMirror brush) {
				if (e.LeftButton == MouseButtonState.Pressed) {
					if (brush.Editor.EditorCheckerboard is EditorCheckerboard checkerboard) {
						var point = e.GetPosition(checkerboard);
						var x = (int)(point.X / checkerboard.CellSize);
						var y = (int)(point.Y / checkerboard.CellSize);
						if (x >= 0 && x < checkerboard.Columns && y >= 0 && y < checkerboard.Rows) {
							if (checkerboard.GameMap != null && checkerboard.GameMap.Cells[x, y] == null) {
								var clone = brush.Clone();
								clone.SetValue(Checkerboard.ColumnProperty, x);
								clone.SetValue(Checkerboard.RowProperty, y);
								checkerboard.Children.Add(clone);
							}
						}
					}
				}
			}
		}

		private static void CancelBrushMode(object sender, RoutedEventArgs e) {
			if (sender is EditorMirror brush) {
				brush.Editor.BrushMode = false;
			}
		}

		public GameEditor() {
			Point start = new Point();
			bool enabled = false;
			HashSet<EditorMirror> selectRecord = null;

			MouseLeftButtonDown += (sender, e) => {
				if (BrushMode || !CanSelect || EditorCheckerboard == null || Selector == null) return;
				CaptureMouse();
				enabled = true;
				start = e.GetPosition(this);
				e.Handled = true;
			};
			MouseMove += (sender, e) => {
				if (!enabled) return;
				var editorCheckerboard = EditorCheckerboard;
				var curr = e.GetPosition(this);
				if (IsSelected || Math.Abs(curr.X - start.X) > 4 || Math.Abs(curr.Y - start.Y) > 4) {
					// select
					var borderRect = new Rect(start, curr);
					var mainRect = new Rect(GetLeft(editorCheckerboard), GetTop(editorCheckerboard), editorCheckerboard.ActualWidth, editorCheckerboard.ActualHeight);
					borderRect = Rect.Intersect(borderRect, mainRect);
					if (!borderRect.IsEmpty) {
						if (!IsSelected) {
							// begin select
							IsSelected = true;
							if (IsAttachedSelect) {
								selectRecord = new HashSet<EditorMirror>(editorCheckerboard.Children.OfType<EditorMirror>().Where(m => m.IsSelected));
							} else {
								selectRecord = null;
							}
						}
						SetLeft(Selector, borderRect.Left);
						SetTop(Selector, borderRect.Top);
						Selector.Width = borderRect.Width;
						Selector.Height = borderRect.Height;

						borderRect.Offset(-GetLeft(editorCheckerboard), -GetTop(editorCheckerboard));
						foreach (EditorMirror child in editorCheckerboard.Children) {
							if (selectRecord != null && selectRecord.Contains(child)) {
								continue;
							}

							var cellSize = editorCheckerboard.CellSize;
							var x = (int)child.GetValue(Checkerboard.ColumnProperty) * cellSize;
							var y = (int)child.GetValue(Checkerboard.RowProperty) * cellSize;
							var childRect = new Rect(x, y, cellSize, cellSize);
							var intersect = Rect.Intersect(childRect, borderRect);
							if (!intersect.IsEmpty && intersect.Width * intersect.Height * 2 > cellSize * cellSize) {
								child.IsSelected = true;
								HasSelection = true;
							} else {
								child.IsSelected = false;
							}
						}
					}
				}
				e.Handled = true;
			};
			MouseLeftButtonUp += (sender, e) => {
				if (!enabled) return;
				IsSelected = false;
				enabled = false;
				ReleaseMouseCapture();
				e.Handled = true;
			};

			MouseMove += (sender, e) => {
				var mirrorBrush = MirrorBrush;
				if (!BrushMode || mirrorBrush == null) return;
				mirrorBrush.Visibility = Visibility.Visible;
				var point = e.GetPosition(this);
				point.Offset(-mirrorBrush.CellSize / 2, -mirrorBrush.CellSize / 2);
				SetLeft(mirrorBrush, point.X);
				SetTop(mirrorBrush, point.Y);
			};

			bool brushMode = false;
			MouseEnter += delegate {
				BrushMode = brushMode;
			};
			MouseLeave += delegate {
				brushMode = BrushMode;
				BrushMode = false;
			};
		}

		public void TryCancelSelect() {
			if (!IsAttachedSelect) {
				CancelSelect();
			}
		}

		public void CancelSelect() {
			HasSelection = false;
			foreach (EditorMirror mirror in EditorCheckerboard.Children) {
				mirror.IsSelected = false;
			}
		}

		public void Select(IEnumerable<EditorMirror> mirrors) {
			foreach (var m in mirrors) {
				HasSelection = true;
				m.IsSelected = true;
			}
		}

		public void RaiseSelectedChanged(bool newValue) {
			if (newValue) {
				HasSelection = true;
			} else {
				if (EditorCheckerboard.Children.OfType<EditorMirror>().Any(m => m.IsSelected)) {
					HasSelection = true;
				} else {
					HasSelection = false;
				}
			}
		}

		
	}
}
