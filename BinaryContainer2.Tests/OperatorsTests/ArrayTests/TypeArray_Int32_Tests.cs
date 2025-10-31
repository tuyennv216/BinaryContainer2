using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Int32_Tests // Tên lớp theo yêu cầu của bạn
	{
		// Sử dụng tên _typeArrayOperator theo quy ước chung khi chỉ có một instance
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Yêu cầu của bạn: Khởi tạo với kiểu phần tử int
			_typeArrayOperator = new TypeArray(typeof(int[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng ---
		private void AssertArrayEqual(int[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(int[]), "Kiểu đọc ra phải là int[].");
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual.GetValue(i), $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng int[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalIntArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: int[]
			int[] originalArray = new int[] { 10, -20, 300, 4000 };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng int[] điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullArray_ShouldReturnNull()
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

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (int[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			int[] originalArray = new int[0];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		/// <summary>
		/// Mảng có 1 phần tử là giá trị biên (int.MinValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleElementEdgeArray_ShouldBePreserved()
		{
			int[] originalArray = new int[] { int.MinValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng có 1 phần tử biên.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Int32.Min, 0, Int32.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_IntEdgeValuesArray_ShouldBePreserved()
		{
			int[] originalArray = new int[] { int.MinValue, 0, int.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Int32.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomIntArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			int[] originalArray = new int[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Sử dụng random.Next cho int
				originalArray[i] = _random.Next(int.MinValue, int.MaxValue);
			}
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử).");
		}
	}
}