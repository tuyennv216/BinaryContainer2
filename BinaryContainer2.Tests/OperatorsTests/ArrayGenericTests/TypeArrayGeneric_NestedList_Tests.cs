using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;


// Định nghĩa alias cho kiểu dữ liệu lồng nhau: List<List<int>>
using NestedList = System.Collections.Generic.List<System.Collections.Generic.List<int>>;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayGenericTests
{
	[TestClass]
	public class TypeArrayGeneric_NestedList_Tests
	{
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeArrayGeneric với kiểu lồng nhau: List<List<int>>
			_typeArrayOperator = new TypeArrayGeneric(typeof(NestedList));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh NestedList ---
		private void AssertNestedListEqual(NestedList expected, object actual, string message)
		{
			Assert.IsNotNull(actual);
			Assert.IsInstanceOfType(actual, typeof(NestedList), $"Kiểu đọc ra phải là NestedList: {message}");

			NestedList actualList = (NestedList)actual;
			Assert.AreEqual(expected.Count, actualList.Count, $"Lớp List ngoài cùng (Level 1) không khớp độ dài: {message}");

			// Level 1: List<List<int>>
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

				// Level 2: List<int>
				for (int j = 0; j < expectedLevel2.Count; j++)
				{
					int expectedValue = expectedLevel2[j];
					int actualValue = actualLevel2[j];

					Assert.AreEqual(expectedValue, actualValue, $"Giá trị int tại Level 2, vị trí {i},{j} không khớp: {message}");
				}
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case cho cấu trúc List<List<int>> điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalNestedListStructure_ShouldPreserveAllValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			NestedList originalData = new NestedList
			{
				// List ngoài cùng (i=0)
				new List<int> { 10, 20, 30 },
				
				// List ngoài cùng (i=1)
				new List<int> { 40, 50 },
				
				// List ngoài cùng (i=2)
				new List<int> { 60 }
			};

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertNestedListEqual(originalData, readData!, "Cấu trúc List<List<int>> điển hình.");
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

			NestedList originalData = new NestedList();
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(NestedList));
			Assert.AreEqual(0, ((NestedList)readData).Count, "List ngoài cùng rỗng.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: List lồng rỗng (ví dụ: List<List<int>> chứa một List<int> rỗng).
		/// </summary>
		[TestMethod]
		public void WriteRead_InnerListEmpty_ShouldPreserveEmptyInnerList()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			NestedList originalData = new NestedList
			{
				new List<int> { 1, 2 },
				new List<int> (), // List rỗng
				new List<int> { 3 }
			};

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertNestedListEqual(originalData, readData!, "List lồng chứa List rỗng.");
			Assert.AreEqual(0, ((NestedList)readData!)[1].Count, "List lồng rỗng phải được bảo toàn.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: Cấu trúc lồng nhau chứa NULL (List lồng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NestedListWithNullInnerList_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			NestedList originalData = new NestedList
			{
				new List<int> { 10, 20 },
				null!, // List lồng NULL
				new List<int> { 30, 40 }
			};

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertNestedListEqual(originalData, readData!, "Cấu trúc lồng nhau chứa List lồng là NULL.");
			Assert.IsNull(((NestedList)readData!)[1], "Phần tử List lồng NULL phải được bảo toàn.");
		}
	}
}
