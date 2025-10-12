using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_Int16_Tests // Test cho mảng short[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử short[]
			_typeArrayOperator = new TypeArray(typeof(short[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42 để đảm bảo tính lặp lại của các giá trị ngẫu nhiên
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng short[] ---
		private void AssertArrayEqual(short[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(short[]), "Kiểu đọc ra phải là short[].");
			for (int i = 0; i < expected.Length; i++)
			{
				// actual.GetValue(i) trả về object, cần ép kiểu an toàn hoặc dùng Assert.AreEqual trực tiếp
				Assert.AreEqual(expected[i], (short)actual.GetValue(i)!, $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng short[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalShortArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: short[]
			short[] originalArray = new short[] { 10, -20, 300, 4000 };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng short[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (short[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			short[] originalArray = new short[0];
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
		/// Mảng có 1 phần tử là giá trị biên (short.MinValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleElementEdgeArray_ShouldBePreserved()
		{
			// short.MinValue = -32768
			short[] originalArray = new short[] { short.MinValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng có 1 phần tử biên của Int16.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Int16.Min, 0, Int16.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_ShortEdgeValuesArray_ShouldBePreserved()
		{
			// short.MinValue = -32768, short.MaxValue = 32767
			short[] originalArray = new short[] { short.MinValue, 0, short.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Int16.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomShortArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			short[] originalArray = new short[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Dùng Random.Next(int minVal, int maxVal) và ép kiểu (cast) về short.
				// maxVal là độc quyền (exclusive), nên cần +1 để bao gồm short.MaxValue.
				originalArray[i] = (short)_random.Next(short.MinValue, short.MaxValue + 1);
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử).");
		}
	}
}
