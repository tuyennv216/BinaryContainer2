using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;


// Định nghĩa kiểu dữ liệu đang được test: IEnumerable<List<int[]>>
using DeepNestedType = System.Collections.Generic.IEnumerable<System.Collections.Generic.List<int[]>>;
using InnerListType = System.Collections.Generic.List<int[]>;

namespace BinaryContainer2.Tests.OperatorsTests.EnumerableTests
{
	[TestClass]
	public class TypeEnumerable_DeepNested_Tests
	{
		// Khởi tạo TypeEnumerable với kiểu IEnumerable<List<int[]>>
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// TypeEnumerable sẽ được khởi tạo với kiểu IEnumerable<List<int[]>>
			_typeArrayOperator = new TypeEnumerable(typeof(DeepNestedType));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh IEnumerable<List<int[]>> ---
		private void AssertDeepNestedEqual(DeepNestedType expected, object actual, string message)
		{
			Assert.IsNotNull(actual);

			// 1. Kiểm tra kiểu của đối tượng đọc ra (phải là kiểu có thể chuyển đổi thành DeepNestedType)
			Assert.IsInstanceOfType(actual, typeof(DeepNestedType), $"[L1] Kiểu đọc ra phải triển khai IEnumerable<List<int[]>>: {message}");

			var expectedList = expected.ToList();
			var actualList = ((DeepNestedType)actual).ToList();

			// 2. Kiểm tra độ dài IEnumerable ngoài cùng
			Assert.AreEqual(expectedList.Count, actualList.Count, $"[L1] Độ dài Collection ngoài cùng không khớp: {message}");

			// 3. Lặp qua các phần tử (là các List<int[]>)
			for (int i = 0; i < expectedList.Count; i++)
			{
				InnerListType? expectedInnerList = expectedList[i];
				object? actualInnerObject = actualList[i];

				if (expectedInnerList == null)
				{
					Assert.IsNull(actualInnerObject, $"[L2] Phần tử thứ {i} phải là NULL: {message}");
					continue;
				}

				Assert.IsNotNull(actualInnerObject, $"[L2] Phần tử thứ {i} không được là NULL: {message}");

				// Kiểm tra kiểu List giữa
				Assert.IsInstanceOfType(actualInnerObject, typeof(InnerListType), $"[L2] Kiểu phần tử {i} phải là List<int[]>: {message}");
				InnerListType actualInnerList = (InnerListType)actualInnerObject;

				// 4. Kiểm tra độ dài List giữa
				Assert.AreEqual(expectedInnerList.Count, actualInnerList.Count, $"[L2] Độ dài List giữa ({i}) không khớp: {message}");

				// 5. Lặp qua các phần tử (là các int[])
				for (int j = 0; j < expectedInnerList.Count; j++)
				{
					int[]? expectedArray = expectedInnerList[j];
					int[]? actualArray = actualInnerList[j];

					if (expectedArray == null)
					{
						Assert.IsNull(actualArray, $"[L3] Mảng con [{i}][{j}] phải là NULL: {message}");
						continue;
					}

					Assert.IsNotNull(actualArray, $"[L3] Mảng con [{i}][{j}] không được là NULL: {message}");

					// 6. Kiểm tra độ dài của mảng con
					Assert.AreEqual(expectedArray.Length, actualArray.Length, $"[L3] Độ dài mảng con [{i}][{j}] không khớp: {message}");

					// 7. Lặp qua các phần tử int bên trong mảng con
					for (int k = 0; k < expectedArray.Length; k++)
					{
						Assert.AreEqual(expectedArray[k], actualArray[k], $"[L4] Phần tử [{i}][{j}][{k}] không khớp: {message}");
					}
				}
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case cho một IEnumerable<List<int[]>> điển hình với cấu trúc lồng nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalDeepNested_ShouldPreserveAllValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			// Sử dụng List làm triển khai cụ thể của IEnumerable<List<int[]>>
			DeepNestedType originalData = new List<InnerListType>
			{
				// Outer List 0: List có 2 mảng
				new InnerListType
				{
					new int[] { 1, 2, 3 },
					new int[] { 10, 20 }
				},
				// Outer List 1: List có 1 mảng rỗng và 1 mảng NULL
				new InnerListType
				{
					new int[0],
					null!
				}
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData, readData!, "IEnumerable<List<int[]>> điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Đặc biệt (Edge Cases)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp biên: IEnumerable ngoài cùng là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullOuterEnumerable_ShouldReturnNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNull(readData, "IEnumerable ngoài cùng là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: IEnumerable ngoài cùng Rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyOuterEnumerable_ShouldReturnEmptyEnumerable()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			DeepNestedType originalData = new List<InnerListType>();
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(DeepNestedType));
			Assert.AreEqual(0, ((DeepNestedType)readData).Count(), "IEnumerable ngoài cùng rỗng.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: IEnumerable chứa phần tử NULL (List giữa là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_EnumerableWithNullInnerList_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			DeepNestedType originalData = new List<InnerListType?>
			{
				new InnerListType { new int[] { 100 } }, // List hợp lệ
				null, // List giữa NULL
				new InnerListType { new int[] { 200 } }
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData!, readData!, "IEnumerable chứa List giữa là NULL.");
		}

		/// <summary>
		/// Test giới hạn (Stress Test): IEnumerable lớn với các mảng con ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomDeepNested_ShouldBeStable()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			const int outerLength = 100;
			DeepNestedType originalData = new List<InnerListType>(outerLength);

			for (int i = 0; i < outerLength; i++)
			{
				// Tạo List giữa
				InnerListType innerList = new InnerListType();
				int innerListLength = _random.Next(1, 5); // 1 đến 4 mảng con
				for (int j = 0; j < innerListLength; j++)
				{
					// Tạo mảng int trong cùng
					int arrayLength = _random.Next(1, 10);
					int[] innerArray = new int[arrayLength];
					for (int k = 0; k < arrayLength; k++)
					{
						innerArray[k] = _random.Next(-10000, 10000);
					}
					innerList.Add(innerArray);
				}
				originalData.ToList().Add(innerList); // Sử dụng ToList() để tránh lỗi Add trên interface
			}

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData, readData!, $"IEnumerable<List<int[]>> ngẫu nhiên lớn ({outerLength} phần tử).");
		}
	}
}
