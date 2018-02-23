using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace 反射镜 {
	[DebuggerDisplay("Count = {Count}")]
	public struct LightSet : IEnumerable<(LightColor Color, Direction Direction)> {
		public static LightSet Red => new LightSet { lights = 0xff };
		public static LightSet Green => new LightSet { lights = 0xff00 };
		public static LightSet Blue => new LightSet { lights = 0xff0000 };
		public static LightSet GetDirection(Direction dir) => new LightSet { lights = 0x010101 << (int) dir };

		private static int[] ColorToHash = { -1, 0, 1, -1, 2 };
		private static LightColor[] HashToColor = { LightColor.Red, LightColor.Green, LightColor.Blue };


		private int lights;

		static int ToHash(LightColor color, Direction dir)
			=> ColorToHash[(int) color] * 8 + (int) dir;

		static (LightColor Color, Direction Direction) FromHash(int hash)
			=> (HashToColor[hash >> 3], (Direction) (hash & 7));

		public bool Contains(LightColor color, Direction dir)
			=> (lights & (1 << ToHash(color, dir))) != 0;

		public void Add(LightColor color, Direction dir)
			=> lights |= 1 << ToHash(color, dir);

		public bool Remove(LightColor color, Direction dir) {
			var and = lights & (1 << ToHash(color, dir));
			lights &= ~and;
			return and != 0;
		}

		private IEnumerator<(LightColor Color, Direction Direction)> GetEnumerator(int lights) {
			if (lights == 0) yield break;

			const int Base = 4, Mask = (1 << Base) - 1;
			for (int j = 0; j < 24; j += Base) {
				if (((Mask << j) & lights) == 0) continue;
				for (int i = 0; i < Base; i++) {
					var hash = j + i;
					if ((lights & (1 << hash)) != 0) yield return FromHash(hash);
				}
			}
		}

		public IEnumerator<(LightColor Color, Direction Direction)> GetEnumerator() => GetEnumerator(lights);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public LightColor GetColor(Direction dir) {
			var result = LightColor.None;
			if (Contains(LightColor.Red, dir)) result |= LightColor.Red;
			if (Contains(LightColor.Green, dir)) result |= LightColor.Green;
			if (Contains(LightColor.Blue, dir)) result |= LightColor.Blue;
			return result;
		}

		public LightColor AllColor
			=> ((lights & Red.lights) != 0 ? LightColor.Red : LightColor.None)
			| ((lights & Green.lights) != 0 ? LightColor.Green : LightColor.None)
			| ((lights & Blue.lights) != 0 ? LightColor.Blue : LightColor.None);

		public int Count {
			get {
				var bits = (lights & 0b0101_0101_0101_0101_0101_0101) + ((lights & 0b1010_1010_1010_1010_1010_1010) >> 1);
				bits = (bits & 0b00110011_00110011_00110011) + ((bits & 0b11001100_11001100_11001100) >> 2);
				bits = (bits + (bits >> 4) + (bits >> 8)) & 0b000000001111_000000001111;
				return (bits + (bits >> 12)) & 0xFFF;
			}
		}

		public bool IsEmpty => lights == 0;

		public static LightSet operator &(LightSet left, LightSet right)
			=> new LightSet { lights = left.lights & right.lights };

		public static LightSet operator |(LightSet left, LightSet right)
			=> new LightSet { lights = left.lights | right.lights };

		public static LightSet operator ^(LightSet left, LightSet right)
			=> new LightSet { lights = left.lights ^ right.lights };

		public static LightSet operator ~(LightSet @this)
			=> new LightSet { lights = ~@this.lights };
	}
}
