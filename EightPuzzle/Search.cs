using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Priority_Queue;

namespace EightPuzzle {
	public class Search<TState, TCost> where TState : class, IEquatable<TState> where TCost : struct, IComparable<TCost> {
		#region Constructors
		public Search() => Path = new List<TState>();
		public Search(TState src, TState dst, Transformer transform):this() {
			Source = src;
			Destination = dst;
			Transform = transform;
		}
		public Search(TState src, StateChecker checkDestination, Transformer transform) : this() {
			Source = src;
			CheckDestination = checkDestination;
			Transform = transform;
		}
		#endregion

		#region Delegates
		public delegate IEnumerable<(TState Next, TCost Cost)> Transformer(TState state);
		public delegate TCost Evaluator(TState src, TState dst);
		public delegate bool StateChecker(TState state);
		#endregion

		#region Properties
		public TState Source { get; set; }
		public TState Destination { get; set; }
		public StateChecker CheckDestination { get; set; }
		public Node SearchDestination { get; private set; }
		public Node SearchSource { get; private set; }
		public Transformer Transform { get; set; }
		public List<TState> Path { get; init; }
		#endregion
		#region Methods
		public TCost? AStar(Evaluator estimate) {
			if (Destination == null)
				throw new NullReferenceException("A* algorithm needs a definite destination for estimation");
			SearchDestination = null;
			Path.Clear();
			var source = new HeuristicNode(Source, default);
			var visited = new HashSet<HeuristicNode>(new NodeEqualityComparer()) { source };
			var heap = new SimplePriorityQueue<HeuristicNode, TCost>();
			heap.Enqueue(source, source.Total);
			while (heap.Count > 0) {
				var node = heap.First;
				heap.Dequeue();
				foreach (var (next, cost) in Transform(node.State)) {
					if (visited.TryGetValue(new HeuristicNode(next), out HeuristicNode nextNode)) {
						TCost newCost = (dynamic)cost + node.Cost;
						if (newCost.CompareTo(nextNode.Cost) >= 0)
							continue;
						else if (nextNode.IsAncestorOf(node))
							throw new Exception("Negative cycle detected");
						else {
							nextNode.Parent = node;
							nextNode.Cost = newCost;
							if (heap.Contains(nextNode))
								heap.UpdatePriority(nextNode, nextNode.Total);
							Update(nextNode, ref visited, ref heap);
						}
					}
					else {
						nextNode = new HeuristicNode(next, (dynamic)node.Cost + cost, node, estimate(next, Destination));
						heap.Enqueue(nextNode, nextNode.Total);
						visited.Add(nextNode);
					}
				}
				if (visited.Contains(new HeuristicNode(Destination))
					|| Destination == null && visited.Any(node=>CheckDestination(node.State)))
					break;
			}
			SearchSource = source;
			HeuristicNode destination = null;
			if (Destination != null)
				visited.TryGetValue(new HeuristicNode(Destination), out destination);
			else
				destination = visited.FirstOrDefault(node => CheckDestination(node.State));
			if (destination == null)
				return null;

			SearchDestination = destination;
			for (HeuristicNode node = destination; node != null; node = node.Parent)
				Path.Add(node.State);
			Path.Reverse();
			return destination.Cost;
		}
		private void Update(HeuristicNode node, ref HashSet<HeuristicNode> visited, ref SimplePriorityQueue<HeuristicNode, TCost> heap) {
			foreach (var (next, cost) in Transform(node.State))
				if (visited.TryGetValue(new HeuristicNode(next), out HeuristicNode nextNode) && nextNode.Parent.Equals(node)) {
					TCost newCost = (dynamic)cost + node.Cost;
					if (newCost.CompareTo(nextNode.Cost) >= 0)
						continue;
					nextNode.Cost = newCost;
					if (heap.Contains(nextNode))
						heap.UpdatePriority(nextNode, nextNode.Total);
					Update(nextNode, ref visited, ref heap);
				}
		}
		#endregion

		#region Classes
		public class Node : TreeNodeBase<Node>, IEquatable<Node> {
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
			public override bool Equals(object obj) => obj is Node && Equals(obj as Node);
			public override int GetHashCode() => State.GetHashCode();
			public override string ToString() {
				var result = new StringBuilder(State.ToString());
				result.AppendLine($"Cost: {Cost}");
				return result.ToString();
			}
		}

		public class HeuristicNode : Node, IComparable<HeuristicNode> {
			public HeuristicNode(IComparer<HeuristicNode> comparer = null) : base() {
				Estimation = default;
				Comparer = comparer;
			}
			public HeuristicNode(TState state, TCost cost = default, HeuristicNode parent = null, TCost estimation = default) : base(state, cost, parent)
				=> Estimation = estimation;
			public HeuristicNode(IComparer<HeuristicNode> comparer, TState state, TCost cost = default, HeuristicNode source = null, TCost estimation = default) : this(state, cost, source, estimation)
				=> Comparer = comparer;
			public int CompareTo(HeuristicNode other) => Comparer == null ? Total.CompareTo(other.Total) : Comparer.Compare(this, other);
			public new HeuristicNode Parent {
				get => (this as Node).Parent as HeuristicNode;
				set => (this as Node).Parent = value;
			}
			public TCost Estimation { get; set; }
			public TCost Total { get => (dynamic)Cost + Estimation; }
			private IComparer<HeuristicNode> Comparer { get; } = null;
			public override string ToString() {
				var result = new StringBuilder(base.ToString());
				result.AppendLine($"Estm: {Estimation}");
				return result.ToString();
			}
		}

		public class ScoredNode : Node, IComparable<ScoredNode> {
			public ScoredNode(IComparer<ScoredNode> comparer = null) : base() {
				Score = default;
				Comparer = comparer;
			}
			public ScoredNode(TState state, TCost cost = default, ScoredNode parent = null, TCost score = default) : base(state, cost, parent)
				=> Score = score;
			public ScoredNode(IComparer<ScoredNode> comparer, TState state, TCost cost = default, ScoredNode source = null, TCost score = default) : this(state, cost, source, score)
				=> Comparer = comparer;
			public int CompareTo(ScoredNode other) => Comparer == null ? Score.CompareTo(other.Score) : Comparer.Compare(this, other);
			public new ScoredNode Parent {
				get => (this as Node).Parent as ScoredNode;
				set => (this as Node).Parent = value;
			}
			public TCost Score { get; set; }
			private IComparer<ScoredNode> Comparer { get; } = null;
			public override string ToString() {
				var result = new StringBuilder(base.ToString());
				result.AppendLine($"Score: {Score}");
				return result.ToString();
			}
		}

		protected class NodeEqualityComparer : IEqualityComparer<Node> {
			public bool Equals(Node x, Node y) => x.State.Equals(y.State);
			public int GetHashCode([DisallowNull] Node obj) => obj.State.GetHashCode();
		}
		#endregion
	}
	public class Search<TState> : Search<TState, int> where TState : class, IEquatable<TState> {
		public Search() : base() { }
		public Search(TState src, TState dst, Transformer transform) : base(src, dst, transform) { }
	}
}
