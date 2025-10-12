using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeInt32 : TypeOperatorBase
	{
		public TypeInt32()
		{
			Raw = typeof(Int32);
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

			var value = (Int32)data;
			container.Flags.Add(value == 0);
			if (value != 0)
			{
				container.Items.AddRange(BitConverter.GetBytes((Int32)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return 0;

			var bytes = container.ReadItems(4); // int là 4 byte

			var value = BitConverter.ToInt32(bytes!, 0);
			return value;
		}
	}
}
