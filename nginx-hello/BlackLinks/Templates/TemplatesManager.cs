using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BlackLinks.Templates
{
	/// <summary>
	/// Templates Source Manager.
	/// </summary>
	public sealed class TemplatesManager
	{
		Dictionary<string,Type> fileCompilers = new Dictionary<string, Type>();
		List<ITemplateInstanceDiscoveryProvider> instanceProviders = new List<ITemplateInstanceDiscoveryProvider>();
		
		public TemplatesManager ()
		{
			
		}
		
		string normalizeFileExtension (string filePath)
		{
			return filePath.ToLower ();
		}
		
		/// <summary>
		/// Register a File Compiler for an Extension
		/// </summary>
		/// <param name="fileExtension">
		/// A <see cref="System.String"/> with the extension. (e.g hcs)
		/// </param>
		/// <param name="templateCompilerType">
		/// A <see cref="Type"/> of a class that Inherits from <see cref="TemplateCompiler"/>.
		/// </param>
		public void RegisterFileCompiler (string fileExtension, Type templateCompilerType)
		{
			if (fileExtension == null)
			{
				throw new ArgumentNullException ("fileExtension");
			}
			if (templateCompilerType == null)
			{
				throw new ArgumentNullException ("templateCompilerType");
			}
			this.fileCompilers.Add (normalizeFileExtension (fileExtension), templateCompilerType);
		}
		public TemplatesCompilationResult Compile (params TemplateSource[] sources)
		{
			var parameters = new TemplatesCompilationParameters ();
			foreach (var source in sources)
			{
				parameters.Templates.Add(source);
			}
			return Compile(parameters);
		}
		/// <summary>
		/// Compiles a Template Filer.
		/// </summary>
		/// <param name="filePath">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="TemplateCompilationResult"/>
		/// </returns>
		/// <exception cref="TemplateCompilerNotFoundException">No template compiler was found for the given file extension</exception>
		public TemplatesCompilationResult Compile (TemplatesCompilationParameters parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			TemplatesCompilationResult generalResult = new TemplatesCompilationResult ();
			Dictionary<string, TemplateCompiler> compilerInstances = new Dictionary<string, TemplateCompiler> ();
			foreach (TemplateSource source in parameters.Templates)
			{
				var fileExtension = normalizeFileExtension (source.TemplateFileExtension);
				if (!this.fileCompilers.ContainsKey (fileExtension)) {
					throw new TemplateCompilerNotFoundException (string.Format ("No template compiler was found for extension '{0}'", fileExtension));
				}
				TemplateCompiler compiler = null;
				if (compilerInstances.ContainsKey (fileExtension))
				{
					compiler = compilerInstances[fileExtension];
				}
				else
				{
					compiler = TemplateCompiler.CreateInstance (this.fileCompilers[fileExtension]);
					compilerInstances[fileExtension] = compiler;
				}
				compiler.Sources.Add (source);
			}
			foreach (TemplateCompiler compiler in compilerInstances.Values)
			{
				var compilationResult = compiler.Compile ();
				foreach (var error in compilationResult.CompilationErrors)
				{
					generalResult.CompilationErrors.Add (error);
				}
				foreach (var assembly in compilationResult.Assemblies)
				{
					generalResult.Assemblies.Add (assembly);
					extractInstanceDiscoveryProvider(assembly);
				}
			}
			return generalResult;
		}
		void extractInstanceDiscoveryProvider (Assembly asm)
		{
			var atts = (TemplateInstanceDiscoveryProviderAttribute[])asm.GetCustomAttributes (typeof(TemplateInstanceDiscoveryProviderAttribute), true);
			if (atts.Length != 0)
			{
				var att = atts[0];
				if (att.DiscoveryType != null)
				{
					this.instanceProviders.Add (
						(ITemplateInstanceDiscoveryProvider)Activator.CreateInstance (att.DiscoveryType));
				
				}
			}
		}
		
		/// <summary>
		/// Creates an Instance of the <see cref="TemplateRenderResource"/> associated with the path.
		/// </summary>
		/// <param name="path">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="TemplateRenderResource"/> if found, otherwise, null.
		/// </returns>
		public TemplateRenderResource DiscoverInstance (string path)
		{
			foreach (var provider in this.instanceProviders)
			{
				TemplateRenderResource resource = provider.DiscoverInstance (path);
				if (resource != null)
					return resource;
			}
			return null;
		}
		
		public bool SupportsExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");
			return this.fileCompilers.ContainsKey(normalizeFileExtension(extension));
		}
		
		public TemplatesCompilationResult AddEmbeddedTemplates (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			
			List<TemplateSource> sources = new List<TemplateSource> ();
			foreach (var resourceName in assembly.GetManifestResourceNames ())
			{
				FileInfo fileInfo = new FileInfo (resourceName);
				
				TemplateSource source = TemplateSource.FromResource (assembly, resourceName, fileInfo.Name.Replace (fileInfo.Extension, string.Empty));
				sources.Add (source);
			}
			return this.Compile(sources.ToArray());
		}
	}
}

