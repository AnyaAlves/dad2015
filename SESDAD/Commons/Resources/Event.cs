using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SESDAD.Commons {

    [Serializable]
    public class Event : ISerializable {
        public String TopicName { get; private set; }
        public String Content { get; private set; }
        public ProcessHeader PublisherHeader { get; private set; }
        public int SeqNumber { get; private set; }

        public Event() {}

        public Event(String topicName, String content, ProcessHeader publisherHeader, int seqNumber) {
            TopicName = topicName;
            Content = content;
            PublisherHeader = publisherHeader;
            SeqNumber = seqNumber;
        }

        public Event(SerializationInfo info, StreamingContext context) {
            TopicName = info.GetString("topicName");
            Content = info.GetString("content");
            PublisherHeader = (ProcessHeader)info.GetValue("publisherHeader", typeof(ProcessHeader));
            SeqNumber = info.GetInt32("seqNumber");
        }

        public Event Clone() {
            Event other = (Event)this.MemberwiseClone();
            other.TopicName = String.Copy(TopicName);
            other.Content = String.Copy(Content);
            other.PublisherHeader = PublisherHeader.Clone();
            return other;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("topicName", TopicName, typeof(String));
            info.AddValue("content", Content, typeof(String));
            info.AddValue("publisherHeader", PublisherHeader, typeof(ProcessHeader));
            info.AddValue("seqNumber", SeqNumber, typeof(int));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
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
