using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Boolean_Tests // Test cho mảng bool[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử bool[]
			_typeArrayOperator = new TypeArray(typeof(bool[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng bool ---
		private void AssertArrayEqual(bool[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(bool[]), "Kiểu đọc ra phải là bool[].");

			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual.GetValue(i), $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		/// <summary>
		/// Phương thức tiện ích để tạo giá trị bool ngẫu nhiên.
		/// </summary>
		private bool GetRandomBool()
		{
			// random.Next(2) trả về 0 hoặc 1.
			return _random.Next(2) == 1;
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng bool[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalBooleanArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: bool[] với sự xen kẽ giữa true/false
			bool[] originalArray = new bool[] { true, false, true, true, false };
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
			AssertArrayEqual(originalArray, readArray!, "Mảng bool[] điển hình.");
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

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (bool[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			bool[] originalArray = new bool[0];
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
		/// Test mảng chỉ chứa TRUE.
		/// </summary>
		[TestMethod]
		public void WriteRead_AllTrueArray_ShouldBePreserved()
		{
			bool[] originalArray = new bool[] { true, true, true, true };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chỉ chứa TRUE.");
		}

		/// <summary>
		/// Test mảng chỉ chứa FALSE.
		/// </summary>
		[TestMethod]
		public void WriteRead_AllFalseArray_ShouldBePreserved()
		{
			bool[] originalArray = new bool[] { false, false, false };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chỉ chứa FALSE.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị bool ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomBooleanArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			bool[] originalArray = new bool[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				originalArray[i] = GetRandomBool();
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
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu Boolean.");
		}
	}
}
