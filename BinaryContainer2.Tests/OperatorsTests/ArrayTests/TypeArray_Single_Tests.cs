using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Single_Tests // Test cho mảng float[] (Single)
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;
		private const float FloatTolerance = 1e-6f; // Độ chính xác cho so sánh float

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử float[]
			_typeArrayOperator = new TypeArray(typeof(float[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng float[] ---
		private void AssertArrayEqual(float[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(float[]), "Kiểu đọc ra phải là float[].");

			for (int i = 0; i < expected.Length; i++)
			{
				float expectedValue = expected[i];
				float actualValue = (float)actual.GetValue(i)!;

				// 1. Xử lý NaN (Not a Number) - NaN không bằng chính nó
				if (float.IsNaN(expectedValue))
				{
					Assert.IsTrue(float.IsNaN(actualValue), $"Phần tử thứ {i} phải là NaN: {message}");
				}
				// 2. Xử lý các giá trị đặc biệt khác (Infinity)
				else if (float.IsInfinity(expectedValue))
				{
					Assert.AreEqual(expectedValue, actualValue, $"Phần tử thứ {i} phải là giá trị Infinity tương ứng: {message}");
				}
				// 3. Xử lý các giá trị số thông thường
				else
				{
					// So sánh bằng độ chính xác (Tolerance)
					Assert.AreEqual(expectedValue, actualValue, FloatTolerance, $"Phần tử thứ {i} không khớp trong độ chính xác: {message}");
				}
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng float[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalFloatArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: float[]
			float[] originalArray = new float[] { 1.234f, -500.0f, 0.000123f, -9876.54f };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng float[] điển hình.");
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

		// --- II. Giá Trị Biên & Đặc Biệt (Edge Cases & Special Values) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (float[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			float[] originalArray = new float[0];
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
		/// Test mảng chứa các giá trị biên của kiểu phần tử (Min, Max, Zero, Epsilon).
		/// </summary>
		[TestMethod]
		public void WriteRead_FloatEdgeValuesArray_ShouldBePreserved()
		{
			float[] originalArray = new float[]
			{
				float.MinValue,
				float.MaxValue,
				0.0f,
				-0.0f, // Số 0 âm
				float.Epsilon, // Số float dương nhỏ nhất khác 0
				-float.Epsilon
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của Single (float).");
		}

		/// <summary>
		/// Test mảng chứa các giá trị đặc biệt (NaN, Infinity).
		/// </summary>
		[TestMethod]
		public void WriteRead_FloatSpecialValuesArray_ShouldBePreserved()
		{
			float[] originalArray = new float[]
			{
				float.NaN,
				float.PositiveInfinity,
				float.NegativeInfinity
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị NaN và Infinity.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomFloatArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			float[] originalArray = new float[arrayLength];

			// 1. Tạo mảng ngẫu nhiên (chỉ trong dải hữu hạn để tránh NaN/Inf ngẫu nhiên)
			for (int i = 0; i < arrayLength; i++)
			{
				// Tạo số ngẫu nhiên trong khoảng [-100000.0f, 100000.0f]
				float randomFloat = (float)(_random.NextDouble() * 200000.0 - 100000.0);
				originalArray[i] = randomFloat;
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu Single.");
		}
	}
}
