using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SESDAD.CommonTypes {

    [Serializable]
    public class Entry {

        private String topicName;
        private String content;

        public Entry(String topicName, String content) {
            this.topicName = topicName;
            this.content = content;
        }

        public String TopicName { get { return topicName; } }
        public String Content { get { return content; } }
    }
}
