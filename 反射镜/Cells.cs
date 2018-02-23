using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace 反射镜 {
	public interface IColor {
		LightColor Color { get; }
	}

	public interface IMap {
		Map Map { get; set; }
	}

	public abstract class Cell : ICloneable, IComparable<Cell> {
		static Func<Cell> CreateConstructor(Type t) {
			var @new = Expression.New(t.GetConstructor(Type.EmptyTypes));
			var lambda = Expression.Lambda<Func<Cell>>(@new);
			if (lambda.CanReduce) lambda.Reduce();
			return lambda.Compile();
		}

		static Func<LightColor, Cell> CreateColorConstructor(Type t) {
			var arg = Expression.Parameter(typeof(LightColor));
			var @new = Expression.New(t.GetConstructor(new[] { typeof(LightColor) }), arg);
			var lambda = Expression.Lambda<Func<LightColor, Cell>>(@new, new[] { arg });
			if (lambda.CanReduce) lambda.Reduce();
			return lambda.Compile();
		}

		static readonly Dictionary<int, Delegate> ctors = new Dictionary<int, Delegate>(256);

		static Cell() {
			var types = Assembly.GetCallingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Cell)) && !t.IsAbstract);
			foreach (var t in types) {
				if (typeof(IColor).IsAssignableFrom(t)) {
					var ctor = CreateColorConstructor(t);
					ctors.Add(ctor(LightColor.None).Id, ctor);
				} else {
					var ctor = CreateConstructor(t);
					ctors.Add(ctor().Id, ctor);
				}
			}
		}


		public abstract int Id { get; }
		public virtual bool Freedom { get; set; } = true;
		public virtual bool CanRotate => true;
		public Direction Direction { get; set; }
		public int X { get; set; } = -1;
		public int Y { get; set; } = -1;
		/// <summary>
		/// 当需要多个输入时，<seealso cref="Lazy"/>为true。
		/// </summary>
		public virtual bool Lazy => false;

		public object Clone() {
			var result = InternalClone();
			result.X = X;
			result.Y = Y;
			result.Direction = Direction;
			result.Freedom = Freedom;
			return result;
		}

		protected abstract Cell InternalClone();

		internal protected virtual IReadOnlyList<Light> Apply(ShortLight light) {
			return new[] { light.Go(Direction._0) };
		}

		internal protected virtual void Reset() { }

		protected static IReadOnlyList<Light> GetComplexLight(int x, int y, LightColor color, Direction dir) {
			var result = new List<Light>(3);
			if (color.HasFlag(LightColor.Red)) result.Add(new ShortLight(x, y, LightColor.Red, dir));
			if (color.HasFlag(LightColor.Green)) result.Add(new ShortLight(x, y, LightColor.Green, dir));
			if (color.HasFlag(LightColor.Blue)) result.Add(new ShortLight(x, y, LightColor.Blue, dir));
			return result.Count > 0 ? result : null;
		}

		public int CompareTo(Cell other) {
			int cmp = Id.CompareTo(other.Id);
			if (cmp != 0) return cmp;
			if (this is IColor thisColor && other is IColor otherColor) {
				return thisColor.Color.CompareTo(otherColor.Color);
			}
			return cmp;
		}

		const int IdBits = 6;

		public virtual void Encode(IBitStreamWriter writer, bool ignoreDirection = false) {
			writer.Write(Id, IdBits);
			if (this is IColor color) {
				writer.Write((int)color.Color, 3);
			}
			writer.Write(Freedom);
			if (CanRotate && !ignoreDirection) {
				writer.Write((int)Direction, 3);
			}
		}

		public static Cell Decode(IBitStreamReader reader, Map map, bool ignoreDirection = false) {
			var len = reader.Read(out var id, IdBits);
			if (len < IdBits) throw new System.IO.EndOfStreamException($"未能读取完整{nameof(Id)}");
			Cell cell;
			if (ctors[id] is Func<LightColor, Cell> colorCellCtor) {
				len = reader.Read(out var color, 3);
				if (len < 3) throw new System.IO.EndOfStreamException($"未能读取完整{nameof(IColor.Color)}");
				cell = colorCellCtor((LightColor)color);
			} else {
				cell = ctors[id].DynamicInvoke() as Cell;
			}
			len = reader.Read(out var freedom, 1);
			if (len < 1) throw new System.IO.EndOfStreamException($"未能读取完整{nameof(Freedom)}");
			cell.Freedom = freedom != 0;
			if (cell.CanRotate && !ignoreDirection) {
				len = reader.Read(out var dir, 3);
				if (len < 3) throw new System.IO.EndOfStreamException($"未能读取完整{nameof(CanRotate)}");
				cell.Direction = (Direction)dir;
			}
			if (cell is IMap mapCell) {
				mapCell.Map = map;
			}
			return cell;
		}
	}

	/// <summary>
	/// 障碍
	/// </summary>
	public sealed class Obstacle : Cell {
		public override int Id => 0;

		public override bool CanRotate => false;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			return null;
		}

		protected override Cell InternalClone() => new Obstacle();
	}

	/// <summary>
	/// 光源
	/// </summary>
	public class LightSource : Cell, IColor {
		public override int Id => 1;
		public LightColor Color { get; }

		public LightSource(LightColor color) => Color = color;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			if (light != null) return null;
			return GetComplexLight(X, Y, Color, Direction);
		}

		protected override Cell InternalClone() => new LightSource(Color);
	}

	/// <summary>
	/// 反射镜
	/// </summary>
	public sealed class Reflection : Cell {
		public override int Id => 2;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			var (x, y) = light.Go();
			switch (dir) {
				case Direction._135: return new[] { light.Go(Direction._90) };
				case Direction._180: return new[] { light.Go(Direction._180) };
				case Direction._225: return new[] { light.Go(Direction._270) };
				default: return null;
			}
		}

		protected override Cell InternalClone() => new Reflection();
	}

	/// <summary>
	/// 斜反射镜（镜面朝向22.5°为<see cref="Direction._0"/>）
	/// </summary>
	public sealed class BeveledReflection : Cell {
		public override int Id => 3;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			switch (dir) {
				case Direction._90: return new[] { light.Go(Direction._45) };
				case Direction._135: return new[] { light.Go(Direction._135) };
				case Direction._180: return new[] { light.Go(Direction._225) };
				case Direction._225: return new[] { light.Go(Direction._315) };
				default: return null;
			}
		}

		protected override Cell InternalClone() => new BeveledReflection();
	}

	/// <summary>
	/// 分光镜
	/// </summary>
	public sealed class Splitting : Cell {
		public override int Id => 4;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction).Mod(2);

			switch (dir) {
				case Direction._0: return new[] { light.Go(Direction._0), light.Go(Direction._180) };
				case Direction._45: return new[] { light.Go(Direction._0), light.Go(Direction._270) };
				case Direction._135: return new[] { light.Go(Direction._0), light.Go(Direction._90) };
				default: return null;
			}
		}

		protected override Cell InternalClone() => new Splitting();
	}

	/// <summary>
	/// 变色镜
	/// </summary>
	public sealed class Discolor : Cell {
		public override int Id => 5;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			var (x, y) = light.Go();
			switch (dir) {
				case Direction._0: return new[] { new ShortLight(x, y, light.Color.Add(1), light.Direction) };
				case Direction._180: return new[] { new ShortLight(x, y, light.Color.Add(-1), light.Direction) };
				default: return null;
			}
		}

		protected override Cell InternalClone() => new Discolor();
	}

	/// <summary>
	/// 量子镜
	/// </summary>
	public sealed class Quantum : Cell {
		public override int Id => 6;

		internal protected override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			if (dir == Direction._180) {
				var light1 = light.Go(Direction._90);
				var light2 = light.Go(Direction._270);
				return new[] { new EntanglementLight(light1, light2) };
			}
			return null;
		}

		protected override Cell InternalClone() => new Quantum();
	}

	public abstract class TargetCell : Cell {
		public override bool CanRotate => false;

		private bool activated = false;

		public bool Activated {
			get => activated;
			protected set {
				if (activated != value) {
					activated = value;
					ActivatedChanged?.Invoke(this);
				}
			}
		}

		public virtual bool Success => Activated;

		public event Action<TargetCell> ActivatedChanged;

		protected internal override void Reset() {
			Activated = false;
		}

		protected abstract bool InternalSetActivated(LightSet halfStart, LightSet halfEnd);

		public void SetActivated(LightSet halfStart, LightSet halfEnd) {
			Activated = InternalSetActivated(halfStart, halfEnd);
		}

		protected static LightSet GetCompleteLights(LightSet halfStart, LightSet halfEnd, bool sameDir = false) {
			var complete = new LightSet();
			void AddColorToComplete(LightColor color, Direction dir) {
				if (color.HasFlag(LightColor.Red)) complete.Add(LightColor.Red, dir);
				if (color.HasFlag(LightColor.Green)) complete.Add(LightColor.Green, dir);
				if (color.HasFlag(LightColor.Blue)) complete.Add(LightColor.Blue, dir);
			}

			if (sameDir) {
				for (int i = 0; i < 8; i++) {
					var dir = (Direction)i;
					var andColor = halfStart.GetColor(dir) & halfEnd.GetColor(dir);
					AddColorToComplete(andColor, dir);
				}
			} else {
				for (int i = 0; i < 4; i++) {
					var dir = (Direction)i;
					var andColor = (halfStart.GetColor(dir) | halfEnd.GetColor(dir.Reverse()))
						& (halfStart.GetColor(dir.Reverse()) | halfEnd.GetColor(dir));
					AddColorToComplete(andColor, dir);
				}
			}
			return complete;
		}
	}

	public sealed class TargetLight : TargetCell, IColor {
		public override int Id => 7;

		public LightColor Color { get; }

		public TargetLight(LightColor color) => Color = color;

		protected override bool InternalSetActivated(LightSet halfStart, LightSet halfEnd) {
			var complete = GetCompleteLights(halfStart, halfEnd);
			if (complete.AllColor != Color) return false;
			if ((halfStart.AllColor | Color) != Color || (halfEnd.AllColor | Color) != Color) return false;
			return true;
		}

		protected override Cell InternalClone() => new TargetLight(Color);
	}

	public sealed class BlackLight : TargetCell {
		public override int Id => 8;

		public override bool Success => !Activated;

		protected override bool InternalSetActivated(LightSet halfStart, LightSet halfEnd) {
			return !(halfStart.IsEmpty && halfEnd.IsEmpty);
		}

		protected override Cell InternalClone() => new BlackLight();
	}

	public abstract class Binary : Cell {
		public sealed override bool Lazy => true;

		private LightColor left;
		private LightColor right;

		protected internal sealed override IReadOnlyList<Light> Apply(ShortLight light) {
			if (light != null) {
				var dir = Direction.Sub(light.Direction);
				switch (dir) {
					case Direction._315: left |= light.Color; break;
					case Direction._45: right |= light.Color; break;
				}
				return null;
			} else {
				var outColor = Operate(left, right);
				return GetComplexLight(X, Y, outColor, Direction);
			}
		}

		protected abstract LightColor Operate(LightColor left, LightColor right);

		protected internal override void Reset() {
			left = right = LightColor.None;
		}
	}

	public sealed class Xor : Binary {
		public override int Id => 9;

		protected override LightColor Operate(LightColor left, LightColor right)
			 => left ^ right;

		protected override Cell InternalClone() => new Xor();
	}

	public sealed class And : Binary {
		public override int Id => 10;

		protected override LightColor Operate(LightColor left, LightColor right)
			 => left & right;

		protected override Cell InternalClone() => new And();
	}

	public sealed class Or : Binary {
		public override int Id => 11;

		protected override LightColor Operate(LightColor left, LightColor right)
			 => left | right;

		protected override Cell InternalClone() => new Or();
	}

	/// <summary>
	/// 三棱镜
	/// </summary>
	public sealed class Prism : Cell {
		public override int Id => 12;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			switch (dir) {
				case Direction._0: return null;
				case Direction._45:
					return light.Color == LightColor.Red ? new[] { light.Go(Direction._0) } :
						light.Color == LightColor.Green ? new[] { light.Go(Direction._45) } :
						light.Color == LightColor.Blue ? new[] { light.Go(Direction._90) } : null;
				case Direction._90:
					return light.Color == LightColor.Red ? new[] { light.Go(Direction._0) } : null;
				case Direction._135:
					return light.Color == LightColor.Green ? new[] { light.Go(Direction._45) } :
						light.Color == LightColor.Blue ? new[] { light.Go(Direction._270) } : null;
				case Direction._180:
					return light.Color == LightColor.Green ? new[] { light.Go(Direction._45.Neg()) } :
						light.Color == LightColor.Blue ? new[] { light.Go(Direction._270.Neg()) } : null;
				case Direction._225:
					return light.Color == LightColor.Red ? new[] { light.Go(Direction._0.Neg()) } : null;
				case Direction._270:
					return light.Color == LightColor.Red ? new[] { light.Go(Direction._0.Neg()) } :
						light.Color == LightColor.Green ? new[] { light.Go(Direction._45.Neg()) } :
						light.Color == LightColor.Blue ? new[] { light.Go(Direction._90.Neg()) } : null;
				case Direction._315: return null;
			}
			return null;
		}

		protected override Cell InternalClone() => new Prism();
	}

	public sealed class Filter : Cell, IColor {
		public override int Id => 13;
		public override bool CanRotate => false;
		public LightColor Color { get; }

		public Filter(LightColor color) => Color = color;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			return (Color & light.Color) != LightColor.None ? new[] { light.Go(Direction._0) } : null;
		}

		protected override Cell InternalClone() => new Filter(Color);
	}

	public sealed class OneWay : Cell {
		public override int Id => 14;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction);
			return dir == Direction._0 ? new[] { light.Go(Direction._0) } : null;
		}

		protected override Cell InternalClone() => new OneWay();
	}

	public sealed class TwoWay : Cell {
		public override int Id => 15;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction).Mod(2);
			return dir == Direction._0 ? new[] { light.Go(Direction._0) } : null;
		}

		protected override Cell InternalClone() => new TwoWay();
	}

	public sealed class Blockade : Cell {
		public override int Id => 16;
		public override bool CanRotate => false;

		protected override Cell InternalClone() => new Blockade();
	}

	public sealed class Wormhole : Cell, IColor, IMap {
		public override int Id => 17;
		public override bool CanRotate => false;
		public LightColor Color { get; }
		public Map Map { get; set; }

		public Wormhole(LightColor color) => Color = color;
		public Wormhole(LightColor color, Map map) : this(color) => Map = map;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			if (!Color.HasFlag(light.Color)) return new[] { light.Go(Direction._0) };
			if (Map == null) return null;
			var (x, y) = light.Go();
			while (true) {
				(x, y) = ShortLight.Go(light.Direction, x, y);
				if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height) return null;
				if (Map.Cells[x, y] is Wormhole other && other.Color.HasFlag(light.Color)) {
					return new[] { new ShortLight(x, y, light.Color, light.Direction) };
				}
			}
		}

		protected override Cell InternalClone() => new Wormhole(Color, Map);
	}

	public sealed class ColorSplitting : Cell, IColor {
		public override int Id => 18;
		public LightColor Color { get; }

		public ColorSplitting(LightColor color) => Color = color;

		protected internal override IReadOnlyList<Light> Apply(ShortLight light) {
			var dir = Direction.Sub(light.Direction).Mod(2);
			switch (dir) {
				case Direction._0:
					return Color.HasFlag(light.Color) ? new[] { light.Go(Direction._0) } : new[] { light.Go(Direction._180) };
				case Direction._45:
					return Color.HasFlag(light.Color) ? new[] { light.Go(Direction._0) } : new[] { light.Go(Direction._270) };
				case Direction._135:
					return Color.HasFlag(light.Color) ? new[] { light.Go(Direction._0) } : new[] { light.Go(Direction._90) };
				default: return null;
			}
		}

		protected override Cell InternalClone() => new ColorSplitting(Color);
	}
}