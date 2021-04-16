using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EightPuzzle {
	public enum Direction {
		Up = 0,
		Down = 1,
		Left = 2,
		Right = 3
	};
	public class Puzzle {
		public int[,] State;
		public (int Row, int Column) Slot {
			get;
			private set;
		}
		public Puzzle(Puzzle puzzle) {
			State = new int[puzzle.RowCount, puzzle.ColumnCount];
			Array.Copy(puzzle.State, State, puzzle.State.Length);
			Slot = puzzle.Slot;
		}
		public Puzzle(int row, int col) {
			State = new int[row, col];
			Slot = (-1, -1);
		}
		public Puzzle(int[,] state) {
			State = new int[state.GetLength(0), state.GetLength(1)];
			Array.Copy(state, State, state.Length);
			Slot = (-1, -1);
			for (int i = 0; i < RowCount && Slot == (-1, -1); ++i)
				for (int j = 0; j < ColumnCount; ++j)
					if (state[i, j] <= 0) {
						Slot = (i, j);
						break;
					}
			if (Slot == (-1, -1))
				throw new ArgumentException("No empty slot provided");
		}
		public int RowCount { get => State.GetLength(0); }
		public int ColumnCount { get => State.GetLength(1); }
		public Puzzle Move(Direction dir) {
			if ((Slot.Row == 0 && dir == Direction.Up) ||
				(Slot.Row == RowCount - 1 && dir == Direction.Down) ||
				(Slot.Column == 0 && dir == Direction.Left) ||
				(Slot.Column == ColumnCount - 1 && dir == Direction.Right))
				throw new InvalidOperationException("Slot is on the border");
			Puzzle result = new Puzzle(this);
			(int deltaRow, int deltaCol) = dir switch {
				Direction.Up => (-1, 0),
				Direction.Down => (1, 0),
				Direction.Left => (0, -1),
				Direction.Right => (0, 1),
				_ => throw new NotImplementedException()
			};
			result.Slot = (Slot.Row + deltaRow, Slot.Column + deltaCol);
			int temp = result.State[Slot.Row, Slot.Column];
			result.State[Slot.Row, Slot.Column] = result.State[result.Slot.Row, result.Slot.Column];
			result.State[result.Slot.Row, result.Slot.Column] = temp;
			return result;
		}
		public int[] ToArray(bool containSlot = false) {
			List<int> result = new List<int>();
			for (int i = 0; i < RowCount; ++i)
				for (int j = 0; j < ColumnCount; ++j) {
					if (!containSlot && Slot == (i, j))
						continue;
					result.Add(State[i, j]);
				}
			return result.ToArray();
		}
	}
	public static class PuzzleUtility {
		private static int CountReversePairs(ArraySegment<int> seg, ref int[] temp) {
			int middle = seg.Count >> 1;
			int ans = CountReversePairs(seg.Slice(0, middle), ref temp) + CountReversePairs(seg.Slice(middle), ref temp);
			for (int i = 0, j = middle, k = 0; i < middle || j < seg.Count; ++k) {
				if ((i < middle && seg[i] <= seg[j]) || j == seg.Count) {
					ans += j - middle;
					temp[k] = seg[i++];
				}
				else
					temp[k] = seg[j++];
			}
			Array.Copy(temp, 0, seg.Array, seg.Offset, seg.Count);
			return ans;
		}
		public static int CountReversePairs(int[] array) {
			var duplicate = new int[array.Length];
			Array.Copy(array, duplicate, array.Length);
			var temp = new int[array.Length];
			return CountReversePairs(duplicate, ref temp);
		}
		public static bool? Achievable(Puzzle src, Puzzle dst) {
			if (src.RowCount != dst.RowCount || src.ColumnCount != dst.ColumnCount)
				throw new ArgumentException("Two puzzles must be of the same size");
			if ((src.ColumnCount & 1) == 0)
				return null;
			var srcArr = src.ToArray();
			var dstArr = dst.ToArray();
			return (CountReversePairs(srcArr) & 1) == (CountReversePairs(dstArr) & 1);
		}
		public static IEnumerable<(Puzzle, int)> Transform(this Puzzle puzzle) {
			if (puzzle.Slot.Row > 0)
				yield return (puzzle.Move(Direction.Up), 1);
			if (puzzle.Slot.Row < puzzle.RowCount - 1)
				yield return (puzzle.Move(Direction.Down), 1);
			if (puzzle.Slot.Column > 0)
				yield return (puzzle.Move(Direction.Left), 1);
			if (puzzle.Slot.Column < puzzle.ColumnCount - 1)
				yield return (puzzle.Move(Direction.Right), 1);
		}
	}
}
