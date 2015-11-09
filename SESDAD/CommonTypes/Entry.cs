using System;

namespace SESDAD.CommonTypes {

    [Serializable]
    public class Entry {
        private String topicName,
                       content;
        private ProcessHeader publisherHeader;
        private int seqNumber;

        public Entry(
            String newTopicName,
            String newContent,
            ProcessHeader newPubliserHeader,
            int newSeqNumber) {
            topicName = newTopicName;
            content = newContent;
            publisherHeader = newPubliserHeader;
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
                "Publisher: " + publisherHeader.ProcessURL + nl +
                "#Seq: " + seqNumber + nl;
        }
    }
}
