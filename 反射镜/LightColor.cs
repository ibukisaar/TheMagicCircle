using System;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	[Flags]
	public enum LightColor {
		None = 0,
		Red = 1,
		Green = 2,
		Blue = 4,
		/// <summary>
		/// 青色
		/// </summary>
		Cyan = Green | Blue,
		Yellow = Red | Green,
		/// <summary>
		/// 洋红
		/// </summary>
		Magenta = Red | Blue,
		White = Red | Green | Blue
	}
}
