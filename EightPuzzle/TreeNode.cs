using System;
using System.Collections.Generic;

namespace EightPuzzle {
	public class TreeNode : TreeNodeBase<TreeNode> {
		public TreeNode() : base() { }
		public TreeNode(TreeNode parent) : base(parent) { }
	}
	/// <summary>
	/// Base class of tree node
	/// </summary>
	/// <typeparam name="TNode">The actual type of node. Used to define relationship reference</typeparam>
	public class TreeNodeBase<TNode> where TNode : TreeNodeBase<TNode> {
		#region Fields
		private TNode parent;
		private int height = 0;
		private int size = 1;
		private int depth = 0;
		private bool heightUpToDate = true;
		private bool depthUpToDate = true;
		#endregion

		#region Constructors
		public TreeNodeBase() => Children = new List<TNode>();
		public TreeNodeBase(TNode parent) : this() => Parent = parent;
		#endregion

		#region Delegates
		public delegate void ValueChanging<TValue>(object sender, ValueChangingEventArg<TValue> e);
		public delegate void ValueChanged<TValue>(object sender, ValueChangedEventArg<TValue> e);
		#endregion

		#region Events
		/// <summary>
		/// Called before parent is changed. Everything remains unchanged.
		/// </summary>
		public event ValueChanging<TNode> ParentChanging = delegate { };
		/// <summary>
		/// Called after parent is changed. All related data are already updated.
		/// </summary>
		public event ValueChanged<TNode> ParentChanged = delegate { };
		#endregion

		#region Properties
		/// <summary>
		/// Parent of the node
		/// </summary>
		public TNode Parent {
			get => parent;
			set {
				if (parent != null && !parent.Equals(value)
					|| parent == null && value != null) {
					OnParentChanging(new ValueChangingEventArg<TNode>(parent, value));
					parent = value;
					OnParentChanged(new ValueChangedEventArg<TNode>(parent));
				}
			}
		}
		/// <summary>
		/// Indicate whether this node is the root of the tree
		/// </summary>
		public bool IsRoot { get => parent == null; }
		/// <summary>
		/// Indicate whether this node is a leaf node
		/// </summary>
		public bool IsLeaf { get => Children.Count == 0; }
		/// <summary>
		/// The root of the tree
		/// </summary>
		public TNode Root {
			get {
				TNode root = this as TNode;
				while (!root.IsRoot)
					root = root.Parent;
				return root;
			}
		}
		/// <summary>
		/// Children of the node
		/// </summary>
		public List<TNode> Children { get; }
		/// <summary>
		/// Height of the subtree whose root is this node
		/// </summary>
		public int Height {
			get {
				UpdateHeightAndSize();
				return height;
			}
		}
		/// <summary>
		/// Size of the subtree whose root is this node
		/// </summary>
		public int Size {
			get {
				UpdateHeightAndSize();
				return size;
			}
		}
		/// <summary>
		/// Depth of the subtree whose root is this node
		/// </summary>
		public int Depth {
			get {
				UpdateDepth();
				return depth;
			}
		}
		private bool HeightUpToDate {
			get => heightUpToDate;
			set {
				if (heightUpToDate && !value && !IsRoot)
					parent.HeightUpToDate = value;
				heightUpToDate = value;
			}
		}
		private bool DepthUpToDate {
			get => depthUpToDate;
			set {
				if (depthUpToDate && !value)
					foreach (var child in Children)
						child.DepthUpToDate = value;
				depthUpToDate = value;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Calculate the latest common ancestor of two nodes
		/// </summary>
		/// <returns>Null if two nodes aren't in the same tree</returns>
		public static TNode GetLatestCommonAncestor(TNode node1, TNode node2) {
			if (node1.Depth > node2.Depth) {
				var temp = node1;
				node1 = node2;
				node2 = temp;
			}
			for (int i = 0; i < node2.Depth - node1.Depth; ++i)
				node2 = node2.Parent;
			while (!node1.Equals(node2) && !node1.IsRoot && !node2.IsRoot) {
				node1 = node1.Parent;
				node2 = node2.Parent;
			}
			return node1.Equals(node2) ? node1 : null;
		}
		/// <summary>
		/// Called when parent is about to change. Will update the original parent and up-to-date flags after the event fires.
		/// </summary>
		/// <param name="e"></param>
		public virtual void OnParentChanging(ValueChangingEventArg<TNode> e) {
			ParentChanging(this, e);
			if (parent != null) {
				parent.Children.Remove(this as TNode);
				parent.HeightUpToDate = false;
			}
			if (!(parent?.Depth == e.NewValue?.Depth))
				DepthUpToDate = false;
		}
		/// <summary>
		/// Called when parent has changed. Will update the new parent and up-to-date flags before the event fires.
		/// </summary>
		/// <param name="e"></param>
		public virtual void OnParentChanged(ValueChangedEventArg<TNode> e) {
			if (parent != null) {
				parent.Children.Add(this as TNode);
				parent.HeightUpToDate = false;
			}
			ParentChanged(this, e);
		}
		/// <summary>
		/// Check whether this node is the child of another node
		/// </summary>
		public bool IsChildOf(TNode node) {
			var cur = Parent;
			while (cur != null && !cur.Equals(node) && !cur.Equals(this))
				cur = cur.Parent;
			return node.Equals(cur);
		}
		/// <summary>
		/// Check whether this node is the ancestor of another node
		/// </summary>
		public bool IsAncestorOf(TNode node) {
			if (node == null)
				return false;
			else
				return node.IsChildOf(this as TNode);
		}
		/// <summary>
		/// Update height and size if out of date. Called in the getters of Height and Size.
		/// </summary>
		private void UpdateHeightAndSize() {
			if (HeightUpToDate)
				return;
			height = 0;
			size = 1;
			foreach (var child in Children) {
				child.UpdateHeightAndSize();
				size += child.size;
				height = Math.Max(height, child.height + 1);
			}
			HeightUpToDate = true;
		}
		/// <summary>
		/// Update depth if out of date. Called in the getter of Depth
		/// </summary>
		private void UpdateDepth() {
			if (DepthUpToDate)
				return;
			else if (IsRoot) {
				depth = 0;
				return;
			}
			Parent.UpdateDepth();
			depth = Parent.Depth + 1;
			DepthUpToDate = true;
		}
		#endregion

		#region Classes
		/// <summary>
		/// A class that preserves the arguments ValueChanging event
		/// </summary>
		/// <typeparam name="TValue">Type of the changing value</typeparam>
		public class ValueChangingEventArg<TValue> : EventArgs {
			public TValue OldValue;
			public TValue NewValue;
			public ValueChangingEventArg(TValue old, TValue @new) {
				OldValue = old;
				NewValue = @new;
			}
		}
		/// <summary>
		/// A class that preserves the arguments ValueChanged event
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		public class ValueChangedEventArg<TValue> : EventArgs {
			public TValue NewValue;
			public ValueChangedEventArg(TValue @new) {
				NewValue = @new;
			}
		}
		#endregion
	}
}
