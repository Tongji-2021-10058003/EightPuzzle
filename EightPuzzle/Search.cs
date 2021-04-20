using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EightPuzzle {
	public class Search<TState, TCost> where TState : class, IEquatable<TState> where TCost : IComparable<TCost> {
		#region Constructors
		public Search() {
			Source = default;
			Destination = default;
			Path = new List<TState>();
		}
		public Search(TState src, TState dst, Transformer transform) {
			Source = src;
			Destination = dst;
			Transform = transform;
			Path = new List<TState>();
		}
		#endregion

		#region Delegates
		public delegate IEnumerable<(TState Next, TCost Cost)> Transformer(TState state);
		private delegate TCost ScoreUpdater(TCost oldScore, TCost oldCost, TCost newCost);
		#endregion

		#region Properties
		public TState Source { get; set; }
		public TState Destination { get; set; }
		public Transformer Transform { get; set; }
		public List<TState> Path { get; }
		#endregion

		#region Methods
		private void Update(ScoredNode node, Func<TState, ScoredNode> map, ScoreUpdater updateScore = null) {
			foreach (var (next, cost) in Transform(node.State))
				if (map(next) is var nextNode && nextNode.Source.Equals(node)) {
					if (((TCost)((dynamic)cost + node.Cost)).CompareTo(nextNode.Cost) == 0)
						continue;
					TCost newCost = (dynamic)cost + node.Cost;
					if (updateScore != null)
						nextNode.Score = updateScore(nextNode.Score, nextNode.Cost, newCost);
					nextNode.Cost = newCost;
					Update(nextNode, map, updateScore);
				}
		}
		public TCost AStar(Func<TState, TState, TCost> estimate) {
			Path.Clear();
			var source = new ScoredNode(Source, default);
			var visited = new HashSet<ScoredNode>(new NodeEqualityComparer()) { source };
			var heap = new C5.IntervalHeap<ScoredNode> { source };
			TCost updateScore(TCost oldScore, TCost oldCost, TCost newCost) => (dynamic)oldScore - oldCost + newCost;
			ScoredNode map(TState state) {
				visited.TryGetValue(new ScoredNode(state), out ScoredNode result);
				return result;
			}
			while (!heap.IsEmpty) {
				var node = heap.FindMin();
				heap.DeleteMin();
				foreach (var (next, cost) in Transform(node.State)) {
					if (visited.TryGetValue(new ScoredNode(next), out ScoredNode nextNode)) {
						TCost newCost = (dynamic)cost + node.Cost;
						if (newCost.CompareTo(nextNode.Cost) >= 0 || nextNode.IsAncestorOf(node))
							continue;
						else {
							nextNode.Score += (dynamic)newCost - nextNode.Cost;
							nextNode.Cost = newCost;
							Update(nextNode, map, updateScore);
						}
					}
					else {
						nextNode = new ScoredNode(next, (dynamic)node.Cost + cost, node, estimate(next, Destination));
						heap.Add(nextNode);
						visited.Add(nextNode);
					}
				}
				if (visited.Contains(new ScoredNode(Destination)))
					break;
			}
			var dstNode = map(Destination);
			for (ScoredNode node = dstNode; node != null; node = node.Source)
				Path.Insert(0, node.State);
			return dstNode.Cost;
		}
		#endregion

		#region Classes
		protected class Node : IEquatable<Node> {
			public Node() {
				State = default;
				Cost = default;
			}
			public Node(TState state, TCost cost = default, Node source = null) {
				State = state;
				Cost = cost;
				Source = source;
			}
			public bool Equals(Node other) => State.Equals(other.State);
			public Node Source { get; set; } = null;
			public TState State { get; set; }
			public TCost Cost { get; set; }
			public bool IsChildOf(Node node) {
				var cur = Source;
				while (cur != null && !cur.Equals(node) && !cur.Equals(this))
					cur = cur.Source;
				return node.Equals(cur);
			}
			public bool IsAncestorOf(Node node) {
				if (node == null)
					return false;
				else
					return node.IsChildOf(this);
			}
		}

		protected class ScoredNode : Node, IComparable<ScoredNode> {
			public ScoredNode(IComparer<ScoredNode> comparer = null) : base() {
				Score = default;
				Comparer = comparer;
			}
			public ScoredNode(TState state, TCost cost = default, ScoredNode source = null, TCost score = default) {
				State = state;
				Cost = cost;
				Source = source;
				Score = score;
			}
			public ScoredNode(IComparer<ScoredNode> comparer, TState state, TCost cost = default, ScoredNode source = null, TCost score = default) : this(state, cost, source, score)
				=> Comparer = comparer;
			public int CompareTo(ScoredNode other) => Comparer == null ? Score.CompareTo(other.Score) : Comparer.Compare(this, other);
			public new ScoredNode Source { get; set; } = null;
			public TCost Score { get; set; }
			private IComparer<ScoredNode> Comparer { get; } = null;
		}

		protected class NodeEqualityComparer : IEqualityComparer<Node> {
			public bool Equals(Node x, Node y) => x.State.Equals(y.State);
			public int GetHashCode([DisallowNull] Node obj) => obj.State.GetHashCode();
		}
		#endregion
	}
	public class Search<TState> : Search<TState, int> where TState : class, IEquatable<TState> { }
}
