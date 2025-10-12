namespace BinaryContainer2.Others
{
	using System;

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class SkipContainerAttribute : Attribute
	{
	}
}
