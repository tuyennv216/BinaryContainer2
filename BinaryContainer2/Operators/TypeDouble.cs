using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeDouble : TypeOperatorBase
	{
		public TypeDouble()
		{
			Raw = typeof(Double);
		}

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			container.Flags.Add(data == null);
			if (data == null) return;

			var value = (Double)data;
			container.Flags.Add(value == 0.0d); // Giá trị mặc định là 0.0d
			if (value != 0.0d)
			{
				container.Items.AddRange(BitConverter.GetBytes((Double)data));
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();

			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return 0.0d;

			var bytes = container.ReadItems(8); // Double là 8 byte

			var value = BitConverter.ToDouble(bytes!, 0);
			return value;
		}
	}
}
