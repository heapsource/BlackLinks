using System;
using BlackLinks;

namespace sampleApplication
{
	public class LandingController : Controller
	{
		public LandingController ()
		{
			
		}
		
		public class Index : BlackAction
		{
			static int count = 0;
			public override void OnExecute ()
			{
				count++;
				this.Context.Request.ResponseContentType ="text/html";
				this.Context.Request.Write(string.Format("<html><head><title>This is the title of the shit</title></head><body><h1>Behold!!!, C# and Mono Running from Nginx, Count {0}</h1><form method=\"POST\" action=\"/go\"><input type=\"text\" name=\"term\" value=\"this is my search therm\"/> <input type=\"submit\" value=\"Submit\"/></form><form method=\"GET\" action=\"/goGet\"><input type=\"text\" name=\"term\" value=\"this is my search therm\"/> <input type=\"submit\" value=\"Submit\"/></form></body></html>",count));
			}
		}
		public class NotFound : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType ="text/html";
				this.Context.Request.Write("<h1>Oops, not found</h1>");
			}
		}
		public class ErrorHandler : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType ="text/html";
				this.Context.Request.Write("<h1>Oops, there is an error... sorry for this</h1>");
			}
		}
		
		public class SubErrorHandler : BlackAction
		{
			public override void OnExecute ()
			{
				this.Context.Request.ResponseContentType ="text/html";
				this.Context.Request.Write("<h1>Oops, there is an error... sorry for this(by the SubErrorHandler</h1>");
			}
		}
	}
}
