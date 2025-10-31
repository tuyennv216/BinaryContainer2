using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Byte_Tests // Test cho mảng byte[] (Unsigned Byte)
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử byte[]
			_typeArrayOperator = new TypeArray(typeof(byte[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng byte ---
		private void AssertArrayEqual(byte[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(byte[]), "Kiểu đọc ra phải là byte[].");

			// So sánh từng phần tử
			byte[] actualBytes = (byte[])actual;
			CollectionAssert.AreEqual(expected, actualBytes, $"Nội dung mảng byte không khớp: {message}");
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng byte[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalByteArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: byte[]
			byte[] originalArray = new byte[] { 10, 255, 0, 127, 254 };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng byte[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (byte[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			byte[] originalArray = new byte[0];
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
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Min=0, Max=255).
		/// </summary>
		[TestMethod]
		public void WriteRead_ByteEdgeValuesArray_ShouldBePreserved()
		{
			byte[] originalArray = new byte[] { byte.MinValue, 1, 254, byte.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Byte.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với dữ liệu byte ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomByteArray_ShouldBeStable()
		{
			const int arrayLength = 4096; // Một mảng byte lớn
			byte[] originalArray = new byte[arrayLength];

			// 1. Tạo mảng ngẫu nhiên (dữ liệu nhị phân thô)
			_random.NextBytes(originalArray);
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu Byte.");
		}
	}
}
