using System;

namespace EightPuzzle {
	public static class Evaluator {
		private static int Square(int value) => value * value;
		public static readonly Search<Puzzle>.Evaluator ManhattanDistance =
			(src, dst) => Math.Abs(src.Slot.Row - dst.Slot.Row) + Math.Abs(src.Slot.Column - dst.Slot.Column);

		public static readonly Search<Puzzle>.Evaluator EuclideanDistance =
			(src, dst) => (int)Math.Round(Math.Sqrt(Square(src.Slot.Row - dst.Slot.Row) + Square(src.Slot.Column - dst.Slot.Column)));

		public static readonly Search<Puzzle>.Evaluator MalposedNumDistance =
			(src, dst) => (Math.Abs(src.Slot.Row - dst.Slot.Row) > 0 || Math.Abs(src.Slot.Column - dst.Slot.Column) > 0) ? 1 : 0;

		public static readonly Search<Puzzle>.Evaluator DiagonalDistance =
			(src, dst) => ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 2) ? 2 : ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 0) ? Math.Abs(src.Slot.Column - dst.Slot.Column) : ((Math.Abs(src.Slot.Column - dst.Slot.Column) == 0) ? 1 : Math.Abs(src.Slot.Column - dst.Slot.Column))));
	}
}
