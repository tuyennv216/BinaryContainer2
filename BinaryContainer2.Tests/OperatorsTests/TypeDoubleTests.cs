using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeDoubleTests
	{
		private TypeDouble _typeDoubleOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeDoubleOperator = new TypeDouble();
			_typeDoubleOperator.Build();
			_refPool = new RefPool();
			_random = new Random();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị double điển hình (ví dụ: 3.14159d).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 3.14159d;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(double));
			// Dùng AreEqual vì double không mất độ chính xác trong quá trình tuần tự hóa/giải tuần tự hóa
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
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên trên cùng: Double.MaxValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			object originalData = Double.MaxValue;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Double.MaxValue.");
		}

		/// <summary>
		/// Giá trị biên dưới cùng: Double.MinValue (số âm lớn nhất).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			object originalData = Double.MinValue;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Double.MinValue.");
		}

		/// <summary>
		/// Giá trị nhỏ nhất gần 0 (chuẩn hóa): Double.Epsilon.
		/// </summary>
		[TestMethod]
		public void WriteRead_EpsilonValue_ShouldReturnCorrectValue()
		{
			object originalData = Double.Epsilon;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Double.Epsilon.");
		}

		// --- III. Giá Trị Đặc Biệt (IEEE 754 Special Values) ---

		/// <summary>
		/// Kiểm tra giá trị Not a Number (NaN).
		/// </summary>
		[TestMethod]
		public void WriteRead_NaNValue_ShouldReturnNaN()
		{
			object originalData = Double.NaN;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(double));
			Assert.IsTrue(Double.IsNaN((double)readData), "Giá trị đọc ra phải là NaN.");
		}

		/// <summary>
		/// Kiểm tra giá trị Vô cùng Dương (PositiveInfinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_PositiveInfinityValue_ShouldReturnCorrectValue()
		{
			object originalData = Double.PositiveInfinity;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải là PositiveInfinity.");
		}

		/// <summary>
		/// Kiểm tra giá trị Vô cùng Âm (NegativeInfinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_NegativeInfinityValue_ShouldReturnCorrectValue()
		{
			object originalData = Double.NegativeInfinity;
			var originalContainer = new DataContainer();
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeDoubleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải là NegativeInfinity.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Double, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonDoubleValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: float)
			object originalData = 123.45f;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeDoubleOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Double ngẫu nhiên
		/// trong phạm vi lớn để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<double>();

			// Phạm vi logarit rộng để bao gồm các số rất lớn và rất nhỏ
			double minLog = Math.Log10(Double.Epsilon); // Khoảng -308
			double maxLog = Math.Log10(Double.MaxValue); // Khoảng 308

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo logarit ngẫu nhiên
				double randomLog = _random.NextDouble() * (maxLog - minLog) + minLog;
				double mantissa = _random.NextDouble() * 2 - 1; // Khoảng [-1, 1]

				// Kết hợp để tạo số double ngẫu nhiên
				double value = mantissa * Math.Pow(10, randomLog);

				// Đảm bảo không tạo ra NaN hoặc Infinity ngẫu nhiên (chúng đã được test riêng)
				if (Double.IsFinite(value) && value != 0)
				{
					originalValues.Add(value);
					_typeDoubleOperator.Write(originalContainer, value, _refPool);
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
				double expected = originalValues[i];
				object? readData = _typeDoubleOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(double));
				// Dùng Tolerance (dung sai) để đề phòng lỗi làm tròn, mặc dù BitConverter
				// thường bảo toàn giá trị chính xác tuyệt đối. Dùng AreEqual để kiểm tra
				// chính xác 64-bit float.
				Assert.AreEqual(expected, (double)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}