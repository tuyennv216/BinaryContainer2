using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests;

[TestClass]
public class DataContainer_ReadArrays_Tests
{
	private DataContainer _container;

	[TestInitialize]
	public void Setup()
	{
		_container = new DataContainer();

		// Thiết lập Arrays ban đầu để có dữ liệu đọc (giả lập dữ liệu đã được ghi)
		// Dữ liệu mẫu: [11, 22, 33, 44, 55, 66, 77, 88]
		_container.Arrays.AddRange(new byte[] { 11, 22, 33, 44, 55, 66, 77, 88 });
		_container.Arrays_Itor = 0; // Đặt con trỏ về đầu
	}

	/// <summary>
	/// Kiểm tra việc đọc một số lượng byte nhỏ hơn tổng số byte có sẵn.
	/// </summary>
	[TestMethod]
	public void ReadArrays_ReadPartialData_ReturnsCorrectBytesAndUpdatesItor()
	{
		// Arrange
		int lengthToRead = 4;
		int initialItor = _container.Arrays_Itor; // 0
		byte[] expectedBytes = new byte[] { 11, 22, 33, 44 };

		// Act
		byte[] actualBytes = _container.ReadArrays(lengthToRead);

		// Assert
		// 1. Kiểm tra mảng kết quả
		Assert.IsNotNull(actualBytes, "The result should not be null when reading within bounds.");
		CollectionAssert.AreEqual(expectedBytes, actualBytes, "The read bytes must match the expected segment.");

		// 2. Kiểm tra con trỏ Arrays_Itor đã được cập nhật
		Assert.AreEqual(initialItor + lengthToRead, _container.Arrays_Itor, "Arrays_Itor should advance by the read length (4).");
	}

	/// <summary>
	/// Kiểm tra việc đọc tất cả các byte có sẵn trong một lần gọi.
	/// </summary>
	[TestMethod]
	public void ReadArrays_ReadAllData_ReturnsAllBytesAndSetsItorToEnd()
	{
		// Arrange
		int lengthToRead = _container.Arrays.Count; // 8
		int initialItor = _container.Arrays_Itor; // 0

		// Act
		byte[] actualBytes = _container.ReadArrays(lengthToRead);

		// Assert
		// 1. Kiểm tra mảng kết quả
		Assert.IsNotNull(actualBytes, "The result should not be null when reading the entire list.");
		Assert.AreEqual(lengthToRead, actualBytes.Length, "The length of the returned array must be correct.");

		// 2. Kiểm tra con trỏ Arrays_Itor đã được cập nhật
		Assert.AreEqual(initialItor + lengthToRead, _container.Arrays_Itor, "Arrays_Itor should be set to the total count (8).");
	}

	/// <summary>
	/// Kiểm tra việc đọc nhiều lần liên tiếp, đảm bảo con trỏ hoạt động đúng.
	/// </summary>
	[TestMethod]
	public void ReadArrays_SequentialReads_UpdatesItorCorrectly()
	{
		// Arrange
		int read1Length = 3; // [11, 22, 33]
		int read2Length = 5; // [44, 55, 66, 77, 88]

		// Act 1
		_container.ReadArrays(read1Length);

		// Assert 1
		Assert.AreEqual(read1Length, _container.Arrays_Itor, "Itor should be 3 after first read.");

		// Act 2
		byte[] read2Bytes = _container.ReadArrays(read2Length);

		// Assert 2
		Assert.AreEqual(read1Length + read2Length, _container.Arrays_Itor, "Itor should be 8 after second read.");
		Assert.AreEqual(44, read2Bytes[0], "The first byte of the second read should be 44.");
		Assert.AreEqual(88, read2Bytes[4], "The last byte of the second read should be 88.");
	}

	/// <summary>
	/// Kiểm tra khi cố gắng đọc vượt quá số byte còn lại.
	/// </summary>
	[TestMethod]
	public void ReadArrays_ReadBeyondBoundary_ReturnsNullAndDoesNotUpdateItor()
	{
		// Arrange
		_container.Arrays_Itor = 6; // Có 2 byte còn lại ([77, 88])
		int lengthToRead = 3; // Cố gắng đọc 3 byte
		int initialItor = _container.Arrays_Itor; // 6

		// Act
		byte[] actualBytes = _container.ReadArrays(lengthToRead);

		// Assert
		// 1. Kiểm tra kết quả
		Assert.IsNull(actualBytes, "Reading beyond the boundary should return null.");

		// 2. Kiểm tra con trỏ Arrays_Itor
		Assert.AreEqual(initialItor, _container.Arrays_Itor, "Arrays_Itor should not be updated if reading failed.");
	}

	/// <summary>
	/// Kiểm tra khi cố gắng đọc 0 byte.
	/// </summary>
	[TestMethod]
	public void ReadArrays_ReadZeroLength_ReturnsEmptyArrayAndDoesNotUpdateItor()
	{
		// Arrange
		_container.Arrays_Itor = 4;
		int lengthToRead = 0;
		int initialItor = _container.Arrays_Itor; // 4

		// Act
		byte[] actualBytes = _container.ReadArrays(lengthToRead);

		// Assert
		// 1. Kiểm tra kết quả: ReadArrays trả về một mảng byte rỗng.
		Assert.IsNotNull(actualBytes, "The result should be an empty array, not null, when length is 0.");
		Assert.AreEqual(0, actualBytes.Length, "The returned array should have length 0.");

		// 2. Kiểm tra con trỏ Arrays_Itor
		Assert.AreEqual(initialItor, _container.Arrays_Itor, "Arrays_Itor should not be updated when length is 0.");
	}

	/// <summary>
	/// Kiểm tra khi Arrays rỗng.
	/// </summary>
	[TestMethod]
	public void ReadArrays_EmptyArraysList_ReturnsNull()
	{
		// Arrange
		// Khởi tạo container mới không có dữ liệu Arrays
		var emptyContainer = new DataContainer();
		int lengthToRead = 1;

		// Act
		byte[] actualBytes = emptyContainer.ReadArrays(lengthToRead);

		// Assert
		Assert.IsNull(actualBytes, "Reading from an empty Arrays list should return null.");
		Assert.AreEqual(0, emptyContainer.Arrays_Itor, "Arrays_Itor should remain 0.");
	}
}