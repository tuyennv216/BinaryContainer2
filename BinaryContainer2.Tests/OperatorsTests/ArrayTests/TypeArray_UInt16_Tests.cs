using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_UInt16_Tests // Test cho mảng ushort[] (UInt16)
	{
		// Sử dụng tên _typeArrayOperator theo quy ước chung khi chỉ có một instance
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử ushort[]
			_typeArrayOperator = new TypeArray(typeof(ushort[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng ---
		private void AssertArrayEqual(ushort[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(ushort[]), "Kiểu đọc ra phải là ushort[].");
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual.GetValue(i), $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng ushort[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalUInt16Array_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: ushort[]
			ushort[] originalArray = new ushort[] { 10, 255, 3000, 60000 };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng ushort[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (ushort[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			ushort[] originalArray = new ushort[0];
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
		/// Mảng chứa các giá trị biên của kiểu phần tử (Min: 0, Max: UInt16.MaxValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_UInt16EdgeValuesArray_ShouldBePreserved()
		{
			ushort[] originalArray = new ushort[] { ushort.MinValue, ushort.MaxValue, 1, 50000 };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của UInt16.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomUInt16Array_ShouldBeStable()
		{
			const int arrayLength = 1000;
			ushort[] originalArray = new ushort[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Tạo ushort ngẫu nhiên trong dải [0, 65535]
				// Cần dùng Next(int, int) và ép kiểu (ushort)
				originalArray[i] = (ushort)_random.Next(ushort.MinValue, ushort.MaxValue + 1);
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu UInt16.");
		}
	}
}
