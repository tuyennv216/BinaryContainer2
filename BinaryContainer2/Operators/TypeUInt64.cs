using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeUInt64 : TypeOperatorBase
	{
		public TypeUInt64()
		{
			Raw = typeof(UInt64);
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

			var value = (UInt64)data;
			container.Flags.Add(value == 0);
			if (value != 0)
			{
				container.Items.AddRange(BitConverter.GetBytes((UInt64)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return (UInt64)0; // Trả về 0 với kiểu UInt64

			var bytes = container.ReadItems(8); // UInt64 là 8 byte

			var value = BitConverter.ToUInt64(bytes!, 0);
			return value;
		}
	}
}
