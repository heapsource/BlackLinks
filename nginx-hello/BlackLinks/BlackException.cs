
using System;

namespace BlackLinks
{
	public class BlackException : ApplicationException
	{
		public BlackException (string message, Exception innerException) : base(message,innerException)
		{
			
		}
	}
}
