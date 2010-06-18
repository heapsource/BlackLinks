using System;

namespace BlackLinks
{
	public sealed class BlackContext
	{
		public BlackContext (BlackApplication applicationInstance,string resourcePath)
		{
			this.ApplicationInstance = applicationInstance;
			this.ResourcePath = resourcePath;
		}
		
		public BlackApplication ApplicationInstance{get;private set;}
		public BlackRequest Request{get;internal set;}
		
		public string ResourcePath{get;private set;}
		
		public BlackAction ActivateAction(Routing.Route route)
		{
			return this.ActivateAction(route.ControllerType,route.ActionName);
		}
		public BlackAction ActivateAction(Type controllerType,string actionName)
		{
			var action =this.ApplicationInstance.ActivateActionCore(controllerType,actionName);
			action.Context = this;
			return action;
		}
		public Exception LastError{get;set;}

		public void writeDefaultErrorPage ()
		{
			this.Request.ResponseContentType = "text/html";
			this.Request.ResponseStatusCode = 500;
			this.Request.Write(string.Format("<html><head><title>BlackLink Application Error</title></head><body><h1>This application is facing some problems at this time</h1><br/><h2>{0}-'{1}'<h2><pre>{2}</pre></body></html>",this.LastError.GetType().FullName,this.LastError.Message,this.LastError.StackTrace));
		}
	}
}
