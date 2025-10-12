using System;

namespace BinaryContainer2.Types
{
	public class TypeBinding
	{
		public Func<object, object>? GetValue { get; set; }
		public Action<object, object>? SetValue { get; set; }
	}
}
