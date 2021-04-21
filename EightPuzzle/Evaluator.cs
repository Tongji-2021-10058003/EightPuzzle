using System;

namespace EightPuzzle {
	public static class Evaluator {
		private static int Square(int value) => value * value;
		public static readonly Search<Puzzle>.Evaluator ManhattanDistance =
			(src, dst) => Math.Abs(src.Slot.Row - dst.Slot.Row) + Math.Abs(src.Slot.Column - dst.Slot.Column);

		public static readonly Search<Puzzle>.Evaluator EuclideanDistance =
			(src, dst) => (int)Math.Round(Math.Sqrt(Square(src.Slot.Row - dst.Slot.Row) + Square(src.Slot.Column - dst.Slot.Column)));

		public static readonly Search<Puzzle>.Evaluator MalposedCount =
			(src, dst) => {
				int count = 0;
				for (int i = 0; i < src.RowCount; ++i)
					for (int j = 0; j < src.ColumnCount; ++j)
						if (src[i, j] != dst[i, j])
							++count;
				return Math.Max(0, count - 1);
			};

		public static readonly Search<Puzzle>.Evaluator DiagonalDistance =
			(src, dst) => ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 2) ? 2 : ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 0) ? Math.Abs(src.Slot.Column - dst.Slot.Column) : ((Math.Abs(src.Slot.Column - dst.Slot.Column) == 0) ? 1 : Math.Abs(src.Slot.Column - dst.Slot.Column))));
	}
}
