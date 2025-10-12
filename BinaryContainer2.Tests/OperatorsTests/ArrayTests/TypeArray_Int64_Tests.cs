using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_Int64_Tests // Test cho mảng long[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử long[]
			_typeArrayOperator = new TypeArray(typeof(long[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42 để đảm bảo tính lặp lại của các giá trị ngẫu nhiên
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng long[] ---
		private void AssertArrayEqual(long[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(long[]), "Kiểu đọc ra phải là long[].");
			for (int i = 0; i < expected.Length; i++)
			{
				// actual.GetValue(i) trả về object, cần ép kiểu an toàn hoặc dùng Assert.AreEqual trực tiếp
				Assert.AreEqual(expected[i], (long)actual.GetValue(i)!, $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		/// <summary>
		/// Phương thức tiện ích để tạo một giá trị long ngẫu nhiên toàn dải.
		/// </summary>
		private long NextLong(Random rng, long min, long max)
		{
			// Để tạo số long ngẫu nhiên trong dải lớn, ta cần sử dụng 8 bytes ngẫu nhiên.
			// Tuy nhiên, vì dải lớn, cách an toàn nhất là tạo 2 giá trị ngẫu nhiên 32-bit và kết hợp.

			// Tạo 64-bit ngẫu nhiên
			byte[] buf = new byte[8];
			rng.NextBytes(buf);
			long longRand = BitConverter.ToInt64(buf, 0);

			// Map giá trị ngẫu nhiên (từ MinValue đến MaxValue) vào dải [min, max]
			long range = max - min;
			if (range <= 0) return min; // Xử lý trường hợp dải không hợp lệ hoặc 0

			// Tính toán modulo và dịch chuyển
			long positiveLongRand = longRand - long.MinValue;
			long scaled = positiveLongRand % range;

			return min + scaled;
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng long[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalLongArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: long[]
			long[] originalArray = new long[] { 10L, -20000000000L, 300000000000L, 4000L };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng long[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (long[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			long[] originalArray = new long[0];
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
		/// Mảng có 1 phần tử là giá trị biên (long.MaxValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleElementEdgeArray_ShouldBePreserved()
		{
			long[] originalArray = new long[] { long.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng có 1 phần tử biên của Int64.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Int64.Min, 0, Int64.Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_LongEdgeValuesArray_ShouldBePreserved()
		{
			long[] originalArray = new long[] { long.MinValue, 0L, long.MaxValue };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Int64.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên (sử dụng dải toàn bộ).
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomLongArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			long[] originalArray = new long[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Tạo giá trị long ngẫu nhiên trong dải toàn bộ (hoặc dải rất lớn)
				originalArray[i] = NextLong(_random, long.MinValue, long.MaxValue);
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu Int64.");
		}
	}
}
