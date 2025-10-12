using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_Scan_Tests
	{
		private BinContainer SetupContainer(bool[] dataToAdd)
		{
			var container = new BinContainer();
			container.AddArray(dataToAdd);
			// Luôn gọi ReadReset() để đảm bảo trạng thái đọc ban đầu
			container.ReadReset();
			return container;
		}

		// ---

		/// <summary>
		/// Kiểm tra Scan bit đầu tiên (True) và đảm bảo trạng thái không đổi.
		/// </summary>
		[TestMethod]
		public void Scan_FirstBitTrue_ShouldReturnTrueAndMaintainState()
		{
			// Arrange: Data = [T, F, T]. Total = 3
			var container = SetupContainer(new bool[] { true, false, true });

			// Ghi lại trạng thái ban đầu
			var initialReadTotal = container.ReadTotal; // 0
			var initialReadItor = container.ReadItor;   // 0
			var initialReadOffset = container.ReadOffset;// 0

			// Act
			var result = container.Scan();

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result.Value, "Bit đầu tiên phải là True.");

			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(initialReadTotal, container.ReadTotal, "ReadTotal phải giữ nguyên.");
			Assert.AreEqual(initialReadItor, container.ReadItor, "ReadItor phải giữ nguyên.");
			Assert.AreEqual(initialReadOffset, container.ReadOffset, "ReadOffset phải giữ nguyên.");
		}

		// ---

		/// <summary>
		/// Kiểm tra Scan bit đầu tiên (False) và đảm bảo trạng thái không đổi.
		/// </summary>
		[TestMethod]
		public void Scan_FirstBitFalse_ShouldReturnFalseAndMaintainState()
		{
			// Arrange: Data = [F, T, F]. Total = 3
			var container = SetupContainer(new bool[] { false, true, false });

			// Ghi lại trạng thái ban đầu
			var initialReadTotal = container.ReadTotal; // 0
			var initialReadOffset = container.ReadOffset;// 0

			// Act
			var result = container.Scan();

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsFalse(result.Value, "Bit đầu tiên phải là False.");

			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(initialReadTotal, container.ReadTotal, "ReadTotal phải giữ nguyên.");
			Assert.AreEqual(initialReadOffset, container.ReadOffset, "ReadOffset phải giữ nguyên.");
		}

		// ---

		/// <summary>
		/// Kiểm tra Scan sau khi đã di chuyển vị trí đọc bằng Read().
		/// </summary>
		[TestMethod]
		public void Scan_AfterReadOperation_ShouldScanCurrentPositionAndMaintainState()
		{
			// Arrange: Data = [T, F, T, T, F]. Total = 5
			var container = SetupContainer(new bool[] { true, false, true, true, false });

			// Đọc 2 bit đầu tiên (vị trí đọc sẽ ở bit thứ 2, giá trị 'True')
			container.Read(); // Đọc True (ReadTotal=1, ReadOffset=1)
			container.Read(); // Đọc False (ReadTotal=2, ReadOffset=2)

			// Ghi lại trạng thái sau khi đọc
			var currentStateTotal = container.ReadTotal; // 2
			var currentStateOffset = container.ReadOffset;// 2

			// Act: Scan bit thứ 3 (vị trí 2), giá trị True
			var result = container.Scan();

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result.Value, "Scan phải trả về giá trị của bit thứ 3 (True).");

			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(currentStateTotal, container.ReadTotal, "ReadTotal phải giữ nguyên là 2.");
			Assert.AreEqual(currentStateOffset, container.ReadOffset, "ReadOffset phải giữ nguyên là 2.");
		}

		// ---

		/// <summary>
		/// Kiểm tra Scan khi đã ở cuối byte đầu tiên (ReadOffset = 7)
		/// </summary>
		[TestMethod]
		public void Scan_LastBitOfByte_ShouldReadCorrectlyAndMaintainState()
		{
			// Arrange: Data = 7 False bits + 1 True bit. Total = 8
			// Data[0] = 0b10000000 = 128
			var data = Enumerable.Repeat(false, 7)
								 .Concat(new bool[] { true })
								 .ToArray();
			var container = SetupContainer(data);

			// Di chuyển vị trí đọc đến bit cuối cùng của byte (bit 7)
			for (int i = 0; i < 7; i++)
			{
				container.Read();
			}

			// Trạng thái: ReadTotal=7, ReadOffset=7, ReadItor=0

			// Act: Scan bit thứ 8 (bit 7), giá trị True
			var result = container.Scan();

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result.Value, "Scan phải trả về bit cuối cùng của byte (True).");

			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(7, container.ReadTotal, "ReadTotal phải giữ nguyên là 7.");
			Assert.AreEqual(7, container.ReadOffset, "ReadOffset phải giữ nguyên là 7.");
		}

		// ---

		/// <summary>
		/// Kiểm tra Scan khi đã đọc hết Total (ReadTotal >= Total).
		/// </summary>
		[TestMethod]
		public void Scan_BeyondTotal_ShouldReturnNullAndMaintainState()
		{
			// Arrange: Data = [T]. Total = 1
			var container = SetupContainer(new bool[] { true });

			// Đọc hết bit có sẵn
			container.Read(); // ReadTotal = 1

			// Ghi lại trạng thái sau khi đọc hết
			var currentStateTotal = container.ReadTotal; // 1
			var currentStateOffset = container.ReadOffset;// 1

			// Act: Scan bit thứ 2 (quá giới hạn)
			var result = container.Scan();

			// Assert
			Assert.IsFalse(result.HasValue, "Scan quá giới hạn phải trả về null.");
			Assert.IsNull(result, "Giá trị trả về phải là null.");

			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(currentStateTotal, container.ReadTotal, "ReadTotal phải giữ nguyên là 1.");
			Assert.AreEqual(currentStateOffset, container.ReadOffset, "ReadOffset phải giữ nguyên là 1.");
		}
	}
}