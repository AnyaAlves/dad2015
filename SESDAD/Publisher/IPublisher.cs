using System;

using SESDAD.Processes;
using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IPublisher : IGenericProcess {
        /// <summary>
        ///  Creates a new entry and sends it to parent broker
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Entry Publish(String topicName, String content);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seqNumber"></param>
        //void Ack(int seqNumber);
    }
}
