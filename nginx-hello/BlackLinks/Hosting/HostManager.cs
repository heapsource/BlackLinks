using System;
using System.IO;
using System.Configuration;

namespace BlackLinks.Hosting
{
	/// <summary>
	/// Host manager for BlackApplication.
	/// </summary>
	public abstract class HostManager : MarshalByRefObject
	{
		public BlackApplication ApplicationInstance{get;private set;}
		public HostManager ()
		{
			
		}
		AppDomain HostDomain;
		/// <summary>
		/// Its called outsite the BlackAppDomain to create a host manager. Returns the Proxy to the HostManager instance.
		/// </summary>
		/// <param name="blackApplicationDirectory">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="T"/>
		/// </returns>
		public static T LoadApplication<T>(string blackApplicationDirectory) where T:HostManager, new()
		{
			var type = typeof(T);
			AppDomainSetup setup = new AppDomainSetup();
			setup.ApplicationBase = blackApplicationDirectory;
			setup.ConfigurationFile = Path.Combine(blackApplicationDirectory,"app.config");
			AppDomain appDomain = AppDomain.CreateDomain("BlackApplicationDomain",null,setup);
			var hostManager = (T)appDomain.CreateInstanceAndUnwrap(type.Assembly.FullName,type.FullName);
			hostManager.HostDomain = appDomain;
			hostManager.Initialize();
			return hostManager;
		}
		
		protected void ProcessRequest(BlackRequest request)
		{
			Console.Error.WriteLine("AppDomain {0} received request to {1}",AppDomain.CurrentDomain.FriendlyName,request.Uri);
			this.ApplicationInstance.ProcessRequest(request);	
		}
		internal void Initialize()
		{
			//Load the Application Instance using Config Files.
			var appType = ConfigurationManager.AppSettings["ApplicationTypePath"];
			var type = Type.GetType(appType);
			ApplicationInstance = (BlackApplication)type.Assembly.CreateInstance(type.FullName);
		}
		public void Unload()
		{
			if(HostDomain != null)
				AppDomain.Unload(HostDomain);
			HostDomain = null;
		}
		public override object InitializeLifetimeService ()
		{
			return null; //The controller is kept forever.
		}
	}
}