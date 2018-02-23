using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;

namespace 反射镜.UI {
	public class Mirror : Chessman {
		static Mirror() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Mirror), new FrameworkPropertyMetadata(typeof(Mirror)));
		}

		public Cell Cell {
			get { return (Cell)GetValue(CellProperty); }
			set { SetValue(CellProperty, value); }
		}

		public static readonly DependencyProperty CellProperty =
			DependencyProperty.Register("Cell", typeof(Cell), typeof(Mirror), new PropertyMetadata(null, OnCellChanged));

		private static void OnCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Mirror @this) {
				if (e.Property == CellProperty) {
					if (e.NewValue is TargetCell target) {
						target.ActivatedChanged += @this.Target_ActivatedChanged;
					}
					if (e.OldValue is TargetCell oldTarget) {
						oldTarget.ActivatedChanged -= @this.Target_ActivatedChanged;
					}
					@this.InvalidateVisual();
				}
			}
		}

		private void Target_ActivatedChanged(TargetCell obj) {
			if (obj.Activated) {
				Effect = new System.Windows.Media.Effects.DropShadowEffect() {
					BlurRadius = CellSize,
					Color = obj is IColor colorObj ? MirrorDrawing.GetColor(colorObj.Color) : Colors.Black,
					ShadowDepth = 0,
					Opacity = 1,
					RenderingBias = System.Windows.Media.Effects.RenderingBias.Quality,
				};
			} else {
				ClearValue(EffectProperty);
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
			base.OnPropertyChanged(e);

			if (e.Property == FreedomProperty) {
				if (Cell != null) {
					Cell.Freedom = (bool)e.NewValue;
				}
				if (Parent is MainCheckerboard checkerboard) {
					checkerboard.RaiseEvent(new RoutedEventArgs(MainCheckerboard.MapChangedEvent, checkerboard));
				}
			}
		}

		private Drawing drawing;

		public Mirror() {
			PreviewEndDrag += Mirror_EndDrag;
			MouseWheel += (sender, e) => {
				if (Cell.CanRotate && Freedom) {
					RotateDirection(e.Delta > 0 ? Direction._45 : Direction._315);
				}
			};
		}

		private void Mirror_EndDrag(object sender, ChessmanDragEventArgs e) {
			if (e.NewCheckerboard is MainCheckerboard mainCheckerboard) {
				if (mainCheckerboard.GameMap?.Cells?[e.NewColumn, e.NewRow] != null) {
					e.Cancel = true;
				}
			}
		}

		protected override void OnCellSizeChanged(double oldValue, double newValue) {
			base.OnCellSizeChanged(oldValue, newValue);
			drawing = GetDrawing();
		}

		private Drawing GetDrawing() {
			var md = new MirrorDrawing(CellSize);
			switch (Cell) {
				case LightSource lightSource: return md.GetLightSource(lightSource.Color);
				case Reflection _: return md.GetReflection();
				case BeveledReflection _: return md.GetBeveledReflection();
				case Discolor _: return md.GetDiscolor();
				case Splitting _: return md.GetSplitting();
				case Quantum _: return md.GetQuantum();
				case TargetLight targetLight: return md.GetTargetLight(targetLight);
				case BlackLight blackLight: return md.GetBlackLight(blackLight);
				case Prism _: return md.GetPrism();
				case Xor _: return md.GetXor();
				case And _: return md.GetAnd();
				case Or _: return md.GetOr();
				case Filter filter: return md.GetFilter(filter.Color);
				case OneWay _: return md.GetOneWay();
				case TwoWay _: return md.GetTwoWay();
				case Obstacle _: return md.GetObstacle();
				case Blockade _: return md.GetBlockade();
				case Wormhole wormhole: return md.GetWormhole(wormhole.Color);
				case ColorSplitting colorSplitting: return md.GetColorSplitting(colorSplitting.Color);
				default: return null;
			}
		}

		protected override void OnRender(DrawingContext dc) {
			base.OnRender(dc);
			if (drawing == null) {
				drawing = GetDrawing();
				if (drawing == null) return;
			}

			var cellSize = CellSize;
			var rotate = new RotateTransform(-(int)Cell.Direction * 45, cellSize / 2, cellSize / 2);
			rotate.Freeze();
			dc.PushTransform(rotate);
			dc.DrawDrawing(drawing);
			dc.Pop();

			// 防止鼠标事件穿透
			dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Size(cellSize, cellSize)));
		}

		public Mirror Clone() {
			var type = GetType();
			var clone = type.GetConstructor(Type.EmptyTypes).Invoke(null) as Mirror;
			var enumer = GetLocalValueEnumerator();
			while (enumer.MoveNext()) {
				if (!enumer.Current.Property.ReadOnly) {
					if (enumer.Current.Property == CellProperty) continue;
					clone.SetValue(enumer.Current.Property, enumer.Current.Value);
				}
			}
			var cloneCell = Cell.Clone() as Cell;
			cloneCell.Y = cloneCell.X = -1;
			clone.Cell = cloneCell;
			return clone;
		}

		public void RotateDirection(Direction dir) {
			Cell.Direction = Cell.Direction.Add(dir);
			NotifyCellsChanged();
		}

		public void ResetDirection() {
			Cell.Direction = Direction._0;
			NotifyCellsChanged();
		}

		private void NotifyCellsChanged() {
			InvalidateVisual();
			if (VisualTreeHelper.GetParent(this) is MainCheckerboard checkerboard) {
				checkerboard.InvalidateVisual();
				checkerboard.RaiseEvent(new RoutedEventArgs(MainCheckerboard.MapChangedEvent, checkerboard));
			}
		}
	}
}
