using System;
using System.Collections.Generic;

namespace PuppetMasterService {
    public class Site {
        private static rootHasBeenDefined
        private Site parent;
        private IList<Site> children;

        public Site(Site parentValue = null) {
            parent = parentValue;
            children = new List<Site>();
        }

        public void Add(Site child) {
            children.Add(child);
        }
    }
}