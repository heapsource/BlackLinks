using System;
using System.Collections.ObjectModel;
using System.CodeDom.Compiler;
using System.Reflection;

namespace BlackLinks.Templates
{
	/// <summary>
	/// Compilation Result Information
	/// </summary>
	public class TemplatesCompilationResult
	{
		public TemplatesCompilationResult ()
		{
			this.CompilationErrors = new Collection<CompilerError> ();
			this.Assemblies = new Collection<Assembly>();
		}
		
		/// <summary>
		/// Indicates whether the compilation was successful or not.
		/// </summary>
		public bool Success {get {return this.CompilationErrors.Count == 0;}}
		
		public Collection<CompilerError> CompilationErrors { get; private set; }
		public Collection<Assembly> Assemblies {get; private set;}
	}
}

