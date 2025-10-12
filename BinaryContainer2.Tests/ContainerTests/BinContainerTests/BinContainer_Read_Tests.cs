using Microsoft.VisualStudio.TestTools.UnitTesting;
using BinaryContainer2.Container;
using System.Linq;

namespace BinaryContainer2.Tests.ContainerTests.BinContainerTests
{
	[TestClass]
	public class BinContainer_Read_Tests
	{
		private BinContainer SetupContainer(bool[] dataToAdd)
		{
			var container = new BinContainer();
			container.AddArray(dataToAdd);
			// Sau khi thêm, luôn gọi ReadReset() để đảm bảo trạng thái đọc ban đầu
			container.ReadReset();
			return container;
		}

		// ---

		/// <summary>
		/// Kiểm tra đọc bit đầu tiên (True và False) và cập nhật trạng thái đọc.
		/// </summary>
		[TestMethod]
		public void Read_FirstBit_ShouldReadCorrectValueAndUpdateState()
		{
			// Arrange: Data = [T, F]
			var container = SetupContainer(new bool[] { true, false });

			// Act 1: Đọc bit True đầu tiên
			var result1 = container.Read();

			// Assert 1
			Assert.IsTrue(result1.HasValue, "Phải đọc được giá trị.");
			Assert.IsTrue(result1.Value, "Bit đầu tiên phải là True.");
			Assert.AreEqual(1, container.ReadTotal, "ReadTotal phải là 1.");
			Assert.AreEqual(0, container.ReadItor, "ReadItor phải là 0.");
			Assert.AreEqual(1, container.ReadOffset, "ReadOffset phải là 1.");

			// Act 2: Đọc bit False thứ hai
			var result2 = container.Read();

			// Assert 2
			Assert.IsTrue(result2.HasValue, "Phải đọc được giá trị.");
			Assert.IsFalse(result2.Value, "Bit thứ hai phải là False.");
			Assert.AreEqual(2, container.ReadTotal, "ReadTotal phải là 2.");
			Assert.AreEqual(0, container.ReadItor, "ReadItor phải là 0.");
			Assert.AreEqual(2, container.ReadOffset, "ReadOffset phải là 2.");
		}

		// ---

		/// <summary>
		/// Kiểm tra logic chuyển sang byte mới (ReadItor++).
		/// Đọc 8 bit đầu tiên, sau đó đọc bit thứ 9.
		/// </summary>
		[TestMethod]
		public void Read_ByteBoundary_ShouldIncrementReadItor()
		{
			// Arrange: Data = 8 False bits + 1 True bit.
			// Data[0] = 0 (8F), Data[1] = 1 (1T)
			var data = Enumerable.Repeat(false, 8)
								 .Concat(new bool[] { true })
								 .ToArray();
			var container = SetupContainer(data); // Total = 9

			// Act 1: Đọc 8 bit đầu tiên (0 đến 7)
			for (int i = 0; i < 8; i++)
			{
				Assert.IsFalse(container.Read(), $"Bit {i} phải là False.");
			}

			// Assert 1: Sau 8 bit
			Assert.AreEqual(8, container.ReadTotal, "ReadTotal phải là 8.");
			Assert.AreEqual(0, container.ReadItor, "ReadItor phải là 0 trước khi đọc bit thứ 9.");
			Assert.AreEqual(8, container.ReadOffset, "ReadOffset phải là 8 trước khi đọc bit thứ 9.");

			// Act 2: Đọc bit thứ 9 (bit đầu tiên của Data[1])
			var result9 = container.Read();

			// Assert 2: Kiểm tra logic chuyển byte:
			// ReadOffset >= 8 -> ReadItor = 1, ReadOffset = 0
			// Sau đó Read Bit -> ReadOffset = 1
			Assert.IsTrue(result9.HasValue, "Phải đọc được giá trị thứ 9.");
			Assert.IsTrue(result9.Value, "Bit thứ 9 phải là True.");
			Assert.AreEqual(9, container.ReadTotal, "ReadTotal phải là 9.");
			Assert.AreEqual(1, container.ReadItor, "ReadItor phải là 1.");
			Assert.AreEqual(1, container.ReadOffset, "ReadOffset phải là 1.");
		}

		// ---

		/// <summary>
		/// Kiểm tra điều kiện kết thúc đọc (ReadTotal >= Total).
		/// </summary>
		[TestMethod]
		public void Read_BeyondTotal_ShouldReturnNull()
		{
			// Arrange: Data = 2 True bits. Total = 2
			var container = SetupContainer(new bool[] { true, true });

			// Act 1 & 2: Đọc 2 bit có sẵn
			container.Read(); // ReadTotal = 1
			container.Read(); // ReadTotal = 2

			// Assert 1: Trạng thái cuối cùng trước khi đọc quá giới hạn
			Assert.AreEqual(2, container.ReadTotal, "ReadTotal phải là 2.");

			// Act 3: Đọc bit thứ 3 (quá giới hạn)
			var result3 = container.Read();

			// Assert 2
			Assert.IsFalse(result3.HasValue, "Đọc quá giới hạn phải trả về null.");
			Assert.IsNull(result3, "Giá trị trả về phải là null.");

			// Trạng thái đọc không được thay đổi nếu trả về null
			Assert.AreEqual(2, container.ReadTotal, "ReadTotal phải vẫn là 2.");
			Assert.AreEqual(0, container.ReadItor, "ReadItor phải vẫn là 0.");
			Assert.AreEqual(2, container.ReadOffset, "ReadOffset phải vẫn là 2.");
		}

		// ---

		/// <summary>
		/// Kiểm tra logic đọc một bit nằm giữa một byte
		/// Data[0] = 0b00001010 = 10 (Bit 1=T, Bit 3=T)
		/// </summary>
		[TestMethod]
		public void Read_SpecificBitLogic_ShouldExtractCorrectBit()
		{
			// Arrange: Data [F, T, F, T, F, F, F, F]
			var data = new bool[] { false, true, false, true, false, false, false, false };
			var container = SetupContainer(data);

			// Act & Assert: Đọc từng bit và kiểm tra giá trị và ReadOffset
			Assert.IsFalse(container.Read(), "Bit 0 phải là F. ReadOffset=1");
			Assert.IsTrue(container.Read(), "Bit 1 phải là T. ReadOffset=2"); // (Data[0] & (1 << 1)) != 0
			Assert.IsFalse(container.Read(), "Bit 2 phải là F. ReadOffset=3");
			Assert.IsTrue(container.Read(), "Bit 3 phải là T. ReadOffset=4"); // (Data[0] & (1 << 3)) != 0
			Assert.IsFalse(container.Read(), "Bit 4 phải là F. ReadOffset=5");
			Assert.IsFalse(container.Read(), "Bit 5 phải là F. ReadOffset=6");
			Assert.IsFalse(container.Read(), "Bit 6 phải là F. ReadOffset=7");
			Assert.IsFalse(container.Read(), "Bit 7 phải là F. ReadOffset=8");

			Assert.AreEqual(8, container.ReadOffset, "ReadOffset cuối cùng phải là 8.");
			Assert.AreEqual(8, container.ReadTotal, "ReadTotal cuối cùng phải là 8.");
		}
	}
}