using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace 反射镜.UI {
	public class PlayMirror : Mirror {
		static PlayMirror() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PlayMirror), new FrameworkPropertyMetadata(typeof(PlayMirror)));
		}

		public PlayMirror() {
			LeftClick += delegate {
				RotateDirection(Direction._45);
			};
			RightClick += delegate {
				RotateDirection(Direction._315);
			};
			MouseDown += (sender, e) => {
				if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed) {
					if (Cell.CanRotate && Freedom) ResetDirection();
				}
			};
		}
	}
}
