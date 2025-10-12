using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;

namespace BinaryContainer2.Operators
{
	public class TypeDecimal : TypeOperatorBase
	{
		public TypeDecimal()
		{
			Raw = typeof(Decimal);
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

			var value = (Decimal)data;
			container.Flags.Add(value == 0m); // Giá trị mặc định là 0m

			if (value != 0m)
			{
				var bits = Decimal.GetBits((Decimal)data); // Lấy 4 Int32

				// Ghi 4 số nguyên 32-bit lần lượt
				foreach (var i in bits)
				{
					container.Items.AddRange(BitConverter.GetBytes(i));
				}
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isdefault = container.Flags.Read();
			if (isdefault == true) return 0m;

			var bits = new Int32[4];
			for (int i = 0; i < 4; i++)
			{
				// Đọc 4 số nguyên 32-bit (tổng cộng 16 byte)
				var bytes = container.ReadItems(4);
				bits[i] = BitConverter.ToInt32(bytes!, 0);
			}

			// Tạo lại Decimal từ 4 số nguyên
			var value = new Decimal(bits);
			return value;
		}
	}
}
