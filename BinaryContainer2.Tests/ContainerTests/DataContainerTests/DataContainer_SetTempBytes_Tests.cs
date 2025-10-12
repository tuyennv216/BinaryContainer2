using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests;

[TestClass]
public class DataContainer_SetTempBytes_Tests
{
	private DataContainer _container;

	[TestInitialize]
	public void Setup()
	{
		// Khởi tạo DataContainer trước mỗi test.
		// Cần đảm bảo rằng TempBytesPosition đã được khởi tạo bên trong DataContainer's constructor (Stack<int>).
		_container = new DataContainer();
	}

	/// <summary>
	/// Kiểm tra trường hợp cơ bản: Đặt byte vào một vị trí đã được cấp phát.
	/// </summary>
	[TestMethod]
	public void SetTempBytes_BasicScenario_OverwritesCorrectRange()
	{
		// Arrange
		int tempByteCount = 5;
		byte[] dataToSet = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE };

		// BƯỚC 1: Cấp phát không gian (AddTempBytes)
		// Items: [0, 0, 0, 0, 0], TempBytesPosition: [0]
		_container.AddTempBytes(tempByteCount);
		int startPosition = _container.TempBytesPosition.Peek(); // Phải là 0

		// Act
		// BƯỚC 2: Ghi đè dữ liệu (SetTempBytes)
		_container.SetTempBytes(dataToSet);

		// Assert
		// 1. Kiểm tra Stack đã pop
		Assert.AreEqual(0, _container.TempBytesPosition.Count, "TempBytesPosition should be empty after Pop.");

		// 2. Kiểm tra dữ liệu trong Items đã được ghi đè chính xác
		for (int i = 0; i < dataToSet.Length; i++)
		{
			Assert.AreEqual(dataToSet[i], _container.Items[startPosition + i], $"Byte at index {startPosition + i} must be overwritten correctly.");
		}

		// 3. Kiểm tra tổng số phần tử không thay đổi
		Assert.AreEqual(tempByteCount, _container.Items.Count, "Items.Count should not change after setting temp bytes.");
	}
    
    /// <summary>
    /// Kiểm tra khi có dữ liệu tồn tại trước và sau vùng cấp phát tạm thời.
    /// </summary>
    [TestMethod]
	public void SetTempBytes_SurroundedData_OverwritesOnlyTempRegion()
	{
		// Arrange
		byte[] preData = new byte[] { 1, 2, 3 };
		byte[] postData = new byte[] { 9, 8 };
		int tempByteCount = 4;
		byte[] dataToSet = new byte[] { 0xF1, 0xF2, 0xF3, 0xF4 };

		// Dữ liệu ban đầu
		_container.Items.AddRange(preData); // Items: [1, 2, 3]
		int startPosition = _container.Items.Count; // Phải là 3

		// Cấp phát không gian tạm thời
		_container.AddTempBytes(tempByteCount); // Items: [1, 2, 3, 0, 0, 0, 0], TempBytesPosition: [3]
		_container.Items.AddRange(postData); // Items: [1, 2, 3, 0, 0, 0, 0, 9, 8]

		// Act
		_container.SetTempBytes(dataToSet); // Ghi đè từ index 3

		// Assert
		// 1. Kiểm tra dữ liệu trước không thay đổi
		Assert.AreEqual(1, _container.Items[0], "Pre-data at index 0 should be unchanged.");

		// 2. Kiểm tra dữ liệu đã được ghi đè chính xác
		for (int i = 0; i < dataToSet.Length; i++)
		{
			Assert.AreEqual(dataToSet[i], _container.Items[startPosition + i], $"Temp byte at index {startPosition + i} must be overwritten correctly.");
		}

		// 3. Kiểm tra dữ liệu sau không thay đổi
		Assert.AreEqual(9, _container.Items[startPosition + tempByteCount], "Post-data should remain unchanged.");
		Assert.AreEqual(8, _container.Items[startPosition + tempByteCount + 1], "Post-data should remain unchanged.");

		// 4. Tổng số phần tử phải đúng
		Assert.AreEqual(preData.Length + tempByteCount + postData.Length, _container.Items.Count);
	}

    /// <summary>
    /// Kiểm tra khi kích thước mảng byte muốn đặt nhỏ hơn kích thước vùng đã cấp phát.
    /// Phương thức SetTempBytes hiện tại không kiểm tra điều kiện này và sẽ chỉ ghi đè một phần.
    /// </summary>
    [TestMethod]
	public void SetTempBytes_SmallerDataThanAllocated_PartialOverwriteAndRemainingZeros()
	{
		// Arrange
		int allocatedCount = 8;
		byte[] partialData = new byte[] { 0xA1, 0xA2, 0xA3 }; // Kích thước 3

		_container.AddTempBytes(allocatedCount); // Items: [0, 0, 0, 0, 0, 0, 0, 0], Pos: [0]
		int startPosition = _container.TempBytesPosition.Peek();

		// Act
		_container.SetTempBytes(partialData); // Ghi đè 3 byte đầu

		// Assert
		// 1. Kiểm tra 3 byte đầu tiên đã được ghi đè
		Assert.AreEqual(partialData[0], _container.Items[startPosition + 0]);
		Assert.AreEqual(partialData[2], _container.Items[startPosition + 2]);

		// 2. Kiểm tra các byte còn lại (từ index 3 đến 7) vẫn là 0 (hoặc giá trị cũ của vùng cấp phát)
		Assert.AreEqual(0, _container.Items[startPosition + 3], "Bytes outside of 'bytes.Length' should retain their previous value (0).");
		Assert.AreEqual(0, _container.Items[startPosition + 7], "Bytes outside of 'bytes.Length' should retain their previous value (0).");
	}

    /// <summary>
    /// Kiểm tra khi gọi Add/Set TempBytes nhiều lần (kiểm tra Pop() đúng).
    /// </summary>
    [TestMethod]
	public void SetTempBytes_MultipleCalls_PopsAndOverwritesCorrectly()
	{
		// Arrange
		// Call 1: Vùng 1
		_container.AddTempBytes(5); // Pos: [0]
									// Call 2: Vùng 2
		_container.AddTempBytes(3); // Pos: [0, 5]
									// Call 3: Vùng 3
		_container.AddTempBytes(1); // Pos: [0, 5, 8]

		byte[] data3 = new byte[] { 0x33 }; // 1 byte
		byte[] data2 = new byte[] { 0x22, 0x22, 0x22 }; // 3 bytes
		byte[] data1 = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11 }; // 5 bytes

		// Act
		// Thực hiện Set theo thứ tự ngược lại (LIFO)
		_container.SetTempBytes(data3); // Pop 8. Ghi từ index 8
		_container.SetTempBytes(data2); // Pop 5. Ghi từ index 5
		_container.SetTempBytes(data1); // Pop 0. Ghi từ index 0

		// Assert
		// 1. Kiểm tra Stack đã rỗng
		Assert.AreEqual(0, _container.TempBytesPosition.Count, "TempBytesPosition must be empty.");

		// 2. Kiểm tra dữ liệu Vùng 1 (index 0-4)
		Assert.AreEqual(0x11, _container.Items[0]);
		Assert.AreEqual(0x11, _container.Items[4]);

		// 3. Kiểm tra dữ liệu Vùng 2 (index 5-7)
		Assert.AreEqual(0x22, _container.Items[5]);
		Assert.AreEqual(0x22, _container.Items[7]);

		// 4. Kiểm tra dữ liệu Vùng 3 (index 8)
		Assert.AreEqual(0x33, _container.Items[8]);
	}
}