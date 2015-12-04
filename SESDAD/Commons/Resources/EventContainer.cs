using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SESDAD.Commons {
    [Serializable]
    public class EventContainer : ISerializable {
        public ProcessHeader SenderBroker { get; set; }
        public Event Event { get; private set; }
        public int NewSeqNumber { get; set; }

        public EventContainer() { }

        public EventContainer(ProcessHeader senderBroker, Event @event, int newSeqNumber) {
            SenderBroker = senderBroker;
            Event = @event;
            NewSeqNumber = newSeqNumber;
        }

        public EventContainer(EventContainer eventContainer) {
            Event = eventContainer.Event.Clone();
        }

        public EventContainer(SerializationInfo info, StreamingContext context) {
            SenderBroker = (ProcessHeader)info.GetValue("senderBroker", typeof(ProcessHeader));
            Event = (Event)info.GetValue("event", typeof(Event));
            NewSeqNumber = info.GetInt32("newSeqNumber");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("senderBroker", SenderBroker, typeof(ProcessHeader));
            info.AddValue("event", Event, typeof(Event));
            info.AddValue("newSeqNumber", NewSeqNumber, typeof(int));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }

        public EventContainer Clone() {
            EventContainer other = (EventContainer)this.MemberwiseClone();
            other.SenderBroker = SenderBroker.Clone();
            other.Event = Event.Clone();
            other.NewSeqNumber = NewSeqNumber;
            return other;
        }
    }
}