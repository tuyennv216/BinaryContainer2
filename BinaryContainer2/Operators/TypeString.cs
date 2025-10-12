using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using System;
using System.Text;

namespace BinaryContainer2.Operators
{
	public class TypeString : TypeOperatorBase
	{
		public TypeString()
		{
			Raw = typeof(String);
		}

		// Sử dụng UTF8 Encoding là tiêu chuẩn cho serialization
		private static readonly Encoding Encoding = Encoding.UTF8;

		public override void Build()
		{
			IsBuilded = true;
			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			// 1. Ghi cờ IsNull
			container.Flags.Add(data == null);
			if (data == null) return;

			var value = (string)data;

			// 2. Nếu không null, kiểm tra IsDefault (chuỗi rỗng)
			container.Flags.Add(value == string.Empty);

			if (value != string.Empty)
			{
				if (refPool.Write(container, data)) return;

				// 3. Chuyển chuỗi thành byte
				var bytes = Encoding.GetBytes(value);

				// 4. Ghi độ dài (Length) của mảng byte
				container.AddNumber(bytes.Length);

				// 5. Ghi bản thân mảng byte của chuỗi
				container.Arrays.AddRange(bytes);
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			// 1. Đọc cờ IsNull
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			// 2. Đọc cờ IsDefault (chuỗi rỗng)
			var isempty = container.Flags.Read();
			if (isempty == true) return string.Empty;

			var refObject = refPool.Read(container);
			if (refObject != null) return refObject;

			// 3. Lấy độ dài (Length)
			var length = container.ReadNumber();

			// 4. Đọc [length] byte tiếp theo là dữ liệu chuỗi
			var stringBytes = container.ReadArrays(length);

			// 5. Chuyển mảng byte ngược lại thành chuỗi
			var value = Encoding.GetString(stringBytes!);
			return value;
		}
	}
}
