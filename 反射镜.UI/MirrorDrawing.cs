using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace 反射镜.UI {
	public class MirrorDrawing {
		const byte White = 255, Black = 100;
		const double PenThickness = 2;

		static readonly Color[] colors = {
			Color.FromRgb(Black, Black, Black),
			Color.FromRgb(White, Black, Black),
			Color.FromRgb(Black, White, Black),
			Color.FromRgb(White, White, Black),
			Color.FromRgb(Black, Black, White),
			Color.FromRgb(White, Black, White),
			Color.FromRgb(Black, White, White),
			Color.FromRgb(White, White, White),
		};

		const byte DarkWhite = 140, DarkBlack = 0;

		static readonly Color[] darkColors = {
			Color.FromRgb(DarkBlack, DarkBlack, DarkBlack),
			Color.FromRgb(DarkWhite, DarkBlack, DarkBlack),
			Color.FromRgb(DarkBlack, (byte)(DarkWhite * 1.2), DarkBlack),
			Color.FromRgb(DarkWhite, DarkWhite, DarkBlack),
			Color.FromRgb(DarkBlack, DarkBlack, DarkWhite),
			Color.FromRgb(DarkWhite, DarkBlack, DarkWhite),
			Color.FromRgb(DarkBlack, DarkWhite, DarkWhite),
			Color.FromRgb(DarkWhite, DarkWhite, DarkWhite),
		};

		static readonly Brush[] brushes = {
			Freeze(new SolidColorBrush(colors[0])),
			Freeze(new SolidColorBrush(colors[1])), // R
			Freeze(new SolidColorBrush(colors[2])), // G
			Freeze(new SolidColorBrush(colors[3])), // R+G
			Freeze(new SolidColorBrush(colors[4])), // B
			Freeze(new SolidColorBrush(colors[5])), // R+B
			Freeze(new SolidColorBrush(colors[6])), // G+B
			Freeze(new SolidColorBrush(colors[7])), // R+G+B
		};



		static readonly Brush[] darkBrushes = {
			Freeze(new SolidColorBrush(darkColors[0])),
			Freeze(new SolidColorBrush(darkColors[1])), // R
			Freeze(new SolidColorBrush(darkColors[2])), // G
			Freeze(new SolidColorBrush(darkColors[3])), // R+G
			Freeze(new SolidColorBrush(darkColors[4])), // B
			Freeze(new SolidColorBrush(darkColors[5])), // R+B
			Freeze(new SolidColorBrush(darkColors[6])), // G+B
			Freeze(new SolidColorBrush(darkColors[7])), // R+G+B
		};

		static readonly Pen[] pens = {
			Freeze(new Pen(brushes[0], PenThickness)),
			Freeze(new Pen(brushes[1], PenThickness)),
			Freeze(new Pen(brushes[2], PenThickness)),
			Freeze(new Pen(brushes[3], PenThickness)),
			Freeze(new Pen(brushes[4], PenThickness)),
			Freeze(new Pen(brushes[5], PenThickness)),
			Freeze(new Pen(brushes[6], PenThickness)),
			Freeze(new Pen(brushes[7], PenThickness)),
		};

		static readonly Pen[] darkPens = {
			Freeze(new Pen(darkBrushes[0], PenThickness)),
			Freeze(new Pen(darkBrushes[1], PenThickness)),
			Freeze(new Pen(darkBrushes[2], PenThickness)),
			Freeze(new Pen(darkBrushes[3], PenThickness)),
			Freeze(new Pen(darkBrushes[4], PenThickness)),
			Freeze(new Pen(darkBrushes[5], PenThickness)),
			Freeze(new Pen(darkBrushes[6], PenThickness)),
			Freeze(new Pen(darkBrushes[7], PenThickness)),
		};

		internal protected static Color GetColor(LightColor lightColor) => colors[(int)lightColor];
		internal protected static Color GetDarkColor(LightColor lightColor) => darkColors[(int)lightColor];
		internal protected static Brush GetBrush(LightColor lightColor) => brushes[(int)lightColor];
		internal protected static Brush GetDarkBrush(LightColor lightColor) => darkBrushes[(int)lightColor];
		internal protected static Pen GetPen(LightColor lightColor) => pens[(int)lightColor];
		internal protected static Pen GetDarkPen(LightColor lightColor) => darkPens[(int)lightColor];

		internal protected static readonly Pen BlackPen = Freeze(new Pen(Brushes.Black, 1));
		internal protected static readonly Brush MirrorBrush = Freeze(new SolidColorBrush(Color.FromArgb(128, 192, 192, 255)));


		static T Freeze<T>(T obj) where T : Freezable {
			if (obj.CanFreeze) obj.Freeze();
			return obj;
		}

		protected readonly double CellSize;
		protected readonly Transform DefaultScaleTransform;

		public MirrorDrawing(double cellSize) {
			CellSize = cellSize;
			DefaultScaleTransform = Freeze(new ScaleTransform(cellSize, cellSize));
		}

		public virtual Drawing GetLightSource(LightColor color) {
			const double GunRange = 0.05;
			const double CoreRange = 0.16;
			const double CoreRange2 = CoreRange * 0.75;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5, 0.5 + GunRange), true, false);
					sgc.LineTo(new Point(0.8, 0.5 + GunRange), true, false);
					sgc.LineTo(new Point(0.8, 0.5 - GunRange), false, false);
					sgc.LineTo(new Point(0.5, 0.5 - GunRange), true, false);
					sgc.ArcTo(new Point(0.5, 0.5 + GunRange), new Size(0.2, 0.2), 0, true, SweepDirection.Counterclockwise, true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(Brushes.Black, BlackPen, geometry);

				var ellipse = new EllipseGeometry(new Rect(0.5 - CoreRange - 0.195, 0.5 - CoreRange, CoreRange * 2, CoreRange * 2)) {
					Transform = DefaultScaleTransform
				};
				ellipse.Freeze();
				dc.DrawGeometry(GetBrush(color), null, ellipse);

				ellipse = new EllipseGeometry(new Rect(0.5 - CoreRange2 - 0.195, 0.5 - CoreRange2, CoreRange2 * 2, CoreRange2 * 2)) {
					Transform = DefaultScaleTransform
				};
				ellipse.Freeze();
				dc.DrawGeometry(GetDarkBrush(color), null, ellipse);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetReflection() {
			const double WidthRange = 0.08;
			const double HeightRange = 0.35;
			const double ReflectionHeight = 0.31;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = Freeze(new RectangleGeometry(new Rect(0.5 - WidthRange, 0.5 - HeightRange, WidthRange * 2, HeightRange * 2)));
				var reflectionGeometry = new RectangleGeometry(new Rect(0.5, 0.5 - ReflectionHeight, WidthRange, ReflectionHeight * 2));
				var backgeometry = Geometry.Combine(geometry, reflectionGeometry, GeometryCombineMode.Exclude, null);
				backgeometry.Transform = DefaultScaleTransform;
				backgeometry.Freeze();
				dc.DrawGeometry(Brushes.Black, null, backgeometry);

				reflectionGeometry.Transform = DefaultScaleTransform;
				reflectionGeometry.Freeze();
				dc.DrawGeometry(MirrorBrush, null, reflectionGeometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetBeveledReflection() {
			const double TailRange = 0.2;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var tailRect = Freeze(new RectangleGeometry(new Rect(0.5 - TailRange, 0.5 - TailRange / 2, TailRange, TailRange)) {
					Transform = DefaultScaleTransform
				});
				dc.PushTransform(Freeze(new RotateTransform(-22.5, CellSize / 2, CellSize / 2)));
				dc.DrawDrawing(GetReflection());
				dc.DrawGeometry(Brushes.Black, null, tailRect);
				dc.Pop();
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetSplitting() {
			const double BorderHalfWidth = 0.12;
			const double BorderHeight = 0.05;
			const double MirrorHalfWidth = 0.08;
			const double MirrorHalfHeight = 0.30;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var mirrorGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - MirrorHalfWidth, 0.5 - MirrorHalfHeight, MirrorHalfWidth * 2, MirrorHalfHeight * 2)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(MirrorBrush, null, mirrorGeometry);

				var borderGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - BorderHalfWidth, 0.5 - MirrorHalfHeight - BorderHeight, BorderHalfWidth * 2, BorderHeight)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, borderGeometry);
				borderGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - BorderHalfWidth, 0.5 + MirrorHalfHeight, BorderHalfWidth * 2, BorderHeight)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, borderGeometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetDiscolor() {
			const double WidthRange = 0.35;
			const double HeightRange = 0.1;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 - WidthRange, 0.5 - HeightRange), true, false);
					sgc.LineTo(new Point(0.5 + WidthRange, 0.5 - HeightRange), true, false);
					sgc.LineTo(new Point(0.5 + WidthRange, 0.5 + HeightRange), false, false);
					sgc.LineTo(new Point(0.5 - WidthRange, 0.5 + HeightRange), true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();

				var brush = new LinearGradientBrush(
					new GradientStopCollection(new[] {
						new GradientStop(Color.FromRgb(White, Black, Black), 0 / 6.0),
						//new GradientStop(Color.FromRgb(White, White, Black), 1 / 6.0),
						new GradientStop(Color.FromRgb(Black, White, Black), 2 / 6.0),
						//new GradientStop(Color.FromRgb(Black, White, White), 3 / 6.0),
						new GradientStop(Color.FromRgb(Black, Black, White), 4 / 6.0),
						//new GradientStop(Color.FromRgb(White, Black, White), 5 / 6.0),
						new GradientStop(Color.FromRgb(White, Black, Black), 6 / 6.0),
					}), 0);
				brush.Freeze();
				dc.DrawGeometry(brush, BlackPen, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetQuantum() {
			const double GunRange = 0.1;
			const double GunHeight = 0.25;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 + GunRange + GunHeight, 0.5 - GunRange), true, false);
					sgc.LineTo(new Point(0.5 + GunRange, 0.5 - GunRange), true, false);
					sgc.LineTo(new Point(0.5 + GunRange, 0.5 - GunRange - GunHeight), true, false);
					sgc.LineTo(new Point(0.5 - GunRange, 0.5 - GunRange - GunHeight), false, false);
					sgc.LineTo(new Point(0.5 - GunRange, 0.5 + GunRange + GunHeight), true, false);
					sgc.LineTo(new Point(0.5 + GunRange, 0.5 + GunRange + GunHeight), false, false);
					sgc.LineTo(new Point(0.5 + GunRange, 0.5 + GunRange), true, false);
					sgc.LineTo(new Point(0.5 + GunRange + GunHeight, 0.5 + GunRange), true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		static void ComplexMultiply(ref double real, ref double imag, double otherReal, double otherImag) {
			var r = real * otherReal - imag * otherImag;
			var i = real * otherImag + imag * otherReal;
			real = r;
			imag = i;
		}

		private Drawing GetTargetLight_Ver0(TargetLight targetLight) {
			const double BigRange = 0.4;
			const double SmallRange = 0.3;
			double rotateReal = Math.Cos(Math.PI / 16), rotateImag = Math.Sin(Math.PI / 16);

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 + BigRange, 0.5), true, true);

					double x = 1, y = 0;

					for (int i = 0; i < 8; i++) {
						ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + BigRange * x, 0.5 + BigRange * y), true, false);
						sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
						ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
						ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
						sgc.LineTo(new Point(0.5 + BigRange * x, 0.5 + BigRange * y), true, false);
						ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					}
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				var pen = new Pen() {
					Thickness = 1,
					Brush = GetDarkBrush(targetLight.Color)
				};
				targetLight.ActivatedChanged += delegate {
					if (targetLight.Activated) {
						pen.Brush = GetBrush(targetLight.Color);
					} else {
						pen.Brush = GetDarkBrush(targetLight.Color);
					}
				};
				dc.DrawGeometry(null, pen, geometry);
			}
			return drawingVisual.Drawing;
		}

		public virtual Drawing GetTargetLight(TargetLight targetLight) {
			const double BigRange = 0.43;
			const double SmallRange = 0.34;
			double rotateReal = Math.Cos(Math.PI * 2 / 3), rotateImag = Math.Sin(Math.PI * 2 / 3);

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry { };
				using (var sgc = geometry.Open()) {
					double x = 0, y = 1;
					sgc.BeginFigure(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), false, true);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);

					x = 0; y = -1;
					sgc.BeginFigure(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), false, true);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();

				var brush = new SolidColorBrush(GetDarkColor(targetLight.Color));
				var lightColor = GetColor(targetLight.Color);
				var pen = new Pen() {
					Thickness = 1,
					Brush = brush
				};
				targetLight.ActivatedChanged += delegate {
					if (targetLight.Activated) {
						brush.Color = lightColor;
					} else {
						brush.Color = GetDarkColor(targetLight.Color);
					}
				};
				dc.DrawGeometry(null, pen, geometry);

				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), SmallRange, SmallRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, pen, ellipse);
				ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), BigRange, BigRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, pen, ellipse);
				
			}
			return drawingVisual.Drawing;
		}

		public virtual Drawing GetBlackLight(BlackLight blackLight) {
			const double BigRange = 0.43;
			const double SmallRange = 0.34;
			double rotateReal = Math.Cos(Math.PI * 2 / 3), rotateImag = Math.Sin(Math.PI * 2 / 3);

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry { };
				using (var sgc = geometry.Open()) {
					double x = 0, y = 1;
					sgc.BeginFigure(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), false, true);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);

					x = 0; y = -1;
					sgc.BeginFigure(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), false, true);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
					ComplexMultiply(ref x, ref y, rotateReal, rotateImag);
					sgc.LineTo(new Point(0.5 + SmallRange * x, 0.5 + SmallRange * y), true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();

				var defaultColor = Color.FromRgb(0x40, 0x40, 0x40);
				var brush = new SolidColorBrush(defaultColor);
				var pen = new Pen() {
					Thickness = 1,
					Brush = brush
				};
				blackLight.ActivatedChanged += delegate {
					if (blackLight.Activated) {
						brush.Color = Colors.Black;
					} else {
						brush.Color = defaultColor;
					}
				};
				dc.DrawGeometry(null, pen, geometry);

				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), SmallRange, SmallRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, pen, ellipse);
				ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), BigRange, BigRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, Freeze(new Pen(Brushes.Black, 1)), ellipse);
			}
			return drawingVisual.Drawing;
		}

		public virtual Drawing GetPrism() {
			const double Left = -0.13;
			const double Right = 0.2;
			const double HeightRange = 0.42;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 + Left, 0.5 - HeightRange), true, true);
					sgc.LineTo(new Point(0.5 + Left, 0.5 + HeightRange), true, false);
					sgc.LineTo(new Point(0.5 + Right, 0.5), true, false);
				}
				var rotate = Freeze(new RotateTransform(-22.5, 0.5, 0.5));
				var scale = DefaultScaleTransform;

				geometry.Transform = Freeze(new MatrixTransform(rotate.Value * scale.Value));
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, null, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		static Geometry GetBinaryGeometry() {
			const double OutRange = 0.45;
			const double InRange = 0.35;
			const double Caliber = 0.2;
			const double HalfCaliber = Caliber / 2;

			var turningPoint = (1 - Math.Sqrt(2) / 2) * Caliber;
			var inOffset = InRange / Math.Sqrt(2);
			var caliberOffset = Caliber / Math.Sqrt(2);
			var geometry = new StreamGeometry();
			using (var sgc = geometry.Open()) {
				var p = new Point(0.5 + OutRange, 0.5 - HalfCaliber);
				sgc.BeginFigure(p, true, false);
				p.Offset(Caliber / 4 - OutRange, 0);
				sgc.LineTo(p, true, false);
				p.Offset(-(inOffset - Caliber / 4), -(inOffset - Caliber / 4));
				sgc.LineTo(p, true, false);
				p.Offset(-caliberOffset, caliberOffset);
				sgc.LineTo(p, false, false);
				p.Offset(inOffset - HalfCaliber, inOffset - HalfCaliber);
				sgc.LineTo(p, true, false);

				p.Offset(-(inOffset - HalfCaliber), inOffset - HalfCaliber);
				sgc.LineTo(p, true, false);
				p.Offset(caliberOffset, caliberOffset);
				sgc.LineTo(p, false, false);
				p.Offset((inOffset - Caliber / 4), -(inOffset - Caliber / 4));
				sgc.LineTo(p, true, false);
				p.Offset(-(Caliber / 4 - OutRange), 0);
				sgc.LineTo(p, true, false);
			}
			return geometry;
		}

		public virtual Drawing GetXor() {
			const double EllipseRange = 0.18;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = GetBinaryGeometry();
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);

				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), EllipseRange, EllipseRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, ellipse);

				var whitePen = Freeze(new Pen(Brushes.White, 1));
				var line = Freeze(new LineGeometry(new Point(0.5 - EllipseRange, 0.5), new Point(0.5 + EllipseRange, 0.5)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, whitePen, line);
				line = Freeze(new LineGeometry(new Point(0.5, 0.5 - EllipseRange), new Point(0.5, 0.5 + EllipseRange)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, whitePen, line);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetAnd() {
			const double EllipseRange = 0.18;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = GetBinaryGeometry();
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);

				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), EllipseRange, EllipseRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, ellipse);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetOr() {
			const double EllipseRange = 0.18;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = GetBinaryGeometry();
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);

				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), EllipseRange, EllipseRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, ellipse);

				var whitePen = Freeze(new Pen(Brushes.White, 1));
				var line = Freeze(new LineGeometry(new Point(0.5 - EllipseRange, 0.5), new Point(0.5 + EllipseRange, 0.5)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(null, whitePen, line);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetFilter(LightColor color) {
			const double EllipseRange = 0.35;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var ellipse = Freeze(new EllipseGeometry(new Point(0.5, 0.5), EllipseRange, EllipseRange) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(MirrorBrush, GetDarkPen(color), ellipse);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetOneWay() {
			const double HeadHeight = 0.4;
			const double TailHeight = 0.4;
			const double HeadRange = 0.4;
			const double TailRange = 0.15;
			const double Cave = TailRange;
			const double Caliber = 0.16;
			const double HalfCaliber = Caliber / 2;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					var p = new Point(0.5 + HeadHeight, 0.5);
					sgc.BeginFigure(p, true, true);
					p.Offset(-HeadHeight, -HeadRange);
					sgc.LineTo(p, true, false);
					p.Offset(0, HeadRange - TailRange);
					sgc.LineTo(p, true, false);
					p.Offset(-TailHeight, 0);
					sgc.LineTo(p, true, false);
					p.Offset(Cave, TailRange);
					sgc.LineTo(p, true, false);
					p.Offset(-Cave, TailRange);
					sgc.LineTo(p, true, false);
					p.Offset(TailHeight, 0);
					sgc.LineTo(p, true, false);
					p.Offset(0, HeadRange - TailRange);
					sgc.LineTo(p, true, false);
				}
				geometry.Freeze();
				var rect = Freeze(new RectangleGeometry(new Rect(0, 0.5 - HalfCaliber, 1, Caliber)));
				var border = Freeze(Geometry.Combine(geometry, rect, GeometryCombineMode.Exclude, DefaultScaleTransform));
				var mirror = Freeze(Geometry.Combine(geometry, rect, GeometryCombineMode.Intersect, DefaultScaleTransform));
				dc.DrawGeometry(Brushes.Black, null, border);
				dc.DrawGeometry(MirrorBrush, null, mirror);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetTwoWay() {
			const double WidthRange = 0.35;
			const double HalfCaliber = 0.1;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 - WidthRange, 0.5 - HalfCaliber), true, false);
					sgc.LineTo(new Point(0.5 + WidthRange, 0.5 - HalfCaliber), true, false);
					sgc.LineTo(new Point(0.5 + WidthRange, 0.5 + HalfCaliber), false, false);
					sgc.LineTo(new Point(0.5 - WidthRange, 0.5 + HalfCaliber), true, false);
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetObstacle() {
			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				dc.DrawRectangle(Freeze(new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60))), null, new Rect(0, 0, CellSize, CellSize));

				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(1, CellSize), false, false);
					sgc.LineTo(new Point(1, 1), true, false);
					sgc.LineTo(new Point(CellSize, 1), true, false);
				}
				geometry.Freeze();
				dc.DrawGeometry(null, Freeze(new Pen(GetDarkBrush(LightColor.White), 2)), geometry);

				geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0, CellSize - 1), false, false);
					sgc.LineTo(new Point(CellSize - 1, CellSize - 1), true, false);
					sgc.LineTo(new Point(CellSize - 1, 0), true, false);
				}
				geometry.Freeze();
				dc.DrawGeometry(null, Freeze(new Pen(new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)), 2)), geometry);

			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetBlockade() {
			const double Range = 0.4;
			const double Caliber = 0.16;
			double rotateReal = Math.Cos(Math.PI / 4), rotateImag = -Math.Sin(Math.PI / 4);

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var smallRange = Caliber / 2 / Math.Tan(Math.PI / 8);
				double x1 = Range, y1 = 0;
				double x2 = Range, y2 = -Caliber / 2;
				double x3 = smallRange, y3 = -Caliber / 2;
				double x4 = Range, y4 = Caliber / 2;

				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 + x1, 0.5 + y1), true, false);

					for (int i = 0; i < 8; i++) {
						sgc.LineTo(new Point(0.5 + x2, 0.5 + y2), false, false);
						ComplexMultiply(ref x2, ref y2, rotateReal, rotateImag);

						sgc.LineTo(new Point(0.5 + x3, 0.5 + y3), true, false);
						ComplexMultiply(ref x3, ref y3, rotateReal, rotateImag);

						ComplexMultiply(ref x4, ref y4, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + x4, 0.5 + y4), true, false);

						ComplexMultiply(ref x1, ref y1, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + x1, 0.5 + y1), false, false);
					}
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(MirrorBrush, BlackPen, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetWormhole(LightColor color) {
			const double Range = 0.32;
			const double Caliber = 0.16;
			double rotateReal = Math.Cos(Math.PI / 4), rotateImag = -Math.Sin(Math.PI / 4);

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var smallRange = Caliber / 2 / Math.Tan(Math.PI / 8);
				double x1 = Range, y1 = 0;
				double x2 = Range, y2 = -Caliber / 2;
				double x3 = smallRange, y3 = -Caliber / 2;
				double x4 = Range, y4 = Caliber / 2;

				var geometry = new StreamGeometry();
				using (var sgc = geometry.Open()) {
					sgc.BeginFigure(new Point(0.5 + x1, 0.5 + y1), true, false);

					for (int i = 0; i < 8; i++) {
						sgc.LineTo(new Point(0.5 + x2, 0.5 + y2), true, false);
						ComplexMultiply(ref x2, ref y2, rotateReal, rotateImag);

						sgc.LineTo(new Point(0.5 + x3, 0.5 + y3), true, false);
						ComplexMultiply(ref x3, ref y3, rotateReal, rotateImag);

						ComplexMultiply(ref x4, ref y4, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + x4, 0.5 + y4), true, false);

						ComplexMultiply(ref x1, ref y1, rotateReal, rotateImag);
						sgc.LineTo(new Point(0.5 + x1, 0.5 + y1), true, false);
					}
				}
				geometry.Transform = DefaultScaleTransform;
				geometry.Freeze();
				dc.DrawGeometry(GetBrush(color), BlackPen, geometry);
			}
			return Freeze(drawingVisual.Drawing);
		}

		public virtual Drawing GetColorSplitting(LightColor color) {
			const double BorderHalfWidth = 0.12;
			const double BorderHeight = 0.05;
			const double MirrorHalfWidth = 0.08;
			const double MirrorHalfHeight = 0.30;
			const double CenterMirrorHalfWidth = 0.05;

			var drawingVisual = new DrawingVisual();
			using (var dc = drawingVisual.RenderOpen()) {
				var mirrorGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - MirrorHalfWidth, 0.5 - MirrorHalfHeight, MirrorHalfWidth * 2, MirrorHalfHeight * 2)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(MirrorBrush, null, mirrorGeometry);

				mirrorGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - CenterMirrorHalfWidth, 0.5 - MirrorHalfHeight, CenterMirrorHalfWidth * 2, MirrorHalfHeight * 2)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(GetDarkBrush(color), null, mirrorGeometry);

				var borderGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - BorderHalfWidth, 0.5 - MirrorHalfHeight - BorderHeight, BorderHalfWidth * 2, BorderHeight)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, borderGeometry);
				borderGeometry = Freeze(new RectangleGeometry(new Rect(0.5 - BorderHalfWidth, 0.5 + MirrorHalfHeight, BorderHalfWidth * 2, BorderHeight)) {
					Transform = DefaultScaleTransform
				});
				dc.DrawGeometry(Brushes.Black, null, borderGeometry);
			}
			return Freeze(drawingVisual.Drawing);
		}
	}
}
