using BinaryContainer2.Utilities;

namespace BinaryContainer2.Tests.UtilitiesTests
{
	// Lớp giả lập cho mục đích kiểm thử
	public class TestData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is TestData other)
			{
				return Id == other.Id && Name == other.Name && IsActive == other.IsActive;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id, Name, IsActive);
		}
	}

	[TestClass]
	public class BackupTests
	{
		// Sử dụng DataRow cho các trường hợp kiểm thử khởi tạo
		[DataTestMethod]
		[DataRow(1, DisplayName = "Case: Min Size (1)")]
		[DataRow(5, DisplayName = "Case: Normal Size (5)")]
		[DataRow(100, DisplayName = "Case: Large Size (100)")]
		public void Constructor_ValidSize_InitializesCorrectly(int size)
		{
			// Arrange & Act
			var backup = new Backup(size);

			// Assert
			// Hiện tại không có cách truy cập các trường private (_backupSize, _backupCount, _index)
			// nên ta kiểm tra gián tiếp thông qua các hành vi như Get(0).
			Assert.IsNull(backup.Get(0), $"Get(0) should be null right after initialization for size {size}");
			Assert.IsNull(backup.Get(size - 1), $"Get(size-1) should be null right after initialization for size {size}");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException), AllowDerivedTypes = true)]
		public void Constructor_SizeZero_ThrowsException()
		{
			// Ta kiểm tra giá trị biên/trường hợp đặc biệt 0.
			var backup = new Backup(0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException), AllowDerivedTypes = true)]
		public void Constructor_SizeNegative_ThrowsException()
		{
			// Ta kiểm tra giá trị biên/trường hợp đặc biệt 0.
			var backup = new Backup(-1);
		}

		// --- Kiểm thử chức năng Add và Get (Trường hợp "Happy Path" và Cơ bản) ---

		[TestMethod]
		public void AddAndGet_SingleItem_ReturnsCorrectData()
		{
			// Arrange
			const int size = 3;
			var backup = new Backup(size);
			var expectedData = new TestData { Id = 1, Name = "Item1", IsActive = true };

			// Act
			backup.Add(expectedData);
			var actualData = backup.Get(0);

			// Assert
			Assert.IsNotNull(actualData);
			Assert.IsInstanceOfType(actualData, typeof(TestData));
			Assert.AreEqual(expectedData, (TestData)actualData, "Data should match the added object.");
		}

		[TestMethod]
		public void Add_NullObject_GetReturnsNull()
		{
			// Arrange
			const int size = 3;
			var backup = new Backup(size);

			// Act
			backup.Add<object>(null);
			var actualData = backup.Get(0);

			// Assert
			Assert.IsNull(actualData, "Getting the latest item should return null if null was added.");
		}

		// --- Kiểm thử Buffer Vòng (Circular Buffer) và Giá trị Biên ---

		[TestMethod]
		public void Add_ExceedsBackupSize_WrapsAroundCorrectly()
		{
			// Arrange
			const int size = 3;
			var backup = new Backup(size);
			var item1 = new TestData { Id = 1 };
			var item2 = new TestData { Id = 2 };
			var item3 = new TestData { Id = 3 };
			var item4 = new TestData { Id = 4, Name = "Newest" }; // Sẽ thay thế item1
			var item5 = new TestData { Id = 5, Name = "Second Newest" }; // Sẽ thay thế item2

			// Act
			backup.Add(item1); // 0
			backup.Add(item2); // 1
			backup.Add(item3); // 2
			backup.Add(item4); // 0 (overwrite item1)
			backup.Add(item5); // 1 (overwrite item2)

			// Assert (Kiểm tra thứ tự và việc ghi đè)
			// diff_version 0: item5
			Assert.AreEqual(item5, backup.Get(0), "Latest (0) should be item5.");

			// diff_version 1: item4
			Assert.AreEqual(item4, backup.Get(1), "Previous (1) should be item4.");

			// diff_version 2: item3
			Assert.AreEqual(item3, backup.Get(2), "Oldest preserved (2) should be item3.");

			// diff_version 3: item2 đã bị ghi đè bởi item5, nên nó không tồn tại
			Assert.IsNull(backup.Get(3), "Item 3 versions back should be null (item2 overwritten).");

			// diff_version lớn hơn _backupCount (cụ thể là 5 versions, size là 3, count là 3)
			Assert.IsNull(backup.Get(size + 1), "Versions beyond backupCount should return null.");
		}

		// --- Kiểm thử diff_version (Giá trị Biên và Đặc biệt) ---

		[TestMethod]
		public void Get_DiffVersionZero_ReturnsLatestItem()
		{
			// Arrange
			var backup = new Backup(2);
			var oldData = new TestData { Id = 10 };
			var newData = new TestData { Id = 20, Name = "Latest" };
			backup.Add(oldData);
			backup.Add(newData);

			// Act
			var actualData = backup.Get(0);

			// Assert
			Assert.AreEqual(newData, actualData, "Get(0) must return the newest item.");
		}

		[TestMethod]
		public void Get_DiffVersionEqualToBackupCount_ReturnsOldestItem()
		{
			// Arrange
			const int size = 4;
			var backup = new Backup(size);
			var oldestItem = new TestData { Id = 1, Name = "Oldest" };
			backup.Add(oldestItem);
			backup.Add(new TestData { Id = 2 });
			backup.Add(new TestData { Id = 3 });

			// Act
			// Sau 3 lần Add, _backupCount = 3. 
			var actualOldest = backup.Get(2); // diff_version 2 là mục cũ nhất (0 -> newest, 1 -> older, 2 -> oldest)

			// Assert
			Assert.AreEqual(oldestItem, actualOldest, "Get(diff_version = _backupCount - 1) should return the oldest item.");
		}

		[DataTestMethod]
		[DataRow(3, 4, DisplayName = "Case: diff_version > _backupCount (3 > 2)")]
		[DataRow(3, 5, DisplayName = "Case: diff_version >> _backupCount (5 > 2)")]
		[DataRow(3, -1, DisplayName = "Case: diff_version is negative")] // Giá trị biên
		public void Get_DiffVersionExceedsOrIsInvalid_ReturnsNull(int size, int diffVersion)
		{
			// Arrange
			var backup = new Backup(size);
			backup.Add(new TestData { Id = 1 });
			backup.Add(new TestData { Id = 2 }); // _backupCount = 2

			// Act
			var actualData = backup.Get(diffVersion);

			// Assert
			Assert.IsNull(actualData, $"Get({diffVersion}) should return null.");
		}

		// --- Kiểm thử Tuần tự hóa/Giải tuần tự hóa (BinConverter) ---

		[TestMethod]
		public void AddAndGet_DifferentDataTypes_MaintainsIntegrity()
		{
			// Arrange
			const int size = 5;
			var backup = new Backup(size);
			var intData = 123;
			var stringData = "Hello World";
			var complexData = new List<int> { 1, 2, 3, 4, 5 };

			// Act
			backup.Add(intData);        // diff 2
			backup.Add(stringData);     // diff 1
			backup.Add(complexData);    // diff 0

			// Assert
			CollectionAssert.AreEqual(complexData, backup.Get<List<int>>(0));
			Assert.AreEqual(stringData, backup.Get(1));
			Assert.AreEqual(intData, backup.Get(2));
		}

		// --- Kiểm thử Ngẫu nhiên (Random) ---

		[TestMethod]
		public void AddAndGet_RandomDataAndOverflow_ConsistencyCheck()
		{
			// Arrange
			const int size = 5;
			var backup = new Backup(size);
			var random = new Random();
			var history = new List<TestData>();
			const int totalAdds = 20;

			// Act
			for (int i = 0; i < totalAdds; i++)
			{
				var data = new TestData { Id = i, Name = $"Item {i}", IsActive = random.Next(2) == 1 };
				backup.Add(data);
				history.Add(data);
			}

			// Assert
			// Kiểm tra 5 mục cuối cùng (0 đến 4)
			for (int diff = 0; diff < size; diff++)
			{
				// Mục newest là mục cuối cùng trong list history
				var expected = history[totalAdds - 1 - diff];
				var actual = backup.Get(diff);

				Assert.IsNotNull(actual, $"Item at diff {diff} should not be null.");
				Assert.AreEqual(expected, (TestData)actual, $"Item at diff {diff} should match the expected data.");
			}

			// Kiểm tra mục ngoài phạm vi buffer
			Assert.IsNull(backup.Get(size), $"Item at diff {size} (outside buffer) should be null.");
		}
	}
}
