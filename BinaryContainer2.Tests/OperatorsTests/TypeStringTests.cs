using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;
using System.Text;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeStringTests
	{
		private TypeString _typeStringOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			_typeStringOperator = new TypeString();
			_typeStringOperator.Build();
			_refPool = new RefPool();
			_random = new Random(42);
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một chuỗi ASCII điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalAsciiString_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = "Hello, World 123!";
			var originalContainer = new DataContainer();

			// 2. Write
			_typeStringOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeStringOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(string));
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
			_typeStringOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeStringOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Chuỗi Rỗng (string.Empty).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyString_ShouldReturnEmpty()
		{
			// 1. Chuẩn bị data
			object originalData = string.Empty;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeStringOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeStringOperator.Read(newContainer, _refPool);
			Assert.AreEqual(string.Empty, readData, "Giá trị phải là chuỗi rỗng.");
		}

		/// <summary>
		/// Giá trị biên: Chuỗi chỉ chứa ký tự Null (Null Terminator).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullCharString_ShouldBePreserved()
		{
			// 1. Chuẩn bị data
			object originalData = "A\0B";
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeStringOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeStringOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Chuỗi chứa ký tự Null phải được giữ nguyên.");
		}

		// --- III. Giá Trị Đặc Biệt (Unicode and Length) ---

		/// <summary>
		/// Kiểm tra chuỗi chứa ký tự Unicode đa byte (ví dụ: Tiếng Việt, Emoji).
		/// </summary>
		[TestMethod]
		public void WriteRead_MultiByteUnicodeString_ShouldBePreserved()
		{
			// Chuỗi có Tiếng Việt và Emoji
			object originalData = "Việt Nam 🇻🇳 và thế giới 🌍!";
			var originalContainer = new DataContainer();

			_typeStringOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeStringOperator.Read(newContainer, _refPool);

			Assert.AreEqual(originalData, readData, "Chuỗi Unicode đa byte phải được giữ nguyên chính xác.");
		}

		/// <summary>
		/// Kiểm tra chuỗi chứa các ký tự điều khiển (control characters) và khoảng trắng.
		/// </summary>
		[TestMethod]
		public void WriteRead_ControlCharacters_ShouldBePreserved()
		{
			// Tab, Newline, Carriage Return
			object originalData = "Line 1\tTab\r\nLine 2";
			var originalContainer = new DataContainer();

			_typeStringOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			object? readData = _typeStringOperator.Read(newContainer, _refPool);

			Assert.AreEqual(originalData, readData, "Chuỗi chứa ký tự điều khiển phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải String, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonStringValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: int)
			object originalData = 12345;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeStringOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- IV. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các chuỗi có độ dài ngẫu nhiên
		/// và ký tự Unicode ngẫu nhiên để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomStrings_ShouldBeStable()
		{
			const int testCount = 100; // Số lượng chuỗi
			const int maxStringLength = 200; // Độ dài chuỗi tối đa
			var originalContainer = new DataContainer();
			var originalValues = new List<string>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				int length = _random.Next(1, maxStringLength);
				var sb = new StringBuilder(length);

				for (int j = 0; j < length; j++)
				{
					// Tạo ký tự Unicode ngẫu nhiên trong dải phổ biến (U+0020 đến U+2000)
					char randomChar = (char)_random.Next(0x0020, 0x2000);
					sb.Append(randomChar);
				}

				string value = sb.ToString();
				originalValues.Add(value);
				_typeStringOperator.Write(originalContainer, value, _refPool);
			}

			// 2. Xuất và Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Đọc và so sánh
			for (int i = 0; i < originalValues.Count; i++)
			{
				string expected = originalValues[i];
				object? readData = _typeStringOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(string));
				Assert.AreEqual(expected, (string)readData, $"Chuỗi ngẫu nhiên thứ {i} không khớp (dài {expected.Length} ký tự).");
			}
		}
	}
}