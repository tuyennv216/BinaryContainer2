using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_ReadArray_Tests
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
		/// Kiểm tra đọc một mảng nhỏ chứa giá trị True và False (tổng 3 bit).
		/// </summary>
		[TestMethod]
		public void ReadArray_SimpleCase_ShouldReturnCorrectArrayAndUpdateState()
		{
			// Arrange: Data = [T, F, T]. Total = 3
			var data = new bool[] { true, false, true };
			var container = SetupContainer(data);
			const int LENGTH = 3;

			// Act
			var result = container.ReadArray(LENGTH);

			// Assert
			Assert.AreEqual(LENGTH, result.Length, "Độ dài mảng kết quả phải bằng LENGTH.");
			Assert.IsTrue(result[0].Value, "Bit 0 phải là True.");
			Assert.IsFalse(result[1].Value, "Bit 1 phải là False.");
			Assert.IsTrue(result[2].Value, "Bit 2 phải là True.");

			// Kiểm tra trạng thái đọc sau khi đọc xong
			Assert.AreEqual(LENGTH, container.ReadTotal, "ReadTotal phải là 3.");
			Assert.AreEqual(3, container.ReadOffset, "ReadOffset phải là 3.");
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc một mảng vượt qua ranh giới byte (tổng 10 bit).
		/// </summary>
		[TestMethod]
		public void ReadArray_CrossByteBoundary_ShouldHandleReadItorIncrement()
		{
			// Arrange: 8 False bits + 2 True bits. Total = 10
			// Data[0] = 0. Data[1] = 0b00000011 = 3.
			var data = Enumerable.Repeat(false, 8)
								 .Concat(Enumerable.Repeat(true, 2))
								 .ToArray();
			var container = SetupContainer(data);
			const int LENGTH = 10;

			// Act
			var result = container.ReadArray(LENGTH);

			// Assert
			Assert.AreEqual(LENGTH, result.Length, "Độ dài mảng kết quả phải là 10.");
			// Kiểm tra 8 bit đầu tiên (False)
			for (int i = 0; i < 8; i++)
			{
				Assert.IsFalse(result[i].Value, $"Bit {i} (Byte 0) phải là False.");
			}
			// Kiểm tra 2 bit cuối (True)
			Assert.IsTrue(result[8].Value, "Bit 8 (Byte 1) phải là True.");
			Assert.IsTrue(result[9].Value, "Bit 9 (Byte 1) phải là True.");

			// Kiểm tra trạng thái đọc sau khi đọc xong
			Assert.AreEqual(LENGTH, container.ReadTotal, "ReadTotal phải là 10.");
			// 10 % 8 = 2
			Assert.AreEqual(2, container.ReadOffset, "ReadOffset phải là 2 (cho byte thứ hai).");
			// 10 / 8 = 1
			Assert.AreEqual(1, container.ReadItor, "ReadItor phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp độ dài mảng yêu cầu lớn hơn Total, phải trả về null cho các phần tử còn lại.
		/// </summary>
		[TestMethod]
		public void ReadArray_LengthExceedsTotal_ShouldContainNullValues()
		{
			// Arrange: Data = [T, F]. Total = 2
			var container = SetupContainer(new bool[] { true, false });
			const int REQUESTED_LENGTH = 5;

			// Act
			var result = container.ReadArray(REQUESTED_LENGTH);

			// Assert
			Assert.AreEqual(REQUESTED_LENGTH, result.Length, "Độ dài mảng kết quả phải là 5.");

			// Kiểm tra 2 giá trị đầu tiên (có dữ liệu)
			Assert.IsTrue(result[0].Value, "Bit 0 phải là True.");
			Assert.IsFalse(result[1].Value, "Bit 1 phải là False.");

			// Kiểm tra các giá trị còn lại (quá giới hạn)
			Assert.IsNull(result[2], "Bit 2 phải là null.");
			Assert.IsNull(result[3], "Bit 3 phải là null.");
			Assert.IsNull(result[4], "Bit 4 phải là null.");

			// Trạng thái đọc phải dừng ở Total = 2
			Assert.AreEqual(2, container.ReadTotal, "ReadTotal phải là 2.");
			Assert.AreEqual(2, container.ReadOffset, "ReadOffset phải là 2.");
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc một mảng rỗng (length = 0).
		/// </summary>
		[TestMethod]
		public void ReadArray_ZeroLength_ShouldReturnEmptyArrayAndMaintainState()
		{
			// Arrange: Data = [T, F]. Total = 2
			var container = SetupContainer(new bool[] { true, false });
			const int LENGTH = 0;

			// Ghi lại trạng thái ban đầu
			var initialReadTotal = container.ReadTotal; // 0

			// Act
			var result = container.ReadArray(LENGTH);

			// Assert
			Assert.AreEqual(0, result.Length, "Mảng kết quả phải rỗng.");
			// Xác nhận trạng thái không thay đổi
			Assert.AreEqual(initialReadTotal, container.ReadTotal, "ReadTotal phải giữ nguyên là 0.");
			Assert.AreEqual(0, container.ReadOffset, "ReadOffset phải giữ nguyên là 0.");
		}

		// ---

		/// <summary>
		/// Kiểm tra việc đọc được nối tiếp (sau khi đã đọc 2 bit, đọc tiếp 2 bit nữa).
		/// </summary>
		[TestMethod]
		public void ReadArray_SequentialReads_ShouldStartFromCurrentPosition()
		{
			// Arrange: Data = [T, F, F, T, F]. Total = 5
			var container = SetupContainer(new bool[] { true, false, false, true, false });

			// Act 1: Đọc 2 bit đầu
			var result1 = container.ReadArray(2); // [T, F]

			// Kiểm tra trạng thái sau Act 1
			Assert.AreEqual(2, container.ReadTotal, "ReadTotal sau lần đọc 1 phải là 2.");

			// Act 2: Đọc 3 bit tiếp theo (bắt đầu từ bit 2)
			var result2 = container.ReadArray(3); // [F, T, F]

			// Assert
			Assert.IsFalse(result2[0].Value, "Bit 2 phải là False.");
			Assert.IsTrue(result2[1].Value, "Bit 3 phải là True.");
			Assert.IsFalse(result2[2].Value, "Bit 4 phải là False.");

			// Kiểm tra trạng thái cuối cùng
			Assert.AreEqual(5, container.ReadTotal, "ReadTotal cuối cùng phải là 5.");
			Assert.AreEqual(5, container.ReadOffset, "ReadOffset cuối cùng phải là 5.");
		}
	}
}