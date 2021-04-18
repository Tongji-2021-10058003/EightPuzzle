using System;
using System.Collections.Generic;

namespace EightPuzzle {
	public class TreeNode : TreeNodeBase<TreeNode> {
		public TreeNode() : base() { }
		public TreeNode(TreeNode parent) : base(parent) { }
	}
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
		public event ValueChanging<TNode> ParentChanging = delegate { };
		public event ValueChanged<TNode> ParentChanged = delegate { };
		#endregion

		#region Properties
		public TNode Parent {
			get => parent;
			set {
				if (parent?.Equals(value) != true) {
					OnParentChanging(new ValueChangingEventArg<TNode>(parent, value));
					parent = value;
					OnParentChanged(new ValueChangedEventArg<TNode>(parent));
				}
			}
		}
		public bool IsRoot { get => parent == null; }
		public bool IsLeaf { get => Children.Count == 0; }
		public TNode Root {
			get {
				TNode root = this as TNode;
				while (!root.IsRoot)
					root = root.Parent;
				return root;
			}
		}
		public List<TNode> Children { get; }
		public int Height {
			get {
				UpdateHeightAndSize();
				return height;
			}
		}
		public int Size {
			get {
				UpdateHeightAndSize();
				return size;
			}
		}
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
		public virtual void OnParentChanging(ValueChangingEventArg<TNode> e) {
			ParentChanging(this, e);
			if (parent != null) {
				parent.Children.Remove(this as TNode);
				parent.HeightUpToDate = false;
			}
			if (!(parent?.Depth == e.NewValue?.Depth))
				DepthUpToDate = false;
		}
		public virtual void OnParentChanged(ValueChangedEventArg<TNode> e) {
			if (parent != null) {
				parent.Children.Add(this as TNode);
				parent.HeightUpToDate = false;
			}
			ParentChanged(this, e);
		}
		public bool IsChildOf(TNode node) {
			var cur = Parent;
			while (cur != null && !cur.Equals(node) && !cur.Equals(this))
				cur = cur.Parent;
			return node.Equals(cur);
		}
		public bool IsAncestorOf(TNode node) {
			if (node == null)
				return false;
			else
				return node.IsChildOf(this as TNode);
		}
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
		public class ValueChangingEventArg<TValue> : EventArgs {
			public TValue OldValue;
			public TValue NewValue;
			public ValueChangingEventArg(TValue old, TValue @new) {
				OldValue = old;
				NewValue = @new;
			}
		}
		public class ValueChangedEventArg<TValue> : EventArgs {
			public TValue NewValue;
			public ValueChangedEventArg(TValue @new) {
				NewValue = @new;
			}
		}
		#endregion
	}
}
