using System;
namespace BlackLinks.Hosting
{
	public interface IRequestReceiver
	{
		void Process(long requestId);
		void Ping();
	}
}

