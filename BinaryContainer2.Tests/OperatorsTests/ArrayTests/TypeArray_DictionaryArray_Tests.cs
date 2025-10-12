using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeArray_DictionaryArray_Tests // Test cho Dictionary<string, int>[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu Dictionary<string, int>[]
			_typeArrayOperator = new TypeArray(typeof(Dictionary<string, int>[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh Dictionary<string, int>[] ---
		private void AssertArrayOfDictionaryEqual(Dictionary<string, int>[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra (cấp 1) không được là NULL: {message}");
			Assert.IsInstanceOfType(actual, typeof(Dictionary<string, int>[]), "Kiểu đọc ra phải là Dictionary<string, int>[].");
			Dictionary<string, int>[] actualArray = (Dictionary<string, int>[])actual;

			Assert.AreEqual(expected.Length, actualArray.Length, $"Độ dài mảng ngoài không khớp: {message}");

			for (int i = 0; i < expected.Length; i++)
			{
				var expectedDict = expected[i];
				var actualDict = actualArray[i];

				if (expectedDict == null)
				{
					Assert.IsNull(actualDict, $"Phần tử thứ {i} phải là NULL: {message}");
					continue;
				}

				Assert.IsNotNull(actualDict, $"Phần tử thứ {i} không được là NULL: {message}");
				Assert.AreEqual(expectedDict.Count, actualDict.Count, $"Số lượng phần tử trong Dictionary thứ {i} không khớp.");

				foreach (var kvp in expectedDict)
				{
					Assert.IsTrue(actualDict.ContainsKey(kvp.Key), $"Dictionary thứ {i} thiếu Key: {kvp.Key}");
					Assert.AreEqual(kvp.Value, actualDict[kvp.Key], $"Giá trị của Key '{kvp.Key}' trong Dictionary thứ {i} không khớp.");
				}
			}
		}

		// --- Phương thức tiện ích để tạo Dictionary ngẫu nhiên ---
		private Dictionary<string, int> CreateRandomDictionary(int size)
		{
			var dict = new Dictionary<string, int>();
			for (int i = 0; i < size; i++)
			{
				string key = Guid.NewGuid().ToString("N");
				int value = _random.Next(int.MinValue, int.MaxValue);
				dict.Add(key, value);
			}
			return dict;
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một Dictionary<string, int>[] điển hình, không có null.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalArrayOfDictionary_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			Dictionary<string, int>[] originalArray = new Dictionary<string, int>[]
			{
				new Dictionary<string, int> { { "A", 1 }, { "B", 2 } },
				new Dictionary<string, int> { { "C", 30 }, { "D", -40 }, { "E", 50 } },
				new Dictionary<string, int> { { "Z", 100 } }
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
			AssertArrayOfDictionaryEqual(originalArray, readArray!, "Mảng Dictionary điển hình.");
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
		/// Giá trị biên: Mảng ngoài Rỗng (0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyOuterArray_ShouldReturnEmptyArray()
		{
			Dictionary<string, int>[] originalArray = new Dictionary<string, int>[0];
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
		/// Mảng chứa các Dictionary là NULL và Dictionary Rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithNullAndEmptyElements_ShouldBePreserved()
		{
			Dictionary<string, int>[] originalArray = new Dictionary<string, int>[]
			{
				CreateRandomDictionary(2),   // Dict 1: Hợp lệ
				null,                        // Dict 2: NULL
				new Dictionary<string, int>(), // Dict 3: Rỗng
				CreateRandomDictionary(1)    // Dict 4: Hợp lệ
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayOfDictionaryEqual(originalArray, readArray!, "Mảng chứa NULL và Dictionary rỗng.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các Dictionary có kích thước ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomArrayOfDictionaries_ShouldBeStable()
		{
			const int arrayLength = 100; // 100 phần tử trong mảng ngoài
			Dictionary<string, int>[] originalArray = new Dictionary<string, int>[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// 10% cơ hội là NULL
				if (_random.Next(10) == 0)
				{
					originalArray[i] = null;
					continue;
				}

				// Kích thước Dictionary từ 1 đến 10 phần tử
				int dictSize = _random.Next(1, 10);
				originalArray[i] = CreateRandomDictionary(dictSize);
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
			AssertArrayOfDictionaryEqual(originalArray, readArray!, $"Mảng Dictionary lớn ngẫu nhiên ({arrayLength} phần tử).");
		}
	}
}
