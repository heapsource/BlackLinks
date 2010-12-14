
using System;
using System.Collections.ObjectModel;

namespace BlackLinks.Routing
{
	public class RouteCollection : Collection<Route>
	{
		readonly Route parent;
		/// <summary>
		/// Creates a Route Collection.
		/// </summary>
		/// <param name="parent">
		/// Optional <see cref="Route"/>
		/// </param>
		public RouteCollection (Route parent)
		{
			this.parent = parent;
		}
		public new void Add (Route item)
		{
			if(this.parent != null)
				this.parent.own(item);
			base.Add(item);
		}
	}
}
