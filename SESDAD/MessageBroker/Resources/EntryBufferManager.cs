using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {

    using Envelope = KeyValuePair<Entry, int>;

    public class EntryBufferManager {
        //FIFO stuff
        private IDictionary<ProcessHeader, int> pubSeqNumberList;
        private IDictionary<Entry, int> inputBuffer;
        private IDictionary<String, Queue<Entry>> pendingDeliveryBuffer;
        private IDictionary<String, bool> isPending;
        private OrderingType ordering;

        public OrderingType Ordering {
            set { ordering = value; }
        }

        public EntryBufferManager() {
            inputBuffer = new Dictionary<Entry, int>();
            pendingDeliveryBuffer = new Dictionary<String, Queue<Entry>>();
            isPending = new Dictionary<String, bool>();
            pubSeqNumberList = new Dictionary<ProcessHeader, int>();
        }

        public void InsertIntoInputBuffer(Entry entry, int brokerSeqNumber) {
            lock (inputBuffer) {
                if (ordering == OrderingType.FIFO) {
                    //if publisher is unknown, add it
                    int seqNumber = 0;
                    if (!pubSeqNumberList.TryGetValue(entry.PublisherHeader, out seqNumber)) {
                        pubSeqNumberList.Add(entry.PublisherHeader, seqNumber);
                    }
                }
                inputBuffer.Add(entry, brokerSeqNumber);
            }
        }

        public bool TryMoveToPendingDeliveryBuffer(ProcessHeader subscriber, Entry entry) {
            lock (pendingDeliveryBuffer) {
                if (ordering == OrderingType.FIFO) {
                    Queue<Entry> pendingDeliveryList;
                    bool pending;
                    if (!pendingDeliveryBuffer.TryGetValue(subscriber + entry.PublisherHeader, out pendingDeliveryList)) {
                        pendingDeliveryList = new Queue<Entry>();
                        pendingDeliveryBuffer.Add(subscriber + entry.PublisherHeader, pendingDeliveryList);
                        isPending.Add(subscriber + entry.PublisherHeader, false);
                    }
                    pending = isPending[subscriber + entry.PublisherHeader];
                    if (pending) {
                        pendingDeliveryList.Enqueue(entry);
                    } else {
                        isPending[subscriber + entry.PublisherHeader] = true;
                    }
                    return pending;
                }
            }
            return false;
        }

        public Entry GetEntry() {
            Entry entry = null;

            lock (inputBuffer) {
                if (ordering == OrderingType.NO_ORDER) {
                    entry = inputBuffer.First().Key;
                }
                else if (ordering == OrderingType.FIFO) {
                    foreach (Envelope envelope in inputBuffer) {
                        entry = envelope.Key;
                        int brokerSeqNumber = envelope.Value;
                        int pubSeqNumber = pubSeqNumberList[entry.PublisherHeader];
                        if (pubSeqNumber == brokerSeqNumber) {
                            pubSeqNumberList[entry.PublisherHeader]++;
                            break;
                        }
                    }
                }
                inputBuffer.Remove(entry);
            }
            return entry;
        }

        public Entry GetPendingEntry(ProcessHeader subscriber, ProcessHeader publisher) { //FIXME
            Queue<Entry> pendingDeliveryList = null;
            Entry entry = null;

            if (ordering == OrderingType.NO_ORDER) { }

            else if (ordering == OrderingType.FIFO) {
                lock (pendingDeliveryBuffer) {
                    pendingDeliveryList = pendingDeliveryBuffer[subscriber + publisher];
                    if (pendingDeliveryList.Any()) {
                        entry = pendingDeliveryList.Dequeue();
                    }
                    else {
                        isPending[subscriber + publisher] = false;
                    }
                }
            }
            return entry;
        }

        public override String ToString() {
            String nl = Environment.NewLine;
            String cOut = "Input Buffer:" + nl + string.Join(nl, inputBuffer) + nl + nl +
                            "Pending Delivery Buffer:" + nl;
            foreach (String subscriber in pendingDeliveryBuffer.Keys) {
                cOut += "Pending to " + subscriber + nl + string.Join(nl, pendingDeliveryBuffer[subscriber]) + nl + nl;
            }
            return cOut;
        }


    }
}
