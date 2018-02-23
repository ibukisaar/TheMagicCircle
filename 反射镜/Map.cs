using System;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public class Map : ICloneable {
		public delegate void RefreshCompletedHandler(Map map, bool missionComplete);

		private readonly List<Cell> lazyCells = new List<Cell>();

		public int Width { get; private set; }
		public int Height { get; private set; }
		public LightSet[,] Lights { get; private set; }
		public LightSet[,] EndingLights { get; private set; }
		public Cell[,] Cells { get; private set; }
		public bool MissionComplete { get; private set; }

		public event RefreshCompletedHandler RefreshCompleted;

		private Map() { }

		public Map(int width, int height) {
			Resize(width, height);
		}

		public void Resize(int width, int height) {
			Width = width;
			Height = height;

			Lights = new LightSet[width, height];
			EndingLights = new LightSet[width, height];
			Cells = new Cell[width, height];
		}

		internal void AddLazyCell(Cell cell) {
			lazyCells.Add(cell);
		}

		internal void AddLight(ShortLight light) {
			Lights[light.X, light.Y].Add(light.Color, light.Direction);
			var (goX, goY) = light.Go();
			AddEndingLight(goX, goY, light.Color, light.Direction);
		}

		private void AddEndingLight(int x, int y, LightColor color, Direction dir) {
			if (x < 0 || x >= Width || y < 0 || y >= Height) return;
			EndingLights[x, y].Add(color, dir);
		}

		public void Refresh() {
			lazyCells.Clear();
			Lights = new LightSet[Width, Height];
			EndingLights = new LightSet[Width, Height];
			var queue = new Queue<Light>();
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					Cells[x, y]?.Reset();
					if (Cells[x, y] is LightSource lightSource) {
						lazyCells.Add(lightSource);
					}
				}
			}

			while (lazyCells.Count > 0) {
				foreach (var cell in lazyCells) {
					var lights = cell.Apply(null);
					if (lights != null) {
						foreach (var light in lights) {
							queue.Enqueue(light);
						}
					}
				}
				lazyCells.Clear();

				while (queue.Count > 0) {
					var light = queue.Dequeue();
					var newLights = light.Handle(this);
					if (newLights != null) {
						foreach (var newLight in newLights) {
							queue.Enqueue(newLight);
						}
					}
				}
			}

			bool success = true;
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					if (Cells[x, y] is TargetCell target) {
						target.SetActivated(Lights[x, y], EndingLights[x, y]);
						if (!target.Success) success = false;
					}
				}
			}
			MissionComplete = success;
			RefreshCompleted?.Invoke(this, success);
		}

		public void AddCell(int x, int y, Cell cell) {
			RemoveCell(cell);
			Cells[x, y] = cell;
			cell.X = x;
			cell.Y = y;
		}

		public void RemoveCell(Cell cell) {
			if (cell.X >= 0 && cell.Y >= 0) {
				Cells[cell.X, cell.Y] = null;
				cell.X = -1;
				cell.Y = -1;
			}
		}

		public void Clear() {
			Cells = new Cell[Width, Height];
		}

		public byte[] Encode(IReadOnlyCollection<Cell> secondCells = null) {
			var writer = new BitStreamWriter();
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					writer.Write(Cells[x, y] != null);
				}
			}

			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					if (Cells[x, y] is Cell cell) {
						cell.Encode(writer);
					}
				}
			}

			if (secondCells != null) {
				writer.Write(secondCells.Count, 8);
				foreach (var cell in secondCells) {
					cell.Encode(writer, true);
				}
			} else {
				writer.Write(0, 8);
			}

			var data = new byte[2 + (writer.Count + 7) / 8];
			data[0] = (byte)(writer.Count);
			data[1] = (byte)(writer.Count >> 8);
			writer.CopyTo(data, 2);
			return data;
		}

		public IReadOnlyList<Cell> Decode(byte[] data) {
			var bitLength = data[0] | (data[1] << 8);
			var reader = new BitStreamReader(data, 2, bitLength);
			var queue = new Queue<(int X, int Y)>();

			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					var hasCell = reader.Read();
					if (hasCell) queue.Enqueue((x, y));
				}
			}

			var newCells = new Cell[Width, Height];
			while (queue.Count > 0) {
				var (x, y) = queue.Dequeue();
				newCells[x, y] = Cell.Decode(reader, this);
				newCells[x, y].X = x;
				newCells[x, y].Y = y;
			}
			Cells = newCells;

			int len = reader.Read(out var count, 8);
			if (len < 8) throw new System.IO.EndOfStreamException("读取数量错误");
			if (count > 0) {
				var freedomCells = new List<Cell>(count);
				for (var i = 0; i < count; i++) {
					freedomCells.Add(Cell.Decode(reader, this, true));
				}
				freedomCells.Sort();
				return freedomCells;
			} else {
				return null;
			}
		}

		private (Cell[,] Cells, IReadOnlyList<Cell> FreedomCells) InternalDecode(byte[] data) {
			var newMap = new Map(Width, Height);
			var freedomCells = newMap.Decode(data);
			var newFreedomCells = freedomCells != null ? new List<Cell>(freedomCells) : new List<Cell>();
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					if (newMap.Cells[x, y] is Cell cell && cell.Freedom) {
						newFreedomCells.Add(cell);
						newMap.Cells[x, y] = null;
					}
				}
			}
			newFreedomCells.Sort();
			return (newMap.Cells, newFreedomCells);
		}

		public bool EqualsMapData(byte[] data1, byte[] data2) {
			var (cells1, freedomCells1) = InternalDecode(data1);
			var (cells2, freedomCells2) = InternalDecode(data2);
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					if ((cells1[x, y] == null && cells2[x, y] == null)
						|| (cells1[x, y] is Cell cell1 && cells2[x, y] is Cell cell2 && cell1.CompareTo(cell2) == 0 && cell1.Direction == cell2.Direction)) {
						continue;
					}
					return false;
				}
			}
			if (freedomCells1.Count != freedomCells2.Count) return false;
			for (int i = 0; i < freedomCells1.Count; i++) {
				if (freedomCells1[i].CompareTo(freedomCells2[i]) != 0) return false;
			}
			return true;
		}

		public struct CellRef {
			private Cell[,] cells;
			public int X { get; }
			public int Y { get; }

			public ref Cell Cell => ref cells[X, Y];

			public CellRef(Cell[,] cells, int x, int y) {
				this.cells = cells;
				this.X = x;
				this.Y = y;
			}
		}

		public IEnumerable<CellRef> ForEachCell() {
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					yield return new CellRef(Cells, x, y);
				}
			}
		}

		object ICloneable.Clone() => Clone();

		public Map Clone() {
			var clone = new Map(Width, Height);
			foreach (var cell in clone.ForEachCell()) {
				cell.Cell = Cells[cell.X, cell.Y]?.Clone() as Cell;
			}
			return clone;
		}
	}
}
