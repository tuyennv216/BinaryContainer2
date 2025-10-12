using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

// Định nghĩa kiểu dữ liệu đang được test: IEnumerable<Dictionary<MyKeyClass, MyValueClass>>
using DeepNestedType = System.Collections.Generic.IEnumerable<System.Collections.Generic.Dictionary<MyKeyClass, MyValueClass>>;
using InnerDictType = System.Collections.Generic.Dictionary<MyKeyClass, MyValueClass>;

// Định nghĩa các lớp tham chiếu
public class MyKeyClass
{
	public int KeyId { get; set; }
	public string KeyName { get; set; } = string.Empty;

	public override bool Equals(object? obj)
	{
		if (obj is not MyKeyClass other) return false;
		return KeyId == other.KeyId && KeyName == other.KeyName;
	}

	public override int GetHashCode() => HashCode.Combine(KeyId, KeyName);
}

public class MyValueClass
{
	public decimal Value { get; set; }
	public bool IsActive { get; set; }

	public override bool Equals(object? obj)
	{
		if (obj is not MyValueClass other) return false;
		return Value == other.Value && IsActive == other.IsActive;
	}

	public override int GetHashCode() => HashCode.Combine(Value, IsActive);
}

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeEnumerable_DeepNestedDict_Tests
	{
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeEnumerable với kiểu IEnumerable<Dictionary<MyKeyClass, MyValueClass>>
			_typeArrayOperator = new TypeEnumerable(typeof(DeepNestedType));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh IEnumerable<Dictionary<class, class>> ---
		private void AssertDeepNestedEqual(DeepNestedType expected, object actual, string message, bool checkIdentity = false)
		{
			Assert.IsNotNull(actual);
			Assert.IsInstanceOfType(actual, typeof(DeepNestedType), $"[L1] Kiểu đọc ra phải triển khai DeepNestedType: {message}");

			var expectedList = expected.ToList();
			var actualList = ((DeepNestedType)actual).ToList();

			Assert.AreEqual(expectedList.Count, actualList.Count, $"[L1] Độ dài Collection ngoài cùng không khớp: {message}");

			for (int i = 0; i < expectedList.Count; i++)
			{
				InnerDictType? expectedDict = expectedList[i];
				object? actualDictObject = actualList[i];

				if (expectedDict == null)
				{
					Assert.IsNull(actualDictObject, $"[L2] Dictionary thứ {i} phải là NULL: {message}");
					continue;
				}

				Assert.IsNotNull(actualDictObject, $"[L2] Dictionary thứ {i} không được là NULL: {message}");
				Assert.IsInstanceOfType(actualDictObject, typeof(InnerDictType), $"[L2] Kiểu phần tử {i} phải là Dictionary: {message}");
				InnerDictType actualDict = (InnerDictType)actualDictObject;

				// 1. Kiểm tra độ dài Dictionary
				Assert.AreEqual(expectedDict.Count, actualDict.Count, $"[L2] Độ dài Dictionary ({i}) không khớp: {message}");

				// 2. Lặp qua các cặp Key-Value
				foreach (var expectedPair in expectedDict)
				{
					// Phải tìm được Key trong Dictionary đọc ra
					MyValueClass? actualValue;
					Assert.IsTrue(actualDict.TryGetValue(expectedPair.Key, out actualValue!), $"[L3] Key không tìm thấy trong Dictionary đọc ra: {expectedPair.Key.KeyName}.");

					// 3. Kiểm tra Value (có thể là NULL)
					if (expectedPair.Value == null)
					{
						Assert.IsNull(actualValue, $"[L4] Value của Key '{expectedPair.Key.KeyName}' phải là NULL: {message}");
					}
					else
					{
						Assert.IsNotNull(actualValue, $"[L4] Value của Key '{expectedPair.Key.KeyName}' không được là NULL: {message}");
						// Kiểm tra giá trị bên trong đối tượng Value
						Assert.AreEqual(expectedPair.Value.Value, actualValue!.Value, $"[L5] Value property không khớp: {message}");
						Assert.AreEqual(expectedPair.Value.IsActive, actualValue.IsActive, $"[L5] IsActive property không khớp: {message}");
					}
				}

				// 4. Kiểm tra Identity (nếu được yêu cầu)
				if (checkIdentity)
				{
					// Lấy các tham chiếu cần kiểm tra
					var sharedKey = expectedDict.Keys.FirstOrDefault(k => expectedDict.Keys.Count(k2 => object.ReferenceEquals(k, k2)) > 1);
					var sharedValue = expectedDict.Values.FirstOrDefault(v => expectedDict.Values.Count(v2 => object.ReferenceEquals(v, v2)) > 1);

					if (sharedKey != null)
					{
						// Tìm tất cả các key trong dictionary đọc ra có cùng giá trị với sharedKey
						var readKeys = actualDict.Keys.Where(k => k.Equals(sharedKey)).ToArray();
						if (readKeys.Length > 1)
						{
							// Đảm bảo rằng các key có cùng giá trị ban đầu phải là cùng một instance sau khi đọc.
							Assert.IsTrue(object.ReferenceEquals(readKeys[0], readKeys[1]), $"[Identity] Key '{sharedKey.KeyName}' phải bảo toàn định danh.");
						}
					}
					// (Kiểm tra tương tự cho sharedValue có thể phức tạp hơn do Dictionary chỉ chứa 1 value duy nhất cho mỗi key,
					//  thường là kiểm tra value được chia sẻ giữa các Dictionary, nhưng ta sẽ tập trung vào tham chiếu trong 1 Dict)
				}
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case điển hình cho cấu trúc lồng nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalDeepNestedDict_ShouldPreserveAllValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			DeepNestedType originalData = new List<InnerDictType>
			{
				new InnerDictType
				{
					{ new MyKeyClass { KeyId = 1, KeyName = "A" }, new MyValueClass { Value = 10.5M, IsActive = true } },
					{ new MyKeyClass { KeyId = 2, KeyName = "B" }, new MyValueClass { Value = 20.0M, IsActive = false } }
				},
				new InnerDictType
				{
					{ new MyKeyClass { KeyId = 3, KeyName = "C" }, new MyValueClass { Value = 5.0M, IsActive = true } }
				}
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData, readData!, "Cấu trúc Dictionary lồng nhau điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Đặc biệt (Edge Cases)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp biên: IEnumerable ngoài cùng là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullOuterEnumerable_ShouldReturnNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNull(readData, "IEnumerable ngoài cùng là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: Dictionary bên trong NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_EnumerableWithNullInnerDict_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			DeepNestedType originalData = new List<InnerDictType?>
			{
				new InnerDictType { { new MyKeyClass { KeyId = 1 }, new MyValueClass { Value = 1 } } },
				null, // Dictionary giữa NULL
				new InnerDictType { { new MyKeyClass { KeyId = 2 }, new MyValueClass { Value = 2 } } }
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData!, readData!, "IEnumerable chứa Dictionary NULL.");
		}

		/// <summary>
		/// Trường hợp biên: Dictionary Rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyInnerDictionary_ShouldReturnEmptyDictionary()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			DeepNestedType originalData = new List<InnerDictType>
			{
				new InnerDictType(), // Dictionary rỗng
				new InnerDictType { { new MyKeyClass { KeyId = 1 }, new MyValueClass { Value = 1 } } }
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData, readData!, "Dictionary rỗng.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: Key hoặc Value là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_DictWithNullKeyOrValue_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var validKey = new MyKeyClass { KeyId = 1 };
			var validValue = new MyValueClass { Value = 1 };

			DeepNestedType originalData = new List<InnerDictType>
			{
				new InnerDictType
				{
					// { NULL Key, Valid Value } -> Sẽ ném ngoại lệ trong C# Dictionary, bỏ qua trường hợp này
					{ validKey, null! } // Key hợp lệ, Value NULL
				}
			};
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepNestedEqual(originalData, readData!, "Dictionary chứa Value NULL.");
		}

		/// <summary>
		/// Kiểm tra bảo toàn định danh tham chiếu (RefPool) cho Key và Value.
		/// Key được sử dụng lại trong cùng một Dictionary (dù không thể xảy ra trong Dictionary chuẩn, ta giả định là có lỗi trong quá trình tạo test data)
		/// hoặc Value được chia sẻ giữa các cặp Key khác nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_DictWithSharedReferences_ShouldPreserveIdentity()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var sharedKey = new MyKeyClass { KeyId = 50, KeyName = "SharedKey" };
			var sharedValue = new MyValueClass { Value = 500M };

			// Tạo Dictionary thứ nhất
			var dict1 = new InnerDictType
			{
				// Key A (độc lập), Value Shared
				{ new MyKeyClass { KeyId = 1, KeyName = "A" }, sharedValue },
				// Key Shared, Value B (độc lập)
				{ sharedKey, new MyValueClass { Value = 200M } }
			};

			// Tạo Dictionary thứ hai (sử dụng lại sharedValue và sharedKey)
			var dict2 = new InnerDictType
			{
				// Key Shared, Value Shared
				{ sharedKey, sharedValue }
			};

			DeepNestedType originalData = new List<InnerDictType> { dict1, dict2 };
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			var readList = ((DeepNestedType)readData!).ToList();
			InnerDictType readDict1 = readList[0];
			InnerDictType readDict2 = readList[1];

			// 1. Kiểm tra giá trị
			AssertDeepNestedEqual(originalData, readData!, "Dictionary chứa tham chiếu chia sẻ.");

			// 2. Kiểm tra định danh tham chiếu cho Key (sharedKey)
			MyValueClass? val1;
			MyValueClass? val2;
			readDict1.TryGetValue(sharedKey, out val1);
			readDict2.TryGetValue(sharedKey, out val2);

			// Kiểm tra Key instance có được bảo toàn giữa dict1 và dict2 không
			var readSharedKey1 = readDict1.Keys.First(k => k.KeyId == 50);
			var readSharedKey2 = readDict2.Keys.First(k => k.KeyId == 50);
			Assert.IsTrue(object.ReferenceEquals(readSharedKey1, readSharedKey2), "[Identity] Shared Key phải là cùng một instance giữa các Dictionary.");

			// 3. Kiểm tra định danh tham chiếu cho Value (sharedValue)
			// sharedValue được sử dụng trong dict1 (Key=1) và dict2
			MyValueClass? readSharedValue1;
			readDict1.TryGetValue(new MyKeyClass { KeyId = 1, KeyName = "A" }, out readSharedValue1);
			MyValueClass? readSharedValue2;
			readDict2.TryGetValue(sharedKey, out readSharedValue2);

			Assert.IsTrue(object.ReferenceEquals(readSharedValue1, readSharedValue2), "[Identity] Shared Value phải là cùng một instance giữa các Dictionary.");
		}
	}
}
