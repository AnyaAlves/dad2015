using System;
using System.Collections.Generic;

namespace SESDAD.PuppetMaster.Library {
    public class Node {
        private Node parent;
        private IList<Node> children;

        public Node(Node parentValue = null) {
            parent = parentValue;
            children = new List<Node>();
        }

        public void Add(Node child) {
            children.Add(child);
        }
    }
}