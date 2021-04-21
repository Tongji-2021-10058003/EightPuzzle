using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EightPuzzle {
	public class Program {
		public static int ReadInteger(string prompt = null) {
			int result;
			do {
				if (!string.IsNullOrEmpty(prompt))
					Console.Write(prompt);
			}
			while (!int.TryParse(Console.ReadLine(), out result));
			return result;
		}
		public static Puzzle ReadPuzzle(int row, int col, string prompt = null) {
			Puzzle result = null;
			bool legal = true;
			do {
				if (!string.IsNullOrEmpty(prompt))
					Console.Write(prompt);
				int[,] state = new int[row, col];
				for (int i = 0; legal && i < row; ++i) {
					string line = Console.ReadLine();
					string[] strs = line.Split(' ');
					if (strs.Length != col)
						legal = false;
					else {
						for (int j = 0; legal && j < col; ++j)
							if (!int.TryParse(strs[j], out state[i, j]))
								legal = false;
					}
				}
				if (legal) {
					try {
						result = new Puzzle(state);
					}
					catch {
						legal = false;
					}
				}
			}
			while (!legal);
			return result;
		}
		public static void Main() {
			int rowCount = ReadInteger("Row count: ");
			int colCount = ReadInteger("Column count: ");
			Puzzle src = ReadPuzzle(rowCount, colCount, "Source puzzle:\n");
			Puzzle dst = ReadPuzzle(rowCount, colCount, "Destination puzzle:\n");
			if (PuzzleUtility.Reachable(src, dst) == false) {
				Console.WriteLine("Not reachable");
				return;
			}
			string funcName;
			do {
				Console.Write("Heuristic function: ");
				funcName = Console.ReadLine();
			}
			while (typeof(Evaluator).GetField(funcName) == null);
			Search<Puzzle>.Evaluator estimate = (dynamic)(typeof(Evaluator).GetField(funcName).GetValue(null));
			var search = new Search<Puzzle>(src, dst, PuzzleUtility.Transform);
			int cost = search.AStar(estimate);
			Console.WriteLine($"Number of steps: {cost}");
			var graph = search.SearchRoot.BuildSearchTree(out var indexToNode);
			var stateToIndex = new Dictionary<Puzzle, int>();
			for (int i = 0; i < indexToNode.Length; ++i)
				stateToIndex.Add(indexToNode[i].State, i);
			graph.FindNode(stateToIndex[search.Source].ToString()).Attr.Color = Color.Red;
			graph.FindNode(stateToIndex[search.Destination].ToString()).Attr.Color = Color.Yellow;
			graph.PaintPath(search.Path.Select(puzzle => stateToIndex[puzzle].ToString()), Color.Cyan);
			string fileName = "search states.jpg";
			graph.RenderImage(fileName);
			Process.Start(new ProcessStartInfo() {
				FileName = fileName,
				UseShellExecute = true
			});
		}
	}
}