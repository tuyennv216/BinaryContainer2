using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_JaggedIntArray_Tests // Test cho mảng int[][]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử int[][]
			_typeArrayOperator = new TypeArray(typeof(int[][]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng int[][] ---
		private void AssertJaggedArrayEqual(int[][] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.IsInstanceOfType(actual, typeof(int[][]), "Kiểu đọc ra phải là int[][].");
			int[][] actualJagged = (int[][])actual;

			Assert.AreEqual(expected.Length, actualJagged.Length, $"Độ dài mảng ngoài không khớp: {message}");

			for (int i = 0; i < expected.Length; i++)
			{
				int[] expectedInner = expected[i];
				int[] actualInner = actualJagged[i];

				if (expectedInner == null)
				{
					Assert.IsNull(actualInner, $"Phần tử thứ {i} phải là NULL: {message}");
				}
				else
				{
					Assert.IsNotNull(actualInner, $"Phần tử thứ {i} không được là NULL: {message}");
					CollectionAssert.AreEqual(expectedInner, actualInner, $"Mảng con thứ {i} không khớp: {message}");
				}
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng int[][] điển hình với các mảng con có độ dài khác nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalJaggedIntArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			int[][] originalArray = new int[][]
			{
				new int[] { 1, 2, 3 },
				new int[] { 40, -50 },
				new int[] { 600, 700, 800, 900 }
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
			AssertJaggedArrayEqual(originalArray, readArray!, "Mảng int[][] điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng ngoài là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullOuterArray_ShouldReturnNull()
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
		/// Giá trị biên: Mảng ngoài Rỗng (int[][] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyOuterArray_ShouldReturnEmptyArray()
		{
			int[][] originalArray = new int[0][];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng ngoài rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		/// <summary>
		/// Giá trị biên: Mảng chứa các mảng con là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithInnerNullArrays_ShouldPreserveNulls()
		{
			int[][] originalArray = new int[][] { new int[] { 10 }, null, new int[] { 20, 30 }, null, null };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertJaggedArrayEqual(originalArray, readArray!, "Mảng chứa các mảng con là NULL.");
		}

		/// <summary>
		/// Giá trị biên: Mảng chứa các mảng con Rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithInnerEmptyArrays_ShouldPreserveEmptyArrays()
		{
			int[][] originalArray = new int[][] { new int[0], new int[] { 5 }, new int[0], new int[] { 10, 20 } };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertJaggedArrayEqual(originalArray, readArray!, "Mảng chứa các mảng con Rỗng.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn ngẫu nhiên với kích thước và nội dung mảng con ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomJaggedIntArray_ShouldBeStable()
		{
			const int outerLength = 100;
			int[][] originalArray = new int[outerLength][];

			// 1. Tạo mảng lồng ngẫu nhiên
			for (int i = 0; i < outerLength; i++)
			{
				// 10% cơ hội mảng con là null
				if (_random.Next(10) == 0)
				{
					originalArray[i] = null;
					continue;
				}

				// Chiều dài mảng con ngẫu nhiên (từ 0 đến 50)
				int innerLength = _random.Next(51);
				originalArray[i] = new int[innerLength];

				for (int j = 0; j < innerLength; j++)
				{
					originalArray[i][j] = _random.Next();
				}
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
			AssertJaggedArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({outerLength} phần tử) kiểu int[][].");
		}
	}
}
