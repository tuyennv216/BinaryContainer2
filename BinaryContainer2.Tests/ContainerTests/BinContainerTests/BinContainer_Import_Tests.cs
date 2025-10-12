using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_Import_Tests
	{
		/// <summary>
		/// Helper để tạo mảng byte đầu vào giống như hàm Export tạo ra.
		/// </summary>
		private byte[] CreateExportedBytes(byte offset, byte[] data)
		{
			// Length = 4 (Length) + 1 (Offset) + data.Length
			var totalLength = 4 + 1 + data.Length;
			var lengthBytes = BitConverter.GetBytes(totalLength);

			// Ghép mảng: Length + Offset + Data
			return lengthBytes.Concat(new byte[] { offset }).Concat(data).ToArray();
		}

		// ---

		/// <summary>
		/// Kiểm tra import container rỗng (chỉ có 4 byte Length = 4).
		/// </summary>
		[TestMethod]
		public void Import_EmptyContainer_ShouldClearAndSetCorrectReturnPosition()
		{
			// Arrange: Buffer chỉ chứa giá trị 4 (Length)
			var buffer = BitConverter.GetBytes(4);
			var container = new BinContainer();
			// Giả định container đã có dữ liệu trước đó để kiểm tra hàm Clear()
			container.Add(true);

			// Act
			var nextStartIndex = container.Import(buffer, 0);

			// Assert
			Assert.AreEqual(4, nextStartIndex, "Vị trí trả về phải là start + totalBytes (0 + 4).");
			Assert.AreEqual(0, container.Total, "Total phải là 0.");
			Assert.AreEqual(0, container.Offset, "Offset phải là 0.");
			Assert.AreEqual(0, container.Data.Count, "Data phải rỗng.");
		}

		// ---

		/// <summary>
		/// Kiểm tra import container với một byte dữ liệu đầy đủ (8 bit).
		/// Length = 6. Offset = 8. Data = [255].
		/// </summary>
		[TestMethod]
		public void Import_FullSingleByte_ShouldRestoreStateCorrectly()
		{
			// Arrange: 8 bit True. Data[0] = 255. Offset = 8.
			var dataBytes = new byte[] { 255 };
			var buffer = CreateExportedBytes(8, dataBytes);
			var container = new BinContainer();

			// Act
			var nextStartIndex = container.Import(buffer, 0);

			// Assert
			Assert.AreEqual(6, nextStartIndex, "Vị trí trả về phải là 6.");

			// Total = (dataSize - 1) * 8 + Offset = (1 - 1) * 8 + 8 = 8
			Assert.AreEqual(8, container.Total, "Total phải là 8 (8 bit).");

			// Offset = buffer[offset_Index] = 8
			Assert.AreEqual(8, container.Offset, "Offset phải là 8.");

			// Data
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			Assert.AreEqual(255, container.Data[0], "Data[0] phải là 255.");

			// Template = Data[dataSize - 1] = 255
			Assert.AreEqual(255, container.Template, "Template phải là 255.");
		}

		// ---

		/// <summary>
		/// Kiểm tra import container với dữ liệu không đầy đủ byte cuối.
		/// Dữ liệu: 8 False (Data[0]) + 3 True (Data[1]).
		/// Length = 7. Offset = 3. Data = [0, 7].
		/// </summary>
		[TestMethod]
		public void Import_PartialLastByte_ShouldCalculateTotalCorrectly()
		{
			// Arrange: 8 False + 3 True. Offset = 3. Data size = 2.
			var dataBytes = new byte[] { 0, 7 }; // 0b00000111 = 7
			var buffer = CreateExportedBytes(3, dataBytes);
			var container = new BinContainer();

			// Act
			container.Import(buffer, 0);

			// Assert
			// Total = (dataSize - 1) * 8 + Offset = (2 - 1) * 8 + 3 = 11
			Assert.AreEqual(11, container.Total, "Total phải là 11 (8 + 3 bit).");
			Assert.AreEqual(3, container.Offset, "Offset phải là 3.");
			Assert.AreEqual(2, container.Data.Count, "Data phải có 2 phần tử.");

			// Template = Data[dataSize - 1] = 7
			Assert.AreEqual(7, container.Template, "Template phải là 7.");

			// Kiểm tra dữ liệu
			Assert.AreEqual(0, container.Data[0]);
			Assert.AreEqual(7, container.Data[1]);
		}

		// ---

		/// <summary>
		/// Kiểm tra import khi có offset (start > 0) trong buffer.
		/// </summary>
		[TestMethod]
		public void Import_WithStartOffset_ShouldReadFromCorrectPosition()
		{
			// Arrange: Dữ liệu container thật. Offset = 1. Data = [1]. Length = 6.
			var containerData = CreateExportedBytes(1, new byte[] { 1 });
			// Buffer: [Garbage, Garbage, ContainerData...]
			var garbage = new byte[] { 0xAA, 0xBB };
			var buffer = garbage.Concat(containerData).ToArray();
			const int START = 2; // Bắt đầu đọc từ vị trí 2

			var container = new BinContainer();

			// Act
			var nextStartIndex = container.Import(buffer, START);

			// Assert
			// totalBytes = 6. nextStartIndex = START + totalBytes = 2 + 6 = 8.
			Assert.AreEqual(8, nextStartIndex, "Vị trí trả về phải là 8.");

			// Khôi phục Total = (1-1)*8 + 1 = 1.
			Assert.AreEqual(1, container.Total, "Total phải là 1.");
			Assert.AreEqual(1, container.Offset, "Offset phải là 1.");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			Assert.AreEqual(1, container.Data[0], "Data[0] phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp DataSize = 0 (tổng Length = 5).
		/// </summary>
		[TestMethod]
		public void Import_LengthIsFive_ShouldBeTreatedAsNoData()
		{
			// Arrange: Buffer cho Length = 5. totalBytes = 5. dataSize = 5 - 4 - 1 = 0.
			var buffer = BitConverter.GetBytes(5).Concat(new byte[] { 5 }).ToArray(); // Length=5, Offset=5
			var container = new BinContainer();
			container.Add(true); // Để kiểm tra Clear()

			// Act
			container.Import(buffer, 0);

			// Assert: Khi dataSize <= 0, container sẽ sạch nhưng Offset có thể được ghi.
			Assert.AreEqual(0, container.Data.Count, "Data phải rỗng vì dataSize = 0.");
			// Offset sẽ được gán là 5.
			Assert.AreEqual(5, container.Offset, "Offset phải là 5 (được đọc từ buffer).");
			Assert.AreEqual(0, container.Total, "Total phải là 0 vì không có Data.");
		}
	}
}