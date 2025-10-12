using BinaryContainer2.Converter;

namespace BinaryContainer2.Tests.ConverterTests
{
	/// <summary>
	/// Mock for a complex custom object.
	/// </summary>
	public class TestObject
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public TestObject? Reference { get; set; } // For self-referencing tests
	}

	/// <summary>
	/// L1: Class with deep collection nesting (5 levels counting primitives/fields)
	/// Root -> List (L2) -> Dictionary (L3) -> TestObject (L4) -> int/string/Reference (L5)
	/// </summary>
	public class ComplexTestObject
	{
		public int RootId { get; set; } // L2: Simple primitive
		public string Description { get; set; } = string.Empty; // L2: Simple primitive
		public List<Dictionary<string, TestObject>> NestedStructure { get; set; } = new List<Dictionary<string, TestObject>>();
	}

	[TestClass]
	public class BinConverterTests
	{
		[TestMethod] // Thay thế [Test]
		[Description("Case: Simple primitive integer serialization and deserialization.")]
		public void GetItem_Int_ReturnsOriginalValue()
		{
			int original = 12345;
			byte[] bytes = BinConverter.GetBytes(original);
			int? result = BinConverter.GetItem<int>(bytes);

			Assert.AreEqual(original, result);
		}

		[TestMethod]
		[Description("Case: String serialization, including non-ASCII characters.")]
		public void GetItem_String_ReturnsOriginalValue()
		{
			string original = "Xin chào Việt Nam!";
			byte[] bytes = BinConverter.GetBytes(original);
			string? result = BinConverter.GetItem<string>(bytes);

			Assert.AreEqual(original, result);
		}

		[TestMethod]
		[Description("Case: Null string input (edge case).")]
		public void GetItem_NullString_ReturnsNull()
		{
			string? original = null;
			byte[] bytes = BinConverter.GetBytes(original);
			string? result = BinConverter.GetItem<string>(bytes);

			Assert.IsNull(result);
		}

		[TestMethod]
		[Description("Case: Simple complex object serialization.")]
		public void GetItem_TestObject_ReturnsEquivalentObject()
		{
			var original = new TestObject { Id = 50, Name = "Container Test" };
			byte[] bytes = BinConverter.GetBytes(original);
			TestObject? result = BinConverter.GetItem<TestObject>(bytes);

			Assert.IsNotNull(result);
									  // Thay thế Assert.Multiple bằng các Assert riêng lẻ
			Assert.AreEqual(original.Id, result!.Id);
			Assert.AreEqual(original.Name, result.Name);
		}

		[TestMethod]
		[Description("Case: Confirms object identity is NOT preserved (as per user's observation about separate RefPools).")]
		public void GetItem_TestObject_IdentityIsNotPreserved()
		{
			var original = new TestObject { Id = 1, Name = "Object A" };
			byte[] bytes = BinConverter.GetBytes(original);
			TestObject? result = BinConverter.GetItem<TestObject>(bytes);

			Assert.IsNotNull(result);
									  // Crucial test: The deserialized object must be a new instance in memory.
			Assert.AreNotSame(original, result, "Deserialized object should be a new memory instance.");
		}

		[TestMethod]
		[Description("Case: Nested object serialization and loss of reference identity.")]
		public void GetItem_NestedObject_ReferenceIdentityIsNotPreserved()
		{
			var child = new TestObject { Id = 100, Name = "Shared Child" };
			var original = new TestObject
			{
				Id = 1,
				Name = "Parent",
				Reference = child
			};

			byte[] bytes = BinConverter.GetBytes(original);
			TestObject? result = BinConverter.GetItem<TestObject>(bytes);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result!.Reference);

			// Confirm reference identity is lost
			Assert.AreNotSame(original.Reference, result.Reference, "Nested reference object should be a new instance.");

			// Confirm values are correct
			Assert.AreEqual(child.Id, result.Reference!.Id);
		}


		[TestMethod]
		[Description("Case: Circular reference (A -> B -> A)")]
		public void GetItem_CircularReference_StructureIsClonedButIdentityLost()
		{
			// Setup circular reference: A -> B -> A
			var originalA = new TestObject { Id = 10, Name = "Node A" };
			var originalB = new TestObject { Id = 20, Name = "Node B" };

			originalA.Reference = originalB;
			originalB.Reference = originalA; // The circle is closed

			// Serialize and Deserialize
			byte[] bytes = BinConverter.GetBytes(originalA);
			TestObject? resultA = BinConverter.GetItem<TestObject>(bytes);

			Assert.IsNotNull(resultA);

			// First level check (A')
			Assert.AreNotSame(originalA, resultA, "A' must be a new instance.");
			Assert.AreEqual(originalA.Id, resultA!.Id);

			// Second level check (B')
			var resultB = resultA.Reference;
			Assert.IsNotNull(resultB);
			Assert.AreNotSame(originalB, resultB, "B' must be a new instance.");
			Assert.AreEqual(originalB.Id, resultB!.Id);

			// Third level check (A's clone, A'')
			var resultA_Cloned = resultB.Reference;
			Assert.IsNotNull(resultA_Cloned);
			Assert.AreSame(resultA, resultA_Cloned, "A must be same.");
			Assert.AreEqual(originalA.Id, resultA_Cloned!.Id);
		}

		[TestMethod]
		[Description("Case: Consecutive nodes (linked list: N1 -> N2 -> N3). Checks chain integrity and values.")]
		public void GetItem_ConsecutiveNodes_ReturnsValidChain()
		{
			// Setup linked list: N1 -> N2 -> N3
			var n3 = new TestObject { Id = 3, Name = "Node 3 (Tail)" };
			var n2 = new TestObject { Id = 2, Name = "Node 2", Reference = n3 };
			var n1 = new TestObject { Id = 1, Name = "Node 1 (Head)", Reference = n2 };

			// Serialize and Deserialize
			byte[] bytes = BinConverter.GetBytes(n1);
			TestObject? result1 = BinConverter.GetItem<TestObject>(bytes);

			Assert.IsNotNull(result1);
			Assert.AreEqual(n1.Id, result1!.Id);

			// Check Node 2
			var result2 = result1.Reference;
			Assert.IsNotNull(result2);
			Assert.AreEqual(n2.Id, result2!.Id);
			Assert.AreNotSame(n2, result2, "Node 2 must be a new instance.");

			// Check Node 3
			var result3 = result2.Reference;
			Assert.IsNotNull(result3);
			Assert.AreEqual(n3.Id, result3!.Id);
			Assert.AreNotSame(n3, result3, "Node 3 must be a new instance.");

			// Check end of chain
			Assert.IsNull(result3.Reference, "Chain must terminate at Node 3.");
		}

		// --- Bổ sung Test cho Cấu trúc phức tạp ---

		[TestMethod]
		[Description("Case: Deeply nested complex structure (L1 ComplexTestObject -> L2 List -> L3 Dictionary -> L4 TestObject) serialization.")]
		public void GetItem_ComplexObject_ReturnsDeeplyEqualStructure()
		{
			// Cấu trúc lồng nhau 4-5 lớp:
			// L1: ComplexTestObject (Root)
			// L2: List<T>
			// L3: Dictionary<string, T>
			// L4: TestObject (Id, Name)
			// L5: Primitives (int, string)

			// Dữ liệu mẫu L4
			var innerObj1 = new TestObject { Id = 101, Name = "Inner A" };
			var innerObj2 = new TestObject { Id = 102, Name = "Inner B" };

			// Dữ liệu mẫu L3 (Dictionary)
			var dict1 = new Dictionary<string, TestObject> { { "KeyA", innerObj1 } };
			var dict2 = new Dictionary<string, TestObject> { { "KeyB", innerObj2 } };

			// Dữ liệu mẫu L2 (List)
			var nestedList = new List<Dictionary<string, TestObject>> { dict1, dict2 };

			// Dữ liệu mẫu L1 (Root)
			var original = new ComplexTestObject
			{
				RootId = 999,
				Description = "Deep Serialization Test",
				NestedStructure = nestedList
			};

			// Thực hiện Serialize và Deserialize
			byte[] bytes = BinConverter.GetBytes(original);
			ComplexTestObject? result = BinConverter.GetItem<ComplexTestObject>(bytes);

			// 1. Assert Root
			Assert.IsNotNull(result);
			Assert.AreEqual(original.RootId, result!.RootId);
			Assert.AreEqual(original.Description, result.Description);

			// 2. Assert L2 (List)
			Assert.IsNotNull(result.NestedStructure);
			Assert.AreEqual(2, result.NestedStructure.Count);

			// 3. Assert L3 (Dictionary) - Item 0
			var resultDict1 = result.NestedStructure[0];
			Assert.IsNotNull(resultDict1);
			Assert.IsTrue(resultDict1.ContainsKey("KeyA"));

			// 4. Assert L4 (TestObject) - Item 0
			var resultInnerObj1 = resultDict1["KeyA"];
			Assert.IsNotNull(resultInnerObj1);
			Assert.AreEqual(innerObj1.Id, resultInnerObj1.Id);
			Assert.AreEqual(innerObj1.Name, resultInnerObj1.Name);
			Assert.AreNotSame(innerObj1, resultInnerObj1, "L4 object identity must be lost."); // Identity check

			// 5. Assert L3 (Dictionary) - Item 1
			var resultDict2 = result.NestedStructure[1];
			Assert.IsNotNull(resultDict2);
			Assert.IsTrue(resultDict2.ContainsKey("KeyB"));

			// 6. Assert L4 (TestObject) - Item 1
			var resultInnerObj2 = resultDict2["KeyB"];
			Assert.IsNotNull(resultInnerObj2);
			Assert.AreEqual(innerObj2.Id, resultInnerObj2.Id);
			Assert.AreEqual(innerObj2.Name, resultInnerObj2.Name);
			Assert.AreNotSame(innerObj2, resultInnerObj2, "L4 object identity must be lost."); // Identity check
		}

		// --- Các Case Giới hạn và Ngoại lệ (giữ nguyên) ---

		[TestMethod]
		[Description("Case: Empty list (limit/edge case).")]
		public void GetItem_EmptyList_ReturnsEmptyList()
		{
			var original = new List<int>();
			byte[] bytes = BinConverter.GetBytes(original);
			List<int>? result = BinConverter.GetItem<List<int>>(bytes);

			Assert.IsNotNull(result);
			Assert.AreEqual(0, result!.Count);
		}

		[TestMethod]
		[Description("Case: List with many items (limit case for collection size).")]
		public void GetItem_LargeList_ReturnsAllItems()
		{
			var original = Enumerable.Range(0, 1000).ToList();
			byte[] bytes = BinConverter.GetBytes(original);
			List<int>? result = BinConverter.GetItem<List<int>>(bytes);

			Assert.IsNotNull(result);
			Assert.AreEqual(1000, result!.Count);
			CollectionAssert.AreEqual(original, result); // MSTest Collection Assert
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		[Description("Exception Case: Deserializing truncated binary data.")]
		public void GetItem_TruncatedBinary_ThrowsException()
		{
			// Serialize a simple int (4 bytes)
			int original = 1337;
			byte[] originalBytes = BinConverter.GetBytes(original);

			// Truncate the binary data (make it too short)
			byte[] truncatedBytes = originalBytes.Take(2).ToArray();

			// Expect EndOfStreamException from the mock DataContainer
			BinConverter.GetItem<int>(truncatedBytes);
		}

	}
}
