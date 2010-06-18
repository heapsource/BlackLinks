
using System;

namespace BlackLinks.Routing
{
	static class StringExtensions
	{
		public const string Slash = "/"; 
		public static bool IsSlash(this string @this)
		{
			return string.IsNullOrEmpty(@this) || @this == Slash;
		}
		/*
		public static string NormalizeForRoute(this string @this)
		{
			return @this.IsSlash() ? Slash : @this;
		}*/
	}
}
