// Thay thế bằng namespace chứa RefPool của bạn
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OthersTests
{
	[TestClass]
	public class RefPoolNewMSTests
	{
		private RefPool _pool;

		[TestInitialize]
		public void Setup()
		{
			_pool = new RefPool();
		}

		// --- Các Trường Hợp Cơ Bản (Basic Cases) ---
		// ---------------------------------------------

		[TestMethod]
		public void AddObject_NewObject_ShouldReturnTrueAndAssignIndex()
		{
			// Arrange
			object obj1 = new object();

			// Act
			bool added = _pool.AddObject(obj1);
			int index = _pool.FindIndex(obj1);
			object retrievedObj = _pool.GetObject(index);

			// Assert
			Assert.IsTrue(added, "Đối tượng mới phải được thêm thành công.");
			Assert.AreEqual(0, index, "Index đầu tiên phải là 0.");
			Assert.AreSame(obj1, retrievedObj, "GetObject phải trả về cùng reference đối tượng.");
		}

		[TestMethod]
		public void AddObject_ExistingObject_ShouldReturnFalseAndKeepSameIndex()
		{
			// Arrange
			object obj1 = new object();
			_pool.AddObject(obj1);
			int initialIndex = _pool.FindIndex(obj1);

			// Act
			bool addedAgain = _pool.AddObject(obj1);
			int finalIndex = _pool.FindIndex(obj1);

			// Assert
			Assert.IsFalse(addedAgain, "Đối tượng đã tồn tại không nên được thêm lại.");
			Assert.AreEqual(initialIndex, finalIndex, "Index của đối tượng phải không thay đổi.");
		}

		[TestMethod]
		public void FindIndex_NonExistentObject_ShouldReturnNegativeOne()
		{
			// Arrange
			object obj1 = new object();

			// Act
			int index = _pool.FindIndex(obj1);

			// Assert
			Assert.AreEqual(-1, index, "Đối tượng không tồn tại phải trả về -1.");
		}

		[TestMethod]
		public void GetObject_NonExistentIndex_ShouldReturnNull()
		{
			// Act
			// Do index bắt đầu từ -1 và tăng lên, index 999 chắc chắn không tồn tại
			object result = _pool.GetObject(999);

			// Assert
			Assert.IsNull(result, "Index không tồn tại phải trả về null.");
		}

		// --- Trường Hợp Đặc Biệt và Ngoại Lệ (Special & Exception Cases) ---
		// --------------------------------------------------------------------

		[TestMethod]
		public void AddObject_DifferentInstances_ShouldGetConsecutiveIndexes()
		{
			// Arrange
			object obj1 = new object();
			object obj2 = new object();

			// Sử dụng các kiểu dữ liệu khác Object
			string obj3 = "TestString";

			// Act
			_pool.AddObject(obj1);
			_pool.AddObject(obj2);
			_pool.AddObject(obj3);

			// Assert
			Assert.AreEqual(0, _pool.FindIndex(obj1), "Index 1 phải là 0.");
			Assert.AreEqual(1, _pool.FindIndex(obj2), "Index 2 phải là 1.");
			Assert.AreEqual(2, _pool.FindIndex(obj3), "Index 3 phải là 2.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddObject_NullArgument_ShouldThrowArgumentNullException()
		{
			// Act
			// Thử gọi AddObject với null, điều này vi phạm chữ ký phương thức `Object obj`
			// và .NET sẽ ném ra ArgumentNullException khi cố gắng gọi phương thức.
			_pool.AddObject(null!);
		}

		// --- Trường Hợp Giới Hạn và Biên (Boundary/Limit Cases) ---
		// -----------------------------------------------------------

		[TestMethod]
		public void IndexOverflow_BoundaryTest()
		{
			// Arrange
			// Mục tiêu: Kiểm tra giá trị index khi nó đạt đến giới hạn trên của int (int.MaxValue)
			// (Không thực tế để chạy trong Unit Test thực, nhưng là một trường hợp biên lý thuyết)

			// Chúng ta mô phỏng việc đặt _index gần giới hạn để kiểm tra việc tăng (Increment)
			var indexField = typeof(RefPool).GetField("_index", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			indexField.SetValue(_pool, int.MaxValue - 2); // Bắt đầu từ MaxValue - 2

			object obj1 = new object();
			object obj2 = new object();
			object obj3 = new object(); // Khi này Interlocked.Increment sẽ vượt quá giới hạn

			// Act & Assert 1: Thêm obj1 (index sẽ là MaxValue - 1)
			_pool.AddObject(obj1);
			Assert.AreEqual(int.MaxValue - 1, _pool.FindIndex(obj1), "Index obj1 phải là MaxValue - 1.");

			// Act & Assert 2: Thêm obj2 (index sẽ là MaxValue)
			_pool.AddObject(obj2);
			Assert.AreEqual(int.MaxValue, _pool.FindIndex(obj2), "Index obj2 phải là MaxValue.");

			// Act & Assert 3: Thêm obj3 (index sẽ bị tràn về int.MinValue)
			// Interlocked.Increment xử lý tràn số nguyên.
			_pool.AddObject(obj3);
			Assert.AreEqual(int.MinValue, _pool.FindIndex(obj3), "Index obj3 phải bị tràn về int.MinValue.");

			// Kiểm tra GetObject cho index bị tràn
			Assert.AreSame(obj3, _pool.GetObject(int.MinValue), "GetObject cho index MinValue phải trả về obj3.");
		}

		// --- Trường Hợp Đa Luồng (Concurrency/Thread Safety) ---
		// --------------------------------------------------------

		[TestMethod]
		public async Task AddObject_HighConcurrency_ShouldBeThreadSafeAndAssignUniqueIndexes()
		{
			// Arrange
			const int NumberOfTasks = 50;
			const int ObjectsPerTask = 20;
			int totalObjects = NumberOfTasks * ObjectsPerTask;
			Task[] tasks = new Task[NumberOfTasks];

			// Act
			for (int i = 0; i < NumberOfTasks; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					for (int j = 0; j < ObjectsPerTask; j++)
					{
						object newObj = new object();
						_pool.AddObject(newObj);
					}
				});
			}

			// Chờ tất cả các tác vụ hoàn thành
			await Task.WhenAll(tasks);

			// Assert
			// Sử dụng Reflection để lấy số lượng phần tử trong các ConcurrentDictionary
			int countFromMapIndex = GetPrivateMapCount("_mapIndex");
			int countFromMapObject = GetPrivateMapCount("_mapObject");

			// Kiểm tra tổng số đối tượng được thêm vào phải chính xác
			Assert.AreEqual(totalObjects, countFromMapIndex, $"Số lượng đối tượng trong _mapIndex phải là {totalObjects}.");
			Assert.AreEqual(totalObjects, countFromMapObject, $"Số lượng đối tượng trong _mapObject phải là {totalObjects}.");
		}

		// --- Phương thức hỗ trợ (Reflection để truy cập Count) ---
		private int GetPrivateMapCount(string fieldName)
		{
			// Lấy FieldInfo cho field private
			var fieldInfo = typeof(RefPool).GetField(fieldName,
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			// Lấy giá trị và ép kiểu thành IDictionary để truy cập Count
			if (fieldInfo?.GetValue(_pool) is System.Collections.IDictionary dictionary)
			{
				return dictionary.Count;
			}
			return -1;
		}
	}
}