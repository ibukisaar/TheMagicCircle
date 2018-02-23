using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 反射镜.UI {
	public class GameView : Canvas {
		static GameView() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(GameView), new FrameworkPropertyMetadata(typeof(GameView)));
		}

		public static readonly DependencyProperty GameViewProperty =
			DependencyProperty.RegisterAttached("GameView", typeof(GameView), typeof(GameView), new FrameworkPropertyMetadata(null));

		public static GameView GetGameView(UIElement element) => (GameView)element.GetValue(GameViewProperty);
		public static void SetGameView(UIElement element, GameView value) => element.SetValue(GameViewProperty, value);

		public PlayCheckerboard PlayCheckerboard {
			get { return (PlayCheckerboard)GetValue(PlayCheckerboardProperty); }
			set { SetValue(PlayCheckerboardProperty, value); }
		}

		public static readonly DependencyProperty PlayCheckerboardProperty =
			DependencyProperty.Register("PlayCheckerboard", typeof(PlayCheckerboard), typeof(GameView), new PropertyMetadata(null));

		public Checkerboard SecondCheckerboard {
			get { return (Checkerboard)GetValue(SecondCheckerboardProperty); }
			set { SetValue(SecondCheckerboardProperty, value); }
		}

		public static readonly DependencyProperty SecondCheckerboardProperty =
			DependencyProperty.Register("SecondCheckerboard", typeof(Checkerboard), typeof(GameView), new PropertyMetadata(null));

		public byte[] EncodeMap() {
			return PlayCheckerboard.EncodeToBytes(SecondCheckerboard.Children.OfType<Mirror>().Select(mirror => mirror.Cell).ToList());
		}

		public void DecodeMap(byte[] data) {
			var freedomCells = PlayCheckerboard.DecodeFromBytes(data);
			SecondCheckerboard.Children.Clear();
			if (freedomCells != null) {
				var cols = SecondCheckerboard.Columns;
				for (int i = 0; i < freedomCells.Count; i++) {
					Checkerboard.SetColumn(freedomCells[i], i % cols);
					Checkerboard.SetRow(freedomCells[i], i / cols);
					SecondCheckerboard.Children.Add(freedomCells[i]);
				}
			}
		}
	}
}
