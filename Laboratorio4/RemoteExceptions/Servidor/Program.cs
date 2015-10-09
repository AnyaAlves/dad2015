using System;
using System.Collections;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;

class Server {
    static void Main() {
        BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
        provider.TypeFilterLevel = TypeFilterLevel.Full;
        IDictionary props = new Hashtable();
        props["port"] = 8086;
        TcpChannel channel = new TcpChannel(props, null, provider);
        //TcpChannel channel = new TcpChannel(8086);
        ChannelServices.RegisterChannel(channel, false);
        MyRemoteObject mo = new MyRemoteObject();
        RemotingServices.Marshal(mo,
        "MyRemoteObjectName",
        typeof(MyRemoteObject));
        System.Console.WriteLine("<enter> para sair...");
        System.Console.ReadLine();
    }
}
