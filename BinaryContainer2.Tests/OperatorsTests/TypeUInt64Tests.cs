using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeUInt64Tests
	{
		private TypeUInt64 _typeUInt64Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeUInt64Operator = new TypeUInt64();
			_typeUInt64Operator.Build();
			_refPool = new RefPool();
			// Khởi tạo Random với seed để đảm bảo tính lặp lại của test
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị UInt64 điển hình (ví dụ: 123456789012345UL).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 123456789012345UL;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(ulong));
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
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: ulong.MinValue (0).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnZero()
		{
			// 1. Chuẩn bị data
			object originalData = ulong.MinValue; // 0
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: ulong.MaxValue (18,446,744,073,709,551,615).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = ulong.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng ulong.MaxValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Kiểm tra giá trị ngay trên long.MaxValue (9,223,372,036,854,775,808).
		/// </summary>
		[TestMethod]
		public void WriteRead_OverLongMaxValue_ShouldBePreserved()
		{
			object originalData = 9223372036854775808UL;
			var originalContainer = new DataContainer();
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị ngay trên Long.MaxValue phải được giữ nguyên.");
		}

		/// <summary>
		/// Kiểm tra giá trị lớn, khoảng giữa (ví dụ: 10 tỷ tỷ).
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeMidRangeValue_ShouldBePreserved()
		{
			object originalData = 10000000000000000000UL; // 10 tỷ tỷ
			var originalContainer = new DataContainer();
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt64Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị giữa phạm vi (10 tỷ tỷ) phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt64 (long âm).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NegativeLongValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: long âm)
			object originalData = -1L;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt64 (decimal).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonUInt64Value_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: decimal)
			object originalData = 123.45m;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt64Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị UInt64 ngẫu nhiên
		/// trong phạm vi đầy đủ để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<ulong>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị ulong ngẫu nhiên trong phạm vi [0, ulong.MaxValue]
				// Cần sử dụng phương pháp tạo số nguyên 64-bit ngẫu nhiên.
				byte[] bytes = new byte[8];
				_random.NextBytes(bytes);
				ulong value = BitConverter.ToUInt64(bytes, 0);

				originalValues.Add(value);
				_typeUInt64Operator.Write(originalContainer, value, _refPool);
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
				ulong expected = originalValues[i];
				object? readData = _typeUInt64Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(ulong));
				Assert.AreEqual(expected, (ulong)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}