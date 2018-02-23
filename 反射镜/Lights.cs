using System;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public abstract class Light {
		internal abstract IReadOnlyList<Light> Handle(Map map);
	}


	public sealed class ShortLight : Light {
		public int X { get; internal set; }
		public int Y { get; internal set; }
		public LightColor Color { get; internal set; }
		public Direction Direction { get; internal set; }

		public ShortLight(int x, int y, LightColor color, Direction direction) {
			X = x;
			Y = y;
			Color = color;
			Direction = direction;
		}

		private static readonly Func<int, int, (int X, int Y)>[] goMap = {
			(x, y) => (x + 1, y),
			(x, y) => (x + 1, y - 1),
			(x, y) => (x, y - 1),
			(x, y) => (x - 1, y - 1),
			(x, y) => (x - 1, y),
			(x, y) => (x - 1, y + 1),
			(x, y) => (x, y + 1),
			(x, y) => (x + 1, y + 1),
		};

		public static (int X, int Y) Go(Direction direction, int x, int y) {
			return goMap[(int) direction](x, y);
		}

		public (int X, int Y) Go() {
			return Go(Direction, X, Y);
		}

		public ShortLight Go(Direction rotate) {
			var (x, y) = Go();
			return new ShortLight(x, y, Color, Direction.Add(rotate));
		}

		internal override IReadOnlyList<Light> Handle(Map map) {
			if (map.Lights[X, Y].Contains(Color, Direction)) return null;
			map.AddLight(this);
			var (x, y) = Go(Direction, X, Y);
			if (x < 0 || y < 0 || x >= map.Width || y >= map.Height) return null;
			if (map.Cells[x, y] is Cell cell) {
				if (cell.Lazy) {
					map.AddLazyCell(cell);
				}
				return cell.Apply(this);
			}
			return new[] { Go(Direction._0) };
		}
	}

	/// <summary>
	/// 纠缠光
	/// </summary>
	public sealed class EntanglementLight : Light {
		public ShortLight Light1 { get; }
		public ShortLight Light2 { get; }

		public EntanglementLight(ShortLight light1, ShortLight light2) {
			Light1 = light1;
			Light2 = light2;
		}

		internal override IReadOnlyList<Light> Handle(Map map) {
			var newLights1 = Light1.Handle(map);
			var newLights2 = Light2.Handle(map);
			var count1 = newLights1?.Count ?? 0;
			var count2 = newLights2?.Count ?? 0;
			if (count1 == 1 && newLights1[0] is ShortLight newLight1 && count2 == 1 && newLights2[0] is ShortLight newLight2) {
				var sub1 = newLight1.Color.Sub(Light1.Color);
				var sub2 = newLight2.Color.Sub(Light2.Color);
				newLight1.Color = newLight1.Color.Add(-sub2);
				newLight2.Color = newLight2.Color.Add(-sub1);
				return new[] { new EntanglementLight(newLight1, newLight2) };
			} else {
				var result = new Light[count1 + count2];
				int index = 0;
				if (count1 > 0) foreach (var light in newLights1) result[index++] = light;
				if (count2 > 0) foreach (var light in newLights2) result[index++] = light;
				return result;
			}
		}
	}
}
