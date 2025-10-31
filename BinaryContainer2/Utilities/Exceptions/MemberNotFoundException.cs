using System;

namespace BinaryContainer2.Utilities.Exceptions
{
	public class MemberNotFoundException : Exception
	{
		public MemberNotFoundException(string message) : base(message) { }
	}
}
