using System;
using System.IO;
using System.Reflection;

namespace BlackLinks.Templates
{
	public class TemplateSource
	{
		public TemplateSource ()
		{
		
		}
		public static TemplateSource FromResource (string fullyQualifiedResourcePath, string discoveryPath)
		{
			return FromResource(Assembly.GetCallingAssembly(),fullyQualifiedResourcePath,discoveryPath);
		}
		public static TemplateSource FromResource (Assembly targetAssembly, string fullyQualifiedResourcePath, string discoveryPath)
		{
			Stream resourceStream = targetAssembly.GetManifestResourceStream (fullyQualifiedResourcePath);
			if (resourceStream == null)
			{
				throw new TemplateSourceException (string.Format ("Invalid resource '{0}' in assembly '{1}'", fullyQualifiedResourcePath, targetAssembly.FullName));
			}
			FileInfo fileInfo = new FileInfo(fullyQualifiedResourcePath);
			TemplateSource source = new TemplateSource ();
			source.DiscoveryPath = discoveryPath;
			source.ReferenceFilePath = fullyQualifiedResourcePath;
			source.SourceStream = resourceStream;
			source.TemplateFileExtension = fileInfo.Extension;
			return source;
		}
		public static TemplateSource FromFile (string filePath, string discoveryPath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if(!fileInfo.Exists)
			{
				throw new TemplateSourceException(string.Format("Template file '{0}' does not exists",filePath));
			}
			TemplateSource source = new TemplateSource ();
			source.DiscoveryPath = discoveryPath;
			source.ReferenceFilePath = filePath;
			source.SourceStream = File.OpenRead (filePath);
			source.TemplateFileExtension = fileInfo.Extension;
			return source;
		}
		public Stream SourceStream { get; set; }
		public string DiscoveryPath { get; set; }
		public string ReferenceFilePath { get; set; }
		public string TemplateFileExtension {get;set;}
		
		public void Close ()
		{
			if (this.SourceStream == null)
				return;
			
			this.SourceStream.Close ();
			this.SourceStream = null;
		}
	}
}

