
using System;

namespace BlackLinks
{
	public abstract class Controller
	{
		public Controller ()
		{
			
		}
		
		internal void own(BlackAction action)
		{
			action.ControllerInstance = this;
		}
	}
}
