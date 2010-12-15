#define DEBUG
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using BlackLinks;
using BlackLinks.Hosting;
using Mono.Remoting.Channels.Unix;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;

[assembly: AssemblyVersion("1.0.0.0")]

public class NginxRequest : IDisposable
{
	internal NginxMonoRequestInfo info;
	IntPtr nginx_request;
	public NginxRequest(IntPtr nginx_request,NginxMonoRequestInfo info, NginxMonoHeader[] headers)
	{
		this.info = info;
		this.Headers = headers;
		this.nginx_request = nginx_request;
	}

	public string MethodName{get{return info.method_name;}}
	public string HttpProtocol{get{return info.http_protocol;}}
	public string Uri{get{return info.uri;}}
	public string Args{get{return info.args;}}
	public NginxMonoHeader[] Headers{get;private set;} 
	public void AddResponseHeader(NginxMonoHeader header)
	{
		MainApp.AddResponseHeader(nginx_request,header.Key,header.Value);
	}
	public void AddResponseHeader(string key,string value)
	{
		AddResponseHeader(new NginxMonoHeader(key,value));
	}
	public void WaitForBody()
	{
		ManualResetEvent ev = new ManualResetEvent(false);
			ReadBodyAsync(() =>
			{
				ev.Set();
			});
			ev.WaitOne();
	}
	public void ReadBodyAsync(Action finishedReading)
	{
		MainApp.ReadClientBody(this.nginx_request,Marshal.GetFunctionPointerForDelegate(new MainApp.ReadClientBodyCallback((r) => 
		{
			string file = MainApp.GetRequestBodyFileName(this.nginx_request);
#if PERSIST_TEMP_FILE
			if(File.Exists("/home/chamo/request.bin")) File.Delete("/home/chamo/request.bin");
			
			File.Copy(file,"/home/chamo/request.bin");
#endif
			MainApp.WriteNginxLog(string.Format("Temporary Filename = '{0}'",file));
			this.PostStream = File.OpenRead(file);
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
			//MainApp.ngx_http_upstream_init(this.nginx_request);
			if(finishedReading != null) finishedReading();
		})));
	}
//	public bool IsMultipart
//	{
//		get
//		{
//			return ContentType.StartsWith("multipart/form-data");
//		}
//	}
//	FormPart[] multiParts;
//	public FormPart[] MultiParts
//	{
//		get
//		{
//			if(!IsMultipart) return null;
//			return multiParts;
//		}
//	}
	public Stream PostStream{get;private set;}
	public void Dispose()
	{
		if(this.PostStream != null)
		{
			this.PostStream.Dispose();
		}
	}
	string contentType;
	public string ContentType
	{
		get
		{
			return contentType != null ? contentType : contentType= (from h in this.Headers where h.Key =="Content-Type" select h).First().Value;
		}
	} 
	//public HttpFile[] Files{get; private set;}
}

[StructLayout(LayoutKind.Sequential)]
public struct NginxMonoRequestInfo
{
	internal string method_name;
	internal string http_protocol;
	internal string uri;
	internal string args;
	internal int headers_count;
}

[StructLayout(LayoutKind.Sequential)]
[Serializable]
public struct NginxMonoHeader 
{
	/*public NginxMonoHeader() //used from embedding.
	{
		
	}*/
	public NginxMonoHeader(string key,string value)
	{
		Key = key;
		Value = value;
		//this.AdditionalValues = new List<NginxMonoHeader>();
	}
	public string Key;
	public string Value;
	/*public List<NginxMonoHeader> AdditionalValues{get;private set;}
	public string this[string key]
	{
		get{
			var additionalHeader = (from h  in this.AdditionalValues where h.Key == key select h).FirstOrDefault();
			return additionalHeader == null ? null :additionalHeader.Value;
		}
	}*/
}

public class MainApp
{
	public MainApp()
	{
		
	}
	#if DEBUG
	static IntPtr global_nginx_request;

	public static void WriteNginxLog(string message)
	{
		if(global_nginx_request.ToInt32() == 0)
			Console.WriteLine("Nginx Error:{0}",message);
		else
			WriteNginxLog(global_nginx_request,message);
	}
#endif
	readonly object AppLoadingLock  = new object();
	NginxBlackHostManager AppInstance = null;
	FileSystemWatcher[] watchers = null;
	void reloadApp ()
	{
		Console.Error.WriteLine ("Nginx Mono Host is Loading the Application");
		lock (AppLoadingLock)
		{
			if (watchers != null)
			{
				foreach (var watcher in watchers)
				{
					watcher.EnableRaisingEvents = false;
					watcher.Dispose ();
				}
			}
			if (AppInstance != null)
			{
				try
				{
					Console.Error.WriteLine ("Nginx Mono Host is Unloading existing HostManager");
					AppInstance.Unload ();
				}
				finally {
					AppInstance = null;
				}
			}
			try
			{
				string appDir = appFolder;
				Console.Error.WriteLine ("Nginx Mono Host running at '{0}' is Loading new HostManager for directory '{1}'",Environment.CurrentDirectory,appDir);
				AppInstance = NginxBlackHostManager.LoadApplication<NginxBlackHostManager>(appDir);
			}finally
			{
				initWatchers();
			}
			
		}
	}
	void initWatchers()
		{
		if(!File.Exists(Path.Combine(appFolder,"app.force-restart")))
			{
				File.Create(Path.Combine(appFolder,"app.force-restart")).Close();
			}
		this.watchers = new FileSystemWatcher[3];
		this.watchers[0] = createWatcher("*.dll");
		this.watchers[1] = createWatcher("app.config");
		this.watchers[2] = createWatcher("app.force-restart");
	}
		FileSystemWatcher createWatcher(string filter)
		{
		
			var watcher = new FileSystemWatcher(appFolder,filter);
			watcher.NotifyFilter = NotifyFilters.LastWrite
            | NotifyFilters.FileName |NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size;
			//watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Attributes;
			watcher.Changed+=new FileSystemEventHandler((s,e)=>
			                                            {
			Console.Error.WriteLine("Nginx Mono Host Detected some changes {0} file",e.FullPath);
				reloadApp();		 
			});
			watcher.Deleted+=new FileSystemEventHandler((s,e)=>
			                                            {
			Console.Error.WriteLine("Nginx Mono Host Detected {0} file as deleted",e.FullPath);
				reloadApp();		
			});
			watcher.Created+=new FileSystemEventHandler((s,e)=>
			                                            {
			Console.Error.WriteLine("Nginx Mono Host Detected {0} file as created",e.FullPath);
				reloadApp();		
			});
		watcher.EnableRaisingEvents = true;
			return watcher;
		}
	string appFolder {
		get
		{
			return MainApp.GetConfigurationAppPathDirectory ();
		}
	}
	public bool DevMode 
	{
		get
		{
			return false;
		}
	}
	
	public void EnsureInitializeApp ()
	{
		Console.Error.WriteLine("Ensure Initialize App");
		if(requestsGateway == null)
		{
		string unixChannelPipePath = Path.GetFullPath (Path.Combine (appFolder, RequestsGateway.ApplicationHostManagerDescriptorFileName));
		if(File.Exists(unixChannelPipePath))
		{
			File.Delete(unixChannelPipePath);
		}
		UnixChannel devChannel = new UnixChannel (unixChannelPipePath);
		ChannelServices.RegisterChannel (devChannel, false);
		requestsGateway = new NginxRequestsGateway (this.appFolder);
		string objName = RequestsGateway.ApplicationHostManagerDescriptorObjectName;
		requestsGatewayRef = RemotingServices.Marshal (requestsGateway, objName);
		Console.Error.WriteLine ("Requests Gateway ({0}) Exposed at {1}",objName, unixChannelPipePath);
		}
	}
	
	ObjRef requestsGatewayRef;
	NginxRequestsGateway requestsGateway;
	
	public unsafe int Process(IntPtr nginx_request)
	{  
		Console.Error.WriteLine("Unix User={0}, Group={1}",Mono.Unix.Native.Syscall.getuid(),Mono.Unix.Native.Syscall.getgid());
		if(DevMode)
		{

			Console.Error.WriteLine("Receiving Request in Development Mode");
			
			//string serverUrl = DevelopmentServer.GetApplicationUnixChannelAbsoluteUri(this.appFolder);
			//Console.Error.WriteLine("Connecting to {0}",serverUrl);
			//devHost = (DevelopmentServer)Activator.GetObject (typeof(DevelopmentServer), serverUrl);
		
			Console.Error.WriteLine("Passing Nginx Request Thru NginxRequestGateway");
			requestsGateway.Process(nginx_request.ToInt64());
			
			return NginxBlackHostManager.NGX_OK;
		}
		else
		{
		lock(AppLoadingLock)
		{
			if(AppInstance == null)
			{
				reloadApp();
			}
		}
		return AppInstance.Process(nginx_request);
		}
		/*
#if DEBUG
		global_nginx_request = nginx_request;
#endif
		MainApp.WriteNginxLog("Request Received in C# ");
		var headers = GetNginxMonoRequestInfo(nginx_request);
		
		var request_headers = GetNginxHeaders(nginx_request);
		using(NginxRequest request = new NginxRequest(nginx_request,headers,request_headers))
{
		StringBuilder texts = new StringBuilder();
		
		texts.AppendLine(string.Format("MethodName={0}<br/>",request.MethodName));
		texts.AppendLine(string.Format("HttpProtocol={0}<br/>",request.HttpProtocol));
		texts.AppendLine(string.Format("Uri={0}<br/>",request.Uri));
		texts.AppendLine(string.Format("Args={0}<br/>",request.Args));
		if(request.Headers == null)
		{
			texts.AppendLine("Headers is Null");
		}
		else
		{
			texts.AppendLine(string.Format("Headers {0} <br/>",request_headers.Length));
			foreach(var h in request.Headers)
				texts.AppendLine(string.Format("Header Key= {0}, Value ={1} <br/>",
					h.Key == null ? "Null" : h.Key,
					h.Value == null ? "Null" : h.Value));

		}
		texts.AppendLine("<form  enctype= \"multipart/form-data\"  action=\"/hellopost\" method=\"POST\"><input type=\"text\" name=\"texto\" value=\"este es mi valor\"></input><input type=\"text\" name=\"texto2\" value=\"este es mi valor para texto 2\"></input><input type=\"file\" name=\"filesitoA\" ></input></input><input type=\"file\" name=\"filesitoB\" ></input><input type=\"submit\" value=\"Submit\"></input></form>");
		texts.AppendLine("<form action=\"/hellopost\" method=\"POST\"><input type=\"text\" name=\"first_name\" value=\"Johan\"></input></input><input type=\"submit\" value=\"Submit\"></input></form>");
			if(request.Uri == "/hellopost")
		{
			request.WaitForBody();
			*/
			/*if(request.IsMultipart)
			{
				texts.Append("<h2>Is multipart!!!</h2>");
				foreach(var part in request.MultiParts)
							{
								texts.AppendFormat("<h3>Form part found</h3>");
								foreach(var header in part.Headers)
								{
									texts.AppendLine(string.Format("\tHeader {0} = {1}<br/>",header.Key,header.Value));
									foreach(var addVal in header.AdditionalValues)
										texts.AppendLine(string.Format("\t Sub-Header {0} = {1}<br/>",addVal.Key,addVal.Value));
								}
							}
					
					foreach(var file in request.Files)
							{
								texts.AppendFormat("<h3>File Found {0}</h3>",file.ContentType);
							}
				foreach(var file in request.Files)
							{
								texts.AppendFormat("File found:{0} ({1})<br/>",file.FileName,file.ContentType);
							}
					
					
					Console.WriteLine("Header of the Part");
					
			}*/
		/*
			request.PostStream.Position = 0;
			using(StreamReader reader = new StreamReader(request.PostStream))
			{
				texts.Append("Body: <pre>");
				texts.Append(reader.ReadToEnd());
				texts.Append("</pre>");
			}
		}

		request.AddResponseHeader("XXX","this is my value");
		texts.AppendLine("<br/>Domain Base:" + AppDomain.CurrentDomain.BaseDirectory);
		texts.AppendLine("<br/>Domain Base:" + System.Environment.CurrentDirectory);
		return NginxWriteResponse(nginx_request,Encoding.UTF8.GetBytes(texts.ToString()),"text/html",200);
		
		}
		*/
	}

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static int NginxWriteResponse (IntPtr nginx_request,byte[] bytes,string contentType,int statusCode);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static NginxMonoRequestInfo GetNginxMonoRequestInfo (IntPtr nginx_request);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void GetNginxHeaders (IntPtr nginx_request,
	                                             [Out]
	                                            [MarshalAs(UnmanagedType.LPArray)]
	                                             out NginxMonoHeader[] headers,int count);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void AddResponseHeader (IntPtr nginx_request,string key,string value);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void ReadClientBody (IntPtr nginx_request, IntPtr callback);

	public delegate void ReadClientBodyCallback(IntPtr nginx_request);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static string GetRequestBodyFileName (IntPtr nginx_request);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void WriteNginxLog (IntPtr nginx_request,string text);

	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void SecureEnvironment ();
	
	/*[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static void ngx_http_upstream_init (IntPtr nginx_request);*/
	
	[DllImport ("__Internal")]
	[MethodImplAttribute(MethodImplOptions.InternalCall)]
	internal extern static String GetConfigurationAppPathDirectory ();
	
	internal static RequestHeader[] GetRequestHeaders (IntPtr nginx_request)
	{
		var info = MainApp.GetNginxMonoRequestInfo (nginx_request);
		Console.Error.WriteLine ("Headers Count = {0} ", info.headers_count);
		NginxMonoHeader[] xheaders = null;
		MainApp.GetNginxHeaders (nginx_request, out xheaders, info.headers_count);
		
		NginxMonoHeader[] headers = new NginxMonoHeader[info.headers_count];
		for (int i = 0; i < info.headers_count; i++) {
			headers[i] = xheaders[i];
		}
		return (from h in headers
			select new RequestHeader { Key = h.Key, Value = h.Value }).ToArray ();
	}
	internal static void WriteNginxDiagnosticPageFooter(StringBuilder pageBuffer)
	{
		pageBuffer.AppendFormat("<p style=\"color:gray\">BlackLinks Framework {0} / Nginx</p>",typeof(MainApp).Assembly.GetName().Version.ToString());
	}
}
static class NginxPointerExtension
{
	public static IntPtr ToPointer(this long p)
	{
		return new IntPtr(p);
	}
}
class NginxRequestsGateway : RequestsGateway
{
	string appDirectory;
	public NginxRequestsGateway (string appDirectory)
	{
		this.appDirectory = appDirectory;
		/*
		 * UnixChannel c = new UnixChannel ();
		 * ChannelServices.RegisterChannel(c,false);
		*/
	}
	public override RequestHeader[] GetHeaders (long nginx_request)
	{
		return MainApp.GetRequestHeaders (nginx_request.ToPointer());
	}
	public override string GetMethodName (long requestId)
	{
		return MainApp.GetNginxMonoRequestInfo (requestId.ToPointer()).method_name;
	}
	public override string GetHttpProtocol (long requestId)
	{
		return MainApp.GetNginxMonoRequestInfo (requestId.ToPointer()).http_protocol;
	}
	public override string GetQueryString (long requestId)
	{
		return MainApp.GetNginxMonoRequestInfo (requestId.ToPointer()).args;
	}
	public override string GetUri (long requestId)
	{
		return MainApp.GetNginxMonoRequestInfo (requestId.ToPointer()).uri;
	}
	public override byte[] GetRequestContent (long requestId)
	{
		ManualResetEvent ev = new ManualResetEvent (false);
		byte[] data = null;
		MainApp.ReadClientBody (requestId.ToPointer(), Marshal.GetFunctionPointerForDelegate (new MainApp.ReadClientBodyCallback (r =>
		{
			string file = MainApp.GetRequestBodyFileName (requestId.ToPointer());
			using (var requestBody = File.OpenRead (file))
			{
				data = new byte[requestBody.Length];
				requestBody.Read (data, 0, data.Length);
			}
			ev.Set ();
		})));
		ev.WaitOne ();
		return data;
	}
	public override void AddResponseHeader (long requestId, RequestHeader header)
	{
		MainApp.AddResponseHeader (requestId.ToPointer (), header.Key, header.Value);
	}
	public override void WriteResponse (long requestId, byte[] bytes, string contentType, int statusCode)
	{
		MainApp.NginxWriteResponse(requestId.ToPointer(),bytes,contentType,statusCode);
	}
	public override void Process (long nginx_request)
	{
		string receiverPath = RequestsGateway.GetReceiverUnixChannelAbsoluteUri (this.appDirectory);
		Console.Error.WriteLine ("Getting Receiver from {0}", receiverPath);
		try 
		{
			this.Receiver = (IRequestReceiver)Activator.GetObject (typeof(IRequestReceiver), receiverPath);
			if (this.Receiver != null)
			{
				this.Receiver.Ping ();
			}
		} catch (System.Runtime.Remoting.RemotingException ex)
		{
			Console.Error.WriteLine ("Error connecting to {0}:", receiverPath, ex.Message);
			this.Receiver = null;
		}
		if (this.Receiver == null)
		{
			Console.Error.WriteLine ("Could not any listener at {0}", receiverPath);
		}
		else 
		{
			Console.Error.WriteLine ("Receiver Found!");
		}
		base.Process (nginx_request);
	}
	protected override void RenderReceiverNotFoundPage (long requestId)
	{
		StringBuilder sb = new StringBuilder ();
		sb.AppendLine("<html><head><title>BlackLinks Gateway Error</title></head>");
		sb.AppendLine("<body>");
		sb.AppendLine("<h1 style=\"color:red;\">Gateway Error</h1>");
		sb.AppendFormat("<p>Application Directory: {0}</p>",this.appDirectory);
		MainApp.WriteNginxDiagnosticPageFooter(sb);
		sb.AppendLine("</body></html>");
		this.WriteResponse(requestId,System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString()),"text/html",500);
	}
}