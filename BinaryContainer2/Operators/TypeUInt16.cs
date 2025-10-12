using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeUInt16 : TypeOperatorBase
	{
		public TypeUInt16()
		{
			Raw = typeof(UInt16);
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

			var value = (UInt16)data;
			container.Flags.Add(value == 0);
			if (value != 0)
			{
				container.Items.AddRange(BitConverter.GetBytes((UInt16)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return (UInt16)0; // Trả về 0 với kiểu UInt16

			var bytes = container.ReadItems(2); // UInt16 là 2 byte

			var value = BitConverter.ToUInt16(bytes!, 0);
			return value;
		}
	}
}
