using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Priority_Queue;

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
		#endregion

		#region Properties
		public TState Source { get; set; }
		public TState Destination { get; set; }
		public Transformer Transform { get; set; }
		public List<TState> Path { get; }
		#endregion

		#region Methods
		public TCost AStar(Func<TState, TState, TCost> estimate) {
			Path.Clear();
			var source = new ScoredNode(Source, default);
			var visited = new HashSet<ScoredNode>(new NodeEqualityComparer()) { source };
			var heap = new SimplePriorityQueue<ScoredNode, TCost>();
			heap.Enqueue(source, source.Score);
			while (heap.Count > 0) {
				var node = heap.First;
				heap.Dequeue();
				foreach (var (next, cost) in Transform(node.State)) {
					if (visited.TryGetValue(new ScoredNode(next), out ScoredNode nextNode)) {
						TCost newCost = (dynamic)cost + node.Cost;
						if (newCost.CompareTo(nextNode.Cost) >= 0)
							continue;
						else if (nextNode.IsAncestorOf(node))
							throw new Exception("Negative cycle detected");
						else {
							nextNode.Parent = node;
							nextNode.Score += (dynamic)newCost - nextNode.Cost;
							if (heap.Contains(nextNode))
								heap.UpdatePriority(nextNode, nextNode.Score);
							nextNode.Cost = newCost;
							Update(nextNode, ref visited, ref heap);
						}
					}
					else {
						nextNode = new ScoredNode(next, (dynamic)node.Cost + cost, node, estimate(next, Destination));
						heap.Enqueue(nextNode, nextNode.Score);
						visited.Add(nextNode);
					}
				}
				if (visited.Contains(new ScoredNode(Destination)))
					break;
			}
			visited.TryGetValue(new ScoredNode(Destination), out ScoredNode dstNode);
			for (ScoredNode node = dstNode; node != null; node = node.Parent)
				Path.Insert(0, node.State);
			return dstNode.Cost;
		}
		private void Update(ScoredNode node, ref HashSet<ScoredNode> visited, ref SimplePriorityQueue<ScoredNode, TCost> heap) {
			foreach (var (next, cost) in Transform(node.State))
				if (visited.TryGetValue(new ScoredNode(next), out ScoredNode nextNode) && nextNode.Parent.Equals(node)) {
					TCost newCost = (dynamic)cost + node.Cost;
					if (newCost.CompareTo(nextNode.Cost) >= 0)
						continue;
					nextNode.Score += (dynamic)newCost - nextNode.Cost;
					if (heap.Contains(nextNode))
						heap.UpdatePriority(nextNode, nextNode.Score);
					nextNode.Cost = newCost;
					Update(nextNode, ref visited, ref heap);
				}
		}
		#endregion

		#region Classes
		protected class Node : TreeNodeBase<Node>, IEquatable<Node> {
			public Node() : base() {
				State = default;
				Cost = default;
			}
			public Node(TState state, TCost cost = default, Node parent = null) : base(parent) {
				State = state;
				Cost = cost;
			}
			public bool Equals(Node other) => State.Equals(other.State);
			public TState State { get; set; }
			public TCost Cost { get; set; }
		}

		protected class ScoredNode : Node, IComparable<ScoredNode> {
			public ScoredNode(IComparer<ScoredNode> comparer = null) : base() {
				Score = default;
				Comparer = comparer;
			}
			public ScoredNode(TState state, TCost cost = default, ScoredNode parent = null, TCost score = default) {
				State = state;
				Cost = cost;
				Parent = parent;
				Score = score;
			}
			public ScoredNode(IComparer<ScoredNode> comparer, TState state, TCost cost = default, ScoredNode source = null, TCost score = default) : this(state, cost, source, score)
				=> Comparer = comparer;
			public int CompareTo(ScoredNode other) => Comparer == null ? Score.CompareTo(other.Score) : Comparer.Compare(this, other);
			public new ScoredNode Parent { get; set; } = null;
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
