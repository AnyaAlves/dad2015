using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Commons {
    public interface ISubscriberService : IGenericProcessService {
        void DeliverEvent(Event @event);

        void ForceSubscribe(String topicName);
        void ForceUnsubscribe(String topicName);
    }
}