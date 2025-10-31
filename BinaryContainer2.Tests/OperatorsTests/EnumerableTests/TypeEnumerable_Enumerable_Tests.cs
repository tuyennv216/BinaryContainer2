using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;


// Định nghĩa alias cho kiểu dữ liệu đang được test
using EnumerableInt = System.Collections.Generic.IEnumerable<int>;

namespace BinaryContainer2.Tests.OperatorsTests.EnumerableTests
{
	[TestClass]
	public class TypeEnumerable_Enumerable_Tests
	{
		// Sẽ khởi tạo là TypeEnumerable(typeof(IEnumerable<int>)) theo yêu cầu
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeEnumerable với kiểu interface IEnumerable<int>
			_typeArrayOperator = new TypeEnumerable(typeof(EnumerableInt));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh EnumerableInt ---
		private void AssertEnumerableEqual(EnumerableInt expected, object actual, string message)
		{
			Assert.IsNotNull(actual);

			// Kiểm tra xem đối tượng đọc ra có triển khai interface EnumerableInt không
			Assert.IsInstanceOfType(actual, typeof(EnumerableInt), $"Kiểu đọc ra phải triển khai IEnumerable<int>: {message}");

			// Chuyển cả hai thành List để so sánh giá trị và độ dài dễ dàng
			var expectedList = expected.ToList();
			var actualEnumerable = (EnumerableInt)actual;
			var actualList = actualEnumerable.ToList();

			Assert.AreEqual(expectedList.Count, actualList.Count, $"Độ dài Collection không khớp: {message}");

			for (int i = 0; i < expectedList.Count; i++)
			{
				Assert.AreEqual(expectedList[i], actualList[i], $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case cho một IEnumerable<int> điển hình (dữ liệu là List<int>).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalEnumerable_ShouldPreserveAllValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			// Khởi tạo dữ liệu thực tế là List<int>, sau đó ép kiểu thành IEnumerable<int>
			EnumerableInt originalData = new List<int> { 10, -20, 300, 4000 };
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertEnumerableEqual(originalData, readData!, "IEnumerable<int> điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Đặc biệt (Edge Cases)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp biên: Collection là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullEnumerable_ShouldReturnNull()
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
			Assert.IsNull(readData, "Collection là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: Collection Rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyEnumerable_ShouldReturnEmptyCollection()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			EnumerableInt originalData = new List<int>();
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(EnumerableInt));
			Assert.AreEqual(0, ((EnumerableInt)readData).Count(), "Collection rỗng.");
		}

		/// <summary>
		/// Test Collection chứa các giá trị biên của kiểu phần tử (Int32.Min, 0, Int32.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_IntEdgeValuesEnumerable_ShouldBePreserved()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			EnumerableInt originalData = new List<int> { int.MinValue, 0, int.MaxValue };
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			AssertEnumerableEqual(originalData, readData!, "Collection chứa giá trị biên của Int32.");
		}

		/// <summary>
		/// Test giới hạn (Stress Test): Write/Read một Collection lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomEnumerable_ShouldBeStable()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			const int listLength = 5000;
			List<int> tempList = new List<int>(listLength);

			for (int i = 0; i < listLength; i++)
			{
				tempList.Add(_random.Next(int.MinValue, int.MaxValue));
			}
			EnumerableInt originalData = tempList;

			var originalContainer = new DataContainer();
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertEnumerableEqual(originalData, readData!, $"Collection ngẫu nhiên lớn ({listLength} phần tử).");
		}
	}
}
