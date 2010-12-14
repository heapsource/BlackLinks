using System;
using System.IO;

namespace BlackLinks.Templates
{
	public abstract class TemplateRenderResource
	{
		public TemplateRenderResource ()
		{
		
		}
		bool IsExecuting {
			get; set;
		}
		TextWriter writer;
		public void Render (TextWriter writer)
		{
			if (IsExecuting)
				throw new InvalidOperationException ("Already executing the Template Resource");
			this.writer = writer;
			IsExecuting = true;
			Render ();
			IsExecuting = false;
		}
		
		protected void Write (object value)
		{
			if (value != null)
			{
				writer.Write (value);
			}
		}
		protected void WriteLine (object value)
		{
			if (value != null) {
				writer.WriteLine (value);
			}
		}
		protected abstract void Render();
	}
}

