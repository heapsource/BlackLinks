using System;
using System.IO;

namespace BlackLinks.Hosting
{
	/// <summary>
	/// Exposes Methods to get and write responses to server side requests using .NET Remoting.
	/// </summary>
	public abstract class RequestsGateway : MarshalByRefObject
	{
		protected IRequestReceiver Receiver { get; set; }
		
		public const string ApplicationHostManagerDescriptorFileName = "listener";
		public const string ApplicationHostManagerDescriptorObjectName = "gateway";
		
		public const string ApplicationHostManagerDescriptorReceiverFileName = "listener.receiver";
		public const string ApplicationHostManagerDescriptorReceiverObjectName = "receiver";
		
		public static string GetApplicationUnixChannelAbsoluteUri (string blackApplicationDirectory)
		{
			return string.Format ("unix://{0}?{1}", Path.Combine (blackApplicationDirectory, ApplicationHostManagerDescriptorFileName), ApplicationHostManagerDescriptorObjectName);
		}
		
		public static string GetReceiverUnixChannelAbsoluteUri (string blackApplicationDirectory)
		{
			return string.Format ("unix://{0}?{1}", Path.Combine (blackApplicationDirectory, ApplicationHostManagerDescriptorReceiverFileName), ApplicationHostManagerDescriptorReceiverObjectName);
		}
		
		public abstract string GetMethodName(long requestId);
		public abstract void AddResponseHeader (long requestId, RequestHeader header);
		public abstract string GetUri (long requestId);
		public abstract RequestHeader[] GetHeaders(long requestId);
		
		public abstract string GetHttpProtocol (long requestId);
		
		public abstract byte[] GetRequestContent(long requestId);
		
		public abstract string GetQueryString(long requestId);
		
		public virtual void Process (long requestId)
		{
			if (this.Receiver != null)
			{
				Console.Error.WriteLine ("Sending nginx_request {0} to gateway receiver", requestId);
				this.Receiver.Process (requestId);
			}
			else
			{
				Console.Error.WriteLine ("No Receiver is Listening");
				RenderReceiverNotFoundPage(requestId);
			}
		}
		
		public override object InitializeLifetimeService ()
		{
			return null;
		}
		
		public abstract void WriteResponse(long requestId, byte[] bytes, string contentType, int statusCode);
		
		protected abstract void RenderReceiverNotFoundPage(long requestId);
	}
}

