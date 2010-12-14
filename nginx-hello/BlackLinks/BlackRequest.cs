using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace BlackLinks
{
	public abstract class BlackRequest : MarshalByRefObject, IDisposable
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
			var data = ResponseTextEncoding.GetBytes (text);
			this.ResponseBody.Write (data, 0, data.Length);
			this.ResponseBody.Flush ();
		}
		
		private void ParseFormValuesIfAny ()
		{
			if (this.ContentType == "application/x-www-form-urlencoded") {
				EnsureRequestBody ();
				using (StreamReader reader = new StreamReader (this.RequestBody)) {
					string encodedFormString = reader.ReadToEnd ();
					string[] formPairs = encodedFormString.Split ('&');
					foreach (string pair in formPairs) {
						var equalCharIndex = pair.IndexOf ('=');
						var name = pair.Substring (0, equalCharIndex);
						var val = System.Web.HttpUtility.UrlDecode (pair.Substring (equalCharIndex + 1));
						//Console.Error.WriteLine ("Form Key={0},Value={1}", name, val);
						this.FormValues.Add (name, val);
					}
				}
			}
		}
		
		private void ParseArgumentsIfAny ()
		{
			if (!string.IsNullOrEmpty (this.QueryString)) {
				string[] argsPairs = this.QueryString.Split ('&');
				foreach (string pair in argsPairs) {
					var equalCharIndex = pair.IndexOf ('=');
					if (equalCharIndex == -1) {
						//if there is no value pair, the key is the value and the value is an empty string.
						this.Arguments.Add (pair, string.Empty);
						continue;
					}
					//Console.Error.WriteLine ("equalCharIndex={0}", equalCharIndex);
					var name = pair.Substring (0, equalCharIndex);
					var val = System.Web.HttpUtility.UrlDecode (pair.Substring (equalCharIndex + 1));
					//Console.Error.WriteLine ("Arg Key={0},Value={1}", name, val);
					this.Arguments.Add (name, val);
				}
			}
		}
		
		protected void Initialize ()
		{
			this.ParseFormValuesIfAny ();
			this.ParseArgumentsIfAny ();
		}
		
		protected abstract void EnsureRequestBody ();
		
		public abstract string QueryString { get; }
	}
}
