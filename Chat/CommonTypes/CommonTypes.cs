using System;

namespace CommonTypes
{
    //<summary>
    // Message repository Interface
    //</summary>
    public interface IConversation
    {
        void Message { get; set; }
        void Register(String nickname, String value);
        String getPort(String nickname);
    }
}