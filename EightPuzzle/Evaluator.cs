using System;

namespace EightPuzzle {
	public static class Evaluator {
		public static readonly Search<Puzzle>.Evaluator ManhattanDistance =
			(src, dst) => Math.Abs(src.Slot.Row - dst.Slot.Row) + Math.Abs(src.Slot.Column - dst.Slot.Column);
	}
}
