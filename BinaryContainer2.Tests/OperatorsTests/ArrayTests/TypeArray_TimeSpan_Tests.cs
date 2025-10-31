using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_TimeSpan_Tests // Test cho mảng TimeSpan[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử TimeSpan[]
			_typeArrayOperator = new TypeArray(typeof(TimeSpan[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng TimeSpan[] ---
		private void AssertArrayEqual(TimeSpan[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(TimeSpan[]), "Kiểu đọc ra phải là TimeSpan[].");

			for (int i = 0; i < expected.Length; i++)
			{
				TimeSpan expectedValue = expected[i];
				TimeSpan actualValue = (TimeSpan)actual.GetValue(i)!;

				// TimeSpan là kiểu struct, so sánh bằng nhau là đủ
				Assert.AreEqual(expectedValue, actualValue, $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng TimeSpan[] điển hình (dương và âm).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalTimeSpanArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: TimeSpan[]
			TimeSpan[] originalArray = new TimeSpan[]
			{
				new TimeSpan(1, 2, 3, 4, 5), 		// 1 ngày, 2 giờ, 3 phút, 4 giây, 5 mili giây
				TimeSpan.FromHours(-10.5),			// Giá trị âm
				TimeSpan.FromSeconds(3600.0),		// 1 giờ chính xác
				TimeSpan.FromMinutes(1)
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
			AssertArrayEqual(originalArray, readArray!, "Mảng TimeSpan[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (TimeSpan[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			TimeSpan[] originalArray = new TimeSpan[0];
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
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Min, Max, Zero).
		/// </summary>
		[TestMethod]
		public void WriteRead_TimeSpanEdgeValuesArray_ShouldBePreserved()
		{
			// TimeSpan dựa trên số ticks (long)
			TimeSpan[] originalArray = new TimeSpan[]
			{
				TimeSpan.MinValue,
				TimeSpan.MaxValue,
				TimeSpan.Zero,
				TimeSpan.FromTicks(1),
				TimeSpan.FromTicks(-1)
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của TimeSpan.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Phương thức tiện ích để tạo một TimeSpan ngẫu nhiên trong dải lớn.
		/// </summary>
		private TimeSpan GetRandomTimeSpan()
		{
			// Tạo số ticks ngẫu nhiên 64-bit (long)
			byte[] buf = new byte[8];
			_random.NextBytes(buf);
			long randomTicks = BitConverter.ToInt64(buf, 0);

			// Giới hạn dải ticks để tránh tràn, tuy nhiên, 
			// vì TimeSpan.Ticks là long, ta có thể giới hạn dải bằng cách 
			// lấy modulo của giá trị ngẫu nhiên với một giới hạn lớn.
			// Tuy nhiên, việc tạo ngẫu nhiên trong khoảng Min/Max là tốt nhất.
			// Ta sẽ giới hạn trong khoảng [-20000 ngày, 20000 ngày] để an toàn hơn
			long maxTicksForTesting = TimeSpan.FromDays(20000).Ticks;

			// Đảm bảo số ticks ngẫu nhiên nằm trong dải kiểm tra 
			// (dùng long.MaxValue/2 để tránh lỗi tràn khi tính toán)
			long range = long.MaxValue / 2;
			long randomLong = (long)(_random.NextDouble() * range * 2) - range;

			// Ép về phạm vi an toàn nếu cần, hoặc sử dụng trực tiếp nếu tin tưởng vào giới hạn của TimeSpan.
			if (randomLong > TimeSpan.MaxValue.Ticks) randomLong = TimeSpan.MaxValue.Ticks;
			if (randomLong < TimeSpan.MinValue.Ticks) randomLong = TimeSpan.MinValue.Ticks;

			return TimeSpan.FromTicks(randomLong);
		}

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị TimeSpan ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomTimeSpanArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			TimeSpan[] originalArray = new TimeSpan[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				originalArray[i] = GetRandomTimeSpan();
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu TimeSpan.");
		}
	}
}
