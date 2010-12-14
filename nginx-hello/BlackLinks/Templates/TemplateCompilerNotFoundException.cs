using System;

namespace BlackLinks.Templates
{
	/// <summary>
	/// Occurs when a Template Compiler was not registered for a file extension.
	/// </summary>
	[Serializable]
	public class TemplateCompilerNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:TemplateCompilerNotFoundException"/> class
		/// </summary>
		public TemplateCompilerNotFoundException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:TemplateCompilerNotFoundException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public TemplateCompilerNotFoundException (string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:TemplateCompilerNotFoundException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public TemplateCompilerNotFoundException (string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:TemplateCompilerNotFoundException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected TemplateCompilerNotFoundException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}

