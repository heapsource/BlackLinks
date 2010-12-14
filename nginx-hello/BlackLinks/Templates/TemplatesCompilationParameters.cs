using System;
using System.Collections.ObjectModel;

namespace BlackLinks.Templates
{
	public class TemplatesCompilationParameters
	{
		public TemplatesCompilationParameters ()
		{
			this.Templates = new Collection<TemplateSource> ();
		}
		/// <summary>
		/// Path to compiled assembly(Optional). Null by default (in memory assembly).
		/// </summary>
		public string OutputAssemblyPath { get; set; }
		public Collection<TemplateSource> Templates{get;private set;}
	}
}

