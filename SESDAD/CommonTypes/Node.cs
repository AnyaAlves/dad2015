using System;
using System.Collections.Generic;

namespace SESDAD.CommonTypes {

    public abstract class Node<T> where T : Node<T> {

        private T parent;
        private String name;
        private IDictionary<String, T> children;

        protected Node(String newName, T newParent) {
            parent = newParent;
            name = newName;
            children = new Dictionary<String, T>();
            if (parent != null) {
                parent.AddChild(this as T);
            }
        }

        public String Name {
            get { return name; }
        }

        public void AddChild(T child) {
            children.Add(child.Name, child);
        }

        public void RemoveChild(String childName) {
            children.Remove(childName);
        }

        public T Parent {
            set { parent = value; }
            get { return parent; }
        }

        public IDictionary<String, T> Children {
            get { return children; }
        }
    }
}