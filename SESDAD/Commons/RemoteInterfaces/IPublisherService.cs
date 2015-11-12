using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Commons {
    public interface IPublisherService : IGenericProcessService {
        //void Ack();

        void ForcePublish(String topicName, String content);
    }
}