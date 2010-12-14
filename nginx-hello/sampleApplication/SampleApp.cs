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
			this.Routes.RootRoute.MemberRoutes.Add (
				new Route { 
				ActionName = "GetBigPage", 
				Name = "GetBigPage", ControllerType = typeof(SearchController) });
			
			this.Routes.RootRoute.MemberRoutes.Add(new Route{
				ActionName = "Index",
				Name = "Patients",
				ControllerType = typeof(PatientsController)
			});
		}
		protected override void OnStart ()
		{
			base.OnStart ();
			this.Templates.RegisterFileCompiler (".ecs", typeof(BlackLinks.Templates.CSharpTemplateCompiler));
			var resourceCompilation = this.Templates.Compile (BlackLinks.Templates.TemplateSource.FromResource ("sampleApplication.Views.Patients.Index.ecs", "sampleApplicationViews.Patients.Index"));
			if (!resourceCompilation.Success)
			{
				throw new Exception("Error Compiling the Stuff:" + resourceCompilation.CompilationErrors[0].ErrorText);
			}
		}
	}
}
