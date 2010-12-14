using System;
using System.IO;
using System.Collections.ObjectModel;

namespace BlackLinks.Templates
{
	/// <summary>
	/// Compiler of Template Source Code.
	/// </summary>
	public abstract class TemplateCompiler
	{
		public TemplateCompiler ()
		{
			this.Sources = new Collection<TemplateSource> ();
		}
		
		public string OutputAssemblyPath { get; set; }
		
		public Collection<TemplateSource> Sources {get;private set;}
		public TemplatesCompilationResult Compile ()
		{
			if (Sources.Count == 0)
			{
				throw new ArgumentException ("Zero Templates to Compile");
			}
			var results = OnCompile ();
			foreach (var source in Sources)
			{
				source.Close();
			}
			return results;
		}
		
		protected abstract TemplatesCompilationResult OnCompile ();
		#region Util
		public static TemplateCompiler CreateInstance (Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException ("type");
			}
			return (TemplateCompiler)Activator.CreateInstance (type);
		}
		#endregion
	}
}

