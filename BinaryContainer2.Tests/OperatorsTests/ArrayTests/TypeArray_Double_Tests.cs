using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Double_Tests // Test cho mảng double[] (Double/float 64-bit)
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		// Độ chính xác nhỏ khi so sánh giá trị double (double precision)
		private const double DoubleTolerance = 1e-15;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử double[]
			_typeArrayOperator = new TypeArray(typeof(double[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng double ---
		private void AssertArrayEqual(double[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(double[]), "Kiểu đọc ra phải là double[].");

			for (int i = 0; i < expected.Length; i++)
			{
				double expectedValue = expected[i];
				double actualValue = (double)actual.GetValue(i)!;

				// 1. Kiểm tra NaN
				if (double.IsNaN(expectedValue))
				{
					Assert.IsTrue(double.IsNaN(actualValue), $"Phần tử thứ {i} phải là NaN: {message}");
				}
				// 2. Kiểm tra Infinity
				else if (double.IsInfinity(expectedValue))
				{
					Assert.AreEqual(expectedValue, actualValue, $"Phần tử thứ {i} phải là Infinity: {message}");
				}
				// 3. Kiểm tra giá trị thông thường với độ chính xác
				else
				{
					Assert.AreEqual(expectedValue, actualValue, DoubleTolerance, $"Phần tử thứ {i} không khớp (Tolerance: {DoubleTolerance}): {message}");
				}
			}
		}

		/// <summary>
		/// Phương thức tiện ích để tạo double ngẫu nhiên.
		/// </summary>
		private double GetRandomDouble()
		{
			// Random.NextDouble() trả về giá trị trong khoảng [0.0, 1.0)
			// Để bao quát dải giá trị của double, ta sử dụng bitwise ops hoặc kết hợp
			// Nhưng cách đơn giản và bao quát nhất là dùng GetBytes và ToDouble
			byte[] buf = new byte[8];
			_random.NextBytes(buf);
			double randomValue = BitConverter.ToDouble(buf, 0);

			// Đảm bảo không trả về NaN, Infinity ngẫu nhiên (trừ khi test đặc biệt)
			if (double.IsNaN(randomValue) || double.IsInfinity(randomValue))
			{
				return _random.NextDouble() * double.MaxValue; // Tạo giá trị hợp lệ
			}

			return randomValue;
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng double[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalDoubleArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: double[]
			double[] originalArray = new double[] { 1.234567890123456, -0.000000001, 987.654 };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng double[] điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullArray_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên và Đặc biệt (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (double[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			double[] originalArray = new double[0];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Min, Max, Zero).
		/// </summary>
		[TestMethod]
		public void WriteRead_DoubleEdgeValuesArray_ShouldBePreserved()
		{
			double[] originalArray = new double[] { double.MinValue, 0.0, double.MaxValue, -1.0 * DoubleTolerance };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Double.");
		}

		/// <summary>
		/// Test mảng chứa các giá trị đặc biệt (NaN, Infinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_DoubleSpecialValuesArray_ShouldBePreserved()
		{
			double[] originalArray = new double[]
			{
				double.NaN,
				double.PositiveInfinity,
				-1.0,
				double.NegativeInfinity,
				double.Epsilon // Giá trị rất nhỏ
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị đặc biệt (NaN, Infinity).");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị double ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomDoubleArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			double[] originalArray = new double[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				originalArray[i] = GetRandomDouble();
			}
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu Double.");
		}
	}
}
