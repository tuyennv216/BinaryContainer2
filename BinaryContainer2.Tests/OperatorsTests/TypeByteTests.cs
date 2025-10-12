using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeByteTests
	{
		private TypeByte _typeByteOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			_typeByteOperator = new TypeByte();
			_typeByteOperator.Build();
			_refPool = new RefPool();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị byte điển hình (ví dụ: 100).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = (byte)100;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeByteOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeByteOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(byte));
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
			_typeByteOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeByteOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: byte.MinValue (0).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnZero()
		{
			// 1. Chuẩn bị data
			object originalData = byte.MinValue; // 0
			var originalContainer = new DataContainer();

			// 2. Write
			_typeByteOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeByteOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 0.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: byte.MaxValue (255).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturn255()
		{
			// 1. Chuẩn bị data
			object originalData = byte.MaxValue; // 255
			var originalContainer = new DataContainer();

			// 2. Write
			_typeByteOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeByteOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng 255.");
		}

		// --- III. Giá Trị Đặc Biệt/Sai Kiểu (Special/Invalid Cases) ---

		/// <summary>
		/// Kiểm tra một giá trị gần biên dưới (ví dụ: 1).
		/// </summary>
		[TestMethod]
		public void WriteRead_NearMinValue_ShouldReturnOne()
		{
			object originalData = (byte)1;
			var originalContainer = new DataContainer();
			_typeByteOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeByteOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Kiểm tra một giá trị gần biên trên (ví dụ: 254).
		/// </summary>
		[TestMethod]
		public void WriteRead_NearMaxValue_ShouldReturn254()
		{
			object originalData = (byte)254;
			var originalContainer = new DataContainer();
			_typeByteOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeByteOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Byte, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonByteValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: số nguyên 16-bit)
			object originalData = (short)500; // Vượt quá 255
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeByteOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Byte ngẫu nhiên
		/// để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var random = new Random();
			var originalContainer = new DataContainer();
			var originalValues = new List<byte>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị byte ngẫu nhiên trong phạm vi [0, 255]
				byte value = (byte)random.Next(byte.MinValue, byte.MaxValue + 1);
				originalValues.Add(value);
				_typeByteOperator.Write(originalContainer, value, _refPool);
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
				byte expected = originalValues[i];
				object? readData = _typeByteOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(byte));
				Assert.AreEqual(expected, (byte)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}