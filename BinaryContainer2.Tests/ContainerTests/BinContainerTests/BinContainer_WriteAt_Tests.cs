using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_WriteAt_Tests
	{
		private BinContainer SetupContainer(bool[] initialData)
		{
			var container = new BinContainer();
			container.AddArray(initialData);
			// Sau khi setup, ta có thể kiểm tra trực tiếp Data[]
			return container;
		}

		// ---

		/// <summary>
		/// Kiểm tra thiết lập bit (WriteAt True) tại index 0.
		/// Ban đầu: Data[0] = 0b00000000. Sau khi ghi: Data[0] = 0b00000001 (1).
		/// </summary>
		[TestMethod]
		public void WriteAt_SetBit_AtIndexZero()
		{
			// Arrange: Data = [F, F, F, F, F]. Total = 5. Data[0] = 0.
			var container = SetupContainer(new bool[] { false, false, false, false, false });
			const int INDEX = 0;

			// Act: Ghi True vào index 0
			container.WriteAt(INDEX, true);

			// Assert: Kiểm tra Data[0] đã được set bit 0 chưa (0 | (1 << 0) = 1)
			Assert.AreEqual(1, container.Data[0], "Data[0] phải là 1 (bit 0 được set).");
		}

		// ---

		/// <summary>
		/// Kiểm tra thiết lập bit (WriteAt True) tại index 3 trong byte đầu tiên.
		/// Ban đầu: Data[0] = 0b00000001 (bit 0 là True). Sau khi ghi: Data[0] = 0b00001001 (9).
		/// </summary>
		[TestMethod]
		public void WriteAt_SetBit_InMiddleOfByte()
		{
			// Arrange: Data = [T, F, F, F, F]. Total = 5. Data[0] = 1.
			var container = SetupContainer(new bool[] { true, false, false, false, false });
			const int INDEX = 3; // Bit 3

			// Act: Ghi True vào index 3
			container.WriteAt(INDEX, true);

			// Assert: 1 | (1 << 3) = 1 | 8 = 9
			Assert.AreEqual(9, container.Data[0], "Data[0] phải là 9 (bit 0 và bit 3 được set).");
		}

		// ---

		/// <summary>
		/// Kiểm tra xóa bit (WriteAt False) tại index 0.
		/// Ban đầu: Data[0] = 0b00000001 (1). Sau khi ghi: Data[0] = 0b00000000 (0).
		/// </summary>
		[TestMethod]
		public void WriteAt_ClearBit_AtIndexZero()
		{
			// Arrange: Data = [T, F, F, F, F]. Total = 5. Data[0] = 1.
			var container = SetupContainer(new bool[] { true, false, false, false, false });
			const int INDEX = 0;

			// Act: Ghi False vào index 0
			container.WriteAt(INDEX, false);

			// Assert: 1 & ~(1 << 0) = 1 & ~1 = 1 & 254 = 0
			Assert.AreEqual(0, container.Data[0], "Data[0] phải là 0 (bit 0 bị xóa).");
		}

		// ---

		/// <summary>
		/// Kiểm tra xóa bit (WriteAt False) tại index 7 (bit cuối cùng của byte 0).
		/// Ban đầu: Data[0] = 0b10000001 (129). Sau khi ghi: Data[0] = 0b00000001 (1).
		/// </summary>
		[TestMethod]
		public void WriteAt_ClearBit_AtEndOfByte()
		{
			// Arrange: Data = [T, F, F, F, F, F, F, T]. Total = 8. Data[0] = 129.
			var data = new bool[] { true, false, false, false, false, false, false, true };
			var container = SetupContainer(data);
			const int INDEX = 7; // Bit 7

			// Act: Ghi False vào index 7
			container.WriteAt(INDEX, false);

			// Assert: 129 & ~(1 << 7) = 129 & ~128 = 129 & 127 = 1
			Assert.AreEqual(1, container.Data[0], "Data[0] phải là 1 (bit 7 bị xóa).");
		}

		// ---

		/// <summary>
		/// Kiểm tra ghi vào byte thứ hai (index 8).
		/// Index 8: readItor = 1, readOffset = 0.
		/// Ban đầu: Data[1] = 0b00000000. Sau khi ghi: Data[1] = 0b00000001 (1).
		/// </summary>
		[TestMethod]
		public void WriteAt_ToSecondByte_ShouldModifyCorrectByte()
		{
			// Arrange: 8 False bits + 4 False bits. Total = 12. Data[0]=0, Data[1]=0.
			var data = Enumerable.Repeat(false, 12).ToArray();
			var container = SetupContainer(data);
			const int INDEX = 8;

			// Act: Ghi True vào index 8
			container.WriteAt(INDEX, true);

			// Assert
			Assert.AreEqual(0, container.Data[0], "Byte đầu tiên không được thay đổi.");
			// Data[1] = 0 | (1 << 0) = 1
			Assert.AreEqual(1, container.Data[1], "Byte thứ hai (Data[1]) phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp index âm (nên bỏ qua, không gây lỗi).
		/// </summary>
		[TestMethod]
		public void WriteAt_NegativeIndex_ShouldDoNothing()
		{
			// Arrange: Data = [T, F]. Data[0] = 1.
			var container = SetupContainer(new bool[] { true, false });
			const byte INITIAL_DATA_0 = 1;

			// Act: Ghi True vào index âm
			container.WriteAt(-1, true);

			// Assert: Dữ liệu không được thay đổi.
			Assert.AreEqual(INITIAL_DATA_0, container.Data[0], "Dữ liệu không được thay đổi khi index âm.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp index bằng Total (nên bỏ qua, không gây lỗi).
		/// </summary>
		[TestMethod]
		public void WriteAt_IndexEqualsTotal_ShouldDoNothing()
		{
			// Arrange: Data = [T, F]. Total = 2. Data[0] = 1.
			var container = SetupContainer(new bool[] { true, false });
			const byte INITIAL_DATA_0 = 1;
			const int TOTAL = 2;

			// Act: Ghi False vào index bằng Total
			container.WriteAt(TOTAL, false);

			// Assert: Dữ liệu không được thay đổi.
			Assert.AreEqual(INITIAL_DATA_0, container.Data[0], "Dữ liệu không được thay đổi khi index = Total.");
		}
	}
}