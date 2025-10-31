using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	// Định nghĩa một lớp đơn giản để sử dụng làm kiểu phần tử.
	// BinaryContainer2 sẽ cần các TypeOperator tương ứng cho lớp này.
	public class MyTestClass
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;

		// Phương thức này là cần thiết để test lớp chứa các thuộc tính đơn giản.
		public override bool Equals(object? obj)
		{
			if (obj is not MyTestClass other) return false;
			return Id == other.Id && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, Name);
		}

		public override string ToString() => $"{{Id: {Id}, Name: {Name}}}";
	}


	[TestClass]
	public class TypeArray_Class_Tests
	{
		private TypeArray _typeArrayOperator;
		// private RefPool _refPool; // Đã loại bỏ

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử là mảng của lớp MyTestClass
			_typeArrayOperator = new TypeArray(typeof(MyTestClass[]));
			_typeArrayOperator.Build();
		}

		// --- Phương thức tiện ích để so sánh mảng đối tượng ---
		private void AssertArrayEqual(MyTestClass[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(MyTestClass[]), "Kiểu đọc ra phải là MyTestClass[].");

			for (int i = 0; i < expected.Length; i++)
			{
				MyTestClass? expectedItem = expected[i];
				MyTestClass? actualItem = (MyTestClass?)actual.GetValue(i);

				if (expectedItem == null)
				{
					Assert.IsNull(actualItem, $"Phần tử thứ {i} phải là NULL: {message}");
				}
				else
				{
					Assert.IsNotNull(actualItem, $"Phần tử thứ {i} không được là NULL: {message}");
					// Kiểm tra giá trị các thuộc tính
					Assert.AreEqual(expectedItem.Id, actualItem.Id, $"Phần tử thứ {i} (Id) không khớp: {message}");
					Assert.AreEqual(expectedItem.Name, actualItem.Name, $"Phần tử thứ {i} (Name) không khớp: {message}");
				}
			}
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng MyTestClass[] điển hình với các đối tượng độc lập.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalClassArray_ShouldReturnCorrectValues()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			// 1. Chuẩn bị data: MyTestClass[]
			MyTestClass[] originalArray = new MyTestClass[]
			{
				new MyTestClass { Id = 1, Name = "Item A" },
				new MyTestClass { Id = 2, Name = "Item B" },
				new MyTestClass { Id = 3, Name = "Item C" }
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, readPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng đối tượng điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullArray_ShouldReturnNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (MyTestClass[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			MyTestClass[] originalArray = new MyTestClass[0];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, readPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		// --- II. Giá Trị Đặc Biệt cho Reference Types ---

		/// <summary>
		/// Test mảng chứa phần tử NULL. Đây là điểm khác biệt quan trọng so với mảng kiểu giá trị.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithNullElements_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			MyTestClass item1 = new MyTestClass { Id = 5, Name = "First" };
			MyTestClass item2 = new MyTestClass { Id = 6, Name = "Third" };

			MyTestClass[] originalArray = new MyTestClass[]
			{
				item1,
				null!, // NULL ở vị trí 1
				item2,
				null!  // NULL ở vị trí 3
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, readPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa các phần tử NULL.");
		}

		/// <summary>
		/// Test mảng chứa nhiều lần cùng một tham chiếu (dùng RefPool).
		/// Mảng đọc ra nên tạo ra các đối tượng mới nhưng có giá trị bằng nhau.
		/// Nếu BinaryContainer2 sử dụng RefPool để tối ưu tham chiếu nội bộ, thì đây là một kiểm tra tốt.
		/// </summary>
		[TestMethod]
		public void WriteRead_ArrayWithRepeatedReferences_ShouldPreserveIdentity() // Đã đổi tên hàm để nhấn mạnh việc kiểm tra Identity
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			MyTestClass sharedItem = new MyTestClass { Id = 99, Name = "Shared" };

			MyTestClass[] originalArray = new MyTestClass[]
			{
				sharedItem,
				new MyTestClass { Id = 100, Name = "Unique" },
				sharedItem, // Tham chiếu lặp lại
				null!
			};
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			MyTestClass[]? readArray = (MyTestClass[]?)_typeArrayOperator.Read(newContainer, readPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa các tham chiếu lặp lại.");

			// Kiểm tra Identity (nếu RefPool hoạt động đúng, hai đối tượng này phải là cùng một instance sau khi Read)
			if (readArray != null)
			{
				// Kiểm tra nếu hệ thống bảo toàn tham chiếu trong RefPool
				// Đây là kiểm tra bắt buộc cho Reference Type serialization
				Assert.IsTrue(ReferenceEquals(readArray[0], readArray[2]), "Tham chiếu lặp lại phải được bảo toàn (RefPool hoạt động).");

				// Kiểm tra tính toàn vẹn (để đảm bảo không có logic bị loại bỏ)
				Assert.IsFalse(ReferenceEquals(readArray[0], readArray[1]), "Các đối tượng khác nhau không được tham chiếu cùng một instance.");
			}
		}
	}
}
