using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeSingleTests
	{
		private TypeSingle _typeSingleOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeSingleOperator = new TypeSingle();
			_typeSingleOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị float dương điển hình (ví dụ: 12.345f).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalPositiveValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 12.345f;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(float));
			// Dùng AreEqual vì BitConverter thường bảo toàn giá trị 32-bit chính xác
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
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên trên cùng: Single.MaxValue.
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnCorrectValue()
		{
			object originalData = Single.MaxValue;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Single.MaxValue.");
		}

		/// <summary>
		/// Giá trị biên dưới cùng: Single.MinValue (số âm lớn nhất).
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnCorrectValue()
		{
			object originalData = Single.MinValue;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Single.MinValue.");
		}

		/// <summary>
		/// Giá trị nhỏ nhất gần 0 (chuẩn hóa): Single.Epsilon.
		/// </summary>
		[TestMethod]
		public void WriteRead_EpsilonValue_ShouldReturnCorrectValue()
		{
			object originalData = Single.Epsilon;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng Single.Epsilon.");
		}

		// --- III. Giá Trị Đặc Biệt (IEEE 754 Special Values) ---

		/// <summary>
		/// Kiểm tra giá trị Not a Number (NaN).
		/// </summary>
		[TestMethod]
		public void WriteRead_NaNValue_ShouldReturnNaN()
		{
			object originalData = Single.NaN;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(float));
			Assert.IsTrue(Single.IsNaN((float)readData), "Giá trị đọc ra phải là NaN.");
		}

		/// <summary>
		/// Kiểm tra giá trị Vô cùng Dương (PositiveInfinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_PositiveInfinityValue_ShouldReturnCorrectValue()
		{
			object originalData = Single.PositiveInfinity;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải là PositiveInfinity.");
		}

		/// <summary>
		/// Kiểm tra giá trị Vô cùng Âm (NegativeInfinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_NegativeInfinityValue_ShouldReturnCorrectValue()
		{
			object originalData = Single.NegativeInfinity;
			var originalContainer = new DataContainer();
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeSingleOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Giá trị phải là NegativeInfinity.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Single, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonSingleValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: double)
			object originalData = 123.45d;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeSingleOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Single ngẫu nhiên
		/// trong phạm vi lớn để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<float>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị float ngẫu nhiên bằng cách tạo 4 bytes ngẫu nhiên.
				// Đây là cách tốt nhất để bao quát toàn bộ dải 32-bit của float.
				byte[] bytes = new byte[4];
				_random.NextBytes(bytes);
				float value = BitConverter.ToSingle(bytes, 0);

				// Đảm bảo không thêm NaN hoặc Infinity ngẫu nhiên (chúng đã được test riêng)
				if (Single.IsFinite(value))
				{
					originalValues.Add(value);
					_typeSingleOperator.Write(originalContainer, value, _refPool);
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
				float expected = originalValues[i];
				object? readData = _typeSingleOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(float));
				// Giá trị phải khớp chính xác 32-bit (vì tuần tự hóa/giải tuần tự hóa không làm mất độ chính xác)
				Assert.AreEqual(expected, (float)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}