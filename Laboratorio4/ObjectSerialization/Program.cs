using System;
using System.IO;

public class clsPerson {
	public string FirstName;
	public string MI;
	public string LastName;
}

class class1 {
	static void Main(string[] args) {
		clsPerson p = new clsPerson();
		p.FirstName = "John";
		p.MI = "A";
		p.LastName = "Smith";
		TextWriter tw = new StreamWriter(@"obj.txt");
		System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(p.GetType());
		x.Serialize(tw, p);
		Console.WriteLine("object written to file");
		Console.ReadLine();
		tw.Close();
		
		TextReader tr = new StreamReader(@"obj.txt");
		clsPerson fileP = (clsPerson)x.Deserialize(tr);
		Console.WriteLine("The person in the file is called " + fileP.FirstName +
			" " + fileP.MI + " " + fileP.LastName + ".");
		tr.Close();
		Console.ReadLine();
	}
}
