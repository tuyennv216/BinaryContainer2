using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

// Lớp chứa tất cả các kiểu dữ liệu cơ bản cần kiểm tra
public class AllBuiltinTypesClass
{
	// Số nguyên
	public sbyte PropSByte { get; set; }
	public byte PropByte { get; set; }
	public short PropInt16 { get; set; }
	public ushort PropUInt16 { get; set; }
	public int PropInt32 { get; set; }
	public uint PropUInt32 { get; set; }
	public long PropInt64 { get; set; }
	public ulong PropUInt64 { get; set; }

	// Số thực và thập phân
	public float PropSingle { get; set; }
	public double PropDouble { get; set; }
	public decimal PropDecimal { get; set; }

	// Các kiểu khác
	public char PropChar { get; set; }
	public string? PropString { get; set; }
	public bool PropBoolean { get; set; }
	public DateTime PropDateTime { get; set; }
	public TimeSpan PropTimeSpan { get; set; }
	public Guid PropGuid { get; set; }

	// Helper method để tạo một đối tượng với các giá trị biên
	public static AllBuiltinTypesClass GetEdgeCaseInstance()
	{
		return new AllBuiltinTypesClass
		{
			PropSByte = sbyte.MinValue,
			PropByte = byte.MaxValue,
			PropInt16 = short.MinValue,
			PropUInt16 = ushort.MaxValue,
			PropInt32 = int.MaxValue,
			PropUInt32 = uint.MinValue,
			PropInt64 = long.MinValue,
			PropUInt64 = ulong.MaxValue,

			PropSingle = float.Epsilon,
			PropDouble = double.MaxValue,
			PropDecimal = decimal.MinValue,

			PropChar = '\u007F', // Ký tự ASCII lớn nhất
			PropString = "This is a long test string with special characters: !@#$%^&*()",
			PropBoolean = true,
			PropDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			PropTimeSpan = new TimeSpan(1, 2, 3, 4, 500),
			PropGuid = Guid.Empty
		};
	}
}

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeClass_BuiltinTypes_Tests
	{
		// Giả định operator cho một Class là TypeClass
		private ITypeOperator _typeClassOperator;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeClass với kiểu AllBuiltinTypesClass
			_typeClassOperator = new TypeClass(typeof(AllBuiltinTypesClass));
			_typeClassOperator.Build();
		}

		// --- Phương thức tiện ích để so sánh 16 thuộc tính ---
		private void AssertBuiltinEqual(AllBuiltinTypesClass expected, AllBuiltinTypesClass actual, string message)
		{
			Assert.IsNotNull(actual);

			// Số nguyên
			Assert.AreEqual(expected.PropSByte, actual.PropSByte, $"[SByte] {message}");
			Assert.AreEqual(expected.PropByte, actual.PropByte, $"[Byte] {message}");
			Assert.AreEqual(expected.PropInt16, actual.PropInt16, $"[Int16] {message}");
			Assert.AreEqual(expected.PropUInt16, actual.PropUInt16, $"[UInt16] {message}");
			Assert.AreEqual(expected.PropInt32, actual.PropInt32, $"[Int32] {message}");
			Assert.AreEqual(expected.PropUInt32, actual.PropUInt32, $"[UInt32] {message}");
			Assert.AreEqual(expected.PropInt64, actual.PropInt64, $"[Int64] {message}");
			Assert.AreEqual(expected.PropUInt64, actual.PropUInt64, $"[UInt64] {message}");

			// Số thực và thập phân (Sử dụng delta cho float/double nếu cần, nhưng ở đây dùng Equals)
			Assert.AreEqual(expected.PropSingle, actual.PropSingle, $"[Single] {message}");
			Assert.AreEqual(expected.PropDouble, actual.PropDouble, $"[Double] {message}");
			Assert.AreEqual(expected.PropDecimal, actual.PropDecimal, $"[Decimal] {message}");

			// Các kiểu khác
			Assert.AreEqual(expected.PropChar, actual.PropChar, $"[Char] {message}");
			Assert.AreEqual(expected.PropString, actual.PropString, $"[String] {message}");
			Assert.AreEqual(expected.PropBoolean, actual.PropBoolean, $"[Boolean] {message}");
			Assert.AreEqual(expected.PropDateTime, actual.PropDateTime, $"[DateTime] {message}");
			Assert.AreEqual(expected.PropTimeSpan, actual.PropTimeSpan, $"[TimeSpan] {message}");
			Assert.AreEqual(expected.PropGuid, actual.PropGuid, $"[Guid] {message}");
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản
		// ---------------------------------------------

		/// <summary>
		/// Test case điển hình với các giá trị trung bình (non-edge).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValues_ShouldPreserveAll()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = new AllBuiltinTypesClass
			{
				PropSByte = 10,
				PropByte = 50,
				PropInt16 = 1000,
				PropUInt16 = 2000,
				PropInt32 = 50000,
				PropUInt32 = 100000,
				PropInt64 = 1000000L,
				PropUInt64 = 2000000UL,

				PropSingle = 1.234f,
				PropDouble = 9.8765,
				PropDecimal = 123.456M,

				PropChar = 'A',
				PropString = "Hello world",
				PropBoolean = false,
				PropDateTime = DateTime.Now.Date,
				PropTimeSpan = TimeSpan.FromHours(5),
				PropGuid = Guid.NewGuid()
			};
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			var readData = (AllBuiltinTypesClass)_typeClassOperator.Read(newContainer, readPool)!;

			AssertBuiltinEqual(originalData, readData, "Giá trị điển hình.");
		}

		/// <summary>
		/// Test case với các giá trị biên (Min/Max).
		/// </summary>
		[TestMethod]
		public void WriteRead_EdgeValues_ShouldPreserveAll()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = AllBuiltinTypesClass.GetEdgeCaseInstance();
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			var readData = (AllBuiltinTypesClass)_typeClassOperator.Read(newContainer, readPool)!;

			AssertBuiltinEqual(originalData, readData, "Giá trị biên (Edge cases).");
		}

		/// <summary>
		/// Test case cho thuộc tính String là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullString_ShouldPreserveNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var originalData = new AllBuiltinTypesClass { PropString = null, PropInt32 = 123 };
			var originalContainer = new DataContainer();

			_typeClassOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			var readData = (AllBuiltinTypesClass)_typeClassOperator.Read(newContainer, readPool)!;

			Assert.IsNull(readData.PropString, "[String] Phải đọc ra là NULL.");
			Assert.AreEqual(123, readData.PropInt32, "[Int32] Các thuộc tính khác phải còn nguyên.");
		}

		/// <summary>
		/// Trường hợp biên: Đối tượng lớp là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullObject_ShouldReturnNull()
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
			Assert.IsNull(readData, "Đối tượng lớp là NULL.");
		}
	}
}
