using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

// ---------------------------------------------
// Định nghĩa cấu trúc lớp 1 cấp độ
// ---------------------------------------------

/// <summary>
/// Lớp 1 cấp độ: Chứa các kiểu Array và Generic Collection được yêu cầu.
/// </summary>
public class SingleLevelCollectionClass
{
	public string Name { get; set; } = string.Empty;

	// 1. Array (TypeArray)
	public int[] ArrayProp { get; set; } = Array.Empty<int>();

	// 2. Generic Collections (TypeArrayGeneric)
	public List<int> ListProp { get; set; } = new List<int>();
	public Queue<int> QueueProp { get; set; } = new Queue<int>();
	public Stack<int> StackProp { get; set; } = new Stack<int>();
	public HashSet<int> HashSetProp { get; set; } = new HashSet<int>();
	public LinkedList<int> LinkedListProp { get; set; } = new LinkedList<int>();

	public static SingleLevelCollectionClass GetTestInstance(bool includeEmptyCollections = false)
	{
		var data = new SingleLevelCollectionClass
		{
			Name = "Single Level Object - V1",
			ArrayProp = includeEmptyCollections ? Array.Empty<int>() : new int[] { 10, 20, 30 },
			ListProp = includeEmptyCollections ? new List<int>() : new List<int> { 1, 2, 3 },
			QueueProp = includeEmptyCollections ? new Queue<int>() : new Queue<int>(new[] { 4, 5, 52 }),
			StackProp = includeEmptyCollections ? new Stack<int>() : new Stack<int>(new[] { 6, 7, 72 }),
			HashSetProp = includeEmptyCollections ? new HashSet<int>() : new HashSet<int> { 8, 9, 10 },
			LinkedListProp = includeEmptyCollections ? new LinkedList<int>() : new LinkedList<int>(new[] { 11, 12 })
		};
		return data;
	}
}

namespace BinaryContainer2.Tests.ClassTests
{
	[TestClass]
	public class TypeClass_SingleLevelCollections_Tests
	{
		private ITypeOperator _typeClassOperator;

		[TestInitialize]
		public void Setup()
		{
			// Giả định operator cho lớp SingleLevelCollectionClass là TypeClass
			_typeClassOperator = new TypeClass(typeof(SingleLevelCollectionClass));
			_typeClassOperator.Build();
		}

		// --- Phương thức tiện ích để so sánh các thuộc tính Collection/Array ---
		private void AssertCollectionsEqual(SingleLevelCollectionClass expected, SingleLevelCollectionClass actual, string message)
		{
			Assert.IsNotNull(actual);

			// Kiểm tra thuộc tính đơn giản
			Assert.AreEqual(expected.Name, actual.Name, $"[Name] Thuộc tính Name không khớp: {message}");

			// Kiểm tra Array
			CollectionAssert.AreEqual(expected.ArrayProp, actual.ArrayProp, $"[ArrayProp] Thuộc tính ArrayProp không khớp: {message}");

			// Kiểm tra Generic Collections
			CollectionAssert.AreEqual(expected.ListProp, actual.ListProp, $"[ListProp] Thuộc tính ListProp không khớp: {message}");
			CollectionAssert.AreEqual(expected.QueueProp.ToArray(), actual.QueueProp.ToArray(), $"[QueueProp] Thuộc tính QueueProp không khớp: {message}");
			CollectionAssert.AreEqual(expected.StackProp.ToArray(), actual.StackProp.ToArray(), $"[StackProp] Thuộc tính StackProp không khớp: {message}");
			CollectionAssert.AreEquivalent(expected.HashSetProp.ToArray(), actual.HashSetProp.ToArray(), $"[HashSetProp] Thuộc tính HashSetProp không khớp: {message}");
			CollectionAssert.AreEqual(expected.LinkedListProp.ToArray(), actual.LinkedListProp.ToArray(), $"[LinkedListProp] Thuộc tính LinkedListProp không khớp: {message}");
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case điển hình cho cấu trúc 1 cấp độ.
		/// </summary>
		[TestMethod]
		public void WriteRead_SingleLevelCollections_ShouldPreserveAll()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = SingleLevelCollectionClass.GetTestInstance();
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			var readData = (SingleLevelCollectionClass)_typeClassOperator.Read(newContainer, readPool)!;

			AssertCollectionsEqual(originalData, readData, "Cấu trúc 1 cấp độ điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên
		// ---------------------------------------------

		/// <summary>
		/// Test case khi tất cả các Collection và Array đều rỗng.
		/// </summary>
		[TestMethod]
		public void WriteRead_AllCollectionsEmpty_ShouldPreserveEmptyState()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = SingleLevelCollectionClass.GetTestInstance(includeEmptyCollections: true);
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			var readData = (SingleLevelCollectionClass)_typeClassOperator.Read(newContainer, readPool)!;

			AssertCollectionsEqual(originalData, readData, "Tất cả Collections đều rỗng.");
		}

		/// <summary>
		/// Trường hợp biên: Đối tượng lớp cấp 1 là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_RootObjectNull_ShouldReturnNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeClassOperator.Read(newContainer, readPool);

			Assert.IsNull(readData, "Đối tượng gốc phải là NULL.");
		}
	}
}
