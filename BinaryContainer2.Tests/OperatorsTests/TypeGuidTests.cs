using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests
{
	[TestClass]
	public class TypeGuidTests
	{
		private TypeGuid _typeGuidOperator;
		private RefPool _refPool;

		[TestInitialize]
		public void Setup()
		{
			_typeGuidOperator = new TypeGuid();
			_typeGuidOperator.Build();
			_refPool = new RefPool();
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một giá trị Guid mới được tạo.
		/// </summary>
		[TestMethod]
		public void WriteRead_NewlyCreatedGuid_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data
			object originalData = Guid.NewGuid();
			var originalContainer = new DataContainer();

			// 2. Write
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeGuidOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(Guid));
			Assert.AreEqual(originalData, readData, "Giá trị Guid mới tạo phải được giữ nguyên.");
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
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);

			// 3. Export/Import
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 4. Read
			object? readData = _typeGuidOperator.Read(newContainer, _refPool);

			// 5. Kiểm tra
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên và Đặc Biệt (Edge and Special Cases) ---

		/// <summary>
		/// Giá trị biên: Guid.Empty (000...000).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyGuid_ShouldReturnEmpty()
		{
			// 1. Chuẩn bị data
			object originalData = Guid.Empty;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeGuidOperator.Read(newContainer, _refPool);
			Assert.AreEqual(Guid.Empty, readData, "Giá trị phải bằng Guid.Empty.");
		}

		/// <summary>
		/// Giá trị Guid với tất cả các bit được bật (giá trị lớn nhất).
		/// Tạo một Guid từ mảng 16 bytes chứa toàn bộ là 0xFF (255).
		/// </summary>
		[TestMethod]
		public void WriteRead_AllBitsSet_ShouldBePreserved()
		{
			// 1. Chuẩn bị data: 16 bytes 0xFF
			byte[] allOnes = new byte[16];
			for (int i = 0; i < 16; i++) allOnes[i] = 0xFF;
			object originalData = new Guid(allOnes);
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeGuidOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Guid với tất cả bit bật phải được giữ nguyên.");
		}

		/// <summary>
		/// Kiểm tra một Guid được tạo từ một chuỗi xác định.
		/// </summary>
		[TestMethod]
		public void WriteRead_FixedStringGuid_ShouldBePreserved()
		{
			// 1. Chuẩn bị data
			string guidString = "12345678-ABCD-EF01-2345-6789ABCDEF01";
			object originalData = new Guid(guidString);
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);
			newContainer.Items_Itor = 0;
			newContainer.Arrays_Itor = 0;

			// 3. Read và Kiểm tra
			object? readData = _typeGuidOperator.Read(newContainer, _refPool);
			Assert.AreEqual(originalData, readData, "Guid cố định phải được giữ nguyên.");
		}

		/// <summary>
		/// Test trường hợp Write một giá trị KHÔNG phải Guid, phải ném ra ngoại lệ.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))] // Giả định thư viện ném InvalidCastException
		public void Write_NonGuidValue_ShouldThrowException()
		{
			// 1. Chuẩn bị data không hợp lệ (ví dụ: string)
			object originalData = "not a guid";
			var originalContainer = new DataContainer();

			// 2. Cố gắng ghi data
			_typeGuidOperator.Write(originalContainer, originalData, _refPool);
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một số lượng lớn các giá trị Guid ngẫu nhiên 
		/// để kiểm tra tính ổn định.
		/// </summary>
		[TestMethod]
		public void WriteRead_RandomValues_ShouldBeStable()
		{
			const int testCount = 1000;
			var originalContainer = new DataContainer();
			var originalValues = new List<Guid>();

			// 1. Ghi một loạt các giá trị ngẫu nhiên
			for (int i = 0; i < testCount; i++)
			{
				Guid value = Guid.NewGuid();
				originalValues.Add(value);
				_typeGuidOperator.Write(originalContainer, value, _refPool);
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
				Guid expected = originalValues[i];
				object? readData = _typeGuidOperator.Read(newContainer, _refPool);
				Guid actual = (Guid)readData!;

				Assert.IsNotNull(readData);
				Assert.IsInstanceOfType(readData, typeof(Guid));

				Assert.AreEqual(expected, actual, $"Giá trị Guid ngẫu nhiên thứ {i} không khớp.");
			}
		}
	}
}