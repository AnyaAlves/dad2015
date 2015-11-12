using System;

namespace SESDAD.Commons {

    [Serializable]
    public class Event {
        public String TopicName { get; private set; }
        public String Content { get; private set; }
        public ProcessHeader PublisherHeader { get; private set; }
        public int SeqNumber { get; private set; }

        public Event(String topicName, String content, ProcessHeader publisherHeader, int seqNumber) {
            TopicName = topicName;
            Content = content;
            PublisherHeader = publisherHeader;
            SeqNumber = seqNumber;
        }

        public override string ToString() {            
            String nl = Environment.NewLine;
             return "Topic Name: " + TopicName + nl +
                "Content: " + Content + nl +
                "Publisher: " + PublisherHeader.ProcessName + "\t" +
                "#Seq: " + SeqNumber + nl;
        }
    }
}
