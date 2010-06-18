
using System;
using BlackLinks.Routing;
using BlackLinks;


namespace BlackLinks_UnitTests
{
	public class MainClass
	{
		class RoutingController : Controller
		{
			
		}
		class TestRootRouteController : Controller
		{
			public class Item : BlackAction
			{
				public override void OnExecute ()
				{
					
				}
			}
		}
		public static void Main()
		{
			var router = new BlackApplication().Routes; 
			Route root = null, item = null, custom = null,subShow = null;
			root= router.RootRoute = new Route()
			{
				ControllerType = typeof(TestRootRouteController),
				ActionName = "Index",
				Name="/"
			};
			
			router.RootRoute.DynamicRoute = item = new Route()
			{
				ControllerType = typeof(TestRootRouteController),
				ActionName = "Item",
				Name="id"
			};
			
			item.MemberRoutes.Add(custom = new Route()
			{
				ControllerType = typeof(TestRootRouteController),
				ActionName = "Custom",
				Name="custom"
			});
			var result = router.Evaluate("/34/custom");
		}
	}
}
