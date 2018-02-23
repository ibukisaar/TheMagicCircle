using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 反射镜.UI {
	public class Checkerboard : Panel {
		static Checkerboard() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Checkerboard), new FrameworkPropertyMetadata(typeof(Checkerboard)));
		}

		public int Rows {
			get { return (int)GetValue(RowsProperty); }
			set { SetValue(RowsProperty, value); }
		}

		public static readonly DependencyProperty RowsProperty =
			DependencyProperty.Register("Rows", typeof(int), typeof(Checkerboard), new PropertyMetadata(0, OnSizeChanged));

		public int Columns {
			get { return (int)GetValue(ColumnsProperty); }
			set { SetValue(ColumnsProperty, value); }
		}

		public static readonly DependencyProperty ColumnsProperty =
			DependencyProperty.Register("Columns", typeof(int), typeof(Checkerboard), new PropertyMetadata(0, OnSizeChanged));

		public double CellSize {
			get { return (double)GetValue(CellSizeProperty); }
			set { SetValue(CellSizeProperty, value); }
		}

		public static readonly DependencyProperty CellSizeProperty =
			DependencyProperty.Register("CellSize", typeof(double), typeof(Checkerboard), new PropertyMetadata(28d, OnCellSizeChanged));

		public static readonly DependencyProperty RowProperty =
			DependencyProperty.RegisterAttached("Row", typeof(int), typeof(Checkerboard), new FrameworkPropertyMetadata(0, OnPositioningChanged));

		public static int GetRow(UIElement element) => (int)element.GetValue(RowProperty);
		public static void SetRow(UIElement element, int value) => element.SetValue(RowProperty, value);

		public static readonly DependencyProperty ColumnProperty =
			DependencyProperty.RegisterAttached("Column", typeof(int), typeof(Checkerboard), new FrameworkPropertyMetadata(0, OnPositioningChanged));

		public static int GetColumn(UIElement element) => (int)element.GetValue(ColumnProperty);
		public static void SetColumn(UIElement element, int value) => element.SetValue(ColumnProperty, value);

		public Brush BorderBrush {
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		public static readonly DependencyProperty BorderBrushProperty =
			DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Checkerboard), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(80, 60, 80)), OnVisualChanged));

		public double BorderThickness {
			get { return (double)GetValue(BorderThicknessProperty); }
			set { SetValue(BorderThicknessProperty, value); }
		}

		public static readonly DependencyProperty BorderThicknessProperty =
			DependencyProperty.Register("BorderThickness", typeof(double), typeof(Checkerboard), new PropertyMetadata(1d, OnVisualChanged));

		public Canvas Canvas {
			get { return (Canvas)GetValue(CanvasProperty); }
			protected set { SetValue(CanvasPropertyKey, value); }
		}

		protected static readonly DependencyPropertyKey CanvasPropertyKey =
			DependencyProperty.RegisterReadOnly("Canvas", typeof(Canvas), typeof(Checkerboard), new PropertyMetadata(null));

		public static readonly DependencyProperty CanvasProperty = CanvasPropertyKey.DependencyProperty;


		private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Checkerboard @this) {
				@this.InvalidateMeasure();
			}
		}

		private static void OnCellSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Checkerboard @this) {
				@this.InvalidateMeasure();
				@this.InvalidateArrange();
				foreach (var child in @this.Children) {
					if (child is Chessman chessman) {
						chessman.CellSize = (double)e.NewValue;
					}
				}
			}
		}

		private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Checkerboard @this) {
				@this.InvalidateVisual();
			}
		}

		private static void OnPositioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Chessman chessman) {
				if (VisualTreeHelper.GetParent(chessman) is Checkerboard p) {
					p.InvalidateArrange();
				}
			}
		}

		public Checkerboard() {
			Loaded += delegate {
				var curr = VisualTreeHelper.GetParent(this);
				while (curr != null) {
					if (curr is Canvas canvas) {
						Canvas = canvas;
						break;
					}
					curr = VisualTreeHelper.GetParent(curr);
				}
			};
		}

		protected override Size MeasureOverride(Size constraint) {
			var cellSize = CellSize;
			foreach (UIElement child in InternalChildren) {
				child.Measure(new Size(cellSize, cellSize));
			}

			return new Size(Columns * cellSize, Rows * cellSize);
		}

		protected override Size ArrangeOverride(Size arrangeBounds) {
			var cellSize = CellSize;
			foreach (UIElement child in InternalChildren) {
				var x = (int)child.GetValue(ColumnProperty);
				var y = (int)child.GetValue(RowProperty);
				child.Arrange(new Rect(x * cellSize, y * cellSize, cellSize, cellSize));
			}
			return arrangeBounds;
		}

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);
			if (BorderBrush == null) return;
			var pen = new Pen(BorderBrush, BorderThickness) {
				LineJoin = PenLineJoin.Bevel,
				StartLineCap = PenLineCap.Flat,
				EndLineCap = PenLineCap.Flat,
			};
			pen.Freeze();

			var columns = Columns;
			var rows = Rows;
			var cellSize = CellSize;
			var width = columns * cellSize;
			var height = rows * cellSize;
			for (int x = 0; x <= columns; x++) {
				dc.DrawLine(pen, new Point(x * cellSize + .5, 0), new Point(x * cellSize + .5, height));
			}
			for (int y = 0; y <= rows; y++) {
				dc.DrawLine(pen, new Point(0, y * cellSize + .5), new Point(width, y * cellSize + .5));
			}
		}
	}
}
