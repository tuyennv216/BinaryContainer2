using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeDecimalTests
	{
		private TypeDecimal _typeDecimalOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeDecimalOperator = new TypeDecimal();
			_typeDecimalOperator.Build();
			_refPool = new RefPool();
			_random = new Random();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị decimal điển hình (ví dụ: 123.45m).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 123.45m;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(decimal));
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
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		/// <summary>
		/// Test case cho giá trị ZEROm (0m).
		/// </summary>
		[TestMethod]
		public void WriteRead_ZeroValue_ShouldReturnZero()
		{
			object originalData = 0m;
			var originalContainer = new DataContainer();
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);

			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên trên cùng: Decimal.MaxValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = Decimal.MaxValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Decimal.MaxValue.");
		}

		/// <summary>
		/// Giá trị biên dưới cùng: Decimal.MinValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = Decimal.MinValue;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Decimal.MinValue.");
		}

		// --- III. Giá Trị Đặc Biệt (Precision and Sign) ---

		/// <summary>
		/// Giá trị có độ chính xác tối đa (28 chữ số thập phân).
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxPrecisionValue_ShouldBePreserved()
		{
			// Độ chính xác tối đa 28 chữ số thập phân
			object originalData = 1.0000000000000000000000000001m;
			var originalContainer = new DataContainer();

			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			Assert.AreEqual(originalData, readData, "Độ chính xác tối đa phải được giữ nguyên.");
		}

		/// <summary>
		/// Giá trị gần nhất với 0 (nhỏ nhất mà decimal có thể biểu diễn).
		/// </summary>
		[TestMethod]
		public void WriteRead_NearZeroValue_ShouldBeCorrect()
		{
			// Decimal.One / 10^28
			object originalData = Decimal.Parse("0.0000000000000000000000000001");
			var originalContainer = new DataContainer();

			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			Assert.AreEqual(originalData, readData, "Giá trị gần 0 phải được giữ nguyên.");
		}

		/// <summary>
		/// Giá trị âm với số thập phân (ví dụ: -987.654m).
		/// </summary>
		[TestMethod]
		public void WriteRead_NegativeValue_ShouldBePreserved()
		{
			object originalData = -987.654m;
			var originalContainer = new DataContainer();

			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

			Assert.AreEqual(originalData, readData, "Giá trị âm phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Decimal, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonDecimalValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: double)
			object originalData = 123.45d;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeDecimalOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Decimal ngẫu nhiên
		/// để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<decimal>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo một số decimal ngẫu nhiên.
				// Decimal được cấu tạo từ 3 Int32 và 1 Int32 (scale, sign).
				// Cách đơn giản hóa: tạo 3 số nguyên ngẫu nhiên và scale ngẫu nhiên.
				int[] bits = new int[4];
				bits[0] = _random.Next(); // low
				bits[1] = _random.Next(); // mid
				bits[2] = _random.Next(0, 100); // high, giữ nhỏ để tránh tràn

				// Scale (tối đa 28)
				int scale = _random.Next(0, 29);

				// Sign (bit thứ 31 của bits[3] là bit dấu: 0=positive, 1=negative)
				if (_random.Next(2) == 1)
				{
					bits[3] |= unchecked((int)0x80000000); // Đặt bit dấu
				}

				bits[3] |= scale << 16; // Đặt scale

				decimal value = new decimal(bits);
				originalValues.Add(value);
				_typeDecimalOperator.Write(originalContainer, value, _refPool);
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
				decimal expected = originalValues[i];
				object? readData = _typeDecimalOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(decimal));
				Assert.AreEqual(expected, (decimal)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}