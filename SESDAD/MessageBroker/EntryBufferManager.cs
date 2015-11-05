using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {

    using PendingDeliveryList = IList<Tuple<ProcessHeader, Entry>>;

    public class EntryBufferManager {
        //FIFO stuff
        private IDictionary<ProcessHeader, int> seqNumberList;
        private IList<Entry> submissionBuffer;
        private IDictionary<String, PendingDeliveryList> pendingDeliveryBuffer;
        private OrderingType ordering;

        public OrderingType Ordering {
            set { ordering = value; }
        }

        public EntryBufferManager() {
            submissionBuffer = new List<Entry>();
            pendingDeliveryBuffer = new Dictionary<String, PendingDeliveryList>();

            seqNumberList = new Dictionary<ProcessHeader, int>();
        }

        public void InsertIntoSubmissionBuffer(Entry entry) {
            lock (submissionBuffer) {
                submissionBuffer.Add(entry);
            }
        }

        public void InsertIntoPendingDeliveryBuffer(ProcessHeader subscriberHeader, Entry entry) {
            lock (pendingDeliveryBuffer) {
                if (ordering == OrderingType.FIFO) {
                    PendingDeliveryList pendingDeliveryList;
                    if (!pendingDeliveryBuffer.TryGetValue(subscriberHeader + entry.PublisherHeader, out pendingDeliveryList)) {
                        pendingDeliveryList = new List<Tuple<ProcessHeader, Entry>>();
                    }
                    pendingDeliveryList.Add(new Tuple<ProcessHeader, Entry>(subscriberHeader, entry));
                }
            }
        }

        public void RemoveFromPendingDeliveryBuffer(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            lock (pendingDeliveryBuffer) {
                if (ordering == OrderingType.FIFO) {
                    pendingDeliveryBuffer.Remove(subscriberHeader + publisherHeader);
                }
            }
        }

        public Entry GetEntry() {
            Entry entry = null;

            lock (submissionBuffer) {
                if (ordering == OrderingType.NO_ORDER) {
                    entry = submissionBuffer.First();
                }
                else if (ordering == OrderingType.FIFO) {
                    foreach (Entry bufferEntry in submissionBuffer) {
                        int seqNumber = 0;
                        if (!seqNumberList.TryGetValue(bufferEntry.PublisherHeader, out seqNumber)) {
                            seqNumberList.Add(bufferEntry.PublisherHeader, seqNumber);
                        }
                        if (seqNumber == bufferEntry.SeqNumber) {
                            entry = bufferEntry;
                            seqNumberList[entry.PublisherHeader]++;
                            break;
                        }
                    }
                }
                submissionBuffer.Remove(entry);
            }
            return entry;
        }
    }
}
