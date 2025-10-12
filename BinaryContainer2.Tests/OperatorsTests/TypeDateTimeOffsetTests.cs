using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeDateTimeOffsetTests
	{
		private TypeDateTimeOffset _typeDateTimeOffsetOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo operator cho DateTimeOffset
			_typeDateTimeOffsetOperator = new TypeDateTimeOffset();
			_typeDateTimeOffsetOperator.Build();
			_refPool = new RefPool();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị DateTimeOffset điển hình với Offset dương (ví dụ: +7 giờ).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveOffsetValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data (Offset +7 giờ)
			var offsetTimeSpan = new TimeSpan(7, 0, 0);
			object originalData = new DateTimeOffset(2023, 10, 27, 14, 30, 15, offsetTimeSpan);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;
			DateTimeOffset originalDateTimeOffset = (DateTimeOffset)originalData;

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(DateTimeOffset));
			// So sánh Ticks (thời điểm) và Offset (múi giờ)
			Assert.AreEqual(originalDateTimeOffset.Ticks, readDateTimeOffset.Ticks, "Ticks (thời điểm tuyệt đối) phải bằng nhau.");
			Assert.AreEqual(originalDateTimeOffset.Offset, readDateTimeOffset.Offset, "Offset (thông tin múi giờ) phải được giữ nguyên.");
		}

		/// <summary>
		/// Test case cho một giá trị DateTimeOffset với Offset âm (ví dụ: -5 giờ).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalNegativeOffsetValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data (Offset -5 giờ)
			var offsetTimeSpan = new TimeSpan(-5, 0, 0);
			object originalData = new DateTimeOffset(2024, 1, 1, 9, 0, 0, offsetTimeSpan);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;
			DateTimeOffset originalDateTimeOffset = (DateTimeOffset)originalData;

			// 5. Kiểm tra
			Assert.AreEqual(originalDateTimeOffset.Ticks, readDateTimeOffset.Ticks, "Ticks phải bằng nhau.");
			Assert.AreEqual(originalDateTimeOffset.Offset, readDateTimeOffset.Offset, "Offset phải giữ nguyên là -5 giờ.");
		}

		/// <summary>
		/// Test case cho giá trị NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullValue_ShouldReturnNull()
		{
			// 1. Chuẩn bị data
			object? originalData = null;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: DateTimeOffset.MinValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data. MinValue có Offset mặc định là TimeSpan.Zero.
			object originalData = DateTimeOffset.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(DateTimeOffset.MinValue.Ticks, readDateTimeOffset.Ticks, "Ticks phải bằng MinValue.");
			Assert.AreEqual(TimeSpan.Zero, readDateTimeOffset.Offset, "Offset phải là TimeSpan.Zero.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: DateTimeOffset.MaxValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data. MaxValue có Offset mặc định là TimeSpan.Zero.
			object originalData = DateTimeOffset.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(DateTimeOffset.MaxValue.Ticks, readDateTimeOffset.Ticks, "Ticks phải bằng MaxValue.");
			Assert.AreEqual(TimeSpan.Zero, readDateTimeOffset.Offset, "Offset phải là TimeSpan.Zero.");
		}

		// --- III. Giá Trị Đặc Biệt (Offset) ---

		/// <summary>
		/// Kiểm tra giá trị với Offset là UTC (00:00).
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroOffset_ShouldReturnZeroOffset()
		{
			// 1. Chuẩn bị data (Offset 0)
			object originalData = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;
			DateTimeOffset originalDateTimeOffset = (DateTimeOffset)originalData;

			// 5. Kiểm tra
			Assert.AreEqual(originalDateTimeOffset.Ticks, readDateTimeOffset.Ticks, "Ticks phải bằng nhau.");
			Assert.AreEqual(TimeSpan.Zero, readDateTimeOffset.Offset, "Offset phải giữ nguyên là TimeSpan.Zero.");
		}

		/// <summary>
		/// Kiểm tra giá trị với độ chính xác đến Tick (ví dụ: Offset 0, Ticks +1).
		/// </summary>
		[TestMethod]
		public void WriteRead_PrecisionToTick_ShouldBePreserved()
		{
			// 1. Chuẩn bị data: Thêm 1 Tick và sử dụng Offset +30 phút
			var offsetTimeSpan = new TimeSpan(0, 30, 0); // +30 phút
			object originalData = new DateTimeOffset(new DateTime(2023, 1, 1).AddTicks(1), offsetTimeSpan);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOffsetOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 4. Read
			object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
			DateTimeOffset readDateTimeOffset = (DateTimeOffset)readData!;
			DateTimeOffset originalDateTimeOffset = (DateTimeOffset)originalData;

			// 5. Kiểm tra
			Assert.AreEqual(originalDateTimeOffset.Ticks, readDateTimeOffset.Ticks, "Độ chính xác Ticks phải được giữ nguyên.");
			Assert.AreEqual(originalDateTimeOffset.Offset, readDateTimeOffset.Offset, "Offset phải được giữ nguyên.");
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị DateTimeOffset ngẫu nhiên
		/// trong một phạm vi rộng, với Offset ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var random = new Random();
			var originalContainer = new DataContainer();
			var originalValues = new List<DateTimeOffset>();

			// Phạm vi thời gian kiểm thử (từ 1900 đến 2100)
			long minTicks = new DateTime(1900, 1, 1).Ticks;
			long maxTicks = new DateTime(2100, 1, 1).Ticks;

			// Phạm vi Offset kiểm thử (từ -13 giờ đến +14 giờ, tính bằng phút)
			int minOffsetMinutes = -13 * 60;
			int maxOffsetMinutes = 14 * 60;

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo Ticks ngẫu nhiên
				long randomTicks = (long)(random.NextDouble() * (maxTicks - minTicks)) + minTicks;

				// Chọn Offset ngẫu nhiên
				int randomOffsetMinutes = random.Next(minOffsetMinutes, maxOffsetMinutes + 1);
				var randomOffset = TimeSpan.FromMinutes(randomOffsetMinutes);

				DateTimeOffset value = new DateTimeOffset(randomTicks, randomOffset);
				originalValues.Add(value);
				_typeDateTimeOffsetOperator.Write(originalContainer, value, _refPool);
			}

			// 2. Xuất và Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Đọc và so sánh
			for (int i = 0; i < testCount; i++)
			{
				DateTimeOffset expected = originalValues[i];
				object? readData = _typeDateTimeOffsetOperator.Read(newContainer, _refPool);
				DateTimeOffset actual = (DateTimeOffset)readData!;

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(DateTimeOffset));

				// Kiểm tra Ticks và Offset
				Assert.AreEqual(expected.Ticks, actual.Ticks, $"Ticks của giá trị ngẫu nhiên thứ {i} không khớp.");
				Assert.AreEqual(expected.Offset, actual.Offset, $"Offset của giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}
