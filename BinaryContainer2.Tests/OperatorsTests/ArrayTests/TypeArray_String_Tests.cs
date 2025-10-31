using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_String_Tests // Test cho mảng string[]
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử string[]
			_typeArrayOperator = new TypeArray(typeof(string[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng string[] ---
		private void AssertArrayEqual(string[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(string[]), "Kiểu đọc ra phải là string[].");

			for (int i = 0; i < expected.Length; i++)
			{
				string expectedValue = expected[i];
				string? actualValue = (string?)actual.GetValue(i);

				// Sử dụng Assert.AreEqual vì các chuỗi phải khớp byte-to-byte
				Assert.AreEqual(expectedValue, actualValue, $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng string[] điển hình (chứa chuỗi thông thường).
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalStringArray_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: string[]
			string[] originalArray = new string[]
			{
				"Hello World",
				"Container Test",
				"BinaryContainer2",
				"Vietnamese characters: Xin chào thế giới"
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng string[] điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullArray_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên & Đặc Biệt (Edge Cases & Special Values) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (string[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			string[] originalArray = new string[0];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		/// <summary>
		/// Test mảng chứa các chuỗi rỗng ("") và chuỗi NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullAndEmptyStringsArray_ShouldBePreserved()
		{
			// Chuỗi NULL và chuỗi rỗng là hai trạng thái khác nhau
			string?[] originalArray = new string?[]
			{
				"", 					// Chuỗi rỗng
				null, 					// Chuỗi null
				" ", 					// Chuỗi chứa khoảng trắng
				string.Empty, 			// Tương đương ""
				null
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray.Cast<string>().ToArray(), readArray!, "Mảng chứa chuỗi NULL và chuỗi rỗng.");
		}

		/// <summary>
		/// Test mảng chứa các chuỗi rất dài.
		/// </summary>
		[TestMethod]
		public void WriteRead_VeryLongStringArray_ShouldBePreserved()
		{
			const int stringLength = 5000;

			// Tạo một chuỗi dài 5000 ký tự ngẫu nhiên
			string longString = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789",
				(stringLength + 61) / 62) // Đảm bảo độ dài
				.SelectMany(s => s)
				.Take(stringLength)
				.ToArray());

			string[] originalArray = new string[]
			{
				longString,
				"Short string",
				longString
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa các chuỗi rất dài.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các chuỗi ngẫu nhiên có độ dài khác nhau.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomStringArray_ShouldBeStable()
		{
			const int arrayLength = 1000;
			string[] originalArray = new string[arrayLength];
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				// Độ dài chuỗi ngẫu nhiên từ 0 đến 50
				int length = _random.Next(0, 51);
				if (length == 0)
				{
					// Thi thoảng chèn NULL hoặc chuỗi rỗng
					originalArray[i] = i % 5 == 0 ? null! : string.Empty;
				}
				else
				{
					// Tạo chuỗi ngẫu nhiên
					originalArray[i] = new string(Enumerable.Repeat(chars, length)
					  .Select(s => s[_random.Next(s.Length)]).ToArray());
				}
			}
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu String.");
		}
	}
}
