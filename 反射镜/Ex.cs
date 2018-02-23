using System;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public static class Ex {
		public static Direction Add(this Direction direction, Direction other)
			=> (Direction) (((uint) direction + (uint) other) & 7);

		public static Direction Sub(this Direction direction, Direction other)
			=> (Direction) (((uint) direction - (uint) other) & 7);

		public static Direction Neg(this Direction direction)
			=> (Direction) ((-(uint) direction) & 7);

		public static Direction Reverse(this Direction direction)
			=> direction.Add(Direction._180);

		public static Direction Mod(this Direction direction, int bits)
			=> (Direction) ((uint) direction & ~(0xFFFFFFFFU << bits));


		private static readonly int[] ColorToIndex = { -1, 0, 1, -1, 2 };
		private static readonly LightColor[] IndexToColor = { LightColor.Red, LightColor.Green, LightColor.Blue };

		public static int Sub(this LightColor color, LightColor other)
			=> ColorToIndex[(int) color] - ColorToIndex[(int) other];

		public static LightColor Add(this LightColor color, int diff) {
			int sum = (ColorToIndex[(int) color] + diff) % 3;
			if (sum < 0) sum += 3;
			return IndexToColor[sum];
		}
	}
}
