using System;
namespace BlackLinks
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ControllerAttribute : Attribute
	{
		public ControllerAttribute ()
		{
		
		}
		public string Route{get;set;}
	}
}

