using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace 反射镜.UI {
	public class ChessmanPreviewDragEventArgs : RoutedEventArgs {
		public Point StartPointRelativeToCanvas { get; set; }
		public Point PointRelativeToCanvas { get; set; }
		public bool Cancel { get; set; } = false;
		public Checkerboard OldCheckerboard { get; }
		public int OldColumn { get; }
		public int OldRow { get; }

		public ChessmanPreviewDragEventArgs(RoutedEvent routedEvent, object source, Checkerboard oldCheckerboard, int oldColumn, int oldRow)
			: base(routedEvent, source) {
			OldCheckerboard = oldCheckerboard;
			OldColumn = oldColumn;
			OldRow = oldRow;
		}
	}

	public class ChessmanDragEventArgs : ChessmanPreviewDragEventArgs {
		public Checkerboard NewCheckerboard { get; }
		public int NewColumn { get; }
		public int NewRow { get; }

		public ChessmanDragEventArgs(RoutedEvent routedEvent, object source, Checkerboard oldCheckerboard, int oldColumn, int oldRow, Checkerboard newCheckerboard, int newColumn, int newRow) 
			: base(routedEvent, source, oldCheckerboard, oldColumn, oldRow) {
			NewCheckerboard = newCheckerboard;
			NewColumn = newColumn;
			NewRow = newRow;
		}
	}
}
