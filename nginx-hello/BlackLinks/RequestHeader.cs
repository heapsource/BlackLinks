
using System;

namespace BlackLinks
{
	[Serializable]
	public class RequestHeader
	{
		public RequestHeader()
		{
			
		}
		public RequestHeader(string key,string value)
		{
			Key = key;
			Value = value;
		}
		public string Key {get; set;}
		public string Value {get; set;}
	}
}
