using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public class PublisherService : GenericProcessService<IPublisher>, IPublisherService {
        //public void Ack() { }

        public void ForcePublish(String topicName, String content) {
            PuppetMaster.WriteIntoLog("PubEvent " + Header.ProcessName + ", " + Header.ProcessName + ", " + topicName + ", " + Process.SeqNumber);
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.Publish(topicName, content); }));
                return;
            }
            Process.Publish(topicName, content);
        }
    }
}
