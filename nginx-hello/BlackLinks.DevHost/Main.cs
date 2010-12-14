using System;
using System.IO;
using BlackLinks.Hosting;


namespace BlackLinks.DevHost
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.Error.WriteLine ("BlackLinks Development Host");
			if (args.Length == 0)
			{
				Console.Error.WriteLine ("BlackLinks Web Application path is required");
				System.Environment.Exit (1);
			}
			string appDir = args[0];
			
			if (!Directory.Exists (appDir)) 
			{
				Console.Error.WriteLine ("{0} is not a valid directory", appDir);
				System.Environment.Exit (2);
			}
			var app = GatewayHostManager.LoadApplication<GatewayHostManager> (appDir);

			
			Console.WriteLine("Development Server Ready");
			var ev = new System.Threading.ManualResetEvent (false);
			
			ev.WaitOne();
			
		}
	}
}

