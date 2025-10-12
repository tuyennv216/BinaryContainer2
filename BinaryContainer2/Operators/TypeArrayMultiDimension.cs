using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;
using System.Collections;

namespace BinaryContainer2.Operators
{
	public class TypeArrayMultiDimension : TypeOperatorBase
	{
		public TypeArrayMultiDimension(Type type)
		{
			Raw = type;
		}

		public override void Build()
		{
			if (IsBuilded) return;
			IsBuilded = true;

			Follows = new();
			var underType = Raw!.GetElementType();
			var underOperator = TypeOperators.Instance.GetOperator(underType);
			Follows!.Add(underOperator);

			BuildCompleteSignal.Set();
		}

		public override void Write(DataContainer container, object? data, RefPool refPool)
		{
			BuildCompleteSignal.Wait();
			container.Flags.Add(data == null);
			if (data == null) return;

			var isany = ((IEnumerable)data).GetEnumerator().MoveNext();
			container.Flags.Add(isany);

			if (isany)
			{
				if (refPool.Write(container, data)) return;

				refPool.AddObject(data);

				var arr = (System.Array)data;
				container.AddNumber(arr.Rank);

				var dimensions = new int[arr.Rank];
				for (var i = 0; i < arr.Rank; i++)
				{
					var dim = arr.GetLength(i);
					container.AddNumber(dim);
					dimensions[i] = dim;
				}

				var indices = new int[arr.Rank];
				while (IncrementIndices(indices, dimensions))
				{
					Follows![0].Write(container, arr.GetValue(indices), refPool);
				}
			}
		}

		public override object? Read(DataContainer container, RefPool refPool)
		{
			BuildCompleteSignal.Wait();
			var isnull = container.Flags.Read();
			if (isnull == true) return null;

			var isany = container.Flags.Read();
			if (isany == false) return Array.CreateInstance(Follows![0].Raw, 0);

			var refObject = refPool.Read(container);
			if (refObject != null) return refObject;

			var rank = container.ReadNumber();
			var dimensions = new int[rank];
			for (var i = 0; i < rank; i++)
			{
				dimensions[i] = container.ReadNumber();
			}
			var indices = new int[rank];

			var arr = Array.CreateInstance(
				Follows![0].Raw,
				dimensions
			);

			while (IncrementIndices(indices, dimensions))
			{
				var value = Follows![0].Read(container, refPool);
				arr.SetValue(value, indices);
			}

			return arr;
		}

		private bool IncrementIndices(int[] indices, int[] dimensions)
		{
			for (int i = indices.Length - 1; i >= 0; i--)
			{
				// 1. Tăng chỉ mục hiện tại lên 1
				indices[i]++;

				// 2. Kiểm tra xem chỉ mục hiện tại có vượt quá kích thước chiều đó không
				if (indices[i] < dimensions[i])
				{
					// Nếu chưa vượt quá, việc tăng đã hoàn tất.
					// Ví dụ: đang ở index [0, 0, 1] và tăng thành [0, 0, 2] (kích thước là 3)
					return true;
				}

				// 3. Nếu chỉ mục vượt quá (bằng kích thước chiều đó), 
				//    đặt lại chỉ mục này về 0 và tiếp tục vòng lặp để 'mang' (carry) sang chỉ mục trước
				// Ví dụ: đang ở index [0, 0, 2] và tăng thành [0, 1, 0] (kích thước là 3)
				indices[i] = 0;
			}

			return false;
		}
	}
}