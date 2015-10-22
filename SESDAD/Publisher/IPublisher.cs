using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Processes {
    public interface IPublisher {
        void Publish(String topicName, String content);

        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
