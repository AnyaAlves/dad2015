using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public class PublisherService : GenericProcessService<IPublisher>, IPublisherService {
        //public void Ack() { }

        public void ForcePublish(String topicName, String content) {
            Entry entry = Process.Publish(topicName, content);
            PuppetMaster.WriteIntoLog("PubEvent " + Header.ProcessName + ", " + Header.ProcessName + ", " + entry.TopicName + ", " + entry.SeqNumber);
        }
    }
}
