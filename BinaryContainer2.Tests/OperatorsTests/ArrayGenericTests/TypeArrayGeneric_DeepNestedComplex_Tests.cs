using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

// ---------------------------------------------
// Helper Classes
// ---------------------------------------------

/// <summary>
/// Lớp đối tượng tham chiếu được sử dụng làm phần tử mảng trong Dictionary Value.
/// </summary>
public class MyComplexValueClass
{
	public int Id { get; set; }
	public string Data { get; set; } = string.Empty;

	public override bool Equals(object? obj)
	{
		if (obj is not MyComplexValueClass other) return false;
		return Id == other.Id && Data == other.Data;
	}

	public override int GetHashCode() => HashCode.Combine(Id, Data);
}

namespace BinaryContainer2.Tests.OperatorsTests.ArrayGenericTests
{
	[TestClass]
	public class TypeArrayGeneric_DeepNestedComplex_Tests
	{
		// Target Type Alias: List<Dictionary<int[], MyComplexValueClass[]>>
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeArrayGeneric với kiểu lồng nhau phức tạp: List<Dictionary<int[], MyComplexValueClass[]>>
			var dictType = typeof(Dictionary<int[], MyComplexValueClass[]>);
			_typeArrayOperator = new TypeArrayGeneric(typeof(List<>).MakeGenericType(dictType));
			_typeArrayOperator.Build();
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để tạo dữ liệu kiểm tra ---
		private List<Dictionary<int[], MyComplexValueClass[]>> GetTestInstance(bool includeRefs = false)
		{
			var sharedKey = new int[] { 1, 1, 1 };
			var sharedValue = new MyComplexValueClass[] { new MyComplexValueClass { Id = 100, Data = "Shared Ref" } };

			var dict1 = new Dictionary<int[], MyComplexValueClass[]>(new ArrayComparer())
			{
				{ new int[] { 1, 2 }, new MyComplexValueClass[] { new MyComplexValueClass { Id = 1, Data = "V1" } } },
				{ new int[] { 3, 4 }, sharedValue } // Shared Value
			};

			var dict2 = new Dictionary<int[], MyComplexValueClass[]>(new ArrayComparer())
			{
				{ sharedKey, new MyComplexValueClass[] { new MyComplexValueClass { Id = 2, Data = "V2" } } }, // Shared Key
				{ new int[] { 5 }, new MyComplexValueClass[] { new MyComplexValueClass { Id = 3, Data = "V3" } } }
			};

			var list = new List<Dictionary<int[], MyComplexValueClass[]>> { dict1, dict2 };

			if (includeRefs)
			{
				// Thêm lại dict1 để kiểm tra tham chiếu List
				list.Add(dict1);
			}

			return list;
		}

		// --- Helper để so sánh mảng int (Key) ---
		private class ArrayComparer : IEqualityComparer<int[]>
		{
			public bool Equals(int[]? x, int[]? y)
			{
				if (x == y) return true;
				if (x == null || y == null) return false;
				return x.SequenceEqual(y);
			}
			public int GetHashCode(int[] obj)
			{
				if (obj == null) return 0;
				return obj.Aggregate(17, (acc, val) => acc * 23 + val.GetHashCode());
			}
		}

		// --- Phương thức tiện ích để so sánh sâu cấu trúc lồng nhau ---
		private void AssertDeepComplexEqual(List<Dictionary<int[], MyComplexValueClass[]>> expected, object actual, string message, bool checkIdentity = false)
		{
			Assert.IsNotNull(actual);
			Assert.IsInstanceOfType(actual, typeof(List<Dictionary<int[], MyComplexValueClass[]>>), $"[L0 Type] Kiểu đọc ra không khớp: {message}");

			var actualList = (List<Dictionary<int[], MyComplexValueClass[]>>)actual;
			Assert.AreEqual(expected.Count, actualList.Count, $"[L1 List Count] Số lượng Dictionary không khớp: {message}");

			for (int i = 0; i < expected.Count; i++)
			{
				var expDict = expected[i];
				var actDict = actualList[i];

				Assert.AreEqual(expDict.Count, actDict.Count, $"[L2 Dict Count] Số lượng cặp Key-Value trong Dictionary {i} không khớp: {message}");

				// Kiểm tra Identity (tham chiếu Dict lặp lại)
				if (checkIdentity && i > 0 && ReferenceEquals(expected[i], expected[i - 1]))
				{
					Assert.IsTrue(ReferenceEquals(actDict, actualList[i - 1]), $"[L1 Identity] Tham chiếu Dictionary tại index {i} phải được bảo toàn.");
				}

				foreach (var expPair in expDict)
				{
					// 1. Tìm Key trong Dict đọc ra (sử dụng ArrayComparer)
					var actualPair = actDict.FirstOrDefault(p => new ArrayComparer().Equals(p.Key, expPair.Key));
					Assert.IsNotNull(actualPair.Key, $"[L3 Key] Không tìm thấy Key: {string.Join(",", expPair.Key!)} trong Dictionary {i}. {message}");

					var expValue = expPair.Value;
					var actValue = actualPair.Value;

					// 2. Kiểm tra Mảng Value
					CollectionAssert.AreEqual(expValue, actValue, $"[L3 Value Array Length] Độ dài mảng Value không khớp: {message}");

					// 3. Kiểm tra các phần tử đối tượng trong Mảng Value
					for (int j = 0; j < expValue.Length; j++)
					{
						Assert.AreEqual(expValue[j], actValue[j], $"[L4 Value Item] Phần tử Value {j} không khớp: {message}");
					}
				}
			}
		}

		// ---------------------------------------------
		// Test Cases
		// ---------------------------------------------

		/// <summary>
		/// Test case điển hình cho cấu trúc lồng nhau phức tạp.
		/// </summary>
		[TestMethod]
		public void WriteRead_DeepComplexNested_ShouldPreserveAllData()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = GetTestInstance();
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertDeepComplexEqual(originalData, readData!, "Cấu trúc lồng nhau phức tạp điển hình.");
		}

		/// <summary>
		/// Test case kiểm tra bảo toàn định danh tham chiếu (RefPool) cho Dictionary
		/// khi cùng một Dictionary được đưa vào List nhiều lần.
		/// </summary>
		[TestMethod]
		public void WriteRead_ListWithSharedDictionaryRef_ShouldPreserveIdentity()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = GetTestInstance(includeRefs: true); // dict1 được thêm 2 lần
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			var readList = (List<Dictionary<int[], MyComplexValueClass[]>>)readData!;

			// 1. Kiểm tra giá trị
			AssertDeepComplexEqual(originalData, readData!, "Bảo toàn tham chiếu Dictionary.", checkIdentity: true);

			// 2. Kiểm tra Identity trực tiếp (readList[0] và readList[2] phải là cùng một instance)
			Assert.IsTrue(ReferenceEquals(readList[0], readList[2]), "RefPool phải bảo toàn định danh tham chiếu cho Dictionary lặp lại.");
			Assert.IsFalse(ReferenceEquals(readList[0], readList[1]), "Các Dictionary khác nhau không được tham chiếu cùng một instance.");
		}
	}
}
