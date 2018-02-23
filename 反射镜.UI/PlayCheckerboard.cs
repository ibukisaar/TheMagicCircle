using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace 反射镜.UI {
	public class PlayCheckerboard : MainCheckerboard {
		static PlayCheckerboard() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayCheckerboard), new FrameworkPropertyMetadata(typeof(PlayCheckerboard)));
		}

		public override Mirror CreateMirror(Cell cell) {
			return new PlayMirror { Cell = cell };
		}
	}
}
