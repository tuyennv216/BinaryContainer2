using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	// Định nghĩa Enum mẫu để sử dụng trong Unit Test
	public enum TestStatusCode : int
	{
		None = 0,
		Success = 1,
		Pending = 50, // Giá trị tùy chỉnh
		Warning = 100,
		Error = 200,
		Fatal = 500
	}

	[TestClass]
	public class TypeArray_Enum_Tests // Test cho mảng Enum (TestStatusCode[])
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;
		private TestStatusCode[] _validEnumValues;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử TestStatusCode[]
			_typeArrayOperator = new TypeArray(typeof(TestStatusCode[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);

			// Lấy tất cả các giá trị enum hợp lệ để dùng cho stress test
			_validEnumValues = (TestStatusCode[])Enum.GetValues(typeof(TestStatusCode));
		}

		// --- Phương thức tiện ích để so sánh mảng ---
		private void AssertArrayEqual(TestStatusCode[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(TestStatusCode[]), "Kiểu đọc ra phải là TestStatusCode[].");
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual.GetValue(i), $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng enum[] điển hình chứa các giá trị hợp lệ.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalEnumArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			TestStatusCode[] originalArray = new TestStatusCode[]
			{
				TestStatusCode.Success,
				TestStatusCode.Pending,
				TestStatusCode.Error
			};
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
			AssertArrayEqual(originalArray, readArray!, "Mảng enum[] điển hình.");
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
		/// Giá trị biên: Mảng Rỗng (enum[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			TestStatusCode[] originalArray = new TestStatusCode[0];
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
		/// Test giá trị Zero/Default (None) của Enum.
		/// </summary>
		[TestMethod]
		public void WriteRead_EnumZeroValue_ShouldBePreserved()
		{
			TestStatusCode[] originalArray = new TestStatusCode[] { TestStatusCode.None, TestStatusCode.Success, TestStatusCode.None };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị Enum Zero (None).");
		}

		/// <summary>
		/// Test giá trị số nguyên không hợp lệ (không được định nghĩa trong Enum).
		/// Serialization phải bảo toàn giá trị số nguyên cơ sở.
		/// </summary>
		[TestMethod]
		public void WriteRead_EnumUnknownValue_ShouldBePreserved()
		{
			// Giá trị 999 không được định nghĩa trong TestStatusCode
			const int unknownValue = 999;
			TestStatusCode unknownStatus = (TestStatusCode)unknownValue;

			TestStatusCode[] originalArray = new TestStatusCode[] { TestStatusCode.Warning, unknownStatus, TestStatusCode.Fatal };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// Đảm bảo giá trị 999 được đọc ra chính xác
			Assert.AreEqual(unknownStatus, readArray!.GetValue(1), "Giá trị Enum không xác định (999) phải được bảo toàn.");
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị Enum không xác định.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị Enum ngẫu nhiên hợp lệ.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomEnumArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			TestStatusCode[] originalArray = new TestStatusCode[arrayLength];

			// 1. Tạo mảng ngẫu nhiên từ các giá trị enum hợp lệ
			for (int i = 0; i < arrayLength; i++)
			{
				int index = _random.Next(0, _validEnumValues.Length);
				originalArray[i] = _validEnumValues[index];
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng enum ngẫu nhiên lớn ({arrayLength} phần tử).");
		}
	}
}
