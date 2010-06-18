using System;
using BlackLinks.Hosting;
using System.Runtime.InteropServices;
using System.Reflection;

public class NginxBlackHostManager : HostManager
{
	public NginxBlackHostManager ()
	{

	}

	public int Process(IntPtr nginx_request)
	{
		Console.Error.WriteLine("NginxBlackHostManager Is processing request at domain = {0} ",AppDomain.CurrentDomain.FriendlyName);
		var info = MainApp.GetNginxMonoRequestInfo(nginx_request);
		Console.Error.WriteLine("Headers Count = {0} ",info.headers_count);
		NginxMonoHeader[] xheaders = null; //;new NginxMonoHeader[info.headers_count];
		MainApp.GetNginxHeaders(nginx_request,out xheaders,info.headers_count);
		
		NginxMonoHeader[] headers = new NginxMonoHeader[info.headers_count];
		for(int i = 0;i < info.headers_count;i++)
		{
			headers[i] = xheaders[i];	
		}
		
		var request = new NginxBlackRequest(nginx_request,info,headers);
		
		this.ProcessRequest(request);
		MainApp.NginxWriteResponse(nginx_request,request.ResponseBodyMemory.ToArray(),request.ResponseContentType,request.ResponseStatusCode);
		return 0;
	}
}
