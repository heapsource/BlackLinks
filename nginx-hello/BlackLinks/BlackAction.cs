using System;
using System.Collections.ObjectModel;
using System.IO;

namespace BlackLinks
{
	public abstract class BlackAction
	{
		public Routing.Route Route{get;internal set;}
		public BlackAction()
		{
			this.Filters = new Collection<Filter>();
		}
		public Collection<Filter> Filters{get;private set;}
		protected virtual void OnLoadFilters()
		{
			foreach(var filter in (Filter[])this.GetType().GetCustomAttributes(typeof(Filter),true))
			{
				this.Filters.Add(filter);
			}
			Console.Error.WriteLine("{0} declarative filters has been loaded for type {1}",this.Filters.Count,this.GetType().FullName);
		}
		internal void loadFilters()
		{
			Console.Error.WriteLine("Loading Filters");
			this.OnLoadFilters();
		}
		void executeFilters()
		{
			Console.Error.WriteLine("Executing {0} Filters",this.Filters.Count);
			foreach(var filter in Filters)
			{
				Console.Error.WriteLine("Executing Filter:{0}",filter.GetType().FullName);
				filter.execute(this);
			}
		}
		
		/// <summary>
		/// Gets a value that indicates wheter the action has been lookup.
		/// </summary>
		public bool Lookup{get;private set;}
		
		/// <summary>
		/// Load all the information required to execute the action. If it's not explicitly called, its called automatically right before Execute.
		/// </summary>
		protected virtual void OnLookup()
		{
			
		}
		
		public void ExecuteLookup()
		{
			this.OnLookup();
			this.Lookup = true;
		}
		public void EnsureLookup()
		{
			if(this.Lookup) return;
			this.ExecuteLookup();
		}
		
		
		/// <summary>
		/// Final step of the Action Chain, execute the action.
		/// </summary>
		public abstract void OnExecute();
		
		public ActionPhase NextPhase{get;private set;}
		
		/// <summary>
		/// Execute the phases of the action.
		/// </summary>
		/// <param name="type">
		/// A <see cref="ActionExecuteType"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		internal bool Execute(ActionExecuteType type)
		{
			this.Writer = new StreamWriter(this.Context.Request.ResponseBody);
			
			bool result = false;
			if(this.NextPhase == ActionPhase.Filters)
			{
				try
				{
					this.executeFilters();
					result = true;
				}finally
				{
					this.NextPhase  = ActionPhase.Execute;
						
				}
			}
			if(!result)return result;
			/*
				if(this.NextPhase == ActionPhase.Authenticate)
				{
					try
					{
						result = this.OnAuthenticate();
					}finally
					{
						this.NextPhase  = ActionPhase.Lookup;
					}
				}
				if(!result)return result;
				if(this.NextPhase == ActionPhase.Lookup)
				{
					try
					{
						var lresult =this.OnLookup(); 
						result = lresult == ActionLookupResult.Found || lresult == ActionLookupResult.Skipped;
					}finally
					{
						this.NextPhase  = ActionPhase.Authorize;
					}
				}
			if(!result)return result;
				if(this.NextPhase == ActionPhase.Authorize)
				{
					try
					{
						result =this.OnAuthorize(); 
					}finally
					{
						this.NextPhase  = ActionPhase.Execute;
					}
				}
			if(!result)return result;
			*/
			if(!this.Lookup)
			{
				this.OnLookup();
			}
			if(type == ActionExecuteType.Complete && this.NextPhase == ActionPhase.Execute)
			{
					try
					{
						this.OnExecute();
						result = true;
					}finally
					{
						this.NextPhase  = ActionPhase.Finish;
					}
			}
			return result;
		}
		
		public Controller ControllerInstance{get;internal set;}
		public BlackContext Context { get; internal set; }
		
		public void RenderHtmlView (string viewName)
		{
			this.Context.Request.ResponseContentType = "text/html";
			var template = this.Context.ApplicationInstance.Templates.DiscoverInstance(viewName);
			template.Render(this.Writer);
			this.Writer.Flush();
		}
		public TextWriter Writer{get;private set;}
	}
}