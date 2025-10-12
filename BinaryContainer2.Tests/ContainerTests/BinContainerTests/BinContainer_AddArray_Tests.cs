using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_AddArray_Tests
	{
		/// <summary>
		/// Kiểm tra trường hợp mảng rỗng. Container phải không thay đổi.
		/// </summary>
		[TestMethod]
		public void AddArray_EmptyArray_ShouldNotChangeContainerState()
		{
			// Arrange
			var container = new BinContainer();
			var data = new bool[] { };

			// Khởi tạo trạng thái ban đầu để kiểm tra
			Assert.AreEqual(0, container.Total);
			Assert.AreEqual(0, container.Data.Count);

			// Act
			container.AddArray(data);

			// Assert
			Assert.AreEqual(0, container.Total, "Total phải giữ nguyên là 0.");
			Assert.AreEqual(0, container.Data.Count, "Data.Count phải giữ nguyên là 0.");
			Assert.AreEqual(0, container.Offset, "Offset phải giữ nguyên là 0.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp mảng chỉ có các giá trị TRUE, không gây tràn byte.
		/// Thêm 5 bit TRUE (Offset = 5, Total = 5). Byte đầu tiên = 0b00011111 = 31.
		/// </summary>
		[TestMethod]
		public void AddArray_SimpleTrueBits_ShouldFillPartialByte()
		{
			// Arrange
			var container = new BinContainer();
			// 5 bits TRUE
			var data = Enumerable.Repeat(true, 5).ToArray();

			// Act
			container.AddArray(data);

			// Assert
			Assert.AreEqual(5, container.Total, "Total phải là 5.");
			Assert.AreEqual(5, container.Offset, "Offset phải là 5.");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			// 0b00011111 = 31
			Assert.AreEqual(31, container.Data[0], "Byte 0 phải là 31.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp mảng có các giá trị FALSE và TRUE, không gây tràn byte.
		/// Thêm [T, F, T, F] (Offset = 4, Total = 4). Byte đầu tiên = 0b00000101 = 5.
		/// </summary>
		[TestMethod]
		public void AddArray_MixedBits_ShouldFillPartialByte()
		{
			// Arrange
			var container = new BinContainer();
			var data = new bool[] { true, false, true, false };

			// Act
			container.AddArray(data);

			// Assert
			Assert.AreEqual(4, container.Total, "Total phải là 4.");
			Assert.AreEqual(4, container.Offset, "Offset phải là 4.");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			// Bit 0 và 2 là 1. 0b00000101 = 5
			Assert.AreEqual(5, container.Data[0], "Byte 0 phải là 5.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp mảng gây tràn byte chính xác 1 lần (tổng 12 bit).
		/// Byte 0: 8 bits TRUE = 255. Byte 1: 4 bits TRUE = 15.
		/// </summary>
		[TestMethod]
		public void AddArray_OverflowOneTime_ShouldStartNewByte()
		{
			// Arrange
			var container = new BinContainer();
			// 12 bits TRUE
			var data = Enumerable.Repeat(true, 12).ToArray();

			// Act
			container.AddArray(data);

			// Assert
			Assert.AreEqual(12, container.Total, "Total phải là 12.");
			// 12 % 8 = 4.
			Assert.AreEqual(4, container.Offset, "Offset phải là 4.");
			Assert.AreEqual(2, container.Data.Count, "Data phải có 2 phần tử.");

			// Byte 0: 8x True = 255
			Assert.AreEqual(255, container.Data[0], "Byte 0 phải là 255.");
			// Byte 1: 4x True = 0b00001111 = 15
			Assert.AreEqual(15, container.Data[1], "Byte 1 phải là 15.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp mảng gây tràn byte nhiều lần (tổng 20 bit).
		/// Byte 0: 8 bits T = 255. Byte 1: 8 bits F = 0. Byte 2: 4 bits T = 15.
		/// </summary>
		[TestMethod]
		public void AddArray_OverflowMultipleTimes_ShouldUseMultipleBytes()
		{
			// Arrange
			var container = new BinContainer();
			// 20 bits: 8T + 8F + 4T
			var data = Enumerable.Repeat(true, 8)
								 .Concat(Enumerable.Repeat(false, 8))
								 .Concat(Enumerable.Repeat(true, 4))
								 .ToArray();

			// Act
			container.AddArray(data);

			// Assert
			Assert.AreEqual(20, container.Total, "Total phải là 20.");
			// 20 % 8 = 4.
			Assert.AreEqual(4, container.Offset, "Offset phải là 4.");
			// ceil(20/8) = 3
			Assert.AreEqual(3, container.Data.Count, "Data phải có 3 phần tử.");

			Assert.AreEqual(255, container.Data[0], "Byte 0 phải là 255 (8T).");
			Assert.AreEqual(0, container.Data[1], "Byte 1 phải là 0 (8F).");
			Assert.AreEqual(15, container.Data[2], "Byte 2 phải là 15 (4T).");
		}
	}
}