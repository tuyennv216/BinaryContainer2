using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	// Đã đổi tên để phản ánh class và kiểu dữ liệu đang được test
	public class TypeArrayGeneric_ListInt_Tests
	{
		private ITypeOperator _typeArrayOperator; // Sử dụng ITypeOperator cho tính linh hoạt
		private RefPool _refPool;
		private Random _random;

		// Khai báo kiểu không phải generic (hoặc generic sai) để kiểm tra ngoại lệ constructor
		private Type _nonGenericType = typeof(int);
		private Type _nonArrayGenericType = typeof(string); // string cũng là IEnumerable, nhưng không có GenericTypeArguments

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeArrayGeneric với kiểu List<int>
			_typeArrayOperator = new TypeArrayGeneric(typeof(List<int>));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh List ---
		private void AssertListEqual(List<int> expected, object actual, string message)
		{
			Assert.IsNotNull(actual);
			// Kiểm tra kiểu đọc ra phải là List<int>
			Assert.IsInstanceOfType(actual, typeof(List<int>), "Kiểu đọc ra phải là List<int>.");

			List<int> actualList = (List<int>)actual;
			Assert.AreEqual(expected.Count, actualList.Count, $"Độ dài List không khớp: {message}");

			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i], actualList[i], $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản (Kiểm tra Chức năng)
		// ---------------------------------------------

		/// <summary>
		/// Test case cho một List<int> điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalListInt_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: List<int>
			List<int> originalList = new List<int> { 10, -20, 300, 4000 };
			object originalData = originalList;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			object? readData = _typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertListEqual(originalList, readData!, "List<int> điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Giới hạn (Edge & Boundary Cases)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp biên: List là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullList_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: List Rỗng (Count = 0).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyList_ShouldReturnEmptyList()
		{
			List<int> originalList = new List<int>();
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(List<int>));
			Assert.AreEqual(0, ((List<int>)readData).Count, "List rỗng phải được đọc ra là List có độ dài 0.");
		}

		/// <summary>
		/// Trường hợp biên: List có 1 phần tử là giá trị biên (int.MinValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleElementEdgeList_ShouldBePreserved()
		{
			List<int> originalList = new List<int> { int.MinValue };
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			AssertListEqual(originalList, readData!, "List có 1 phần tử biên.");
		}

		/// <summary>
		/// Trường hợp giới hạn: List chứa các giá trị biên của kiểu phần tử (Int32.Min, 0, Int32.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_IntEdgeValuesList_ShouldBePreserved()
		{
			List<int> originalList = new List<int> { int.MinValue, 0, int.MaxValue, -1, 1 };
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			AssertListEqual(originalList, readData!, "List chứa giá trị biên của Int32.");
		}

		/// <summary>
		/// Test giới hạn (Stress Test): Write/Read một List lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomList_ShouldBeStable()
		{
			const int listLength = 5000;
			List<int> originalList = new List<int>(listLength);

			// 1. Tạo List ngẫu nhiên
			for (int i = 0; i < listLength; i++)
			{
				originalList.Add(_random.Next(int.MinValue, int.MaxValue));
			}
			object originalData = originalList;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			object? readData = _typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertListEqual(originalList, readData!, $"List ngẫu nhiên lớn ({listLength} phần tử).");
		}


		// ---------------------------------------------
		// III. Ngoại lệ & Lỗi (Exception Cases)
		// ---------------------------------------------

		/// <summary>
		/// Kiểm tra ngoại lệ khi khởi tạo TypeArrayGeneric với kiểu KHÔNG PHẢI là generic có 1 tham số (ví dụ: int).
		/// Nó sẽ ném ra ngoại lệ khi truy cập GenericTypeArguments[0].
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void Constructor_NonGenericType_ShouldThrowException()
		{
			// Thử khởi tạo với kiểu non-generic (int)
			var operatorInstance = new TypeArrayGeneric(_nonGenericType);
			operatorInstance.Build();
		}

		/// <summary>
		/// Kiểm tra ngoại lệ khi khởi tạo TypeArrayGeneric với kiểu non-generic nhưng là IEnumerable (ví dụ: string).
		/// Nó vẫn sẽ ném ra ngoại lệ khi truy cập GenericTypeArguments[0].
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void Constructor_InvalidGenericArguments_ShouldThrowException()
		{
			// Thử khởi tạo với string (là IEnumerable nhưng không phải generic type<T>)
			var operatorInstance = new TypeArrayGeneric(_nonArrayGenericType);
			operatorInstance.Build();
		}

		/// <summary>
		/// Kiểm tra ngoại lệ khi cố gắng ghi một kiểu không phải IEnumerable (ví dụ: int) vào Write method.
		/// Đây là trường hợp kiểm tra runtime cast ((IEnumerable)data).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))]
		public void Write_NonEnumerableType_ShouldThrowInvalidCastException()
		{
			// Tạo một đối tượng không phải IEnumerable (ví dụ: int)
			int invalidData = 42;
			var originalContainer = new DataContainer();

			// Cố gắng ghi dữ liệu
			_typeArrayOperator.Write(originalContainer, invalidData, _refPool);
		}
	}
}
