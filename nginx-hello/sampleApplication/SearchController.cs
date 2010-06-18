using System;
using BlackLinks;

namespace sampleApplication
{
	public class SearchController : Controller
	{
		public class Go : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType ="text/html";
				this.Context.Request.Write(string.Format("<h2>Do you want to search some?</h2> Form Values={0},BaseDirectory={1}",this.Context.Request.FormValues.Count,AppDomain.CurrentDomain.BaseDirectory));
			}
		}
		class goArgsFilter : Filter
		{
			protected override void OnExecute ()
			{
				this.Action.Context.Request.Write("Go Filter in action or something");
			}
		}
		[goArgsFilter]
		public class GoArgs : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType ="text/html";
				
				foreach(string k in this.Context.Request.Arguments.Keys)
				{
					this.Context.Request.Write(string.Format("<p>{0}={1}</p>",k,this.Context.Request.Arguments[k]));
				}
			}
		}
	}
}