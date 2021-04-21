using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using Priority_Queue;

namespace EightPuzzle {
	/// <summary>
	/// The container class that preserves the details during search process
	/// </summary>
	/// <typeparam name="TState">A type that presents the state</typeparam>
	/// <typeparam name="TCost">A type that presents the cost of each step. Should support + and - operators</typeparam>
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
		/// <summary>
		/// Starting point of the search
		/// </summary>
		public TState Source { get; set; }
		/// <summary>
		/// Terminal point of the search
		/// </summary>
		public TState Destination { get; set; }
		/// <summary>
		/// A function that could check whether a state is the destination. Used in search with undefinite destinations
		/// </summary>
		public StateChecker CheckDestination { get; set; }
		/// <summary>
		/// Search node of the source. Could be used to render a search tree.
		/// </summary>
		public Node SearchSource { get; private set; }
		/// <summary>
		/// Search node of the destination. Null if no paths were found.
		/// </summary>
		public Node SearchDestination { get; private set; }
		/// <summary>
		/// A function that presents the transforming action of each state
		/// </summary>
		public Transformer Transform { get; set; }
		/// <summary>
		/// Path found in the search process. Empty if not found.
		/// </summary>
		public List<TState> Path { get; init; }
		#endregion
		#region Methods
		/// <summary>
		/// A* search algorithm
		/// </summary>
		/// <param name="estimate">The heuristic function that estimate the cost from one state to the destination</param>
		/// <returns>Cost of the path. Null if destination is not reached.</returns>
		public TCost? AStar(Evaluator estimate) {
			if (Destination == null)
				throw new NullReferenceException("A* algorithm needs a definite destination for estimation");
			SearchDestination = null;
			Path.Clear();
			var source = new HeuristicNode(Source, default);
			//A hashset containing all nodes in both open list and close list
			var visited = new HashSet<HeuristicNode>(new NodeEqualityComparer()) { source };
			//A priority queue containing nodes in open list
			var heap = new SimplePriorityQueue<HeuristicNode, TCost>();
			heap.Enqueue(source, source.Total);
			while (heap.Count > 0) {
				var node = heap.First;
				heap.Dequeue();
				foreach (var (next, cost) in Transform(node.State)) {
					if (visited.TryGetValue(new HeuristicNode(next), out HeuristicNode nextNode)) {
						TCost newCost = (dynamic)cost + node.Cost;
						//Continue if the new cost is no less than the original one
						if (newCost.CompareTo(nextNode.Cost) >= 0)
							continue;
						//If the new cost is less and the next node is the ancestor of current node, a negative cycle is detected, which will lead to endless loop and thus the process must be terminated
						else if (nextNode.IsAncestorOf(node))
							throw new Exception("Negative cycle detected");
						else {
							nextNode.Parent = node;
							nextNode.Cost = newCost;
							if (heap.Contains(nextNode))
								heap.UpdatePriority(nextNode, nextNode.Total);
							//Recursively update the subtree of the node
							Update(nextNode, ref visited, ref heap);
						}
					}
					else {
						//Create a new node if neither open nor close list contains the next state
						nextNode = new HeuristicNode(next, (dynamic)node.Cost + cost, node, estimate(next, Destination));
						//Add the new node to open list
						heap.Enqueue(nextNode, nextNode.Total);
						visited.Add(nextNode);
					}
				}
				if (visited.Contains(new HeuristicNode(Destination))
					|| Destination == null && visited.Any(node=>CheckDestination(node.State)))
					break;
			}
			SearchSource = source;
			//Look for the destination node
			HeuristicNode destination = null;
			if (Destination != null)
				visited.TryGetValue(new HeuristicNode(Destination), out destination);
			else
				destination = visited.FirstOrDefault(node => CheckDestination(node.State));
			if (destination == null)
				return null;

			SearchDestination = destination;
			//Calculate the path
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
		/// <summary>
		/// A node presents a state during search process.
		/// </summary>
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
		/// <summary>
		/// Node with estimation
		/// </summary>
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
		/// <summary>
		/// Node with score
		/// </summary>
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
		/// <summary>
		/// Comparer of nodes
		/// </summary>
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
