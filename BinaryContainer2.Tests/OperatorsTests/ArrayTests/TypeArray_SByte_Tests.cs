using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_SByte_Tests // Test cho mảng sbyte[] (Signed Byte)
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử sbyte[]
			_typeArrayOperator = new TypeArray(typeof(sbyte[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42 để đảm bảo tính lặp lại của các giá trị ngẫu nhiên
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng sbyte[] ---
		private void AssertArrayEqual(sbyte[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(sbyte[]), "Kiểu đọc ra phải là sbyte[].");
			for (int i = 0; i < expected.Length; i++)
			{
				// actual.GetValue(i) trả về object, cần ép kiểu về sbyte
				Assert.AreEqual(expected[i], (sbyte)actual.GetValue(i)!, $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng sbyte[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalSByteArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: sbyte[] (dải -128 đến 127)
			sbyte[] originalArray = new sbyte[] { 10, -50, 127, -1 };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng sbyte[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (sbyte[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			sbyte[] originalArray = new sbyte[0];
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
		/// Mảng có 1 phần tử là giá trị biên (sbyte.MinValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleElementEdgeArray_ShouldBePreserved()
		{
			// sbyte.MinValue = -128
			sbyte[] originalArray = new sbyte[] { sbyte.MinValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng có 1 phần tử biên của SByte.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị biên của kiểu phần tử (SByte.Min, 0, SByte.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_SByteEdgeValuesArray_ShouldBePreserved()
		{
			// sbyte.MinValue = -128, sbyte.MaxValue = 127
			sbyte[] originalArray = new sbyte[] { sbyte.MinValue, 0, sbyte.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của SByte.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên trong dải [-128, 127].
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomSByteArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			sbyte[] originalArray = new sbyte[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Dùng Random.Next(int minVal, int maxVal) và ép kiểu (cast) về sbyte.
				// maxVal là độc quyền (exclusive), nên cần +1 để bao gồm sbyte.MaxValue (127 + 1 = 128)
				originalArray[i] = (sbyte)_random.Next(sbyte.MinValue, sbyte.MaxValue + 1);
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu SByte.");
		}
	}
}
