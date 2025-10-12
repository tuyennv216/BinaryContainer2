using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests;

[TestClass]
public class DataContainer_ReadNumber_Tests
{
	private DataContainer _container;

	[TestInitialize]
	public void Setup()
	{
		// Khởi tạo DataContainer trước mỗi test.
		_container = new DataContainer();
	}

	/// <summary>
	/// Chuẩn bị container bằng cách gọi AddNumber.
	/// </summary>
	private void PrepareContainerWithNumbers(params int[] numbers)
	{
		// Cách đơn giản nhất là ghi thêm các số vào, và đọc lại từ đầu.
		foreach (var number in numbers)
		{
			_container.AddNumber(number);
		}

		_container.Arrays_Itor = 0; // Arrays_Itor luôn được sử dụng để đọc từ đầu Arrays.
	}

	/// <summary>
	/// Kiểm tra khi đọc một số đã được lưu dưới dạng 1 byte (Cờ = TRUE).
	/// </summary>
	[TestMethod]
	public void ReadNumber_StoredAsByte_ReturnsCorrectValue()
	{
		// Arrange
		int expectedNumber = 123;
		PrepareContainerWithNumbers(expectedNumber);
		int initialFlagsItor = _container.Flags.ReadOffset;
		int initialArraysItor = _container.Arrays_Itor; // 0

		// Act
		int actualNumber = _container.ReadNumber();

		// Assert
		Assert.AreEqual(expectedNumber, actualNumber, "Read number must match the expected byte value.");

		// 1. Kiểm tra con trỏ Flags đã di chuyển qua 1 bit
		Assert.AreEqual(initialFlagsItor + 1, _container.Flags.ReadOffset, "Flags cursor should advance by 1 bit.");

		// 1. Kiểm tra con trỏ Arrays đã di chuyển qua 1 byte
		Assert.AreEqual(initialArraysItor + 1, _container.Arrays_Itor, "Arrays cursor should advance by 1 byte.");
	}

	/// <summary>
	/// Kiểm tra khi đọc số 0, được lưu dưới dạng 1 byte (Cờ = TRUE).
	/// </summary>
	[TestMethod]
	public void ReadNumber_Zero_ReturnsCorrectValue()
	{
		// Arrange
		int expectedNumber = 0;
		PrepareContainerWithNumbers(expectedNumber);

		// Act
		int actualNumber = _container.ReadNumber();

		// Assert
		Assert.AreEqual(expectedNumber, actualNumber, "Read number must match 0.");
		Assert.AreEqual(1, _container.Arrays_Itor, "Arrays cursor should advance by 1 byte.");
	}

	/// <summary>
	/// Kiểm tra khi đọc một số đã được lưu dưới dạng 4 byte (Cờ = FALSE).
	/// </summary>
	[TestMethod]
	public void ReadNumber_StoredAsInt_ReturnsCorrectValue()
	{
		// Arrange
		int expectedNumber = 300000;
		PrepareContainerWithNumbers(expectedNumber);
		int initialFlagsItor = _container.Flags.ReadOffset;
		int initialArraysItor = _container.Arrays_Itor;

		// Act
		int actualNumber = _container.ReadNumber();

		// Assert
		Assert.AreEqual(expectedNumber, actualNumber, "Read number must match the expected int value.");

		// 1. Kiểm tra con trỏ Flags đã di chuyển qua 1 bit
		Assert.AreEqual(initialFlagsItor + 1, _container.Flags.ReadOffset, "Flags cursor should advance by 1 bit.");

		// 2. Kiểm tra con trỏ Arrays đã di chuyển qua 4 byte
		Assert.AreEqual(initialArraysItor + 4, _container.Arrays_Itor, "Arrays cursor should advance by 4 bytes.");
	}

	/// <summary>
	/// Kiểm tra khi đọc một số âm (luôn được lưu dưới dạng 4 byte, Cờ = FALSE).
	/// </summary>
	[TestMethod]
	public void ReadNumber_NegativeInt_ReturnsCorrectValue()
	{
		// Arrange
		int expectedNumber = -55555;
		PrepareContainerWithNumbers(expectedNumber);

		// Act
		int actualNumber = _container.ReadNumber();

		// Assert
		Assert.AreEqual(expectedNumber, actualNumber, "Read number must match the expected negative int value.");
		Assert.AreEqual(4, _container.Arrays_Itor, "Arrays cursor should advance by 4 bytes.");
	}

	/// <summary>
	/// Kiểm tra việc đọc xen kẽ các loại số (byte và int) theo đúng thứ tự.
	/// </summary>
	[TestMethod]
	public void ReadNumber_MixedNumbers_ReadsInCorrectOrder()
	{
		// Arrange
		int num1 = 200; // Byte (TRUE)
		int num2 = -1;  // Int (FALSE)
		int num3 = 1;   // Byte (TRUE)
		PrepareContainerWithNumbers(num1, num2, num3);

		// Act & Assert (Đọc num1 - Byte)
		Assert.AreEqual(num1, _container.ReadNumber(), "First number (byte) must be read correctly.");
		Assert.AreEqual(1, _container.Arrays_Itor, "Arrays cursor should be at 1 after reading 1 byte.");

		// Act & Assert (Đọc num2 - Int)
		Assert.AreEqual(num2, _container.ReadNumber(), "Second number (int) must be read correctly.");
		Assert.AreEqual(1 + 4, _container.Arrays_Itor, "Arrays cursor should be at 5 after reading 4 bytes.");

		// Act & Assert (Đọc num3 - Byte)
		Assert.AreEqual(num3, _container.ReadNumber(), "Third number (byte) must be read correctly.");
		Assert.AreEqual(1 + 4 + 1, _container.Arrays_Itor, "Arrays cursor should be at 6 after reading 1 byte.");
	}
}