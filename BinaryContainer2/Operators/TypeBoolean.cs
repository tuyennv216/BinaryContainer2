using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeBoolean : TypeOperatorBase
	{
		public TypeBoolean()
		{
			Raw = typeof(Boolean);
		}

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			container.Flags.Add(data == null);
			if (data != null)
			{
				container.Flags.Add((Boolean)data);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			return container.Flags.Read();
		}
	}
}
