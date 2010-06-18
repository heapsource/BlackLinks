using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace BlackLinks
{
	public abstract class BlackRequest : IDisposable
	{
		public Encoding ResponseTextEncoding{get;set;}
		public abstract string MethodName{get;}
		public abstract string HttpProtocol{get;}
		public abstract string Uri{get;}
		
		public abstract RequestHeader[] Headers{get;} 
		
		/// <summary>
		/// Http Post Values.
		/// </summary>
		public NameValueCollection FormValues{get;private set;}
		
		/// <summary>
		/// Arguments Sent via Querystring.
		/// </summary>
		public NameValueCollection Arguments{get;private set;}
		public BlackRequest ()
		{
			this.ResponseTextEncoding = Encoding.UTF8;
			this.ResponseContentType = "text/plain";
			this.ResponseStatusCode = 200;
			this.FormValues = new NameValueCollection();
			this.Arguments = new NameValueCollection();
		}
		public abstract void AddResponseHeader(RequestHeader header);
		
		public void AddResponseHeader(string key,string value)
		{
			AddResponseHeader(new RequestHeader(key,value));
		}
		
		public abstract Stream RequestBody{get;}
		void IDisposable.Dispose()
		{
			if(this.RequestBody != null)
			{
				this.RequestBody.Dispose();
			}
		}
		string contentType = string.Empty;
		public virtual string ContentType
		{
			get
			{
				foreach(var h in this.Headers)
				{
					if(string.Compare(h.Key,"Content-Type",true) == 0)
					{
						contentType = h.Value;
					}
				}
				return contentType;
			}
		} 

		public int ResponseStatusCode{get;set;}
		public abstract Stream ResponseBody{get;}
		public string ResponseContentType{get;set;}

		/// <summary>
		/// Write partial response.
		/// </summary>
		/// <param name="text">
		/// Text to be sent using the Encoding of <see cref="ResponseTextEncoding"/> property.
		/// </param>
		public void Write (string text)
		{
			var data = ResponseTextEncoding.GetBytes(text);
			this.ResponseBody.Write( data,0, data.Length);
			this.ResponseBody.Flush();
		}
	}
}
