using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_DateTime_Tests // Test cho mảng DateTime[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử DateTime[]
			_typeArrayOperator = new TypeArray(typeof(DateTime[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng DateTime ---
		private void AssertArrayEqual(DateTime[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(DateTime[]), "Kiểu đọc ra phải là DateTime[].");

			for (int i = 0; i < expected.Length; i++)
			{
				DateTime expectedValue = expected[i];
				DateTime actualValue = (DateTime)actual.GetValue(i)!;

				// So sánh giá trị chính xác (Ticks)
				Assert.AreEqual(expectedValue.Ticks, actualValue.Ticks, $"Phần tử thứ {i} Ticks không khớp: {message}");

				// So sánh Kind, vì đây là một phần quan trọng của DateTime
				Assert.AreEqual(expectedValue.Kind, actualValue.Kind, $"Phần tử thứ {i} Kind không khớp: {message}");
			}
		}

		/// <summary>
		/// Phương thức tiện ích để tạo DateTime ngẫu nhiên với Kind ngẫu nhiên
		/// </summary>
		private DateTime GetRandomDateTime()
		{
			// Dải ticks an toàn để tránh tràn hoặc MinValue/MaxValue quá sát
			const long minTicks = 1000000; // Sau MinValue
			const long maxTicks = 3155378975999999999L; // Khoảng 1000 năm (2000-3000)

			// Tạo Ticks ngẫu nhiên
			byte[] buf = new byte[8];
			_random.NextBytes(buf);
			long randomTicks = BitConverter.ToInt64(buf, 0);

			// Giới hạn trong dải an toàn
			randomTicks = Math.Abs(randomTicks % (maxTicks - minTicks)) + minTicks;

			// Chọn Kind ngẫu nhiên
			DateTimeKind kind = (DateTimeKind)_random.Next(0, 3); // 0=Unspecified, 1=Utc, 2=Local

			return new DateTime(randomTicks, kind);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng DateTime[] điển hình với các Kind khác nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalDateTimeArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: DateTime[]
			DateTime[] originalArray = new DateTime[]
			{
				new DateTime(2023, 10, 27, 10, 30, 0, DateTimeKind.Local),
				new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2050, 5, 5, 12, 0, 0, DateTimeKind.Unspecified)
			};
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
			AssertArrayEqual(originalArray, readArray!, "Mảng DateTime[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (DateTime[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			DateTime[] originalArray = new DateTime[0];
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
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Min, Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_DateTimeEdgeValuesArray_ShouldBePreserved()
		{
			DateTime[] originalArray = new DateTime[]
			{
				DateTime.MinValue, // 01/01/0001
				DateTime.MaxValue, // 31/12/9999
				DateTime.UtcNow.Date.AddDays(1)
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của DateTime.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị DateTime ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomDateTimeArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			DateTime[] originalArray = new DateTime[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				originalArray[i] = GetRandomDateTime();
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu DateTime.");
		}
	}
}
