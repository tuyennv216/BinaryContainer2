using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeUInt16Tests
	{
		private TypeUInt16 _typeUInt16Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeUInt16Operator = new TypeUInt16();
			_typeUInt16Operator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị UInt16 điển hình (ví dụ: 12345).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = (ushort)12345;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(ushort));
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
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: ushort.MinValue (0).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnZero()
		{
			// 1. Chuẩn bị data
			object originalData = ushort.MinValue; // 0
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: ushort.MaxValue (65535).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = ushort.MaxValue; // 65535
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 65535.");
		}

		/// <summary>
		/// Giá trị gần biên trên (ví dụ: 65534).
		/// </summary>
		[TestMethod]
		public void WriteRead_NearMaxValue_ShouldReturnCorrectValue()
		{
			object originalData = (ushort)(ushort.MaxValue - 1);
			var originalContainer = new DataContainer();
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}


		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Kiểm tra giá trị giữa (ví dụ: 32768).
		/// </summary>
		[TestMethod]
		public void WriteRead_MidRangeValue_ShouldBePreserved()
		{
			object originalData = (ushort)32768; // Lớn hơn short.MaxValue
			var originalContainer = new DataContainer();
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeUInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị giữa phạm vi (32768) phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt16, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonUInt16Value_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: số âm short)
			object originalData = (short)-10;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải UInt16 (int vượt quá 65535).
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_OversizeIntValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: int)
			object originalData = 70000; // Vượt quá 65535
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeUInt16Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị UInt16 ngẫu nhiên
		/// trong phạm vi đầy đủ để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<ushort>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị ushort ngẫu nhiên trong phạm vi [0, 65535]
				// Random.Next(min, max) bao gồm min và loại trừ max.
				// ushort.MaxValue + 1 = 65536 (phạm vi hợp lệ cho Next)
				int randomInt = _random.Next(ushort.MinValue, ushort.MaxValue + 1);
				ushort value = (ushort)randomInt;

				originalValues.Add(value);
				_typeUInt16Operator.Write(originalContainer, value, _refPool);
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
				ushort expected = originalValues[i];
				object? readData = _typeUInt16Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(ushort));
				Assert.AreEqual(expected, (ushort)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}