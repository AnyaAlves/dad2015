using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;


namespace RemotingSample {

	class Client {

		public delegate string RemoteAsyncDelegate();

	  // This is the call that the AsyncCallBack delegate will reference.
   public static void OurRemoteAsyncCallBack(IAsyncResult ar){
		 // Alternative 2: Use the callback to get the return value
      RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
      Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: "  + del.EndInvoke(ar) );
      
        return;
   }

		static void Main() {
                // allocate and register channel
			TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel,true);
                // get reference to remote service
			MyRemoteObject obj = (MyRemoteObject) Activator.GetObject(
				typeof(MyRemoteObject),
				"tcp://localhost:8086/MyRemoteObjectName");   

	 		try
	 		{
                        // first a simple synchronous call
	 			Console.WriteLine(obj.MetodoOla());

                        // change this to true to use the callback (alt.2)
                        bool useCallback = false;

                        if (!useCallback) {
                        // Alternative 1: asynchronous call without callback
                        // Create delegate to remote method
				RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.MetodoOla);
                                // Call delegate to remote method
				IAsyncResult RemAr = RemoteDel.BeginInvoke(null, null);
			  // Wait for the end of the call and then explictly call EndInvoke
				RemAr.AsyncWaitHandle.WaitOne();
				Console.WriteLine(RemoteDel.EndInvoke(RemAr));

                               } else {
                        // Alternative 2: asynchronous call with callback
                        // Create delegate to remote method
				RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(obj.MetodoOla);
                        // Create delegate to local callback
				AsyncCallback RemoteCallback = new AsyncCallback(Client.OurRemoteAsyncCallBack);
                                // Call remote method
				IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
                               }

			}
	 		catch(SocketException)
	 		{
	 			System.Console.WriteLine("Could not locate server");
	 		}

			Console.ReadLine();
		}
	}
}
