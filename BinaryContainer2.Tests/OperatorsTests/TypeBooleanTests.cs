using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeBooleanTests
	{
		private TypeBoolean _typeBooleanOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			_typeBooleanOperator = new TypeBoolean();
			_typeBooleanOperator.Build();
			_refPool = new RefPool();
		}

		// --- Các test case cơ bản (giữ lại từ mẫu) ---

		/// <summary>
		/// Test case cho giá trị boolean TRUE.
		/// Các bước: Write(true) -> Export -> Import -> Read -> Assert(true)
		/// </summary>
		[TestMethod]
		public void WriteRead_TrueValue_ShouldReturnTrue()
		{
			// 1. Chuẩn bị data
			object originalData = true;
			var originalContainer = new DataContainer();

			// 2. Dùng TypeBoolean.Write ghi data vào DataContainer
			_typeBooleanOperator.Write(originalContainer, originalData, _refPool);

			// 3. Xuất ra bytes
			byte[] exportedBytes = originalContainer.Export();

			// 4. Tạo 1 DataContainer mới và Import
			var newContainer = new DataContainer();
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			newContainer.Import(exportedBytes);

			// 5. Dùng TypeBoolean.Read đọc data từ DataContainer mới
			object? readData = _typeBooleanOperator.Read(newContainer, _refPool);

			// 6. Kiểm tra
			Assert.IsNotNull(readData, "Dữ liệu đọc ra không được là NULL.");
			Assert.IsInstanceOfType(readData, typeof(bool), "Dữ liệu đọc ra phải là kiểu Boolean.");
			Assert.AreEqual(originalData, readData, "Giá trị Boolean đọc ra phải bằng giá trị ban đầu (True).");
		}

		/// <summary>
		/// Test case cho giá trị boolean FALSE.
		/// Các bước: Write(false) -> Export -> Import -> Read -> Assert(false)
		/// </summary>
		[TestMethod]
		public void WriteRead_FalseValue_ShouldReturnFalse()
		{
			// ... (Code tương tự WriteRead_TrueValue_ShouldReturnTrue nhưng với false)
			object originalData = false;
			var originalContainer = new DataContainer();
			_typeBooleanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();

			var newContainer = new DataContainer();
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			newContainer.Import(exportedBytes);

			object? readData = _typeBooleanOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readData, "Dữ liệu đọc ra không được là NULL.");
			Assert.IsInstanceOfType(readData, typeof(bool), "Dữ liệu đọc ra phải là kiểu Boolean.");
			Assert.AreEqual(originalData, readData, "Giá trị Boolean đọc ra phải bằng giá trị ban đầu (False).");
		}

		/// <summary>
		/// Test case cho giá trị NULL.
		/// Các bước: Write(null) -> Export -> Import -> Read -> Assert(null)
		/// </summary>
		[TestMethod]
		public void WriteRead_NullValue_ShouldReturnNull()
		{
			// ... (Code tương tự WriteRead_TrueValue_ShouldReturnTrue nhưng với null)
			object? originalData = null;
			var originalContainer = new DataContainer();
			_typeBooleanOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();

			var newContainer = new DataContainer();
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;
			newContainer.Import(exportedBytes);

			object? readData = _typeBooleanOperator.Read(newContainer, _refPool);

			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- Bổ sung: Giá trị Biên và Giá trị Đặc biệt ---

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Boolean, phải ném ra ngoại lệ.
		/// Đây là kiểm tra giá trị đầu vào không hợp lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonBooleanValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: số nguyên)
			object originalData = 123;
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeBooleanOperator.Write(originalContainer, originalData, _refPool);

			// 3. Nếu không ném ngoại lệ, test sẽ thất bại
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Boolean (ví dụ: string), phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_StringValue_ShouldThrowException()
		{
			object originalData = "not_a_bool";
			var originalContainer = new DataContainer();
			_typeBooleanOperator.Write(originalContainer, originalData, _refPool);
		}

		/// <summary>
		/// Test trường hợp Write/Read một mảng các giá trị True và False liên tiếp
		/// để kiểm tra tính độc lập và liên tục của các lần ghi.
		/// </summary>
		[TestMethod]
		public void WriteRead_SequenceOfValues_ShouldBeCorrect()
		{
			// 1. Chuẩn bị một chuỗi các giá trị
			List<bool> originalSequence = new List<bool> { true, false, true, true, false, false, true, false };
			var originalContainer = new DataContainer();

			// 2. Ghi từng giá trị vào container
			foreach (var data in originalSequence)
			{
				_typeBooleanOperator.Write(originalContainer, data, _refPool);
			}

			// 3. Xuất ra bytes và Import vào container mới
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Đọc từng giá trị và so sánh
			foreach (var expected in originalSequence)
			{
				object? readData = _typeBooleanOperator.Read(newContainer, _refPool);
				Assert.IsNotNull(readData);
				Assert.AreEqual(expected, readData, $"Giá trị đọc ra không khớp với giá trị ban đầu: {expected}.");
			}
		}

		// --- Bổ sung: Giá trị Ngẫu nhiên (Random/Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Boolean ngẫu nhiên
		/// để kiểm tra tính ổn định (bền vững).
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var random = new Random();
			var originalContainer = new DataContainer();
			var originalValues = new List<bool>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				// 50/50 True/False
				bool value = random.NextDouble() > 0.5;
				originalValues.Add(value);
				_typeBooleanOperator.Write(originalContainer, value, _refPool);
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
				bool expected = originalValues[i];
				object? readData = _typeBooleanOperator.Read(newContainer, _refPool);

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(bool));
				Assert.AreEqual(expected, (bool)readData, $"Giá trị ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}