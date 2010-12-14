using System;
namespace BlackLinks.Templates
{
	public interface ITemplateInstanceDiscoveryProvider
	{
		TemplateRenderResource DiscoverInstance(string path);
	}
}

