using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

using SESDAD.Commons;

namespace SESDAD.Processes {

    public class EventOrderManager {
        public OrderingType Ordering { get; set; }
        private ConcurrentQueue<EventContainer> inputBuffer;
        //FIFO stuff
        private IDictionary<ProcessHeader, EventTable> fifoBuffer;
        private IDictionary<String, Queue<Event>> pendingDeliveryBuffer;

        public EventOrderManager() {
            inputBuffer = new ConcurrentQueue<EventContainer>();
            fifoBuffer = new Dictionary<ProcessHeader, EventTable>();
            pendingDeliveryBuffer = new Dictionary<String, Queue<Event>>();
        }

        public void InsertIntoInputBuffer(EventContainer eventContainer) {

            if (Ordering == OrderingType.NO_ORDER) {
                inputBuffer.Enqueue(eventContainer);
            }

            else if (Ordering == OrderingType.FIFO) {
                //if publisher is unknown, add it
                ProcessHeader publisher = eventContainer.Event.PublisherHeader;
                EventTable eventTable;
                lock (fifoBuffer) {
                    if (!fifoBuffer.TryGetValue(publisher, out eventTable)) {
                        eventTable = new EventTable(ref inputBuffer);
                        fifoBuffer.Add(publisher, eventTable);
                    }
                }
                lock (eventTable) {
                    eventTable.AddEvent(eventContainer);
                }
            }
        }

        public EventContainer GetNextEvent() {
            EventContainer eventContainer = null;
            while (!inputBuffer.TryDequeue(out eventContainer)) {
                Thread.Sleep(10);
            }
            return eventContainer;
        }

        public void SetPendingEvent(ProcessHeader subscriber, Event @event) {
            if (Ordering == OrderingType.NO_ORDER) { }
            else if (Ordering == OrderingType.FIFO) {
                Queue<Event> pendingDeliveryList;
                lock (pendingDeliveryBuffer) {
                    if (!pendingDeliveryBuffer.TryGetValue(subscriber + @event.PublisherHeader, out pendingDeliveryList)) {
                        pendingDeliveryList = new Queue<Event>();
                        pendingDeliveryBuffer.Add(subscriber + @event.PublisherHeader, pendingDeliveryList);
                    }
                }
                lock (pendingDeliveryList) {
                    if (pendingDeliveryList.Any()) {
                        pendingDeliveryList.Enqueue(@event);
                    }
                }
            }
        }

        public bool TryGetPendingEvent(ProcessHeader subscriber, ProcessHeader publisher, out Event @event) {
            if (Ordering == OrderingType.NO_ORDER) { }
            else if (Ordering == OrderingType.FIFO) {
                Queue<Event> pendingDeliveryList;
                lock (pendingDeliveryBuffer) {
                    pendingDeliveryList = pendingDeliveryBuffer[subscriber + publisher];
                    if (pendingDeliveryList.Any()) {
                        @event = pendingDeliveryList.Dequeue();
                        return true;
                    }
                }
            }
            @event = null;
            return false;
        }

        private class EventTable {
            private ConcurrentQueue<EventContainer> InputBuffer { get; set; }
            private int CurrentSeqNumber { get; set; }
            private IDictionary<int, EventContainer> EventList { get; set; }
            private Thread OrderingThread { get; set; }

            public EventTable(ref ConcurrentQueue<EventContainer> inputBuffer) {
                InputBuffer = inputBuffer;
                CurrentSeqNumber = 0;
                EventList = new Dictionary<int, EventContainer>();
                OrderingThread = new Thread(new ThreadStart(OrderEvents));
                OrderingThread.Start();
            }

            public void AddEvent(EventContainer eventContainer) {
                int newSeqNumber = eventContainer.NewSeqNumber;
                EventList.Add(newSeqNumber, eventContainer);
            }

            public void OrderEvents() {
                EventContainer eventContainer;
                while (true) { //ADD LOCK
                    if (EventList.TryGetValue(CurrentSeqNumber, out eventContainer)) {
                        EventList.Remove(CurrentSeqNumber);
                        CurrentSeqNumber++;
                        InputBuffer.Enqueue(eventContainer);
                    }
                }
            }

        }
    }
}
