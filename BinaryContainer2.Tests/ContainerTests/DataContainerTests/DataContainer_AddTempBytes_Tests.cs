using BinaryContainer2.Container; // Giả sử DataContainer nằm ở đây

namespace BinaryContainer2.Tests.ContainerTests.DataContainerTests
{
	[TestClass]
	public class DataContainer_AddTempBytes_Tests
	{
		private DataContainer _container;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo DataContainer trước mỗi test.
			// Cần đảm bảo rằng TempBytesPosition đã được khởi tạo bên trong DataContainer's constructor (Stack<int>).
			_container = new DataContainer();
		}

		/// <summary>
		/// Kiểm tra khi thêm bytes lần đầu tiên (Items rỗng).
		/// </summary>
		[TestMethod]
		public void AddTempBytes_FirstCall_CorrectlyAddsBytesAndPushesPosition()
		{
			// Arrange
			int count = 10;
			int initialItemsCount = _container.Items.Count; // Ban đầu là 0
			int initialStackCount = _container.TempBytesPosition.Count; // Ban đầu là 0

			// Act
			_container.AddTempBytes(count);

			// Assert
			// 1. Kiểm tra Items.Count đã tăng lên đúng bằng count
			Assert.AreEqual(initialItemsCount + count, _container.Items.Count, "Items.Count should increase by 'count'.");

			// 2. Kiểm tra vị trí ban đầu (0) đã được push vào Stack
			Assert.AreEqual(initialStackCount + 1, _container.TempBytesPosition.Count, "One position should be pushed onto TempBytesPosition.");
			Assert.AreEqual(initialItemsCount, _container.TempBytesPosition.Peek(), "The pushed position should be the initial Items.Count (0).");

			// 3. Kiểm tra các phần tử mới thêm vào đều là byte 0 (mặc định của new byte[count])
			for (int i = 0; i < count; i++)
			{
				Assert.AreEqual(0, _container.Items[i], "Newly added bytes should be initialized to 0.");
			}
		}

		/// <summary>
		/// Kiểm tra khi thêm bytes sau khi Items đã có dữ liệu.
		/// </summary>
		[TestMethod]
		public void AddTempBytes_AfterExistingData_CorrectlyAddsBytesAndPushesPosition()
		{
			// Arrange
			// Thêm một số dữ liệu ban đầu
			_container.Items.AddRange(new byte[] { 1, 2, 3, 4, 5 });
			int count = 7;
			int initialItemsCount = _container.Items.Count; // Hiện tại là 5

			// Act
			_container.AddTempBytes(count);

			// Assert
			// 1. Kiểm tra Items.Count đã tăng lên
			Assert.AreEqual(initialItemsCount + count, _container.Items.Count, "Items.Count should increase by 'count' (5 + 7 = 12).");

			// 2. Kiểm tra vị trí (5) đã được push vào Stack
			Assert.AreEqual(initialItemsCount, _container.TempBytesPosition.Peek(), "The pushed position should be the initial Items.Count (5).");

			// 3. Kiểm tra dữ liệu ban đầu không bị thay đổi
			Assert.AreEqual(1, _container.Items[0], "Existing data at index 0 should not be changed.");
			Assert.AreEqual(5, _container.Items[4], "Existing data at index 4 should not be changed.");

			// 4. Kiểm tra các phần tử mới thêm vào đều là byte 0
			for (int i = 0; i < count; i++)
			{
				Assert.AreEqual(0, _container.Items[initialItemsCount + i], $"Newly added bytes at index {initialItemsCount + i} should be initialized to 0.");
			}
		}

		/// <summary>
		/// Kiểm tra khi thêm bytes với count = 0.
		/// </summary>
		[TestMethod]
		public void AddTempBytes_CountIsZero_OnlyPushesPosition()
		{
			// Arrange
			_container.Items.AddRange(new byte[] { 10, 20 });
			int count = 0;
			int initialItemsCount = _container.Items.Count; // Hiện tại là 2

			// Act
			_container.AddTempBytes(count);

			// Assert
			// 1. Kiểm tra Items.Count không thay đổi
			Assert.AreEqual(initialItemsCount, _container.Items.Count, "Items.Count should remain the same when count is 0.");

			// 2. Kiểm tra vị trí (2) vẫn được push vào Stack
			Assert.AreEqual(1, _container.TempBytesPosition.Count, "One position should be pushed onto TempBytesPosition.");
			Assert.AreEqual(initialItemsCount, _container.TempBytesPosition.Peek(), "The pushed position should be the initial Items.Count (2).");
		}

		/// <summary>
		/// Kiểm tra khi gọi AddTempBytes nhiều lần.
		/// </summary>
		[TestMethod]
		public void AddTempBytes_MultipleCalls_CorrectlyPushesMultiplePositions()
		{
			// Arrange
			int count1 = 5;
			int count2 = 3;
			int count3 = 1;

			// Act
			_container.AddTempBytes(count1); // Items.Count = 5. Push: 0
			int pos1 = _container.Items.Count;

			_container.AddTempBytes(count2); // Items.Count = 8. Push: 5
			int pos2 = _container.Items.Count;

			_container.AddTempBytes(count3); // Items.Count = 9. Push: 8

			// Assert
			// 1. Kiểm tra tổng Items.Count
			Assert.AreEqual(count1 + count2 + count3, _container.Items.Count, "Total Items.Count should be the sum of all counts."); // 5+3+1 = 9

			// 2. Kiểm tra Stack có 3 phần tử
			Assert.AreEqual(3, _container.TempBytesPosition.Count, "Three positions should be pushed onto TempBytesPosition.");

			// 3. Kiểm tra thứ tự và giá trị của các vị trí được push (Stack là LIFO)
			Assert.AreEqual(pos2, _container.TempBytesPosition.Pop(), "The last pushed position should be pos2 (8)."); // Vị trí trước khi thêm count3
			Assert.AreEqual(pos1, _container.TempBytesPosition.Pop(), "The second pushed position should be pos1 (5)."); // Vị trí trước khi thêm count2
			Assert.AreEqual(0, _container.TempBytesPosition.Pop(), "The first pushed position should be 0."); // Vị trí trước khi thêm count1
		}
	}
}
