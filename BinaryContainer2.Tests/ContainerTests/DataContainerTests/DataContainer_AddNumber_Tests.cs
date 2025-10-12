using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests;

[TestClass]
public class DataContainer_AddNumber_Tests
{
	private DataContainer _container;

	[TestInitialize]
	public void Setup()
	{
		// Khởi tạo DataContainer trước mỗi test.
		_container = new DataContainer();
	}

	/// <summary>
	/// Kiểm tra khi thêm một số nằm trong phạm vi byte (ví dụ: 10).
	/// </summary>
	[TestMethod]
	public void AddNumber_ByteRange_SavesAsOneByteAndFlagTrue()
	{
		// Arrange
		int number = 10;
		int initialFlagsCount = _container.Flags.Total;
		int initialArraysCount = _container.Arrays.Count;

		// Act
		_container.AddNumber(number);

		// Assert
		// 1. Kiểm tra Flags: Phải thêm 1 cờ và cờ đó phải là TRUE
		Assert.AreEqual(initialFlagsCount + 1, _container.Flags.Total, "Flags count should increase by 1.");
		Assert.IsTrue(_container.Flags.ReadAt(initialFlagsCount), "The new flag should be TRUE (is a byte).");

		// 2. Kiểm tra Arrays: Phải thêm 1 byte
		Assert.AreEqual(initialArraysCount + 1, _container.Arrays.Count, "Arrays count should increase by 1.");

		// 3. Kiểm tra giá trị byte đã lưu
		Assert.AreEqual((byte)number, _container.Arrays[initialArraysCount], "The stored value should be the byte value of the number.");
	}

	/// <summary>
	/// Kiểm tra khi thêm số 0 (giá trị biên dưới của byte.MinValue).
	/// </summary>
	[TestMethod]
	public void AddNumber_Zero_SavesAsOneByteAndFlagTrue()
	{
		// Arrange
		int number = 0;
		int initialFlagsCount = _container.Flags.Total;
		int initialArraysCount = _container.Arrays.Count;

		// Act
		_container.AddNumber(number);

		// Assert
		Assert.IsTrue(_container.Flags.ReadAt(initialFlagsCount), "The new flag should be TRUE for 0, as 0 is >= byte.MinValue.");
		Assert.AreEqual(initialArraysCount + 1, _container.Arrays.Count, "Arrays count should increase by 1.");
		Assert.AreEqual(0, _container.Arrays[initialArraysCount], "The stored value should be 0.");
	}

	/// <summary>
	/// Kiểm tra khi thêm số 255 (giá trị biên trên của byte.MaxValue).
	/// </summary>
	[TestMethod]
	public void AddNumber_MaxValueByte_SavesAsOneByteAndFlagTrue()
	{
		// Arrange
		int number = byte.MaxValue; // 255
		int initialFlagsCount = _container.Flags.Total;
		int initialArraysCount = _container.Arrays.Count;

		// Act
		_container.AddNumber(number);

		// Assert
		Assert.IsTrue(_container.Flags.ReadAt(initialFlagsCount), "The new flag should be TRUE for 255.");
		Assert.AreEqual(initialArraysCount + 1, _container.Arrays.Count, "Arrays count should increase by 1.");
		Assert.AreEqual(byte.MaxValue, _container.Arrays[initialArraysCount], "The stored value should be 255.");
	}

	/// <summary>
	/// Kiểm tra khi thêm một số vượt quá phạm vi byte (ví dụ: 256).
	/// </summary>
	[TestMethod]
	public void AddNumber_IntRange_SavesAsFourBytesAndFlagFalse()
	{
		// Arrange
		int number = byte.MaxValue + 1; // 256
		int initialFlagsCount = _container.Flags.Total;
		int initialArraysCount = _container.Arrays.Count;
		byte[] expectedBytes = BitConverter.GetBytes(number);

		// Act
		_container.AddNumber(number);

		// Assert
		// 1. Kiểm tra Flags: Phải thêm 1 cờ và cờ đó phải là FALSE
		Assert.AreEqual(initialFlagsCount + 1, _container.Flags.Total, "Flags count should increase by 1.");
		Assert.IsFalse(_container.Flags.ReadAt(initialFlagsCount), "The new flag should be FALSE (is an int) because 256 > byte.MaxValue.");

		// 2. Kiểm tra Arrays: Phải thêm 4 byte
		Assert.AreEqual(initialArraysCount + 4, _container.Arrays.Count, "Arrays count should increase by 4.");

		// 3. Kiểm tra các byte đã lưu có khớp với BitConverter.GetBytes(number) không
		var actualBytes = _container.Arrays.GetRange(initialArraysCount, 4).ToArray();
		CollectionAssert.AreEqual(expectedBytes, actualBytes, "The stored bytes should match BitConverter.GetBytes(number).");
	}

	/// <summary>
	/// Kiểm tra khi thêm một số nguyên âm (luôn luôn là int).
	/// </summary>
	[TestMethod]
	public void AddNumber_NegativeInt_SavesAsFourBytesAndFlagFalse()
	{
		// Arrange
		int number = -100;
		int initialFlagsCount = _container.Flags.Total;
		int initialArraysCount = _container.Arrays.Count;
		byte[] expectedBytes = BitConverter.GetBytes(number);

		// Act
		_container.AddNumber(number);

		// Assert
		// 1. Kiểm tra Flags: Phải thêm 1 cờ và cờ đó phải là FALSE
		Assert.IsFalse(_container.Flags.ReadAt(initialFlagsCount), "The new flag should be FALSE for a negative number because it is < byte.MinValue.");

		// 2. Kiểm tra Arrays: Phải thêm 4 byte
		Assert.AreEqual(initialArraysCount + 4, _container.Arrays.Count, "Arrays count should increase by 4.");

		// 3. Kiểm tra các byte đã lưu
		var actualBytes = _container.Arrays.GetRange(initialArraysCount, 4).ToArray();
		CollectionAssert.AreEqual(expectedBytes, actualBytes, "The stored bytes should match BitConverter.GetBytes(-100).");
	}
}