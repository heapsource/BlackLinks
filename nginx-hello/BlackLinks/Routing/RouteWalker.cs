
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace BlackLinks.Routing
{
	internal class RouteWalker
	{
		public string ResourcePath;
		public Route LastEvaluatedRoute;
		//public bool FinalRouteFound;
		List<string> PathParts = null;
		int LastPartIndex = 0;
		int CurrentPartIndex = -1;
		internal Router router;
		public BlackContext Context;
		public bool IsLastPart
		{
			get
			{
				return this.CurrentPartIndex == this.LastPartIndex;
			}
		}
		public bool IsFirstPart
		{
			get
			{
				return this.CurrentPartIndex == 0;
			}
		}
		public string CurrentPart
		{
			get
			{
				return PathParts[CurrentPartIndex];
			}
		}
		public bool MoveNextPart()
		{
			if(this.IsLastPart)return false;
			this.CurrentPartIndex++;
			return true;
		}
		public RouteEvaluation Walk()	
		{
			if(string.IsNullOrEmpty(this.ResourcePath))
				this.ResourcePath = "/";
			
			this.PathParts = new List<string>();
			StringBuilder sb = new StringBuilder();
			foreach(char c in this.ResourcePath)
			{
				if(c == '/')
				{
					if(this.PathParts.Count == 0)
					{
						this.PathParts.Add("/");
						sb = null;
						continue;
					}
					else
					{
						this.PathParts.Add(sb.ToString());
						sb = null;
						continue;
					}
				}
				else
				{
					if(sb == null) sb = new StringBuilder();
					sb.Append(c);
				}
			}
			if(sb != null)
				this.PathParts.Add(sb.ToString());
			
			
			this.LastPartIndex = this.PathParts.Count -1;

			while(MoveNextPart())
			{
				var part = CurrentPart;
				
				if(part.IsSlash()) //the only slash we ever found is the root.
				{
					this.LastEvaluatedRoute = this.router.RootRoute;
					if(this.IsLastPart)
					{
						return new RouteEvaluation
						{
							Route = this.LastEvaluatedRoute
						};
					}
				}
				else //is not root, then we either dynamic... shit or sub...shit :)
				{
					Route memberRoute = null;
					if(this.LastEvaluatedRoute.MemberRoutes.Count != 0)
					{
						memberRoute = (from r in this.LastEvaluatedRoute.MemberRoutes where string.Compare(r.Name,part,true) == 0 select r).FirstOrDefault();
						if(memberRoute != null)
						{
							this.LastEvaluatedRoute = memberRoute;
							if(this.IsLastPart)
							{
								return new RouteEvaluation
								{
									Route = this.LastEvaluatedRoute
								};
							}
						}
					}
					if(memberRoute == null && this.LastEvaluatedRoute.DynamicRoute != null)
					{
						var action = this.Context.ActivateAction(this.LastEvaluatedRoute.DynamicRoute);
						if(action.Execute(ActionExecuteType.Filters))
						{
							this.LastEvaluatedRoute = this.LastEvaluatedRoute.DynamicRoute;
						}
						if(this.IsLastPart)
						{
							return new RouteEvaluation
							{
								InstantiatedAction = action,
								Route = this.LastEvaluatedRoute
							};
						}
					}
				}
			}
			return new RouteEvaluation();
		}
	}
}