using System;

namespace EightPuzzle {
	public static class Evaluator {
		public static readonly Search<Puzzle>.Evaluator ManhattanDistance =
			(src, dst) => Math.Abs(src.Slot.Row - dst.Slot.Row) + Math.Abs(src.Slot.Column - dst.Slot.Column);
			
		public static readonly Search<Puzzle>.Evaluator EuclideanDistance =
			(src, dst) => Math.sqrt(Math.pow((src.Slot.Row - dst.Slot.Row), 2) + Math.pow((src.Slot.Column - dst.Slot.Column), 2));
			
		public static readonly Search<Puzzle>.Evaluator MalposedNumDistance =
			(src, dst) => ((Math.Abs(src.Slot.Row - dst.Slot.Row) > 0)||(Math.Abs(src.Slot.Column - dst.Slot.Column) > 0)) ? 1 : 0;
			
		public static readonly Search<Puzzle>.Evaluator DiagonalDistance =
			(src, dst) =>  ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 2) ? 2 : ((Math.Abs(src.Slot.Row - dst.Slot.Row) == 0) ? Math.Abs(src.Slot.Column - dst.Slot.Column) : ((Math.Abs(src.Slot.Column - dst.Slot.Column) == 0) ? 1 : Math.Abs(src.Slot.Column - dst.Slot.Column))));
	}
}
