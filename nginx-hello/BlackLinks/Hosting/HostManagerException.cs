using System;
using System.Runtime.Serialization;

namespace BlackLinks
{
	[Serializable]
	public class HostManagerException : Exception
	{
		public HostManagerException (string message) : base(message)
		{
		
		}
		protected HostManagerException( SerializationInfo info, 
            StreamingContext context ) :
                base( info, context )
        { }

	}
}

