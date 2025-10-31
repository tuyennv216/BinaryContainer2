using BinaryContainer2.Utilities.Datamodel;

namespace BinaryContainer2.Tests.UtilitiesTests.DatamodelTests
{
	[TestClass]
	public class SementIndexesTests
	{
		[TestMethod]
		[TestCategory("Constructor")]
		public void Constructor_NormalLength_SetsPropertiesCorrectly()
		{
			// Kiểm tra constructor với giá trị length bình thường
			var indexes = new SementIndexes(10);

			Assert.AreEqual(10, indexes.Length);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_ZeroLength_DefaultsToOne()
		{
			// Kiểm tra constructor với length = 0 (giá trị biên)
			var indexes = new SementIndexes(0);

			Assert.AreEqual(1, indexes.Length);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_NegativeLength_DefaultsToOne()
		{
			// Kiểm tra constructor với length âm (trường hợp đặc biệt)
			var indexes = new SementIndexes(-5);

			Assert.AreEqual(1, indexes.Length);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MoveNextWorkingIndex")]
		public void MoveNextWorkingIndex_NormalCase_IncrementsWorkingIndex()
		{
			// Kiểm tra di chuyển working index trong trường hợp bình thường
			var indexes = new SementIndexes(10, 0, 5, 2);

			indexes.MoveNext();

			Assert.AreEqual(3, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MoveNextWorkingIndex")]
		public void MoveNextWorkingIndex_AtEndIndex_StaysAtEndIndex()
		{
			// Kiểm tra khi working index đã ở cuối cùng
			var indexes = new SementIndexes(10, 0, 5, 4);

			indexes.MoveNext();

			Assert.AreEqual(4, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MoveNextWorkingIndex")]
		public void MoveNextWorkingIndex_LengthOne_NoChange()
		{
			// Kiểm tra với Length = 1 (trường hợp đặc biệt)
			var indexes = new SementIndexes(1, 0, 1, 0);

			indexes.MoveNext();

			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MoveNextWorkingIndex")]
		public void MoveNextWorkingIndex_UseZero_NoChange()
		{
			// Kiểm tra khi Use = 0 (trường hợp đặc biệt)
			var indexes = new SementIndexes(10, 0, 0, 0);

			indexes.MoveNext();

			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MoveNextWorkingIndex")]
		public void MoveNextWorkingIndex_WrapAround_CorrectModulo()
		{
			// Kiểm tra wrap-around khi vượt quá Length
			var indexes = new SementIndexes(5, 3, 4, 6);

			indexes.MoveNext();

			Assert.AreEqual(1, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MovePreviousWorkingIndex")]
		public void MovePreviousWorkingIndex_NormalCase_DecrementsWorkingIndex()
		{
			// Kiểm tra di chuyển working index về trước trong trường hợp bình thường
			var indexes = new SementIndexes(10, 0, 5, 3);

			indexes.MovePrevious();

			Assert.AreEqual(2, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MovePreviousWorkingIndex")]
		public void MovePreviousWorkingIndex_AtStartIndex_StaysAtStartIndex()
		{
			// Kiểm tra khi working index đã ở vị trí đầu
			var indexes = new SementIndexes(10, 2, 5, 2);

			indexes.MovePrevious();

			Assert.AreEqual(2, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MovePreviousWorkingIndex")]
		public void MovePreviousWorkingIndex_LengthOne_NoChange()
		{
			// Kiểm tra với Length = 1
			var indexes = new SementIndexes(1, 0, 1, 0);

			indexes.MovePrevious();

			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MovePreviousWorkingIndex")]
		public void MovePreviousWorkingIndex_UseZero_NoChange()
		{
			// Kiểm tra khi Use = 0
			var indexes = new SementIndexes(10, 0, 0, 0);

			indexes.MovePrevious();

			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("MovePreviousWorkingIndex")]
		public void MovePreviousWorkingIndex_WrapAround_CorrectModulo()
		{
			// Kiểm tra wrap-around khi working index âm
			var indexes = new SementIndexes(5, 3, 4, 3);

			indexes.MovePrevious();

			Assert.AreEqual(3, indexes.WorkingIndex); // Không thể nhỏ hơn StartIndex
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_NormalCase_IncrementsWorkingIndexAndUse()
		{
			// Kiểm tra mở rộng working index trong trường hợp bình thường
			var indexes = new SementIndexes(10, 0, 3, 1);

			indexes.NewIndex();

			Assert.AreEqual(2, indexes.WorkingIndex);
			Assert.AreEqual(3, indexes.Use); // Use = WorkingIndex - StartIndex + 1 = 2 - 0 + 1 = 3
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_AtEndIndex_FullLength_IncreasesStartIndex()
		{
			// Kiểm tra khi ở cuối và đã dùng hết length
			var indexes = new SementIndexes(5, 2, 5, 6);

			indexes.NewIndex();

			Assert.AreEqual(3, indexes.StartIndex); // 2 + 1 = 3 → 3 % 5 = 3
			Assert.AreEqual(5, indexes.Use);
			Assert.AreEqual(2, indexes.WorkingIndex); // 6 + 1 = 7 → 7 % 5 = 2
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_AtEndIndex_NotFullLength_IncreasesUse()
		{
			// Kiểm tra khi ở cuối nhưng chưa dùng hết length
			var indexes = new SementIndexes(10, 0, 5, 4);

			indexes.NewIndex();

			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(6, indexes.Use);
			Assert.AreEqual(5, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_BeforeStartIndex_WrapAround()
		{
			// Kiểm tra khi working index nằm trước start index (wrap-around)
			var indexes = new SementIndexes(5, 3, 3, 0);

			indexes.NewIndex();

			Assert.AreEqual(3, indexes.StartIndex);
			Assert.AreEqual(4, indexes.Use); // (1 + 5) - 3 + 1 = 4
			Assert.AreEqual(1, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_LengthOne_NoChange()
		{
			// Kiểm tra với Length = 1
			var indexes = new SementIndexes(1, 0, 1, 0);

			indexes.NewIndex();

			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(1, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("ExpandWorkingIndex")]
		public void ExpandWorkingIndex_UseZero_SetsUseToOne()
		{
			// Kiểm tra khi Use = 0
			var indexes = new SementIndexes(10, 0, 0, 0);

			indexes.NewIndex();

			Assert.AreEqual(1, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[TestCategory("Random")]
		public void RandomOperations_ConsistentBehavior()
		{
			for (int r = 0; r < 1000; r++)
			{
				// Kiểm tra ngẫu nhiên với nhiều thao tác liên tiếp
				var indexes = new SementIndexes(8);
				var random = new Random();
				//List<int> abc = new();

				for (int i = 0; i < 100; i++)
				{
					var operation = random.Next(3);
					//abc.Add(operation);

					switch (operation)
					{
						case 0:
							indexes.MoveNext();
							break;
						case 1:
							indexes.MovePrevious();
							break;
						case 2:
							indexes.NewIndex();
							break;
					}

					// Kiểm tra ràng buộc cơ bản
					Assert.IsTrue(indexes.StartIndex >= 0 && indexes.StartIndex < indexes.Length);
					Assert.IsTrue(indexes.WorkingIndex >= 0 && indexes.WorkingIndex < indexes.Length);
					Assert.IsTrue(indexes.Use >= 0 && indexes.Use <= indexes.Length);
				}
			}
		}

		[TestMethod]
		[TestCategory("Boundary")]
		public void Boundary_MaxValues_CorrectModule()
		{
			// Kiểm tra giá trị biên lớn với module
			var indexes = new SementIndexes(100, 99, 100, 198);

			indexes.MoveNext();
			Assert.AreEqual(98, indexes.WorkingIndex); // 198 + 1 = 199 → 199 % 100 = 99
		}

		[TestMethod]
		[TestCategory("Boundary")]
		public void Boundary_MinValues_ValidState()
		{
			// Kiểm tra giá trị biên nhỏ nhất
			var indexes = new SementIndexes(1, 0, 1, 0);

			indexes.MoveNext();
			indexes.MovePrevious();
			indexes.NewIndex();

			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(1, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}
	}
}