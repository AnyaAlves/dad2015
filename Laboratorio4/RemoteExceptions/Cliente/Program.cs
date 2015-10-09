using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;	
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

	class Client {
		static void Main() {
			TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel, false);
			MyRemoteInterface obj = (MyRemoteInterface)Activator.GetObject(
			typeof(MyRemoteInterface),
			"tcp://localhost:8086/MyRemoteObjectName");
			if (obj == null)
				System.Console.WriteLine("Could not locate server");
			else {
				try {
					Console.WriteLine(obj.MetodoOla());
				} catch (MyException e) {
					Console.WriteLine("I caught an exception: " + e.campo);
					Console.WriteLine("I caught an exception: " + e.mo.OlaSemExcepcao());
				}
			}
			Console.ReadLine();
		}
	}

