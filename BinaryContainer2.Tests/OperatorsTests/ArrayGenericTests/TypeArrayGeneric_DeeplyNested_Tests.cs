using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

// Định nghĩa alias cho kiểu dữ liệu phức tạp để dễ đọc
using ComplexNestedList = System.Collections.Generic.List<System.Collections.Generic.List<int[][]>>;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArrayGeneric_DeeplyNested_Tests
	{
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeArrayGeneric với kiểu lồng nhau phức tạp: List<List<int[][]>>
			_typeArrayOperator = new TypeArrayGeneric(typeof(ComplexNestedList));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh ComplexNestedList ---
		private void AssertComplexListEqual(ComplexNestedList expected, object actual, string message)
		{
			Assert.IsNotNull(actual);
			Assert.IsInstanceOfType(actual, typeof(ComplexNestedList), $"Kiểu đọc ra phải là ComplexNestedList: {message}");

			ComplexNestedList actualList = (ComplexNestedList)actual;
			Assert.AreEqual(expected.Count, actualList.Count, $"Lớp List ngoài cùng (Level 1) không khớp độ dài: {message}");

			// Level 1: List<List<int[][]>>
			for (int i = 0; i < expected.Count; i++)
			{
				var expectedLevel2 = expected[i];
				var actualLevel2 = actualList[i];

				if (expectedLevel2 == null)
				{
					Assert.IsNull(actualLevel2, $"Phần tử List lồng (Level 2) thứ {i} phải là NULL: {message}");
					continue;
				}

				Assert.IsNotNull(actualLevel2, $"Phần tử List lồng (Level 2) thứ {i} không được là NULL: {message}");
				Assert.AreEqual(expectedLevel2.Count, actualLevel2.Count, $"Lớp List lồng (Level 2) thứ {i} không khớp độ dài: {message}");

				// Level 2: List<int[][]>
				for (int j = 0; j < expectedLevel2.Count; j++)
				{
					var expectedLevel3 = expectedLevel2[j];
					var actualLevel3 = actualLevel2[j];

					if (expectedLevel3 == null)
					{
						Assert.IsNull(actualLevel3, $"Phần tử Jagged Array (Level 3) thứ {i},{j} phải là NULL: {message}");
						continue;
					}

					Assert.IsNotNull(actualLevel3, $"Phần tử Jagged Array (Level 3) thứ {i},{j} không được là NULL: {message}");
					Assert.AreEqual(expectedLevel3.Length, actualLevel3.Length, $"Độ dài mảng ngoài (Level 3) thứ {i},{j} không khớp: {message}");

					// Level 3: int[][] (Outer Jagged Array)
					for (int k = 0; k < expectedLevel3.Length; k++)
					{
						var expectedLevel4 = expectedLevel3[k];
						var actualLevel4 = actualLevel3[k];

						if (expectedLevel4 == null)
						{
							Assert.IsNull(actualLevel4, $"Phần tử mảng int[] (Level 4) thứ {i},{j},{k} phải là NULL: {message}");
							continue;
						}

						Assert.IsNotNull(actualLevel4, $"Phần tử mảng int[] (Level 4) thứ {i},{j},{k} không được là NULL: {message}");
						Assert.AreEqual(expectedLevel4.Length, actualLevel4.Length, $"Độ dài mảng int[] (Level 4) thứ {i},{j},{k} không khớp: {message}");

						// Level 4: int[] (Inner Array)
						for (int l = 0; l < expectedLevel4.Length; l++)
						{
							int expectedValue = expectedLevel4[l];
							int actualValue = actualLevel4[l];
							Assert.AreEqual(expectedValue, actualValue, $"Giá trị int tại {i},{j},{k},{l} không khớp: {message}");
						}
					}
				}
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case cho cấu trúc lồng nhau điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalNestedStructure_ShouldPreserveAllValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			ComplexNestedList originalData = new ComplexNestedList
			{
				// List ngoài cùng (i=0)
				new List<int[][]>
				{
					// List lồng (j=0)
					new int[][]
					{
						new int[] { 1, 2, 3 }, // k=0
						new int[] { 4 }        // k=1
					},
					// List lồng (j=1)
					new int[][]
					{
						new int[] { 5, 6 }     // k=0
					}
				},
				// List ngoài cùng (i=1)
				new List<int[][]>
				{
					// List lồng (j=0)
					new int[][]
					{
						new int[] { 7, 8 }
					}
				}
			};

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertComplexListEqual(originalData, readData!, "Cấu trúc lồng nhau điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Đặc biệt (NULLs & Empties)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp biên: List ngoài cùng là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullOuterList_ShouldReturnNull()
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
			Assert.IsNull(readData, "List ngoài cùng là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: List ngoài cùng rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyOuterList_ShouldReturnEmptyList()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			ComplexNestedList originalData = new ComplexNestedList();
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(ComplexNestedList));
			Assert.AreEqual(0, ((ComplexNestedList)readData).Count, "List ngoài cùng rỗng.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: Cấu trúc lồng nhau chứa NULL ở các cấp độ khác nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_NestedListWithNullsAtDifferentLevels_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			ComplexNestedList originalData = new ComplexNestedList
			{
				// i=0: List lồng có một phần tử NULL (Level 2)
				new List<int[][]> { null! }, 
				
				// i=1: List lồng bình thường
				new List<int[][]>
				{
					// j=0: Jagged array có phần tử NULL (Level 3)
					new int[][] { new int[] { 10 }, null!, new int[] { 20 } }
				},
				
				// i=2: NULL List lồng (Level 1)
				null!
			};

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertComplexListEqual(originalData, readData!, "Cấu trúc lồng nhau chứa NULL ở các cấp độ.");
		}
	}
}
