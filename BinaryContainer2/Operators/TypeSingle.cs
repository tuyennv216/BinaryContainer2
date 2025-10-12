using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeSingle : TypeOperatorBase
	{
		public TypeSingle()
		{
			Raw = typeof(Single);
		}

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			container.Flags.Add(data == null);
			if (data == null) return;

			var value = (Single)data;
			container.Flags.Add(value == 0.0f); // Giá trị mặc định là 0.0f
			if (value != 0.0f)
			{
				container.Items.AddRange(BitConverter.GetBytes((Single)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return 0.0f;

			var bytes = container.ReadItems(4); // Single là 4 byte

			var value = BitConverter.ToSingle(bytes!, 0);
			return value;
		}
	}
}
