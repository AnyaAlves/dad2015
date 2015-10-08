using System;

namespace CommonTypes
{
    //<summary>
    // Message repository Interface
    //</summary>
    public interface IConversation
    {
        void Message(String value);
        void ClientName(String value);
        string ShowMessage();
    }
}