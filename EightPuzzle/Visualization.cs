using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

namespace EightPuzzle {
	public static class Visualization {
		private static int count = 0;
		private static void BuildSearchTree<TState, TCost>(Search<TState, TCost>.Node searchNode, ref Node graphNode, ref Graph graph, ref Search<TState, TCost>.Node[] indexToNode) where TState : class, IEquatable<TState> where TCost : IComparable<TCost> {
			foreach (var child in searchNode.Children) {
				indexToNode[count] = child;
				var node = new Node((count++).ToString()) {
					LabelText = child.State.ToString()
				};
				var edge = new Edge(graphNode, node, ConnectionToGraph.Connected);
				graphNode.AddOutEdge(edge);
				BuildSearchTree(child, ref node, ref graph, ref indexToNode);
			}
			graph.AddNode(graphNode);
		}
		public static Graph BuildSearchTree<TState, TCost>(this Search<TState, TCost>.Node root, out Search<TState, TCost>.Node[] indexToNode) where TState : class, IEquatable<TState> where TCost : IComparable<TCost> {
			count = 0;
			indexToNode = new Search<TState, TCost>.Node[root.Size];
			indexToNode[count] = root;
			var gNode = new Node((count++).ToString()) {
				LabelText = root.State.ToString()
			};
			var graph = new Graph();
			BuildSearchTree(root, ref gNode, ref graph, ref indexToNode);
			count = 0;
			return graph;
		}
		public static Graph BuildSearchTree<TState, TCost>(this Search<TState, TCost>.Node root) where TState : class, IEquatable<TState> where TCost : IComparable<TCost>
			=> root.BuildSearchTree(out _);
		public static void PaintPath(this Graph graph, IEnumerable<string> path, Microsoft.Msagl.Drawing.Color color) {
			var enumerator = path.GetEnumerator();
			if (!enumerator.MoveNext())
				return;
			var srcId = enumerator.Current;
			var srcNode = graph.FindNode(srcId);
			if (srcNode == null)
				throw new ArgumentException($"Id \"{srcId}\" doesn't exist in graph");
			string dstId;
			Node dstNode;
			while (enumerator.MoveNext()) {
				dstId = enumerator.Current;
				dstNode = graph.FindNode(dstId);
				if (dstNode == null)
					throw new ArgumentException($"Node \"{dstId}\" doesn't exist in graph");
				var edge = dstNode.InEdges.FirstOrDefault(edge => edge.Source == srcId);
				if (edge == default)
					throw new ArgumentException($"No edge found between \"{srcId}\" and \"{dstNode}\"");
				else
					edge.Attr.Color = color;
				srcId = dstId;
				srcNode = dstNode;
			}
		}
		public static Bitmap RenderImage(this Graph graph, int width = -1, int height = -1) {
			var renderer = new GraphRenderer(graph);
			renderer.CalculateLayout();
			if (width == -1 && height == -1)
				width = (int)(Math.Sqrt(graph.NodeCount)) * 50;
			else if (width != -1 && height != -1) {
				if (width / height > graph.Width / graph.Height)
					height = -1;
				else
					width = -1;
			}
			if (width == -1)
				width = (int)Math.Round(graph.Width / graph.Height * height);
			else
				height = (int)Math.Round(graph.Height / graph.Width * width);
			var image = new Bitmap(width, height);
			renderer.Render(image);
			return image;
		}
		public static void RenderImage(this Graph graph, string filename, int width = -1, int height = -1)
			=> graph.RenderImage(width, height).Save(filename);
	}
}
