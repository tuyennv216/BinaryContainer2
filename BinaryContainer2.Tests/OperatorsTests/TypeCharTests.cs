using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeCharTests
	{
		private TypeChar _typeCharOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			_typeCharOperator = new TypeChar();
			_typeCharOperator.Build();
			_refPool = new RefPool();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị char điển hình (ví dụ: 'A').
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalValue_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = 'A';
			var originalContainer = new DataContainer();

			// 2. Write
			_typeCharOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeCharOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(char));
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Test case cho giá trị NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullValue_ShouldReturnNull()
		{
			// 1. Chuẩn bị data
			object? originalData = null;
			var originalContainer = new DataContainer();

			// 2. Write
			_typeCharOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeCharOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên dưới cùng: char.MinValue (ký tự Null, '\u0000').
		/// </summary>
		[TestMethod]
		public void WriteRead_MinValue_ShouldReturnNullChar()
		{
			// 1. Chuẩn bị data
			object originalData = char.MinValue; // \u0000
			var originalContainer = new DataContainer();

			// 2. Write
			_typeCharOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeCharOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng '\u0000'.");
		}

		/// <summary>
		/// Giá trị biên trên cùng: char.MaxValue ('\uffff').
		/// </summary>
		[TestMethod]
		public void WriteRead_MaxValue_ShouldReturnFFFF()
		{
			// 1. Chuẩn bị data
			object originalData = char.MaxValue; // \uffff
			var originalContainer = new DataContainer();

			// 2. Write
			_typeCharOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeCharOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.AreEqual(originalData, readData, "Giá trị phải bằng '\uffff'.");
		}

		// --- III. Giá Trị Đặc Biệt (Special Cases) ---

		/// <summary>
		/// Ký tự ASCII đặc biệt: Dấu cách (' ').
		/// </summary>
		[TestMethod]
		public void WriteRead_SpaceChar_ShouldReturnSpace()
		{
			object originalData = ' ';
			var originalContainer = new DataContainer();
			_typeCharOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeCharOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Ký tự không phải ASCII (Unicode): Ví dụ: Ký tự tiếng Việt ('ệ').
		/// </summary>
		[TestMethod]
		public void WriteRead_VietnameseChar_ShouldReturnVietnameseChar()
		{
			object originalData = 'ệ';
			var originalContainer = new DataContainer();
			_typeCharOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeCharOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Ký tự không phải ASCII (Unicode): Ví dụ: Biểu tượng (ví dụ: '™').
		/// </summary>
		[TestMethod]
		public void WriteRead_SymbolChar_ShouldReturnSymbol()
		{
			object originalData = '™';
			var originalContainer = new DataContainer();
			_typeCharOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeCharOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData);
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Char, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonCharValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: số nguyên 32-bit)
			object originalData = 70000; // Vượt quá phạm vi 16-bit của char
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeCharOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các ký tự Unicode ngẫu nhiên
		/// để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var random = new Random();
			var originalContainer = new DataContainer();
			var originalValues = new List<char>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// Tạo giá trị char ngẫu nhiên trong phạm vi [0, 65535]
				char value = (char)random.Next(char.MinValue, char.MaxValue + 1);
				originalValues.Add(value);
				_typeCharOperator.Write(originalContainer, value, _refPool);
			}

			// 2. Xuất và Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Đọc và so sánh
			for (int i = 0; i < testCount; i++)
			{
				char expected = originalValues[i];
				object? readData = _typeCharOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(char));
				Assert.AreEqual(expected, (char)readData, $"Giá trị ngẫu nhiên thứ {i} (Unicode: {(int)expected}) không khớp.");
			}
		}
	}
}