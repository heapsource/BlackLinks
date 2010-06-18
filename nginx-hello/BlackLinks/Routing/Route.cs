using System;

namespace BlackLinks.Routing
{
	/// <summary>
	/// Represent a route to access a controller inside the application.
	/// </summary>
	public sealed class Route
	{
		public Route ()
		{
			this.MemberRoutes = new RouteCollection(this);
		}
		/// <summary>
		/// Name of the Route (E.g: /list)
		/// </summary>
		public string Name {get;set;}
		
		/// <summary>
		/// System.Type controller to handle the route.
		/// </summary>
		public Type ControllerType{get;set;}
		
		/// <summary>
		/// The name of the action inside the controller.
		/// </summary>
		public string ActionName{get;set;}
		
		Route dynamicRoute;
		/// <summary>
		/// The Dynamic Child route.
		/// </summary>
		public Route DynamicRoute
		{
			get
			{
				return dynamicRoute;
			}
			set
			{
				if(value != null)
					this.own(value);
				this.dynamicRoute = value;
			}
		}
		
		
		Route errorRoute;
		/// <summary>
		/// The Error route. If null, the parent will be used.
		/// </summary>
		public Route ErrorRoute
		{
			get
			{
				return errorRoute;
			}
			set
			{
				if(value != null)
					this.own(value);
				this.errorRoute = value;
			}
		}
		
		internal void own(Route child)
		{
			if(child.Parent != null)
				throw new InvalidOperationException("Route already belongs to a parent");
			child.Router = this.Router;
			child.Parent = this;
		}
		public Route Parent{get;private set;}
		
		/// <summary>
		/// Routes that acts like member of the dynamic items of the feature.
		/// </summary>
		public RouteCollection MemberRoutes{get;private set;}
		
		/// <summary>
		/// Gets a value that says either the Route is Dynamic Route of it's parent.
		/// </summary>
		public bool IsDynamic
		{
			get
			{
				if(this.Parent == null)
					return false;
				return this.Parent.DynamicRoute == this;
			}
		}
		Router router;
		public Router Router
		{
			get{return router;}
			internal set
			{
				this.router = value;
				if(this.router != null)
					this.router.ApplicationInstance.EnsureControllerType(this.ControllerType);
			}
		}
	}
}
