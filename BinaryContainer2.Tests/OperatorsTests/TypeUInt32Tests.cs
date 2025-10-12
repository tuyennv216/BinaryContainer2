using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeUInt32Tests
	{
		private TypeUInt32 _typeUInt32Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeUInt32Operator = new TypeUInt32();
			_typeUInt32Operator.Build();
			_refPool = new RefPool();
			// Khởi tạo Random với seed để đảm bảo tính lặp lại của test
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị UInt32 điển hình (ví dụ: 12345678U).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 12345678U;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(uint));
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
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: uint.MinValue (0).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnZero()
		{
			// 1. Chuẩn bị data
			object originalData = uint.MinValue; // 0
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: uint.MaxValue (4,294,967,295).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = uint.MaxValue; // 4,294,967,295
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng uint.MaxValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Kiểm tra giá trị giữa (ví dụ: 3 tỷ).
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeMidRangeValue_ShouldBePreserved()
		{
			object originalData = 3000000000U; // 3 tỷ
			var originalContainer = new DataContainer();
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị giữa phạm vi (3 tỷ) phải được giữ nguyên.");
		}

		/// <summary>
		/// Kiểm tra giá trị ngay trên Int32.MaxValue (2,147,483,648).
		/// </summary>
		[TestMethod]
		public void WriteRead_OverIntMaxValue_ShouldBePreserved()
		{
			object originalData = 2147483648U;
			var originalContainer = new DataContainer();
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị ngay trên Int32.MaxValue phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt32 (int âm).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NegativeIntValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: int âm)
			object originalData = -1;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt32 (long vượt quá 4.2 tỷ).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_OversizeLongValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: long)
			object originalData = 5000000000L; // Vượt quá UInt32.MaxValue
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt32Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị UInt32 ngẫu nhiên
		/// trong phạm vi đầy đủ để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<uint>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị uint ngẫu nhiên trong phạm vi [0, 4,294,967,295]
				// Cần sử dụng phương pháp tạo số nguyên 32-bit ngẫu nhiên.
				byte[] bytes = new byte[4];
				_random.NextBytes(bytes);
				uint value = BitConverter.ToUInt32(bytes, 0);

				originalValues.Add(value);
				_typeUInt32Operator.Write(originalContainer, value, _refPool);
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
				uint expected = originalValues[i];
				object? readData = _typeUInt32Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(uint));
				Assert.AreEqual(expected, (uint)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}