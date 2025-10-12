using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeDateTimeTests
	{
		private TypeDateTime _typeDateTimeOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			_typeDateTimeOperator = new TypeDateTime();
			_typeDateTimeOperator.Build();
			_refPool = new RefPool();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị DateTime điển hình (Local Kind).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalLocalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data (Local Kind)
			object originalData = new DateTime(2023, 10, 27, 14, 30, 15, DateTimeKind.Local);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(DateTime));
			// So sánh Ticks (giá trị số) và Kind
			Assert.AreEqual(((DateTime)originalData).Ticks, readDateTime.Ticks, "Ticks (giá trị số) phải bằng nhau.");
			Assert.AreEqual(((DateTime)originalData).Kind, readDateTime.Kind, "Kind phải giữ nguyên là Local.");
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
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: DateTime.MinValue (Kind Unspecified).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = DateTime.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(DateTime.MinValue.Ticks, readDateTime.Ticks, "Giá trị phải bằng MinValue.");
			Assert.AreEqual(DateTimeKind.Unspecified, readDateTime.Kind, "Kind phải giữ nguyên là Unspecified.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: DateTime.MaxValue (Kind Unspecified).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = DateTime.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(DateTime.MaxValue.Ticks, readDateTime.Ticks, "Giá trị phải bằng MaxValue.");
			Assert.AreEqual(DateTimeKind.Unspecified, readDateTime.Kind, "Kind phải giữ nguyên là Unspecified.");
		}

		// --- III. Giá Trị Đặc Biệt (Kind) ---

		/// <summary>
		/// Kiểm tra giá trị với Kind là UTC.
		/// </summary>
		[TestMethod]
		public void WriteRead_UtcKind_ShouldReturnUtcKind()
		{
			// 1. Chuẩn bị data (UTC Kind)
			object originalData = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(((DateTime)originalData).Ticks, readDateTime.Ticks, "Ticks (giá trị số) phải bằng nhau.");
			Assert.AreEqual(DateTimeKind.Utc, readDateTime.Kind, "Kind phải giữ nguyên là Utc.");
		}

		/// <summary>
		/// Kiểm tra giá trị với Kind là Unspecified (ví dụ: ngày 01/01/2000).
		/// </summary>
		[TestMethod]
		public void WriteRead_UnspecifiedKind_ShouldReturnUnspecifiedKind()
		{
			// 1. Chuẩn bị data (Unspecified Kind)
			object originalData = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(((DateTime)originalData).Ticks, readDateTime.Ticks, "Ticks (giá trị số) phải bằng nhau.");
			Assert.AreEqual(DateTimeKind.Unspecified, readDateTime.Kind, "Kind phải giữ nguyên là Unspecified.");
		}

		/// <summary>
		/// Kiểm tra giá trị với độ chính xác đến Tick (ví dụ: 1/1/2023 10:00:00.0000001).
		/// </summary>
		[TestMethod]
		public void WriteRead_PrecisionToTick_ShouldBePreserved()
		{
			// 1. Chuẩn bị data
			object originalData = new DateTime(2023, 1, 1).AddTicks(1);
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDateTimeOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
			DateTime readDateTime = (DateTime)readData!;

			// 5. Kiểm tra
			Assert.AreEqual(((DateTime)originalData).Ticks, readDateTime.Ticks, "Độ chính xác đến Tick phải được giữ nguyên.");
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị DateTime ngẫu nhiên
		/// trong một phạm vi rộng, với Kind ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var random = new Random();
			var originalContainer = new DataContainer();
			var originalValues = new List<DateTime>();

			// Phạm vi thời gian kiểm thử (từ 1900 đến 2100)
			long minTicks = new DateTime(1900, 1, 1).Ticks;
			long maxTicks = new DateTime(2100, 1, 1).Ticks;

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo Ticks ngẫu nhiên
				long randomTicks = (long)(random.NextDouble() * (maxTicks - minTicks)) + minTicks;

				// Chọn Kind ngẫu nhiên
				DateTimeKind randomKind = (DateTimeKind)random.Next(0, 3); // 0=Unspecified, 1=Utc, 2=Local

				DateTime value = new DateTime(randomTicks, randomKind);
				originalValues.Add(value);
				_typeDateTimeOperator.Write(originalContainer, value, _refPool);
			}

			// 2. Xuất và Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Đọc và so sánh
			for (int i = 0; i < testCount; i++)
			{
				DateTime expected = originalValues[i];
				object? readData = _typeDateTimeOperator.Read(newContainer, _refPool);
				DateTime actual = (DateTime)readData!;

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(DateTime));

				// Kiểm tra Ticks và Kind
				Assert.AreEqual(expected.Ticks, actual.Ticks, $"Ticks của giá trị ngẫu nhiên thứ {i} không khớp.");
				Assert.AreEqual(expected.Kind, actual.Kind, $"Kind của giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}