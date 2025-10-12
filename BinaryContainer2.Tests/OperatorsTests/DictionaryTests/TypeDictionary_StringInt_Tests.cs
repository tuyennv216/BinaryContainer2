using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeDictionary_StringInt_Tests // Test cho Dictionary<string, int>
	{
		private TypeDictionary _typeDictionaryOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeDictionaryOperator = new TypeDictionary(typeof(Dictionary<string, int>));
			_typeDictionaryOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh Dictionary<string, int> ---
		private void AssertDictionaryEqual(Dictionary<string, int> expected, object actual, string message)
		{
			Assert.IsNotNull(actual, $"Dictionary đọc ra không được là NULL: {message}");
			Assert.IsInstanceOfType(actual, typeof(Dictionary<string, int>), "Kiểu đọc ra phải là Dictionary<string, int>.");
			Dictionary<string, int> actualDict = (Dictionary<string, int>)actual;

			Assert.AreEqual(expected.Count, actualDict.Count, $"Số lượng phần tử không khớp: {message}");

			foreach (var kvp in expected)
			{
				Assert.IsTrue(actualDict.ContainsKey(kvp.Key), $"Thiếu Key: {kvp.Key}");
				Assert.AreEqual(kvp.Value, actualDict[kvp.Key], $"Giá trị của Key '{kvp.Key}' không khớp.");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một Dictionary điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalStringIntDictionary_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			var originalDict = new Dictionary<string, int>
			{
				{ "Item_1", 100 },
				{ "Item_2", -25 },
				{ "Item_3", 50000 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import (SỬ DỤNG _typeDictionaryOperator)
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read (SỬ DỤNG _typeDictionaryOperator)
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertDictionaryEqual(originalDict, readData!, "Dictionary<string, int> điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Dictionary là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullDictionary_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			// SỬ DỤNG _typeDictionaryOperator
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// SỬ DỤNG _typeDictionaryOperator
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Dictionary Rỗng (0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyDictionary_ShouldReturnEmptyDictionary()
		{
			var originalDict = new Dictionary<string, int>();
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// SỬ DỤNG _typeDictionaryOperator
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// SỬ DỤNG _typeDictionaryOperator
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary rỗng phải được đọc ra.");
		}

		/// <summary>
		/// Dictionary chứa Key là chuỗi rỗng ("") và các giá trị biên (Min/Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_EdgeKeysAndValues_ShouldBePreserved()
		{
			var originalDict = new Dictionary<string, int>
			{
				{ "", int.MaxValue },
				{ "ZeroValue", 0 },
				{ "Min_Val_Key", int.MinValue }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// SỬ DỤNG _typeDictionaryOperator
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// SỬ DỤNG _typeDictionaryOperator
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary chứa key rỗng và giá trị biên Int32.");
		}

		/// <summary>
		/// Dictionary chứa các key có ký tự Unicode (tiếng Việt).
		/// </summary>
		[TestMethod]
		public void WriteRead_UnicodeKeys_ShouldBePreserved()
		{
			var originalDict = new Dictionary<string, int>
			{
				{ "Sản Phẩm Tốt", 999 },
				{ "Giá Của Nó", 42 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// SỬ DỤNG _typeDictionaryOperator
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// SỬ DỤNG _typeDictionaryOperator
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary chứa key Unicode (tiếng Việt).");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một Dictionary lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomDictionary_ShouldBeStable()
		{
			const int dictSize = 1000;
			var originalDict = new Dictionary<string, int>();

			// 1. Tạo Dictionary ngẫu nhiên
			for (int i = 0; i < dictSize; i++)
			{
				// Key ngẫu nhiên: sử dụng Guid để đảm bảo tính duy nhất
				string key = Guid.NewGuid().ToString("N");
				// Value ngẫu nhiên: Int32 trong toàn dải
				int value = _random.Next(int.MinValue, int.MaxValue);

				originalDict.Add(key, value);
			}
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import (SỬ DỤNG _typeDictionaryOperator)
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read (SỬ DỤNG _typeDictionaryOperator)
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertDictionaryEqual(originalDict, readData!, $"Dictionary ngẫu nhiên lớn ({dictSize} phần tử).");
		}
	}
}