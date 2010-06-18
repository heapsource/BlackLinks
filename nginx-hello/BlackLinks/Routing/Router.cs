using System;
using System.Text;
using System.Collections.Generic;

namespace BlackLinks.Routing
{
	public class Router
	{
		BlackApplication app;
		internal Router(BlackApplication app)
		{
			this.app = app;	
		}
		public BlackApplication ApplicationInstance
		{
			get
			{
				return this.app;
			}
		}
		Route notFoundRoute, rootRoute;
		/// <summary>
		/// Root of the Application, it's name will be always '/'.
		/// </summary>
		public Route RootRoute
		{
			get
			{
				return rootRoute;
			}
			set
			{
				rootRoute = value;
				if(rootRoute != null)
					rootRoute.Router = this;
			}
		}
		
		/// <summary>
		/// The route used when there is no matched route.
		/// </summary>
		public Route NotFoundRoute
		{
			get
			{
				return notFoundRoute;
			}
			set
			{
				notFoundRoute = value;
				if(notFoundRoute != null)
					notFoundRoute.Router = this;
			}
		}
		
		public RouteEvaluation Evaluate(string resourcePath)
		{
			return Evaluate(new BlackContext(this.app,resourcePath));
		}
		/// <summary>
		/// Evaluates the Routing tree and return at least one route.
		/// </summary>
		/// <param name="resourcePath">
		/// HTTP path to the requested resource.
		/// </param>
		/// <returns>
		/// Returns a route to execute.
		/// </returns>
		public RouteEvaluation Evaluate(BlackContext context)
		{
			if(this.RootRoute == null)
				throw new BlackException("Router requires at least a root route",null);
			
			if(context == null)
				throw new ArgumentNullException("context");
			
			RouteWalker walker = new RouteWalker();
			walker.router = this;
			walker.ResourcePath = context.ResourcePath;
			walker.Context = context;
			var finalRoute = walker.Walk();
			if(finalRoute.Route != null)
			{
				return finalRoute;
			}
			return new RouteEvaluation
			{
				Route = NotFoundRoute
			};
		}
	}
	
}
