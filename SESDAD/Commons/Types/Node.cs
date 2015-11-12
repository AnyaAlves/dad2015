using System;
using System.Collections.Generic;

namespace SESDAD.Commons {

    public abstract class Node<T> where T : Node<T> {

        public T Parent { get; private set; }
        public String Name { get; private set; }
        public IDictionary<String, T> Children { get; private set; }

        public Node(String name, T parent) {
            Parent = parent;
            Name = name;
            Children = new Dictionary<String, T>();
            if (Parent != null) {
                Parent.AddChild(this as T);
            }
        }

        public void AddChild(T child) {
            Children.Add(child.Name, child);
        }

        public void RemoveChild(String childName) {
            Children.Remove(childName);
        }

        
    }
}