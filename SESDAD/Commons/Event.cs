using System;

namespace SESDAD.Commons {

    [Serializable]
    public class Event {
        private String topicName;
        private String content;
        private ProcessHeader publisherHeader;
        private int seqNumber;

        public Event(String newTopicName, String newContent, ProcessHeader newPublisherHeader, int newSeqNumber) {
            topicName = newTopicName;
            content = newContent;
            publisherHeader = newPublisherHeader;
            seqNumber = newSeqNumber;
        }

        public String TopicName {
            get { return topicName; }
        }
        public String Content {
            get { return content; }
        }
        public ProcessHeader PublisherHeader {
            get { return publisherHeader; }
        }
        public int SeqNumber {
            get { return seqNumber; }
        }

        public override string ToString() {            
            String nl = Environment.NewLine;
             return "Topic Name: " + topicName + nl +
                "Content: " + content + nl +
                "Publisher: " + publisherHeader.ProcessName + "\t" +
                "#Seq: " + seqNumber + nl;
        }
    }
}
