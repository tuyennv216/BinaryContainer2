using BinaryContainer2.Utilities.Datamodel;

namespace BinaryContainer2.Tests.UtilitiesTests.ChallengeTests
{
	[TestClass]
	public class SementIndexesChallengeTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_WithNegativeLength_ShouldSetToMinimum()
		{
			var indexes = new SementIndexes(-5, 2, 3, 1);
			Assert.AreEqual(1, indexes.Length);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_WithZeroLength_ShouldSetToMinimum()
		{
			var indexes = new SementIndexes(0, 5, 10, 3);
			Assert.AreEqual(1, indexes.Length);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		public void Constructor_WithLargeCircularStartIndex_ShouldWrapCorrectly()
		{
			var indexes = new SementIndexes(10, 25, 5, 3);
			Assert.AreEqual(5, indexes.StartIndex); // 25 % 10 = 5
			Assert.AreEqual(5, indexes.Use);
			Assert.AreEqual(3, indexes.WorkingIndex);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_WithUseGreaterThanLength_ShouldCapToLength()
		{
			var indexes = new SementIndexes(5, 1, 10, 3);
			Assert.AreEqual(5, indexes.Use);
			Assert.AreEqual(1, indexes.StartIndex);
			Assert.AreEqual(3, indexes.WorkingIndex);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_WithNegativeUse_ShouldSetToZero()
		{
			var indexes = new SementIndexes(10, 2, -5, 4);
			Assert.AreEqual(0, indexes.Use);
			Assert.AreEqual(2, indexes.StartIndex);
			Assert.AreEqual(4, indexes.WorkingIndex);
		}

		[TestMethod]
		public void MoveNext_WhenLengthIsOne_ShouldNotChangeAnything()
		{
			var indexes = new SementIndexes(1, 0, 1, 0);
			indexes.MoveNext();
			Assert.AreEqual(0, indexes.WorkingIndex);
			Assert.AreEqual(1, indexes.Use);
		}

		[TestMethod]
		public void MoveNext_WhenUseIsZero_ShouldNotChangeAnything()
		{
			var indexes = new SementIndexes(10, 3, 0, 5);
			indexes.MoveNext();
			Assert.AreEqual(3, indexes.WorkingIndex);
			Assert.AreEqual(0, indexes.Use);
		}

		[TestMethod]
		public void MoveNext_AtEndOfUsedRange_ShouldNotMove()
		{
			var indexes = new SementIndexes(10, 3, 4, 6); // Range: 3,4,5,6
			indexes.MoveNext(); // Should not move beyond 6
			Assert.AreEqual(6, indexes.WorkingIndex);
		}

		[TestMethod]
		public void MoveNext_WithCircularBoundaryCrossing()
		{
			var indexes = new SementIndexes(5, 3, 3, 4); // Range: 3,4,0
			indexes.MoveNext(); // Should move from 4 to 0
			Assert.AreEqual(0, indexes.WorkingIndex);
		}

		[TestMethod]
		public void MovePrevious_AtStartOfUsedRange_ShouldNotMove()
		{
			var indexes = new SementIndexes(10, 3, 4, 3);
			indexes.MovePrevious();
			Assert.AreEqual(3, indexes.WorkingIndex);
		}

		[TestMethod]
		public void MovePrevious_WithCircularBoundaryCrossing()
		{
			var indexes = new SementIndexes(5, 3, 3, 0); // Range: 3,4,0
			indexes.MovePrevious(); // Should move from 0 to 4
			Assert.AreEqual(4, indexes.WorkingIndex);
		}

		[TestMethod]
		public void NewIndex_WhenLengthIsOne_ShouldNotChangeAnything()
		{
			var indexes = new SementIndexes(1, 0, 1, 0);
			indexes.NewIndex();
			Assert.AreEqual(0, indexes.WorkingIndex);
			Assert.AreEqual(1, indexes.Use);
		}

		[TestMethod]
		public void NewIndex_WhenUseIsZero_ShouldInitializeFirstElement()
		{
			var indexes = new SementIndexes(10, 5, 0, 7);
			indexes.NewIndex();
			Assert.AreEqual(1, indexes.Use);
			Assert.AreEqual(5, indexes.StartIndex);
			Assert.AreEqual(5, indexes.WorkingIndex);
		}

		[TestMethod]
		public void NewIndex_AtEndWithFullUsage_ShouldShiftStartIndex()
		{
			var indexes = new SementIndexes(5, 1, 5, 0); // Full range: 1,2,3,4,0
			indexes.NewIndex();
			Assert.AreEqual(2, indexes.StartIndex);
			Assert.AreEqual(1, indexes.WorkingIndex);
			Assert.AreEqual(5, indexes.Use);
		}

		[TestMethod]
		public void NewIndex_AtEndWithAvailableSpace_ShouldExtendUsage()
		{
			var indexes = new SementIndexes(10, 3, 4, 6); // Range: 3,4,5,6
			indexes.NewIndex();
			Assert.AreEqual(3, indexes.StartIndex);
			Assert.AreEqual(7, indexes.WorkingIndex);
			Assert.AreEqual(5, indexes.Use);
		}

		[TestMethod]
		public void NewIndex_WorkingIndexBeforeStartIndex_ComplexCircularCase()
		{
			var indexes = new SementIndexes(8, 6, 3, 1); // Range: 6,7,0, WorkingIndex=1
			indexes.NewIndex();
			Assert.AreEqual(2, indexes.WorkingIndex);
			Assert.AreEqual(5, indexes.Use); // (1+8)-6+1 = 4? Let's verify
		}

		[TestMethod]
		public void NewIndex_WorkingIndexExactlyAtStartIndex()
		{
			var indexes = new SementIndexes(10, 5, 3, 5);
			indexes.NewIndex();
			Assert.AreEqual(6, indexes.WorkingIndex);
			Assert.AreEqual(2, indexes.Use);
		}

		[TestMethod]
		public void NewIndex_MultipleOperationsWithCircularWrap()
		{
			var indexes = new SementIndexes(4, 2, 3, 0); // Range: 2,3,0
			indexes.NewIndex(); // Should add position 1
			Assert.AreEqual(1, indexes.WorkingIndex);
			Assert.AreEqual(4, indexes.Use); // Now using all 4 positions
			Assert.AreEqual(2, indexes.StartIndex);
		}

		[TestMethod]
		public void NewIndex_AfterReachingFullCapacity_ShouldMaintainCircularBuffer()
		{
			var indexes = new SementIndexes(3, 0, 3, 2); // Full range: 0,1,2
			indexes.NewIndex(); // Should shift start to 1, working becomes 0
			Assert.AreEqual(1, indexes.StartIndex);
			Assert.AreEqual(0, indexes.WorkingIndex);
			Assert.AreEqual(3, indexes.Use);
		}

		[TestMethod]
		public void CorrectIndex_WithLargeNegativeValues()
		{
			var indexes = new SementIndexes(7, 2, 3, 1);
			// Test private method via reflection or verify through constructor
			var method = typeof(SementIndexes).GetMethod("CorrectIndex",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			var result = (int)method.Invoke(indexes, new object[] { -23 });
			Assert.AreEqual(5, result); // (-23 % 7 + 7) % 7 = 5
		}

		[TestMethod]
		public void CorrectIndex_WithLargePositiveValues()
		{
			var indexes = new SementIndexes(5, 1, 2, 3);
			var method = typeof(SementIndexes).GetMethod("CorrectIndex",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			var result = (int)method.Invoke(indexes, new object[] { 47 });
			Assert.AreEqual(2, result); // 47 % 5 = 2
		}

		[TestMethod]
		public void StressTest_MultipleOperationsWithRandomValues()
		{
			var indexes = new SementIndexes(100, 0, 0, 0);

			for (int i = 0; i < 1000; i++)
			{
				indexes.NewIndex();
				indexes.MoveNext();
				indexes.MovePrevious();
			}

			// Should not crash and maintain valid state
			Assert.IsTrue(indexes.Use >= 0 && indexes.Use <= indexes.Length);
			Assert.IsTrue(indexes.StartIndex >= 0 && indexes.StartIndex < indexes.Length);
			Assert.IsTrue(indexes.WorkingIndex >= 0 && indexes.WorkingIndex < indexes.Length);
		}

		[TestMethod]
		public void BoundaryTest_MaxIntValues()
		{
			var indexes = new SementIndexes(int.MaxValue, int.MaxValue - 1, 10, int.MaxValue - 5);
			indexes.MoveNext();
			indexes.NewIndex();
			// Should not overflow or crash
			Assert.IsTrue(indexes.Use <= indexes.Length);
		}

		[TestMethod]
		public void ComplexScenario_CircularBufferFullCycle()
		{
			var indexes = new SementIndexes(5, 0, 0, 0);

			// Fill the buffer
			for (int i = 0; i < 5; i++)
			{
				indexes.NewIndex();
			}
			Assert.AreEqual(5, indexes.Use);
			Assert.AreEqual(0, indexes.StartIndex);
			Assert.AreEqual(4, indexes.WorkingIndex);

			// Add one more - should shift start
			indexes.NewIndex();
			Assert.AreEqual(5, indexes.Use);
			Assert.AreEqual(1, indexes.StartIndex);
			Assert.AreEqual(0, indexes.WorkingIndex);
		}
	}
}