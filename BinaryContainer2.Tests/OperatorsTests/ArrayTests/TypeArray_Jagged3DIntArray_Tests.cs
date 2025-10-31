using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_Jagged3DIntArray_Tests // Test cho mảng int[][][]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử int[][][]
			_typeArrayOperator = new TypeArray(typeof(int[][][]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng int[][][] ---
		private void AssertJagged3DArrayEqual(int[][][] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra (cấp 1) không được là NULL: {message}");
			Assert.IsInstanceOfType(actual, typeof(int[][][]), "Kiểu đọc ra phải là int[][][].");
			int[][][] actualJagged = (int[][][])actual;

			Assert.AreEqual(expected.Length, actualJagged.Length, $"Độ dài mảng ngoài (cấp 1) không khớp: {message}");

			for (int i = 0; i < expected.Length; i++)
			{
				int[][] expectedMiddle = expected[i];
				int[][] actualMiddle = actualJagged[i];

				if (expectedMiddle == null)
				{
					Assert.IsNull(actualMiddle, $"Phần tử cấp 2 (thứ {i}) phải là NULL: {message}");
					continue;
				}

				Assert.IsNotNull(actualMiddle, $"Phần tử cấp 2 (thứ {i}) không được là NULL: {message}");
				Assert.AreEqual(expectedMiddle.Length, actualMiddle.Length, $"Độ dài mảng cấp 2 (thứ {i}) không khớp: {message}");

				for (int j = 0; j < expectedMiddle.Length; j++)
				{
					int[] expectedInner = expectedMiddle[j];
					int[] actualInner = actualMiddle[j];

					if (expectedInner == null)
					{
						Assert.IsNull(actualInner, $"Phần tử cấp 3 (thứ {i},{j}) phải là NULL: {message}");
						continue;
					}

					Assert.IsNotNull(actualInner, $"Phần tử cấp 3 (thứ {i},{j}) không được là NULL: {message}");
					CollectionAssert.AreEqual(expectedInner, actualInner, $"Mảng con cấp 3 (thứ {i},{j}) không khớp: {message}");
				}
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng int[][][] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalJagged3DIntArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: 2x?x?
			int[][][] originalArray = new int[][][]
			{
				// Cấp 1, phần tử 0 (2x?)
				new int[][]
				{
					new int[] { 1, 2 }, // Cấp 3: 2 phần tử
					new int[] { 30, 40, 50 } // Cấp 3: 3 phần tử
				},

				// Cấp 1, phần tử 1 (1x?)
				new int[][]
				{
					new int[] { 600 } // Cấp 3: 1 phần tử
				},

				// Cấp 1, phần tử 2 - Chứa NULL ở cấp 2
				null
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
			AssertJagged3DArrayEqual(originalArray, readArray!, "Mảng int[][][] điển hình.");
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
		/// Giá trị biên: Mảng ngoài Rỗng (int[0][][]).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyOuterArray_ShouldReturnEmptyArray()
		{
			int[][][] originalArray = new int[0][][];
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
		/// Giá trị biên: Mảng chứa các mảng con cấp 3 là NULL.
		/// </summary>
		[TestMethod]



		public void WriteRead_ArrayWithNullInnerArrays_ShouldPreserveNulls()
		{
			int[][][] originalArray = new int[][][]
			{
				new int[][] { new int[] { 1 }, null, new int[] { 2, 3 } }, // Có null cấp 3
				new int[][] { null, new int[0] } // Có null và mảng rỗng cấp 3
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertJagged3DArrayEqual(originalArray, readArray!, "Mảng chứa các mảng con cấp 3 là NULL.");
		}

		/// <summary>
		/// Giá trị biên: Mảng chứa các mảng con cấp 2 là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithNullMiddleArrays_ShouldPreserveNulls()
		{
			int[][][] originalArray = new int[][][]
			{
				null, // Null cấp 2
				new int[][] { new int[] { 10, 20 } }, // Mảng hợp lệ
				null, // Null cấp 2
				new int[][] { null } // Mảng hợp lệ nhưng có null cấp 3
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertJagged3DArrayEqual(originalArray, readArray!, "Mảng chứa các mảng con cấp 2 là NULL.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn ngẫu nhiên với cấu trúc 3D lởm chởm ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomJagged3DIntArray_ShouldBeStable()
		{
			const int outerLength = 50;
			int[][][] originalArray = new int[outerLength][][];

			// 1. Tạo mảng lồng 3D ngẫu nhiên
			for (int i = 0; i < outerLength; i++)
			{
				// Cấp 2: 10% cơ hội là null
				if (_random.Next(10) == 0)
				{
					originalArray[i] = null;
					continue;
				}

				int middleLength = _random.Next(1, 10);
				originalArray[i] = new int[middleLength][];

				for (int j = 0; j < middleLength; j++)
				{
					// Cấp 3: 10% cơ hội là null
					if (_random.Next(10) == 0)
					{
						originalArray[i][j] = null;
						continue;
					}

					int innerLength = _random.Next(1, 20);
					originalArray[i][j] = new int[innerLength];

					for (int k = 0; k < innerLength; k++)
					{
						originalArray[i][j][k] = _random.Next();
					}
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
			AssertJagged3DArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn 3D lởm chởm ({outerLength} phần tử cấp 1).");
		}
	}
}
