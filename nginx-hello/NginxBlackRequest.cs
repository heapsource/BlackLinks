using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

using BlackLinks;

	public class NginxBlackRequest : BlackRequest
	{
		internal NginxMonoRequestInfo info;
		IntPtr nginx_request;
		Stream requestBody;
		internal MemoryStream ResponseBodyMemory = null;
		public NginxBlackRequest(IntPtr nginx_request,NginxMonoRequestInfo info, NginxMonoHeader[] headers)
		{
		
			this.info = info;
		if(headers == null)
			Console.Error.WriteLine("headers IS NULL");
		Console.Error.WriteLine("headers count {0}",headers.Length);
		for(int i = 0;i <info.headers_count;i++)
		{
			Console.Error.WriteLine("Header Returned Key= '{0}',Value='{1}'",headers[i].Key,headers[i].Value);
		}
		 Console.Error.WriteLine("Finished Iterating headers");
	/*	foreach(var h in headers)
		{
			Console.Error.WriteLine("H={0},V={1}",h.Key,h.Value);
		}*/
			this.headers = (from h in headers select new RequestHeader
		                {
					Key = h.Key,
					Value = h.Value
				}).ToArray();
		 foreach(var h in this.headers)
		{
			Console.Error.WriteLine("header Key= '{0}',Value='{1}'",h.Key,h.Value);
		}
		
			ResponseBodyMemory = new MemoryStream();
			this.nginx_request = nginx_request;
		Console.Error.WriteLine("Content Type={0}",this.ContentType);
			if(this.ContentType == "application/x-www-form-urlencoded")
			{
				WaitForBody();
				using(StreamReader reader = new StreamReader(this.requestBody))
				{
					string encodedFormString = reader.ReadToEnd();
					string[] formPairs = encodedFormString.Split('&');
					foreach(string pair in formPairs)
					{
						var equalCharIndex = pair.IndexOf('=');
						var name = pair.Substring(0,equalCharIndex);
						var val =  System.Web.HttpUtility.UrlDecode(pair.Substring(equalCharIndex+1));
					Console.Error.WriteLine("Form Key={0},Value={1}",name,  val);
						this.FormValues.Add(name,val);
					}
				}
			}
				if(!string.IsNullOrEmpty(this.info.args))
		{
			string[] argsPairs = this.info.args.Split('&');
					foreach(string pair in argsPairs)
					{
						var equalCharIndex = pair.IndexOf('=');
				if(equalCharIndex == -1)
				{
					//if there is no value pair, the key is the value and the value is an empty string.
					this.Arguments.Add(pair,string.Empty);
					continue;	
				}
						Console.Error.WriteLine("equalCharIndex={0}",equalCharIndex);
						var name = pair.Substring(0,equalCharIndex);
						var val =  System.Web.HttpUtility.UrlDecode(pair.Substring(equalCharIndex+1));
					Console.Error.WriteLine("Arg Key={0},Value={1}",name,  val);
						this.Arguments.Add(name,val);
					}
		}
					
		}
	
	 	RequestHeader[] headers;
	
		public override string Uri {
			get {
				return this.info.uri;
			}
		}
			
		public override string HttpProtocol {
			get {
				return this.info.http_protocol;
			}
		}
		public override string MethodName {
			get {
				return this.info.method_name;
			}
		}
		public override RequestHeader[] Headers {
			get {
				return headers;
			}
		}

		public override void AddResponseHeader (RequestHeader header)
		{
			MainApp.AddResponseHeader(this.nginx_request,header.Key,header.Value);
		}
	
	internal void WaitForBody()
	{
		ManualResetEvent ev = new ManualResetEvent(false);
			ReadBodyAsync(() =>
			{
				ev.Set();
			});
			ev.WaitOne();
	}
	internal void ReadBodyAsync(Action finishedReading)
	{
		MainApp.ReadClientBody(this.nginx_request,Marshal.GetFunctionPointerForDelegate(new MainApp.ReadClientBodyCallback((r) => 
		{
			string file = MainApp.GetRequestBodyFileName(this.nginx_request);
			/*
			
#if PERSIST_TEMP_FILE
			if(File.Exists("/home/chamo/request.bin")) File.Delete("/home/chamo/request.bin");
			
			File.Copy(file,"/home/chamo/request.bin");
#endif
			MainApp.WriteNginxLog(string.Format("Temporary Filename = '{0}'",file));
			*/
			requestBody = File.OpenRead(file);
			
			/*if(this.IsMultipart)
			{
				string boundary = this.ContentType.Substring(this.ContentType.IndexOf("boundary"));
				string boundaryValue = boundary.Split('=')[1];
				MainApp.WriteNginxLog(string.Format("MultiPart Boundary is '{0}'",boundaryValue));
				multiParts = MultiPartUtil.GetPartsFromBodyStream(this.PostStream,boundaryValue).ToArray();
				Files = (from part in multiParts
				         let contentDisposition = part.Headers["Content-Disposition"]
				         let contentType = part.Headers["Content-Type"]
				         where contentDisposition != null && contentDisposition["filename"] != null select new HttpFile
				         {
					FileName =contentDisposition["filename"],
					ContentType = contentType != null ? contentType.Value : string.Empty,
					Stream = part.Stream
				}).ToArray();
	
			}*/
			if(finishedReading != null) finishedReading();
		})));
	}
		
		public override Stream RequestBody {
			get {
				return requestBody;
			}
		}
	public override Stream ResponseBody {
		get {
			return this.ResponseBodyMemory;
		}
	}

	
	}

