using System;
using System.Collections.Generic;

namespace SESDAD.CommonTypes {

    public abstract class Node<T> where T : Node<T> {

        private T parent;
        protected IList<T> children;

        protected Node(T newParent) {
            parent = newParent;
            children = new List<T>();
            if (parent != null) {
                parent.AddChild(this as T);
            }
        }

        internal void AddChild(T child) {
            children.Add(child);
        }

        internal void RemoveChild(T child) {
            children.Remove(child);
        }

        internal T Parent {
            set { parent = value; }
            get { return parent; }
        }
    }
}