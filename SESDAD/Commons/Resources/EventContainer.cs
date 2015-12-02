using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SESDAD.Commons {
    [Serializable]
    public class EventContainer {
        public ProcessHeader SenderBroker { get; set; }
        public Event Event { get; private set; }
        public int NewSeqNumber { get; set; }

        public EventContainer(ProcessHeader senderBroker, Event @event, int newSeqNumber) {
            SenderBroker = senderBroker;
            Event = @event;
            NewSeqNumber = newSeqNumber;
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