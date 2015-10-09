using System;
using System.Collections.Generic;
using System.Text;

public class MyRemoteObject : MarshalByRefObject, MyRemoteInterface {
	public string MetodoOla() {
		//return "ola!";
		throw new MyException(3, this);
	}
	public string OlaSemExcepcao() {
		return "ola!";
	}
}

public interface MyRemoteInterface {
	 string MetodoOla();
	 string OlaSemExcepcao();
}

[Serializable]
public class MyException : ApplicationException {
	public int campo;
	public MyRemoteInterface mo;

	public MyException(int c, MyRemoteInterface mo) {
		campo = c;
		this.mo = mo;
	}

	public MyException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context)
		: base(info, context) {
		campo = info.GetInt32("campo");
		mo = (MyRemoteInterface)info.GetValue("mo", typeof(MyRemoteInterface));
	}

	public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) {
		base.GetObjectData(info, context);
		info.AddValue("campo", campo);
		info.AddValue("mo", mo);
	}
}
