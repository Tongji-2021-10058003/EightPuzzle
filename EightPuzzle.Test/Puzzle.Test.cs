using NUnit.Framework;
using System;

namespace EightPuzzle.Test {
	public class PuzzleTest {
		private Puzzle puzzle;
		[SetUp]
		public void Setup() {
			puzzle = new Puzzle(new int[,] { { 1, 2, 3 }, { 4, 5, 0 } });
		}

		[Test]
		public void Constructor() {
			Assert.AreEqual(puzzle.RowCount, 2);
			Assert.AreEqual(puzzle.ColumnCount, 3);
			Assert.AreEqual(puzzle.Slot, (1, 2));
		}
		[Test]
		public void CopyConstructor() {
			var duplicate = new Puzzle(puzzle);
			Assert.AreEqual(duplicate.RowCount, puzzle.RowCount);
			Assert.AreEqual(duplicate.ColumnCount, puzzle.ColumnCount);
			duplicate[0, 0] = 10;
			Assert.AreNotEqual(duplicate[0, 0], puzzle[0, 0]);
		}
		[Test]
		public void Move() {
			Assert.Throws(typeof(InvalidOperationException), () => puzzle.Move(Direction.Down));
			Assert.Throws(typeof(InvalidOperationException), () => puzzle.Move(Direction.Right));
			Assert.AreEqual((1, 1), puzzle.Move(Direction.Left).Slot);
			Assert.AreEqual((0, 2), puzzle.Move(Direction.Up).Slot);
		}
		[Test]
		public void ToArray() {
			Assert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, puzzle.ToArray());
			Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 0 }, puzzle.ToArray(true));
		}
	}

	public class PuzzleUtilityTest {
		[Test]
		public void CountReversePairs() {
			Assert.AreEqual(0, PuzzleUtility.CountReversePairs(new int[] { 1, 2, 3, 4, 5 }));
			Assert.AreEqual(10, PuzzleUtility.CountReversePairs(new int[] { 5, 4, 3, 2, 1 }));
			Assert.AreEqual(13, PuzzleUtility.CountReversePairs(new int[] { 1, 9, 2, 6, 0, 8, 1, 7 }));
		}
		[Test]
		public void Achievable() {
			var src = new Puzzle(new int[,] { { 1, 2, 3 }, { 4, 5, 0 } });
			var dst = src.MoveSelf(Direction.Left).MoveSelf(Direction.Left).MoveSelf(Direction.Up);
			Assert.IsTrue(PuzzleUtility.Achievable(src, dst));
			Assert.IsFalse(PuzzleUtility.Achievable(src, new Puzzle(new int[,] { { 1, 2, 3 }, { 5, 4, 0 } })));
		}
	}
}