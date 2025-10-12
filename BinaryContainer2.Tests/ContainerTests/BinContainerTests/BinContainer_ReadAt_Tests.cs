using BinaryContainer2.Container;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_ReadAt_Tests
	{
		private BinContainer SetupContainer(bool[] dataToAdd)
		{
			var container = new BinContainer();
			container.AddArray(dataToAdd);
			// Hàm ReadAt không sử dụng/thay đổi trạng thái đọc (ReadTotal/Offset/Itor), 
			// nhưng ta vẫn reset để đảm bảo trạng thái sạch.
			container.ReadReset();
			return container;
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc bit đầu tiên (index = 0).
		/// </summary>
		[TestMethod]
		public void ReadAt_IndexZero_ShouldReturnCorrectValue()
		{
			// Arrange: Data = [T, F, T]. Total = 3
			var container = SetupContainer(new bool[] { true, false, true });

			// Act
			var result = container.ReadAt(0);

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result.Value, "Bit tại index 0 phải là True.");
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc bit nằm giữa byte đầu tiên.
		/// Data[0] = 0b00001000 = 8. Bit 3 = True, các bit khác = False.
		/// </summary>
		[TestMethod]
		public void ReadAt_MiddleOfFirstByte_ShouldReturnCorrectValue()
		{
			// Arrange: Data [F, F, F, T, F, F, F, F]. Total = 8
			var data = new bool[] { false, false, false, true, false, false, false, false };
			var container = SetupContainer(data);

			// Act
			var resultTrue = container.ReadAt(3); // Bit 3
			var resultFalse = container.ReadAt(5); // Bit 5

			// Assert
			Assert.IsTrue(resultTrue.Value, "Bit tại index 3 phải là True.");
			Assert.IsFalse(resultFalse.Value, "Bit tại index 5 phải là False.");
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc bit cuối cùng trong container (index = Total - 1).
		/// </summary>
		[TestMethod]
		public void ReadAt_LastIndex_ShouldReturnCorrectValue()
		{
			// Arrange: 8 False + 7 False + 1 True. Total = 16. Bit 15 là True.
			var data = Enumerable.Repeat(false, 15)
								 .Concat(new bool[] { true })
								 .ToArray();
			var container = SetupContainer(data);
			const int LAST_INDEX = 15; // Total - 1

			// Act
			var result = container.ReadAt(LAST_INDEX);

			// Assert
			Assert.IsTrue(result.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result.Value, "Bit tại index 15 phải là True.");
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc bit nằm giữa byte thứ hai.
		/// Index 12: readItor = 12/8 = 1. readOffset = 12%8 = 4.
		/// Data[1] = 0b00010000 = 16.
		/// </summary>
		[TestMethod]
		public void ReadAt_SecondByte_ShouldCalculateIndicesCorrectly()
		{
			// Arrange: 8 False + [F, F, F, F, T, F, F, F]. Total = 13.
			// Bit 12 là True.
			var data = Enumerable.Repeat(false, 8)
								 .Concat(new bool[] { false, false, false, false, true })
								 .ToArray();
			var container = SetupContainer(data);
			const int INDEX_TRUE = 12; // Bit 4 của byte 1

			// Act
			var resultTrue = container.ReadAt(INDEX_TRUE);
			var resultFalse = container.ReadAt(10); // Bit 2 của byte 1

			// Assert
			Assert.IsTrue(resultTrue.Value, "Bit tại index 12 phải là True.");
			Assert.IsFalse(resultFalse.Value, "Bit tại index 10 phải là False.");

			// Kiểm tra trạng thái đọc không bị thay đổi
			Assert.AreEqual(0, container.ReadTotal, "ReadTotal phải giữ nguyên là 0.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp index âm.
		/// </summary>
		[TestMethod]
		public void ReadAt_NegativeIndex_ShouldReturnNull()
		{
			// Arrange
			var container = SetupContainer(new bool[] { true, false });

			// Act
			var result = container.ReadAt(-1);

			// Assert
			Assert.IsFalse(result.HasValue, "Index âm phải trả về null.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp index bằng Total (index ngoài phạm vi).
		/// </summary>
		[TestMethod]
		public void ReadAt_IndexEqualsTotal_ShouldReturnNull()
		{
			// Arrange: Total = 2
			var container = SetupContainer(new bool[] { true, false });
			const int TOTAL = 2;

			// Act
			var result = container.ReadAt(TOTAL);

			// Assert
			Assert.IsFalse(result.HasValue, "Index bằng Total phải trả về null.");
		}

		// ---

		/// <summary>
		/// Kiểm tra trường hợp index lớn hơn Total (index ngoài phạm vi).
		/// </summary>
		[TestMethod]
		public void ReadAt_IndexGreaterThanTotal_ShouldReturnNull()
		{
			// Arrange: Total = 2
			var container = SetupContainer(new bool[] { true, false });

			// Act
			var result = container.ReadAt(5);

			// Assert
			Assert.IsFalse(result.HasValue, "Index lớn hơn Total phải trả về null.");
		}
	}
}