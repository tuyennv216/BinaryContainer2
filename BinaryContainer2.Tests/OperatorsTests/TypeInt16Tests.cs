using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeInt16Tests
	{
		private TypeInt16 _typeInt16Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeInt16Operator = new TypeInt16();
			_typeInt16Operator.Build();
			_refPool = new RefPool();
			_random = new Random();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị Int16 dương điển hình (ví dụ: 1234).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = (short)1234;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(short));
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
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: short.MinValue (-32768).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = short.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng -32768.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: short.MaxValue (32767).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = short.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 32767.");
		}

		/// <summary>
		/// Giá trị gần biên trên (ví dụ: 32766).
		/// </summary>
		[TestMethod]
		public void WriteRead_NearMaxValue_ShouldReturnCorrectValue()
		{
			object originalData = (short)(short.MaxValue - 1);
			var originalContainer = new DataContainer();
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}


		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Giá trị âm điển hình (ví dụ: -5000).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalNegativeValue_ShouldBePreserved()
		{
			object originalData = (short)-5000;
			var originalContainer = new DataContainer();
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị âm phải được giữ nguyên.");
		}

		/// <summary>
		/// Giá trị 0.
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroValue_ShouldReturnZero()
		{
			object originalData = (short)0;
			var originalContainer = new DataContainer();
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt16Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Int16, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonInt16Value_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: int)
			object originalData = 50000; // Vượt quá 32767
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeInt16Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Int16 ngẫu nhiên
		/// trong phạm vi lớn để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<short>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị short ngẫu nhiên trong phạm vi [-32768, 32767]
				// Cần ép kiểu vì Random.Next mặc định trả về int
				short value = (short)_random.Next(short.MinValue, short.MaxValue + 1);
				originalValues.Add(value);
				_typeInt16Operator.Write(originalContainer, value, _refPool);
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
				short expected = originalValues[i];
				object? readData = _typeInt16Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(short));
				Assert.AreEqual(expected, (short)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}