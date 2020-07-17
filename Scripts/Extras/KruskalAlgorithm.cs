using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2HeadedDog.Math
{
	/// <summary>
	/// Kruskal SPANNING TREE
	/// </summary>
	public static class KruskalAlgorithm
	{
		public static List<Edge> Kruskal(int numberOfVertices, List<Edge> edges)
		{
			// Set parents table
			var parent = Enumerable.Range(0, numberOfVertices).ToArray();

			// Spanning tree list
			var spanningTree = new List<Edge>();
			foreach (var edge in edges)
			{
				var startNodeRoot = FindRoot(edge.p1, parent);
				var endNodeRoot = FindRoot(edge.p2, parent);

				if (startNodeRoot != endNodeRoot)
				{
					// Add edge to the spanning tree
					spanningTree.Add(edge);

					// Mark one root as parent of the other
					parent[endNodeRoot] = startNodeRoot;
				}
			}

			// Return the spanning tree
			return spanningTree;
		}

		private static int FindRoot(int node, int[] parent)
		{
			var root = node;
			while (root != parent[root])
			{
				root = parent[root];
			}

			while (node != root)
			{
				var oldParent = parent[node];
				parent[node] = root;
				node = oldParent;
			}

			return root;
		}
	}
}