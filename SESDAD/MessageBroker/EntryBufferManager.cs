using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {

    public class EntryBufferManager {
        //FIFO stuff
        private IDictionary<ProcessHeader, int> seqNumberList;
        private IList<Entry> inputBuffer;
        private IDictionary<String, Queue<Entry>> pendingDeliveryBuffer;
        private OrderingType ordering;

        public OrderingType Ordering {
            set { ordering = value; }
        }

        public EntryBufferManager() {
            inputBuffer = new List<Entry>();
            pendingDeliveryBuffer = new Dictionary<String, Queue<Entry>>();
            seqNumberList = new Dictionary<ProcessHeader, int>();
        }

        public void InsertIntoInputBuffer(Entry entry) {
            lock (inputBuffer) {
                if (ordering == OrderingType.FIFO) {
                    //if publisher is unknown, add it
                    int seqNumber = 0;
                    if (!seqNumberList.TryGetValue(entry.PublisherHeader, out seqNumber)) {
                        seqNumberList.Add(entry.PublisherHeader, seqNumber);
                    }
                }
                inputBuffer.Add(entry);
            }
        }

        public void MoveToPendingDeliveryBuffer(ProcessHeader subscriber, Entry entry) {
            lock (pendingDeliveryBuffer) {
                if (ordering == OrderingType.FIFO) {
                    Queue<Entry> pendingDeliveryList;
                    if (!pendingDeliveryBuffer.TryGetValue(subscriber + entry.PublisherHeader, out pendingDeliveryList)) {
                        pendingDeliveryList = new Queue<Entry>();
                    }
                    pendingDeliveryList.Enqueue(entry);
                }
            }
        }

        public Entry GetEntry() {
            Entry entry = null;

            lock (inputBuffer) {            
                if (ordering == OrderingType.NO_ORDER) {
                    entry = inputBuffer.First();
                }
                else if (ordering == OrderingType.FIFO) {
                    foreach (Entry bufferEntry in inputBuffer) {
                        int seqNumber = seqNumberList[bufferEntry.PublisherHeader];
                        if (seqNumber == bufferEntry.SeqNumber) {
                            entry = bufferEntry;
                            seqNumberList[entry.PublisherHeader]++;
                            break;
                        }
                    }
                }
                inputBuffer.Remove(entry);
            }
            return entry;
        }

        public void SendPendingEntry(ProcessHeader subscriber, ProcessHeader publisher, MessageBroker mb) { //FIXME
            lock (pendingDeliveryBuffer) {
                if (ordering == OrderingType.NO_ORDER) { return; }
                if (ordering == OrderingType.FIFO) {
                    Queue<Entry> pendingDeliveryList = pendingDeliveryBuffer[subscriber + publisher];
                    pendingDeliveryList.Dequeue();
                    if (pendingDeliveryList.Any()) {
                        Entry entry = pendingDeliveryList.Peek();
                        mb.SendEntry(subscriber, entry);
                    }
                }
            }
        }

        public override String ToString() {
            String nl = Environment.NewLine;
            return "Input Buffer:" + nl + string.Join(nl, inputBuffer) + nl +
                "Pending Delivery Buffer:" + nl + string.Join(nl, pendingDeliveryBuffer.Keys.ToList());
        }


    }
}
