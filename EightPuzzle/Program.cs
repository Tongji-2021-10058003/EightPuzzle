using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EightPuzzle {
	/// <summary>
	/// A console application that runs search algorhthm with an interactive CLI
	/// </summary>
	public class Program {
		/// <summary>
		/// Read an integer from console. The method will loop until the input is valid.
		/// </summary>
		/// <param name="prompt">An optional message to print before reading</param>
		/// <returns></returns>
		public static int ReadInteger(string prompt = null) {
			int result;
			do {
				if (!string.IsNullOrEmpty(prompt))
					Console.Write(prompt);
			}
			while (!int.TryParse(Console.ReadLine(), out result));
			return result;
		}/// <summary>
		 /// Read the puzzle from console. The method will loop until the input is valid.
		 /// </summary>
		 /// <param name="row">Row count of the puzzle</param>
		 /// <param name="col">Column count of the puzzle</param>
		 /// <param name="prompt">An optional message to print before reading</param>
		 /// <returns></returns>
		public static Puzzle ReadPuzzle(int row, int col, string prompt = null) {
			Puzzle result = null;
			bool legal;
			do {
				legal = true;
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
			//Read from console
			int rowCount = ReadInteger("Row count: ");
			int colCount = ReadInteger("Column count: ");
			Puzzle src = ReadPuzzle(rowCount, colCount, "Source puzzle:\n");
			Puzzle dst = ReadPuzzle(rowCount, colCount, "Destination puzzle:\n");

			//Check reachability before searching
			bool? reachable;
			try {
				reachable = PuzzleUtility.Reachable(src, dst);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.Message);
				return;
			}
			if (reachable == false) {
				Console.WriteLine("Not reachable");
				return;
			}

			//Read the heuristic function to be used. Function name must be presented in Evaluator class
			string funcName;
			do {
				Console.Write("Heuristic function: ");
				funcName = Console.ReadLine();
			}
			while (typeof(Evaluator).GetField(funcName) == null);
			Search<Puzzle>.Evaluator estimate = (dynamic)(typeof(Evaluator).GetField(funcName).GetValue(null));

			//Start searching
			var search = new Search<Puzzle>(src, dst, PuzzleUtility.Transform);
			var cost = search.AStar(estimate);
			if (!cost.HasValue) {
				Console.WriteLine("Not reachable");
				return;
			}
			Console.WriteLine($"Number of steps: {cost}");

			//Render the search tree to an image
			var graph = search.SearchSource.BuildSearchTree(out var indexToNode);
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