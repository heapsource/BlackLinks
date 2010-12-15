using System;
using System.Linq;
using BlackLinks.Routing;
using BlackLinks.Templates;
using System.Threading;

namespace BlackLinks
{
	public class BlackApplication
	{
		public BlackApplication ()
		{
			this.Routes = new BlackLinks.Routing.Router (this);
			this.Templates = new TemplatesManager ();
		}
		public Routing.Router Routes{get;private set;}
		
		
		public BlackAction ActivateActionCore (Type controllerType, string actionName)
		{
			if (controllerType == null)
				throw new ArgumentNullException ("controllerType");
			if (actionName == null)
				throw new ArgumentNullException ("actionName");
			var controllerInstance = (Controller)Activator.CreateInstance (controllerType);
			
			var actionType = (from nt in controllerType.GetNestedTypes ()
				where nt.Name == actionName
				select nt).FirstOrDefault ();
			if (actionType == null)
				throw new BlackException (string.Format ("No type for action '{0}' was found in controller '{1}'", actionName, controllerType.AssemblyQualifiedName), null);
			
			var actionInstance = (BlackAction)Activator.CreateInstance (actionType);
			controllerInstance.own (actionInstance);
			actionInstance.loadFilters ();
			return actionInstance;
		}
		
		internal void EnsureControllerType (Type controllerType)
		{
		
		}
		public TemplatesManager Templates {get;private set;}
		bool Initialized;
		static readonly object StartLock = new object();
		
		protected virtual void OnStart ()
		{
		
		}
		
		void initialize ()
		{
			this.OnStart ();
			Initialized = true;
		}
		
		void ensureInitialized ()
		{
			lock (StartLock) {
				if (!Initialized) {
					this.initialize();
				}
			}
		}
		
		internal void ProcessRequest (BlackRequest request)
		{
			ensureInitialized();
			BlackContext context = new BlackContext(this,request.Uri);
			context.Request = request;
			
			try
			{
				Routing.RouteEvaluation evalResult = null;
				//All evaluation and Pre-Execution(on dynamic routing discovery) exceptions will be popped up to root error routing(if present) or BlackLinkx default.
					evalResult = this.Routes.Evaluate(context);
					if(evalResult.Route == null)
					{
						Console.Error.WriteLine("Route evaluation returned nothing to show");
					}
					else
					{
						Console.Error.WriteLine("Route evaluation returned {0} as Controller and Action {1}",evalResult.Route.ControllerType.FullName,evalResult.Route.ActionName);
					}
				
				if(evalResult == null)
					evalResult = new Routing.RouteEvaluation();
				
				if(evalResult.Route != null)
				{
					try
					{
						var action = evalResult.InstantiatedAction != null ? evalResult.InstantiatedAction :
							context.ActivateAction(evalResult.Route);
						Console.Error.WriteLine("Executing Action");
						action.Execute(ActionExecuteType.Complete);
						Console.Error.WriteLine("Action next phase is: {0}",action.NextPhase);
					}
					catch(Exception ex)
					{
						context.LastError = ex;
						executeErrorAction(evalResult.Route,context);
					}
				}
			}catch(Exception ex)
			{
				context.LastError = ex;
				if(this.Routes.RootRoute.ErrorRoute == null)
				{
					context.writeDefaultErrorPage();
					return;
				}
				else
				{
					executeErrorAction(this.Routes.RootRoute,context);
				}
			}
		}
		
		/// <summary>
		/// Executes the error route or it's parent. This function does not catch any exception.
		/// </summary>
		/// <param name="route">
		/// A <see cref="Routing.Route"/>
		/// </param>
		void executeErrorAction (Routing.Route route, BlackContext context)
		{
			Route currentRoute = route;
			up_route:
			Route errorRoute = currentRoute.ErrorRoute;
			if (errorRoute == null && currentRoute.Parent != null)
			{
				currentRoute = currentRoute.Parent;
				goto up_route;
			}
			if (errorRoute == null)
			{
				context.writeDefaultErrorPage ();
			}
			else
			{
				var errorAction = context.ActivateAction (errorRoute);
				errorAction.Execute (ActionExecuteType.Complete);
			}
		}
		
		/// <summary>
		/// Translate an Extension into a HTTP Mime Type.
		/// </summary>
		/// <param name="extension">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string MimeForEntityName(string entityName)
		{
			if(entityName.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase))
				return "text/html";
			if (entityName.EndsWith (".text", StringComparison.InvariantCultureIgnoreCase))
				return "text/plain";
			return null;	
		}
	}
}
