using System;
using NUnit.Core;
using NUnit.Framework;
using BlackLinks;
using BlackLinks.Routing;

namespace BlackLinks_UnitTests
{
	[TestFixture]
	public class RoutingTest
	{
		class RoutingController : Controller
		{
			
		}
		[Test]
		public void TestNotFoundRoute()
		{
			var router = new BlackApplication().Routes;
			router.RootRoute = new Route()
			{
				ControllerType = typeof(RoutingController),
				ActionName = "Index",
				Name="/"
			};
			router.NotFoundRoute = new Route()
			{
				ControllerType = typeof(RoutingController),
				ActionName = "NotFound",
				Name="/lost"
			};
			Assert.AreEqual(router.NotFoundRoute,router.Evaluate("/this_route_does_not_exists_at_all").Route);
			Assert.AreEqual(router.NotFoundRoute,router.Evaluate("/this/route/does/not/exists/at/all").Route);
		}
		
		class TestRootRouteController : Controller
		{
			
		}
		[Test]
		public void TestRootRoute()
		{
			var router = new BlackApplication().Routes;
			router.NotFoundRoute = new Route()
			{
				ControllerType = typeof(TestRootRouteController),
				ActionName = "NotFound",
				Name=""
			};
			router.RootRoute = new Route()
			{
				ControllerType = typeof(TestRootRouteController),
				ActionName = "Index",
				Name="/"
			};
			Assert.AreEqual(router.RootRoute,router.Evaluate("/").Route);
		}
		
		class TestDynamicRoutesAndMemberRoutesController : Controller
		{
			public class Item : BlackAction
			{
				public override void OnExecute ()
				{
					
				}
			}
		}
		
		[Test]
		public void TestDynamicRoutesAndMemberRoutes()
		{
			var router = new BlackApplication().Routes; 
			Route root = null, item = null, custom = null,subShow = null;
			root= router.RootRoute = new Route()
			{
				ControllerType = typeof(TestDynamicRoutesAndMemberRoutesController),
				ActionName = "Index",
				Name="/"
			};
			
			router.RootRoute.DynamicRoute = item = new Route()
			{
				ControllerType = typeof(TestDynamicRoutesAndMemberRoutesController),
				ActionName = "Item",
				Name="id"
			};
			
			item.MemberRoutes.Add(custom = new Route()
			{
				ControllerType = typeof(TestDynamicRoutesAndMemberRoutesController),
				ActionName = "Custom",
				Name="custom"
			});
			
			Assert.AreEqual(router.RootRoute,router.Evaluate("/").Route,"Route / should be found for /");
			Assert.AreEqual(item,router.Evaluate("/23").Route,"Route Controller.Item should be found for /23");
			Assert.AreEqual(item,router.Evaluate("/23/").Route,"Route Controller.Item should be found for /23/");
			Assert.AreEqual(custom,router.Evaluate("/23/custom").Route,"Route Controller.Custom should be found for /23/custom");
			Assert.AreEqual(custom,router.Evaluate("/23/custom/").Route,"Route Controller.Custom should be found for /23/custom/");
			Assert.AreEqual(custom,router.Evaluate("/23/Custom").Route,"Route Controller.Custom should be found for /23/Custom (routing is case insensitive)");
		}
	}
}
