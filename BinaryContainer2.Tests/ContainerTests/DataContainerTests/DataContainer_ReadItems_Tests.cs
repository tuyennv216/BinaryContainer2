using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests;

[TestClass]
public class DataContainer_ReadItems_Tests
{
	private DataContainer _container;

	[TestInitialize]
	public void Setup()
	{
		_container = new DataContainer();
		// Thiết lập Items ban đầu để có dữ liệu đọc (metadata không ảnh hưởng đến Items).
		// Dữ liệu mẫu: [10, 20, 30, 40, 50, 60, 70, 80]
		_container.Items.AddRange(new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 });
		_container.Items_Itor = 0; // Đặt con trỏ về đầu
	}

	/// <summary>
	/// Kiểm tra việc đọc một số lượng byte nhỏ hơn tổng số byte có sẵn.
	/// </summary>
	[TestMethod]
	public void ReadItems_ReadPartialData_ReturnsCorrectBytesAndUpdatesItor()
	{
		// Arrange
		int lengthToRead = 3;
		int initialItor = _container.Items_Itor; // 0
		byte[] expectedBytes = new byte[] { 10, 20, 30 };

		// Act
		byte[] actualBytes = _container.ReadItems(lengthToRead);

		// Assert
		// 1. Kiểm tra mảng kết quả
		Assert.IsNotNull(actualBytes, "The result should not be null when reading within bounds.");
		CollectionAssert.AreEqual(expectedBytes, actualBytes, "The read bytes must match the expected segment.");

		// 2. Kiểm tra con trỏ Items_Itor đã được cập nhật
		Assert.AreEqual(initialItor + lengthToRead, _container.Items_Itor, "Items_Itor should advance by the read length (3).");
	}

	/// <summary>
	/// Kiểm tra việc đọc tất cả các byte có sẵn trong một lần gọi.
	/// </summary>
	[TestMethod]
	public void ReadItems_ReadAllData_ReturnsAllBytesAndSetsItorToEnd()
	{
		// Arrange
		int lengthToRead = _container.Items.Count; // 8
		int initialItor = _container.Items_Itor; // 0

		// Act
		byte[] actualBytes = _container.ReadItems(lengthToRead);

		// Assert
		// 1. Kiểm tra mảng kết quả
		Assert.IsNotNull(actualBytes, "The result should not be null when reading the entire list.");
		Assert.AreEqual(lengthToRead, actualBytes.Length, "The length of the returned array must be correct.");

		// 2. Kiểm tra con trỏ Items_Itor đã được cập nhật
		Assert.AreEqual(initialItor + lengthToRead, _container.Items_Itor, "Items_Itor should be set to the total count (8).");
	}

	/// <summary>
	/// Kiểm tra việc đọc nhiều lần liên tiếp, đảm bảo con trỏ hoạt động đúng.
	/// </summary>
	[TestMethod]
	public void ReadItems_SequentialReads_UpdatesItorCorrectly()
	{
		// Arrange
		int read1Length = 2;
		int read2Length = 4;

		// Act 1
		_container.ReadItems(read1Length); // Đọc 2 byte: [10, 20]

		// Assert 1
		Assert.AreEqual(read1Length, _container.Items_Itor, "Itor should be 2 after first read.");

		// Act 2
		byte[] read2Bytes = _container.ReadItems(read2Length); // Đọc 4 byte: [30, 40, 50, 60]

		// Assert 2
		Assert.AreEqual(read1Length + read2Length, _container.Items_Itor, "Itor should be 6 after second read.");
		Assert.AreEqual(30, read2Bytes[0], "The first byte of the second read should be 30.");
		Assert.AreEqual(60, read2Bytes[3], "The last byte of the second read should be 60.");
	}

	/// <summary>
	/// Kiểm tra khi cố gắng đọc vượt quá số byte còn lại.
	/// </summary>
	[TestMethod]
	public void ReadItems_ReadBeyondBoundary_ReturnsNullAndDoesNotUpdateItor()
	{
		// Arrange
		_container.Items_Itor = 5; // Có 3 byte còn lại ([60, 70, 80])
		int lengthToRead = 4; // Cố gắng đọc 4 byte
		int initialItor = _container.Items_Itor; // 5

		// Act
		byte[] actualBytes = _container.ReadItems(lengthToRead);

		// Assert
		// 1. Kiểm tra kết quả
		Assert.IsNull(actualBytes, "Reading beyond the boundary should return null.");

		// 2. Kiểm tra con trỏ Items_Itor
		Assert.AreEqual(initialItor, _container.Items_Itor, "Items_Itor should not be updated if reading failed.");
	}

	/// <summary>
	/// Kiểm tra khi cố gắng đọc 0 byte.
	/// </summary>
	[TestMethod]
	public void ReadItems_ReadZeroLength_ReturnsEmptyArrayAndDoesNotUpdateItor()
	{
		// Arrange
		_container.Items_Itor = 2;
		int lengthToRead = 0;
		int initialItor = _container.Items_Itor; // 2

		// Act
		byte[] actualBytes = _container.ReadItems(lengthToRead);

		// Assert
		// 1. Kiểm tra kết quả: ReadItems sẽ trả về một mảng byte rỗng (do `new byte[0]`), không phải null.
		// Tuy nhiên, logic hiện tại là: `var result = new byte[length];` (length=0) -> mảng rỗng. 
		// Sau đó `Items_Itor += length;` (length=0) -> Itor không đổi.
		Assert.IsNotNull(actualBytes, "The result should be an empty array, not null, when length is 0.");
		Assert.AreEqual(0, actualBytes.Length, "The returned array should have length 0.");

		// 2. Kiểm tra con trỏ Items_Itor
		Assert.AreEqual(initialItor, _container.Items_Itor, "Items_Itor should not be updated when length is 0.");
	}

	/// <summary>
	/// Kiểm tra khi Items rỗng.
	/// </summary>
	[TestMethod]
	public void ReadItems_EmptyItemsList_ReturnsNull()
	{
		// Arrange
		// Khởi tạo container mới không có dữ liệu Items
		var emptyContainer = new DataContainer();
		int lengthToRead = 1;

		// Act
		byte[] actualBytes = emptyContainer.ReadItems(lengthToRead);

		// Assert
		Assert.IsNull(actualBytes, "Reading from an empty Items list should return null.");
		Assert.AreEqual(0, emptyContainer.Items_Itor, "Items_Itor should remain 0.");
	}
}