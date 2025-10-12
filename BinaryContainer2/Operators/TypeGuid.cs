using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeGuid : TypeOperatorBase
	{
		public TypeGuid()
		{
			Raw = typeof(Guid);
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

			var value = (Guid)data;
			var defaultValue = Guid.Empty; // Giá trị mặc định cho Guid

			container.Flags.Add(value == defaultValue);

			if (value != null && value != defaultValue)
			{
				// Guid.ToByteArray() trả về một mảng byte 16 phần tử
				container.Items.AddRange(value.ToByteArray());
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return Guid.Empty;

			// Guid là 16 byte
			var bytes = container.ReadItems(16);

			// Khởi tạo Guid từ mảng byte 16 phần tử
			var value = new Guid(bytes!);
			return value;
		}
	}
}
