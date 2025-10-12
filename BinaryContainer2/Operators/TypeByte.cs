using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeByte : TypeOperatorBase
	{
		public TypeByte()
		{
			Raw = typeof(Byte);
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

			var value = (Byte)data;
			container.Flags.Add(value == 0);

			if (value != 0)
			{
				container.Items.Add((Byte)data);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return (Byte)0; // Trả về 0 với kiểu Byte

			var bytes = container.ReadItems(1); // Byte là 1 byte

			var value = (Byte)bytes![0];
			return value;
		}
	}
}
