using System;
using BlackLinks;
using System.Collections.Generic;

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
		public class GetBigPage : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType = "text/html";
				this.Context.Request.Write ("<h1>Title Number XXX</h1>");
				for(int i = 0;i < 1000;i++) {
					string s = string.Format ("<p>This is the title of the shit{0}</p>",i);
					this.Context.Request.Write (s);
				}
			}
		}
	}
}