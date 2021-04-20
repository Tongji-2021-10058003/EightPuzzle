using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EightPuzzle.Test {
	public class TreeNodeTest {
		private static readonly int count = 5;
		private List<TreeNode> nodes;
		[SetUp]
		public void Initialize() {
			nodes = new List<TreeNode>(count);
			for (int i = 0; i < count; ++i)
				nodes.Add(new TreeNode());
		}
		[SetUp]
		public void Build() {
			nodes[1].Parent = nodes[0];
			nodes[2].Parent = nodes[0];
			nodes[3].Parent = nodes[1];
			nodes[4].Parent = nodes[1];
		}
		[Test]
		public void SizeHeightDepth() {
			Assert.AreEqual(new int[] { 2, 1, 0, 0, 0 }, nodes.Select(node => node.Height));
			Assert.AreEqual(new int[] { 5, 3, 1, 1, 1 }, nodes.Select(node => node.Size));
			Assert.AreEqual(new int[] { 0, 1, 1, 2, 2 }, nodes.Select(node => node.Depth));
			nodes[1].Parent = null;
			Assert.AreEqual(new int[] { 1, 1, 0, 0, 0 }, nodes.Select(node => node.Height));
			Assert.AreEqual(new int[] { 2, 3, 1, 1, 1 }, nodes.Select(node => node.Size));
			Assert.AreEqual(new int[] { 0, 0, 1, 1, 1 }, nodes.Select(node => node.Depth));
		}
		[Test]
		public void Relationship() {
			Assert.IsTrue(nodes[0].IsRoot);
			Assert.AreSame(nodes[0], nodes[4].Root);
			Assert.IsTrue(nodes[0].IsAncestorOf(nodes[4]));
			Assert.IsFalse(nodes[1].IsChildOf(nodes[2]));
			Assert.IsTrue(nodes[3].IsLeaf);
		}
		[Test]
		public void LatestCommonAncestor() {
			Assert.AreSame(nodes[0], TreeNode.GetLatestCommonAncestor(nodes[2], nodes[4]));
			nodes[1].Parent = null;
			Assert.IsNull(TreeNode.GetLatestCommonAncestor(nodes[2], nodes[4]));
		}
	}
}
