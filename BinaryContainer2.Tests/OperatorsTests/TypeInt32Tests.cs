using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeInt32Tests
	{
		private TypeInt32 _typeInt32Operator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeInt32Operator = new TypeInt32();
			_typeInt32Operator.Build();
			_refPool = new RefPool();
			// Khởi tạo Random với seed để đảm bảo tính lặp lại của test (nếu cần)
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị Int32 dương điển hình (ví dụ: 123456).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 123456;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(int));
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
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: int.MinValue (-2,147,483,648).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = int.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng int.MinValue.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: int.MaxValue (2,147,483,647).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = int.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng int.MaxValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Giá trị âm lớn (ví dụ: -1 tỷ).
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeNegativeValue_ShouldBePreserved()
		{
			object originalData = -1000000000;
			var originalContainer = new DataContainer();
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị âm lớn phải được giữ nguyên.");
		}

		/// <summary>
		/// Giá trị 0.
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroValue_ShouldReturnZero()
		{
			object originalData = 0;
			var originalContainer = new DataContainer();
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeInt32Operator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Int32, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonInt32Value_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: long, lớn hơn int.MaxValue)
			object originalData = 3000000000L;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeInt32Operator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Int32 ngẫu nhiên
		/// trong phạm vi đầy đủ để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<int>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị int ngẫu nhiên trong phạm vi [int.MinValue, int.MaxValue]
				// Cần sử dụng phương pháp tạo số nguyên 32-bit ngẫu nhiên từ Random.NextBytes
				// hoặc kết hợp hai giá trị ngẫu nhiên từ Random.Next (nếu .NET Framework không có Next(min, max) cho int.MinValue)

				// Cách đơn giản và chính xác:
				byte[] bytes = new byte[4];
				_random.NextBytes(bytes);
				int value = BitConverter.ToInt32(bytes, 0);

				originalValues.Add(value);
				_typeInt32Operator.Write(originalContainer, value, _refPool);
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
				int expected = originalValues[i];
				object? readData = _typeInt32Operator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(int));
				Assert.AreEqual(expected, (int)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}