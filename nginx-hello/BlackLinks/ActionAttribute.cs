using System;
namespace BlackLinks
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ActionAttribute : Attribute
	{
		public ActionAttribute ()
		{
		
		}
		public string Name{get;set;}
	}
}

