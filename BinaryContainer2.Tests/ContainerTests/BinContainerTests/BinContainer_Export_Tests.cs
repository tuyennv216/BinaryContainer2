using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_Export_Tests
	{
		/// <summary>
		/// Kiểm tra trường hợp container rỗng.
		/// Đầu ra chỉ nên là 4 byte chứa giá trị 4 (tổng độ dài).
		/// </summary>
		[TestMethod]
		public void Export_EmptyContainer_ShouldContainOnlyLength()
		{
			// Arrange
			var container = new BinContainer(); // Data.Count = 0. Length = 4.

			// Act
			var exportedBytes = container.Export();

			// Assert
			// Độ dài mong muốn: 4 byte (Length)
			Assert.AreEqual(4, exportedBytes.Length, "Độ dài mảng byte phải là 4.");

			// Giá trị 4 phải được ghi dưới dạng Int32 Little-Endian
			var expectedLengthBytes = BitConverter.GetBytes(4);
			CollectionAssert.AreEqual(expectedLengthBytes, exportedBytes, "4 byte đầu tiên phải là giá trị Length (4).");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp dữ liệu chỉ chứa một byte đầy đủ.
		/// Độ dài: 4 (Length) + 1 (Offset) + 1 (Data) = 6.
		/// Offset: 8.
		/// </summary>
		[TestMethod]
		public void Export_FullSingleByte_ShouldContainLengthOffsetAndData()
		{
			// Arrange: 8 bit True. Data[0] = 255. Offset = 8. Length = 6.
			var container = new BinContainer();
			container.AddArray(Enumerable.Repeat(true, 8).ToArray());

			// Kiểm tra nội bộ trước khi export
			Assert.AreEqual(6, container.TotalExportBytes);
			Assert.AreEqual(8, container.Offset);
			Assert.AreEqual(1, container.Data.Count);

			// Act
			var exportedBytes = container.Export();

			// Assert
			// Độ dài mong muốn: 6
			Assert.AreEqual(6, exportedBytes.Length, "Độ dài mảng byte phải là 6.");

			// 1. Length (4 bytes)
			var expectedLengthBytes = BitConverter.GetBytes(6);
			CollectionAssert.AreEqual(expectedLengthBytes, exportedBytes.Take(4).ToArray(), "4 byte đầu tiên phải là Length (6).");

			// 2. Offset (1 byte) - Vị trí 4
			Assert.AreEqual(8, exportedBytes[4], "Byte thứ 5 phải là Offset (8).");

			// 3. Data (1 byte) - Vị trí 5
			Assert.AreEqual(255, exportedBytes[5], "Byte thứ 6 phải là dữ liệu (255).");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp dữ liệu chứa nhiều byte và byte cuối cùng không đầy đủ.
		/// Dữ liệu: 8 False (Data[0]) + 3 True (Data[1]).
		/// Length: 4 (Length) + 1 (Offset) + 2 (Data) = 7.
		/// Offset: 3.
		/// </summary>
		[TestMethod]
		public void Export_MultipleBytesPartialLast_ShouldContainCorrectStructure()
		{
			// Arrange: 8 False + 3 True. Total = 11.
			var data = Enumerable.Repeat(false, 8)
								 .Concat(Enumerable.Repeat(true, 3))
								 .ToArray();
			var container = new BinContainer();
			container.AddArray(data);

			// Kiểm tra nội bộ
			Assert.AreEqual(7, container.TotalExportBytes);
			Assert.AreEqual(3, container.Offset);
			Assert.AreEqual(2, container.Data.Count); // 2 bytes
			Assert.AreEqual(0, container.Data[0]);
			Assert.AreEqual(7, container.Data[1]); // 0b00000111 = 7

			// Act
			var exportedBytes = container.Export();

			// Assert
			// Độ dài mong muốn: 7
			Assert.AreEqual(7, exportedBytes.Length, "Độ dài mảng byte phải là 7.");

			// 1. Length (4 bytes)
			var expectedLengthBytes = BitConverter.GetBytes(7);
			CollectionAssert.AreEqual(expectedLengthBytes, exportedBytes.Take(4).ToArray(), "4 byte đầu tiên phải là Length (7).");

			// 2. Offset (1 byte) - Vị trí 4
			Assert.AreEqual(3, exportedBytes[4], "Byte thứ 5 phải là Offset (3).");

			// 3. Data (2 bytes) - Vị trí 5 và 6
			Assert.AreEqual(0, exportedBytes[5], "Byte thứ 6 phải là Data[0] (0).");
			Assert.AreEqual(7, exportedBytes[6], "Byte thứ 7 phải là Data[1] (7).");
		}
	}
}