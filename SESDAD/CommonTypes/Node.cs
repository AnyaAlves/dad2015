using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {

    public class Node {
        private Node parent;
        private IList<Node> children;

        public Node(Node parent) {
            this.parent = parent;
            if (parent != null)
                parent.AddChild(this);
            children = new List<Node>();
        }

        public void AddChild(Node child) {
            children.Add(child);
        }

        public void RemoveChild(Node child) {
            children.Remove(child);
        }

        public void replaceParent(Node parent) {
            this.parent = parent;
        }
    }
}