using System;
using Mono.Remoting.Channels.Unix;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.IO;

namespace BlackLinks.Hosting
{
	public class GatewayHostManager : Hosting.HostManager
	{
		RequestsGateway gateway;
		public GatewayHostManager ()
		{
			inner = new Inner ();
			inner.gt = this;
		}
		Inner inner = null;
		protected override void OnInitialize ()
		{
			
			UnixChannel channel = new UnixChannel ();
			ChannelServices.RegisterChannel (channel, false);
			string listenerObjectPath = RequestsGateway.GetApplicationUnixChannelAbsoluteUri (Path.GetFullPath (this.BaseDirectory));
			Console.WriteLine ("Connecting to Unix Pipe {0}", listenerObjectPath);
			gateway = (RequestsGateway)Activator.GetObject (typeof(RequestsGateway), listenerObjectPath);
			Console.WriteLine ("Connected to Gateway = {0}", gateway != null);
			//gateway.Receiver = this.inner;
			Console.WriteLine ("Gateway Host has Initialized Connection with Nginx Server");
			
			exposeRequestReceiver();
		}
		void exposeRequestReceiver ()
		{
			Console.WriteLine ("Exposing Receiver");
			var receiverObjectPath = Path.Combine (Path.GetFullPath (BaseDirectory), RequestsGateway.ApplicationHostManagerDescriptorReceiverFileName);
			Console.WriteLine("Enabling Receiver at {0}",receiverObjectPath);
			UnixChannel channel = new UnixChannel (receiverObjectPath);
			RemotingServices.Marshal(this.inner,RequestsGateway.ApplicationHostManagerDescriptorReceiverObjectName);
		}
		class Inner : MarshalByRefObject, IRequestReceiver
		{
			internal GatewayHostManager gt;
			public void Process (long requestId)
			{
				Console.WriteLine ("Processing request {0}", requestId);
				GatewayRequest request = new GatewayRequest (gt.gateway, requestId);
				
				gt.ProcessRequest (request);
				gt.gateway.WriteResponse (requestId, request.responseBody.ToArray (), request.ResponseContentType, request.ResponseStatusCode);
			
			}
			public void Ping ()
			{
				
			}
		}
		
		class GatewayRequest : BlackRequest
		{
			long requestId;
			RequestsGateway gateway;
			MemoryStream requestBody;
			internal MemoryStream responseBody;
			public GatewayRequest (RequestsGateway gateway, long requestId)
			{
				this.requestId = requestId;
				this.gateway = gateway;
				this.requestBody = new MemoryStream ();
				this.responseBody = new MemoryStream ();
				
			}
			public override string MethodName {
				get {
					Console.WriteLine ("MethodName {0}", requestId);
					return gateway.GetMethodName (this.requestId);
				}
			}
			public override void AddResponseHeader (RequestHeader header)
			{
				Console.WriteLine ("AddResponseHeader {0}", requestId);
				gateway.AddResponseHeader (this.requestId, header);
			}
			public override string Uri {
				get {
					Console.WriteLine ("Uri {0}", requestId);
					return gateway.GetUri (this.requestId);
				}
			}
			public override RequestHeader[] Headers {
				get {
					Console.WriteLine ("Headers {0}", requestId);
					return gateway.GetHeaders (this.requestId);
				}
			}
			public override string HttpProtocol {
				get {
					Console.WriteLine ("HttpProtocol {0}", requestId);
					return gateway.GetHttpProtocol (this.requestId);
				}
			}
			public override Stream RequestBody {
				get {
					return requestBody;
				}
			}
			public override Stream ResponseBody {
				get {
					return responseBody;
				}
			}
			protected override void EnsureRequestBody ()
			{
				byte[] requestContent = null;
				requestContent = gateway.GetRequestContent (requestId);
				if (requestContent != null) {
					this.requestBody.Write (requestContent, 0, requestContent.Length);
				}
			}
			public override string QueryString {
				get {
					Console.WriteLine ("QueryString {0}", requestId);
					return gateway.GetQueryString(requestId);
				}
			}
		}
	}
}

