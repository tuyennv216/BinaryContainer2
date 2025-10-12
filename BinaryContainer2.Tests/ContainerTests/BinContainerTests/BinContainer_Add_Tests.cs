using BinaryContainer2.Container; // Giả sử BinContainer nằm trong namespace này

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_Add_Tests
	{
		/// <summary>
		/// Kiểm tra khởi tạo ban đầu.
		/// </summary>
		[TestMethod]
		public void Add_InitialState_ShouldBeCorrect()
		{
			// Arrange
			var container = new BinContainer();

			// Act & Assert
			Assert.AreEqual(0, container.Offset, "Offset ban đầu phải là 0.");
			Assert.AreEqual(0, container.Total, "Total ban đầu phải là 0.");
			Assert.AreEqual(0, container.Template, "Template ban đầu phải là 0.");
			Assert.AreEqual(0, container.Data.Count, "Data ban đầu phải rỗng.");
		}

		// ---

		/// <summary>
		/// Kiểm tra khi thêm bit đầu tiên (true).
		/// </summary>
		[TestMethod]
		public void Add_FirstBitTrue_ShouldInitializeCorrectly()
		{
			// Arrange
			var container = new BinContainer();

			// Act
			container.Add(true);

			// Assert
			Assert.AreEqual(1, container.Offset, "Offset phải là 1.");
			Assert.AreEqual(1, container.Total, "Total phải là 1.");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			// Bit 'true' ở vị trí 0 (1 << 0) là 1
			Assert.AreEqual(1, container.Template, "Template phải là 1 (0b00000001).");
			Assert.AreEqual(1, container.Data[0], "Byte đầu tiên trong Data phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra khi thêm bit đầu tiên (false).
		/// </summary>
		[TestMethod]
		public void Add_FirstBitFalse_ShouldInitializeCorrectly()
		{
			// Arrange
			var container = new BinContainer();

			// Act
			container.Add(false);

			// Assert
			Assert.AreEqual(1, container.Offset, "Offset phải là 1.");
			Assert.AreEqual(1, container.Total, "Total phải là 1.");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");
			// Bit 'false' không thay đổi Template, vẫn là 0
			Assert.AreEqual(0, container.Template, "Template phải là 0.");
			Assert.AreEqual(0, container.Data[0], "Byte đầu tiên trong Data phải là 0.");
		}

		// ---

		/// <summary>
		/// Kiểm tra việc thêm 8 bit liên tiếp (không tràn byte).
		/// Thêm 7 bit False, sau đó 1 bit True (0b01000000 = 64).
		/// </summary>
		[TestMethod]
		public void Add_EightBitsNoOverflow_ShouldFillFirstByte()
		{
			// Arrange
			var container = new BinContainer();

			// Act
			for (int i = 0; i < 7; i++) // 7 bits False (vị trí 0-6)
			{
				container.Add(false);
			}
			container.Add(true); // 1 bit True (vị trí 7)

			// Assert
			Assert.AreEqual(8, container.Offset, "Offset phải là 8.");
			Assert.AreEqual(8, container.Total, "Total phải là 8.");
			Assert.AreEqual(1, container.Data.Count, "Data chỉ nên có 1 phần tử.");
			// Bit True ở vị trí 7 là 1 << 7 = 128 (0b10000000)
			Assert.AreEqual(128, container.Template, "Template phải là 128.");
			Assert.AreEqual(128, container.Data[0], "Byte đầu tiên phải là 128.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp tràn byte (Offset >= 8), bắt đầu byte mới.
		/// Thêm 8 bit đầu tiên (Offset = 8), sau đó thêm bit thứ 9 (Offset = 1, Data.Count = 2).
		/// </summary>
		[TestMethod]
		public void Add_NinthBit_ShouldStartNewByte()
		{
			// Arrange
			var container = new BinContainer();
			// Thêm 8 bit True
			for (int i = 0; i < 8; i++)
			{
				container.Add(true);
			}

			// Kiểm tra trạng thái sau 8 bit
			Assert.AreEqual(8, container.Offset, "Offset phải là 8 sau 8 bit.");
			Assert.AreEqual(255, container.Data[0], "Byte 0 phải là 255 (0b11111111).");
			Assert.AreEqual(1, container.Data.Count, "Data phải có 1 phần tử.");

			// Act: Thêm bit thứ 9 (True)
			container.Add(true);

			// Assert
			// Kiểm tra logic khởi tạo byte mới:
			// 1. Data.Add(0) -> Data.Count = 2
			// 2. Offset = 0
			// 3. Template = 0
			// Sau đó:
			// 4. Template |= 1 << Offset (0b00000001)
			// 5. Offset++ -> 1
			// 6. Total++ -> 9

			Assert.AreEqual(1, container.Offset, "Offset phải là 1 (cho byte mới).");
			Assert.AreEqual(9, container.Total, "Total phải là 9.");
			Assert.AreEqual(2, container.Data.Count, "Data phải có 2 phần tử.");
			Assert.AreEqual(1, container.Template, "Template phải là 1.");
			Assert.AreEqual(1, container.Data[1], "Byte thứ hai (Data[1]) phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra điều kiện Offset >= 8. Thêm 8 bit, Offset = 8.
		/// Sau đó thêm một bit False để kiểm tra xem nó có khởi tạo byte mới không.
		/// </summary>
		[TestMethod]
		public void Add_OverflowThenFalse_ShouldStartNewByteWithZero()
		{
			// Arrange
			var container = new BinContainer();
			// Thêm 8 bit True
			for (int i = 0; i < 8; i++)
			{
				container.Add(true);
			}

			// Kiểm tra trạng thái sau 8 bit
			Assert.AreEqual(8, container.Offset, "Offset phải là 8 sau 8 bit.");

			// Act: Thêm bit thứ 9 (False)
			container.Add(false);

			// Assert
			// Kiểm tra logic khởi tạo byte mới:
			// 1. Data.Add(0) -> Data.Count = 2
			// 2. Offset = 0
			// 3. Template = 0
			// Sau đó:
			// 4. Bỏ qua if (data)
			// 5. Offset++ -> 1
			// 6. Total++ -> 9

			Assert.AreEqual(1, container.Offset, "Offset phải là 1 (cho byte mới).");
			Assert.AreEqual(9, container.Total, "Total phải là 9.");
			Assert.AreEqual(2, container.Data.Count, "Data phải có 2 phần tử.");
			Assert.AreEqual(0, container.Template, "Template phải là 0.");
			Assert.AreEqual(0, container.Data[1], "Byte thứ hai (Data[1]) phải là 0.");
		}
	}
}