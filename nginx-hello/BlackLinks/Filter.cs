using System;

namespace BlackLinks
{
	[AttributeUsage(System.AttributeTargets.Class, AllowMultiple=true)]
	/// <summary>
	/// Represents a filter applied to an action.
	/// </summary>
	public abstract class Filter : Attribute
	{
		public Filter ()
		{
			
		}
		public BlackAction Action{get; set;}
		protected abstract void OnExecute();
		internal void execute(BlackAction action)
		{
			this.Action = action;
			this.OnExecute();
		}
	}
}
