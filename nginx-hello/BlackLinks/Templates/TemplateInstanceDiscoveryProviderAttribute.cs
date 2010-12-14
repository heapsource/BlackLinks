using System;
using System.Reflection;

namespace BlackLinks.Templates
{
	[AttributeUsage(AttributeTargets.Assembly,AllowMultiple = false)]
	public class TemplateInstanceDiscoveryProviderAttribute : Attribute
	{
		public TemplateInstanceDiscoveryProviderAttribute ()
		{
			
		}
		public TemplateInstanceDiscoveryProviderAttribute (Type type)
		{
			this.DiscoveryType = type;
		}
		public Type DiscoveryType{get;set;}
	}
}

