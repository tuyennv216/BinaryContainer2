using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeTimeSpanTests
	{
		private TypeTimeSpan _typeTimeSpanOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeTimeSpanOperator = new TypeTimeSpan();
			_typeTimeSpanOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị TimeSpan dương điển hình (ví dụ: 1 ngày, 2 giờ).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: 1 ngày, 2 giờ, 30 phút, 5 giây, 100 milliseconds
			object originalData = new TimeSpan(1, 2, 30, 5, 100);
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(TimeSpan));
			Assert.AreEqual(originalData, readData, "Giá trị TimeSpan dương phải được giữ nguyên.");
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

			// 2. Write, Export, Import
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: TimeSpan.MinValue (số ticks âm lớn nhất).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = TimeSpan.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng TimeSpan.MinValue.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: TimeSpan.MaxValue (số ticks dương lớn nhất).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = TimeSpan.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng TimeSpan.MaxValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Giá trị Zero: TimeSpan.Zero (0 ticks).
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroValue_ShouldReturnZero()
		{
			object originalData = TimeSpan.Zero;
			var originalContainer = new DataContainer();
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng TimeSpan.Zero.");
		}

		/// <summary>
		/// Giá trị âm điển hình (ví dụ: -1 giờ, -30 phút).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalNegativeValue_ShouldBePreserved()
		{
			object originalData = new TimeSpan(-1, -30, -0);
			var originalContainer = new DataContainer();
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị âm phải được giữ nguyên.");
		}

		/// <summary>
		/// Độ chính xác đến Tick (100 nano giây).
		/// </summary>
		[TestMethod]
		public void WriteRead_PrecisionToTick_ShouldBePreserved()
		{
			// 1 Tick sau Zero
			object originalData = new TimeSpan(1);
			var originalContainer = new DataContainer();
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Độ chính xác đến 1 tick phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải TimeSpan, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonTimeSpanValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: DateTime)
			object originalData = DateTime.Now;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeTimeSpanOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị TimeSpan ngẫu nhiên
		/// trong phạm vi rộng (bao gồm âm và dương) để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<TimeSpan>();

			// Phạm vi ticks kiểm thử (khoảng 10000 năm)
			long minTicks = -(long)TimeSpan.FromDays(365 * 10000).Ticks;
			long maxTicks = (long)TimeSpan.FromDays(365 * 10000).Ticks;

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo 8 bytes ngẫu nhiên và chuyển thành long (bao phủ toàn bộ dải 64-bit)
				byte[] bytes = new byte[8];
				_random.NextBytes(bytes);
				long randomTicks = BitConverter.ToInt64(bytes, 0);

				// Hạn chế phạm vi ticks để tránh MinValue/MaxValue quá thường xuyên, 
				// nhưng vẫn bao gồm âm và dương.
				if (randomTicks > minTicks && randomTicks < maxTicks)
				{
					TimeSpan value = new TimeSpan(randomTicks);
					originalValues.Add(value);
					_typeTimeSpanOperator.Write(originalContainer, value, _refPool);
				}
				else
				{
					i--; // Lặp lại nếu giá trị nằm quá xa giới hạn (dù ít xảy ra)
				}
			}

			// 2. Xuất và Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Đọc và so sánh
			for (int i = 0; i < originalValues.Count; i++)
			{
				TimeSpan expected = originalValues[i];
				object? readData = _typeTimeSpanOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(TimeSpan));
				Assert.AreEqual(expected, (TimeSpan)readData, $"Giá trị TimeSpan ngẫu nhiên thứ {i} không khớp (Ticks: {expected.Ticks}).");
			}
		}
	}
}