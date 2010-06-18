using System;
using BlackLinks;
using BlackLinks.Routing;

namespace sampleApplication
{
	public class SampleApp : BlackApplication
	{
		public SampleApp ()
		{
			Console.Error.WriteLine("Sample App instantiated");
			this.Routes.NotFoundRoute = new Route
			{
				ActionName="NotFound",
				Name ="/NotFound",
				ControllerType = typeof(LandingController)
			};     
			this.Routes.RootRoute = new Route
			{
				ActionName="Index",
				Name ="/",
				ControllerType = typeof(LandingController)
			};
			this.Routes.RootRoute.MemberRoutes.Add(new Route
			{ 
				ActionName="Go",
				Name ="Go",   
				ControllerType = typeof(SearchController)
			});
			this.Routes.RootRoute.MemberRoutes.Add(new Route
			{
				ActionName="GoArgs",
				Name ="GoGet",
				ControllerType = typeof(SearchController)
			});
		}
	}
}
