using System;
using BlackLinks.Hosting;
using System.Runtime.InteropServices;
using System.Reflection;

public class NginxBlackHostManager : HostManager
{
	public NginxBlackHostManager ()
	{

	}
	
	public const int NGX_OK = 0;

	public int Process (IntPtr nginx_request)
	{
		Console.Error.WriteLine ("NginxBlackHostManager Is processing request at domain = {0} ", AppDomain.CurrentDomain.FriendlyName);
		
		
		var request = NginxBlackRequest.RequestFromNginxRequest (nginx_request);
		
		this.ProcessRequest (request);
		MainApp.NginxWriteResponse (nginx_request, request.ResponseBodyMemory.ToArray (), request.ResponseContentType, request.ResponseStatusCode);
		return NGX_OK;
	}
	protected override void OnInitialize ()
	{
		
	}
}
