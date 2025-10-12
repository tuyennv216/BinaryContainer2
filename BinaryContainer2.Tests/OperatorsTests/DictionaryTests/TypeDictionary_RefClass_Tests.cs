using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	// --- Các Lớp Hỗ Trợ (MUST override Equals và GetHashCode cho Key Class) ---

	/// <summary>
	/// Class được sử dụng làm Key trong Dictionary (Reference Type).
	/// Phải override Equals và GetHashCode để Dictionary hoạt động đúng.
	/// </summary>
	public class TestKeyClass
	{
		public int Id { get; set; }
		public string KeyName { get; set; } = string.Empty;
		public double Version { get; set; }

		public override bool Equals(object? obj)
		{
			// Kiểm tra nếu đối tượng khác là TestKeyClass và tất cả các thuộc tính khớp
			return obj is TestKeyClass other &&
				   Id == other.Id &&
				   KeyName == other.KeyName &&
				   Version == other.Version;
		}

		public override int GetHashCode()
		{
			// Tạo mã hash dựa trên các thuộc tính dùng để so sánh
			return HashCode.Combine(Id, KeyName, Version);
		}
	}

	/// <summary>
	/// Class được sử dụng làm Value trong Dictionary (Reference Type).
	/// </summary>
	public class TestValueClass
	{
		public float Value { get; set; }
		public bool IsActive { get; set; }
		public string Description { get; set; } = string.Empty;

		// Phương thức hỗ trợ so sánh giá trị
		public bool ValueEquals(TestValueClass? other)
		{
			if (other == null) return false;
			return Math.Abs(Value - other.Value) < 0.001f &&
				   IsActive == other.IsActive &&
				   Description == other.Description;
		}
	}

	// --- Lớp Test Chính ---

	[TestClass]
	public class TypeDictionary_RefClass_Tests // Test cho Dictionary<TestKeyClass, TestValueClass>
	{
		private TypeDictionary _typeDictionaryOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeDictionary cho kiểu Dictionary<TestKeyClass, TestValueClass>
			_typeDictionaryOperator = new TypeDictionary(typeof(Dictionary<TestKeyClass, TestValueClass>));
			_typeDictionaryOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh Dictionary<TestKeyClass, TestValueClass> ---
		private void AssertDictionaryEqual(Dictionary<TestKeyClass, TestValueClass> expected, object actual, string message)
		{
			Assert.IsNotNull(actual, $"Dictionary đọc ra không được là NULL: {message}");
			Assert.IsInstanceOfType(actual, typeof(Dictionary<TestKeyClass, TestValueClass>), "Kiểu đọc ra phải là Dictionary<TestKeyClass, TestValueClass>.");
			Dictionary<TestKeyClass, TestValueClass> actualDict = (Dictionary<TestKeyClass, TestValueClass>)actual;

			Assert.AreEqual(expected.Count, actualDict.Count, $"Số lượng phần tử không khớp: {message}");

			foreach (var expectedKvp in expected)
			{
				// Tìm Key khớp trong Dictionary đọc ra. Phụ thuộc vào implement của TestKeyClass.Equals()
				var actualKey = actualDict.Keys.FirstOrDefault(k => k.Equals(expectedKvp.Key));
				Assert.IsNotNull(actualKey, $"Thiếu Key khớp: KeyName='{expectedKvp.Key.KeyName}' (Id: {expectedKvp.Key.Id})");

				// So sánh Value
				var expectedValue = expectedKvp.Value;
				var actualValue = actualDict[actualKey];

				Assert.IsTrue(expectedValue.ValueEquals(actualValue),
							  $"Giá trị (Value) của Key '{actualKey.KeyName}' không khớp.");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một Dictionary điển hình chứa các lớp tùy chỉnh.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalRefClassDictionary_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			var key1 = new TestKeyClass { Id = 1, KeyName = "Config_A", Version = 1.0 };
			var key2 = new TestKeyClass { Id = 2, KeyName = "Config_B", Version = 1.5 };
			var key3 = new TestKeyClass { Id = 3, KeyName = "Config_C", Version = 0.9 };

			var value1 = new TestValueClass { Value = 100.5f, IsActive = true, Description = "High priority item." };
			var value2 = new TestValueClass { Value = -25.0f, IsActive = false, Description = "Low priority item." };
			var value3 = new TestValueClass { Value = 50000.7f, IsActive = true, Description = "Max budget item." };

			var originalDict = new Dictionary<TestKeyClass, TestValueClass>
			{
				{ key1, value1 },
				{ key2, value2 },
				{ key3, value3 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertDictionaryEqual(originalDict, readData!, "Dictionary<RefClass, RefClass> điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Dictionary là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullDictionary_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

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
			var originalDict = new Dictionary<TestKeyClass, TestValueClass>();
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary rỗng phải được đọc ra.");
		}

		/// <summary>
		/// Dictionary chứa các key/value class với các thuộc tính là giá trị biên (chuỗi rỗng, 0, false).
		/// </summary>
		[TestMethod]
		public void WriteRead_EdgeKeysAndValues_ShouldBePreserved()
		{
			var key1 = new TestKeyClass { Id = 0, KeyName = "", Version = 0.0 };
			var key2 = new TestKeyClass { Id = -1, KeyName = "Negative", Version = double.MaxValue };

			var value1 = new TestValueClass { Value = 0f, IsActive = false, Description = "" };
			var value2 = new TestValueClass { Value = float.MinValue, IsActive = true, Description = "Min Float Value" };

			var originalDict = new Dictionary<TestKeyClass, TestValueClass>
			{
				{ key1, value1 },
				{ key2, value2 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary chứa key/value class với thuộc tính biên.");
		}

		/// <summary>
		/// Dictionary chứa các key/value class có thuộc tính là chuỗi Unicode (tiếng Việt).
		/// </summary>
		[TestMethod]
		public void WriteRead_UnicodeStringProperties_ShouldBePreserved()
		{
			var key1 = new TestKeyClass { Id = 10, KeyName = "Sản Phẩm Tốt", Version = 9.9 };
			var value1 = new TestValueClass { Value = 1000f, IsActive = true, Description = "Mô tả bằng tiếng Việt." };

			var originalDict = new Dictionary<TestKeyClass, TestValueClass>
			{
				{ key1, value1 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			AssertDictionaryEqual(originalDict, readData!, "Dictionary chứa thuộc tính chuỗi Unicode.");
		}

		/// <summary>
		/// Kiểm tra Reference Tracking: Đảm bảo nếu một đối tượng được dùng nhiều lần,
		/// nó chỉ được serialize 1 lần và đọc ra vẫn là cùng một đối tượng (hoặc các bản sao độc lập).
		/// *Quan trọng*: Đối với Dictionary, các khóa và giá trị thường được coi là các bản sao dữ liệu.
		/// Tuy nhiên, chúng ta kiểm tra tính toàn vẹn của dữ liệu của Reference Types.
		/// </summary>
		[TestMethod]
		public void WriteRead_SameObjectReference_ShouldPreserveDataIntegrity()
		{
			// Key được sử dụng 1 lần (key1)
			var key1 = new TestKeyClass { Id = 1, KeyName = "UniqueKey", Version = 1.0 };

			// Value được sử dụng 2 lần (value1)
			var value1 = new TestValueClass { Value = 42.0f, IsActive = true, Description = "Shared Value" };

			var originalDict = new Dictionary<TestKeyClass, TestValueClass>
			{
				{ key1, value1 },
				// Tạo một Key khác nhưng gắn cùng một Value object
				{ new TestKeyClass { Id = 2, KeyName = "OtherKey", Version = 2.0 }, value1 }
			};
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import (RefPool sẽ kiểm tra value1 đã được ghi chưa)
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);
			var readDict = (Dictionary<TestKeyClass, TestValueClass>)readData!;

			// 4. Kiểm tra
			AssertDictionaryEqual(originalDict, readData!, "Dictionary với cùng một object value được dùng lại.");

			// *Kiểm tra bổ sung (tính năng của BinaryContainer/RefPool)*: 
			// Sau khi đọc ra, hai giá trị tương ứng với hai key khác nhau
			// phải là các đối tượng khác nhau (hoặc cùng một đối tượng nếu Reference Tracking được kích hoạt).
			// Giả sử RefPool làm việc đúng, các bản sao phải được tạo, nhưng ta chỉ kiểm tra data integrity.
			var readKey1 = readDict.Keys.First(k => k.KeyName == "UniqueKey");
			var readKey2 = readDict.Keys.First(k => k.KeyName == "OtherKey");

			var readValue1 = readDict[readKey1];
			var readValue2 = readDict[readKey2];

			// Kiểm tra xem dữ liệu đọc ra của cả hai value có khớp không
			Assert.IsTrue(value1.ValueEquals(readValue1), "Giá trị đọc ra của value 1 bị sai.");
			Assert.IsTrue(value1.ValueEquals(readValue2), "Giá trị đọc ra của value 2 bị sai.");

			// Kiểm tra xem chúng có phải là cùng một instance object không.
			// Nếu BinaryContainer2 hỗ trợ Reference Tracking, Assert.AreSame sẽ pass.
			// Nếu nó chỉ hỗ trợ Serialization, Assert.AreSame sẽ fail, nhưng Assert.AreNotSame cũng không bắt buộc.
			// Ở đây ta chỉ tập trung vào việc dữ liệu đã được bảo toàn.
		}


		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một Dictionary lớn với các đối tượng ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomDictionary_ShouldBeStable()
		{
			const int dictSize = 500;
			var originalDict = new Dictionary<TestKeyClass, TestValueClass>();

			// 1. Tạo Dictionary ngẫu nhiên
			for (int i = 0; i < dictSize; i++)
			{
				var key = new TestKeyClass
				{
					Id = i,
					KeyName = Guid.NewGuid().ToString("N"),
					Version = _random.NextDouble() * 100
				};

				var value = new TestValueClass
				{
					Value = (float)(_random.NextDouble() * 1000000),
					IsActive = _random.Next(0, 2) == 1,
					Description = $"Random Item {i}"
				};

				originalDict.Add(key, value);
			}
			object originalData = originalDict;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeDictionaryOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			object? readData = _typeDictionaryOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertDictionaryEqual(originalDict, readData!, $"Dictionary ngẫu nhiên lớn ({dictSize} phần tử).");
		}
	}
}
