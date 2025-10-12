using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeInt64Tests
	{
		private TypeInt64 _typeInt64Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeInt64Operator = new TypeInt64();
			_typeInt64Operator.Build();
			_refPool = new RefPool();
			// Khởi tạo Random với seed để đảm bảo tính lặp lại của test
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị Int64 dương điển hình (ví dụ: 543210987654L).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 543210987654L;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(long));
			Assert.AreEqual(originalData, readData);
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
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: long.MinValue (-9.223...E18).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = long.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng long.MinValue.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: long.MaxValue (9.223...E18).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = long.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng long.MaxValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Giá trị âm rất lớn (ví dụ: -5 tỷ tỷ).
		/// </summary>
		[TestMethod]
		public void WriteRead_VeryLargeNegativeValue_ShouldBePreserved()
		{
			object originalData = -5000000000000000000L;
			var originalContainer = new DataContainer();
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị âm rất lớn phải được giữ nguyên.");
		}

		/// <summary>
		/// Giá trị 0.
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroValue_ShouldReturnZero()
		{
			object originalData = 0L;
			var originalContainer = new DataContainer();
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Int64, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonInt64Value_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: decimal)
			object originalData = 123.45m;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeInt64Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Int64 ngẫu nhiên
		/// trong phạm vi đầy đủ để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<long>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị long ngẫu nhiên trong phạm vi [long.MinValue, long.MaxValue]
				// Cần sử dụng phương pháp tạo số nguyên 64-bit ngẫu nhiên.
				byte[] bytes = new byte[8];
				_random.NextBytes(bytes);
				long value = BitConverter.ToInt64(bytes, 0);

				originalValues.Add(value);
				_typeInt64Operator.Write(originalContainer, value, _refPool);
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
				long expected = originalValues[i];
				object? readData = _typeInt64Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(long));
				Assert.AreEqual(expected, (long)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}