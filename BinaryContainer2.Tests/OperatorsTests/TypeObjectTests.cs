using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	public class MockClass
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public override bool Equals(object? obj) => obj is MockClass other && Id == other.Id && Name == other.Name;
		public override int GetHashCode() => HashCode.Combine(Id, Name);
		public override string? ToString() => $"MockClass(Id={Id}, Name={Name})";
	}

	[TestClass]
	public class TypeObjectTests
	{
		private readonly TypeObject _operator = new TypeObject();
		private readonly RefPool _refPool = new RefPool();

		public TypeObjectTests()
		{
			_operator.Build();
		}

		// Helper method to execute the full serialization/deserialization cycle
		private object? RoundTrip(object? data)
		{
			var container = new DataContainer();

			// 1. Write Value
			_operator.Write(container, data, _refPool);

			// 2. Export and import
			var bytes = container.Export();
			var readContainer = new DataContainer();
			readContainer.Import(bytes);

			// 3. Read Value
			return _operator.Read(readContainer, _refPool);
		}

		[TestMethod]
		[DataRow((sbyte)0, "SByte Zero")]
		[DataRow(sbyte.MinValue, "SByte MinValue")]
		[DataRow(sbyte.MaxValue, "SByte MaxValue")]
		[DataRow(null, "SByte Null")]
		public void Test_SByte_Serialization(sbyte? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow((byte)0, "Byte Zero")]
		[DataRow(byte.MinValue, "Byte MinValue")]
		[DataRow(byte.MaxValue, "Byte MaxValue")]
		[DataRow(null, "Byte Null")]
		public void Test_Byte_Serialization(byte? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow((short)0, "Int16 Zero")]
		[DataRow(short.MinValue, "Int16 MinValue")]
		[DataRow(short.MaxValue, "Int16 MaxValue")]
		[DataRow(null, "Int16 Null")]
		public void Test_Int16_Serialization(short? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow((ushort)0, "UInt16 Zero")]
		[DataRow(ushort.MinValue, "UInt16 MinValue")]
		[DataRow(ushort.MaxValue, "UInt16 MaxValue")]
		[DataRow(null, "UInt16 Null")]
		public void Test_UInt16_Serialization(ushort? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(0, "Int32 Zero")]
		[DataRow(int.MinValue, "Int32 MinValue")]
		[DataRow(int.MaxValue, "Int32 MaxValue")]
		[DataRow(null, "Int32 Null")]
		public void Test_Int32_Serialization(int? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow((uint)0, "UInt32 Zero")]
		[DataRow(uint.MinValue, "UInt32 MinValue")]
		[DataRow(uint.MaxValue, "UInt32 MaxValue")]
		[DataRow(null, "UInt32 Null")]
		public void Test_UInt32_Serialization(uint? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(0L, "Int64 Zero")]
		[DataRow(long.MinValue, "Int64 MinValue")]
		[DataRow(long.MaxValue, "Int64 MaxValue")]
		[DataRow(null, "Int64 Null")]
		public void Test_Int64_Serialization(long? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(0UL, "UInt64 Zero")]
		[DataRow(ulong.MinValue, "UInt64 MinValue")]
		[DataRow(ulong.MaxValue, "UInt64 MaxValue")]
		[DataRow(null, "UInt64 Null")]
		public void Test_UInt64_Serialization(ulong? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(0.0f, "Single Zero")]
		[DataRow(float.MinValue, "Single MinValue")]
		[DataRow(float.MaxValue, "Single MaxValue")]
		[DataRow(null, "Single Null")]
		public void Test_Single_Serialization(float? input, string name)
		{
			var result = RoundTrip(input);
			// Use Assert.AreEqual for floating point types with delta if necessary, 
			// but for this mock setup, direct comparison works.
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(0.0, "Double Zero")]
		[DataRow(double.MinValue, "Double MinValue")]
		[DataRow(double.MaxValue, "Double MaxValue")]
		[DataRow(null, "Double Null")]
		public void Test_Double_Serialization(double? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow("0", "Decimal Zero")]
		[DataRow("-79228162514264337593543950335", "Decimal MinValue")] // Decimal.MinValue
		[DataRow("79228162514264337593543950335", "Decimal MaxValue")] // Decimal.MaxValue
		[DataRow(null, "Decimal Null")]
		public void Test_Decimal_Serialization(string input, string name)
		{
			Decimal? value = input == null ? (Decimal?)null : Decimal.Parse(input);
			var result = RoundTrip(value);
			Assert.AreEqual(value, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow('A', "Char A")]
		[DataRow('\0', "Char MinValue")]
		[DataRow('\uFFFF', "Char MaxValue")]
		[DataRow(null, "Char Null")]
		public void Test_Char_Serialization(char? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow("Test string", "String Normal")]
		[DataRow("", "String Empty")]
		[DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit.", "String Long")]
		[DataRow(null, "String Null")]
		public void Test_String_Serialization(string? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		[DataRow(true, "Boolean True")]
		[DataRow(false, "Boolean False")]
		[DataRow(null, "Boolean Null")]
		public void Test_Boolean_Serialization(bool? input, string name)
		{
			var result = RoundTrip(input);
			Assert.AreEqual(input, result, $"Failed for {name}");
		}

		[TestMethod]
		public void Test_DateTime_Serialization()
		{
			var min = DateTime.MinValue;
			var max = DateTime.MaxValue;
			var normal = new DateTime(2023, 10, 26, 15, 30, 0, DateTimeKind.Utc);

			Assert.AreEqual(min, RoundTrip(min), "Failed for DateTime MinValue");
			Assert.AreEqual(max, RoundTrip(max), "Failed for DateTime MaxValue");
			Assert.AreEqual(normal, RoundTrip(normal), "Failed for DateTime Normal");
			Assert.IsNull(RoundTrip(null), "Failed for DateTime Null");
		}

		[TestMethod]
		public void Test_TimeSpan_Serialization()
		{
			var min = TimeSpan.MinValue;
			var max = TimeSpan.MaxValue;
			var normal = new TimeSpan(1, 2, 30, 45);

			Assert.AreEqual(min, RoundTrip(min), "Failed for TimeSpan MinValue");
			Assert.AreEqual(max, RoundTrip(max), "Failed for TimeSpan MaxValue");
			Assert.AreEqual(normal, RoundTrip(normal), "Failed for TimeSpan Normal");
			Assert.IsNull(RoundTrip(null), "Failed for TimeSpan Null");
		}

		[TestMethod]
		public void Test_Guid_Serialization()
		{
			var empty = Guid.Empty;
			var newGuid = Guid.NewGuid();

			Assert.AreEqual(empty, RoundTrip(empty), "Failed for Guid Empty");
			Assert.AreEqual(newGuid, RoundTrip(newGuid), "Failed for Guid New");
			Assert.IsNull(RoundTrip(null), "Failed for Guid Null");
		}

		[TestMethod]
		public void Test_CustomClass_Serialization()
		{
			var mock = new MockClass { Id = 123, Name = "CustomTest" };

			var result = RoundTrip(mock);

			// Check if the result is not null
			Assert.IsNotNull(result, "Failed to deserialize custom class: Null result.");

			// Check if the result is the correct type
			Assert.IsInstanceOfType(result, typeof(MockClass), "Deserialized object is not of type MockClass.");

			// Check if the object values are equal
			Assert.AreEqual(mock, result, "Deserialized MockClass object does not match original.");

			// Check null case
			Assert.IsNull(RoundTrip(null), "Failed for Custom Class Null");
		}
	}
}
