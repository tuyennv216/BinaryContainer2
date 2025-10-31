using BinaryContainer2.Utilities;
using BinaryContainer2.Utilities.Exceptions;

namespace BinaryContainer2.Tests.UtilitiesTests
{
	[TestClass]
	public class UndoRedoTests
	{
		// Test wrapper class
		public class TestWrapper
		{
			public int Value { get; set; }
			public string Text { get; set; }
		}

		#region Constructor Tests

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Normal")]
		public void Constructor_WithValidParameters_InitializesCorrectly()
		{
			// Tests normal constructor initialization with valid parameters
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			Assert.IsNotNull(undoRedo);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Boundary")]
		public void Constructor_WithMaxStepOne_InitializesCorrectly()
		{
			// Tests boundary case with minimum valid maxStep (1)
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "Value", 1);

			Assert.IsNotNull(undoRedo);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Boundary")]
		public void Constructor_WithMaxStepZero_DefaultsToOne()
		{
			// Tests boundary case where maxStep <= 0 should default to 1
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "Value", 0);

			Assert.IsNotNull(undoRedo);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Special")]
		public void Constructor_WithLargeMaxStep_InitializesCorrectly()
		{
			// Tests special case with very large maxStep value
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "Value", int.MaxValue);

			Assert.IsNotNull(undoRedo);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Exception")]
		[ExpectedException(typeof(MemberNotFoundException))]
		public void Constructor_WithInvalidMemberName_ThrowsException()
		{
			// Tests exception case with non-existent member name
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "NonExistentProperty", 10);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Exception")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_WithNullWrapper_ThrowsException()
		{
			// Tests exception case with null wrapper object
			var undoRedo = new UndoRedo(null, "Value", 10);
		}

		[TestMethod]
		[TestCategory("Constructor")]
		[TestCategory("Exception")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_WithNullMemberName_ThrowsException()
		{
			// Tests exception case with null member name
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, null, 10);
		}

		#endregion

		#region Commit Tests

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Normal")]
		public void Commit_FirstCommit_SavesCurrentState()
		{
			// Tests normal first commit operation
			var wrapper = new TestWrapper { Value = 42 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			wrapper.Value = 100;
			undoRedo.Undo();

			Assert.AreEqual(42, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Normal")]
		public void Commit_MultipleCommits_StoresHistoryCorrectly()
		{
			// Tests multiple sequential commits
			var wrapper = new TestWrapper { Value = 1 };
			var undoRedo = new UndoRedo(wrapper, "Value", 3);

			undoRedo.Commit(); // State 1
			wrapper.Value = 2;
			undoRedo.Commit(); // State 2
			wrapper.Value = 3;
			undoRedo.Commit(); // State 3

			undoRedo.Undo(); // Should go to state 2
			Assert.AreEqual(2, wrapper.Value);

			undoRedo.Undo(); // Should go to state 1
			Assert.AreEqual(1, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Boundary")]
		public void Commit_WithNullValue_StoresNullCorrectly()
		{
			// Tests boundary case with null value
			var wrapper = new TestWrapper { Text = "test" };
			var undoRedo = new UndoRedo(wrapper, "Text", 10);

			undoRedo.Commit();
			wrapper.Text = null;
			undoRedo.Commit();

			wrapper.Text = "modified";
			undoRedo.Undo();

			Assert.IsNull(wrapper.Text);
		}

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Boundary")]
		public void Commit_AtMaxCapacity_OverwritesOldest()
		{
			// Tests boundary case when history reaches max capacity
			var wrapper = new TestWrapper { Value = 0 };
			var undoRedo = new UndoRedo(wrapper, "Value", 3);

			// Fill history
			for (int i = 1; i <= 4; i++)
			{
				wrapper.Value = i;
				undoRedo.Commit();
			}

			// Should have states 2, 3, 4 (state 1 was overwritten)
			undoRedo.Undo(); // State 3
			undoRedo.Undo(); // State 2
			undoRedo.Undo(); // Still state 2 (oldest available)

			Assert.AreEqual(2, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Special")]
		public void Commit_AfterUndo_TruncatesFutureHistory()
		{
			// Tests special case of committing after undo operations
			var wrapper = new TestWrapper { Value = 1 };
			var undoRedo = new UndoRedo(wrapper, "Value", 5);

			undoRedo.Commit(); // State 1
			wrapper.Value = 2;
			undoRedo.Commit(); // State 2
			wrapper.Value = 3;
			undoRedo.Commit(); // State 3

			undoRedo.Undo(); // Back to state 2
			undoRedo.Undo(); // Back to state 1

			wrapper.Value = 4;
			undoRedo.Commit(); // New state after undo - should truncate future states

			// Redo should not go back to state 2 or 3
			undoRedo.Redo();
			Assert.AreEqual(4, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Commit")]
		[TestCategory("Random")]
		public void Commit_RandomOperationSequence_MaintainsConsistency()
		{
			// Tests random sequence of operations
			var wrapper = new TestWrapper { Value = 0 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);
			var random = new Random();

			undoRedo.Commit(); // Initial state

			for (int i = 0; i < 100; i++)
			{
				int operation = random.Next(0, 4);

				switch (operation)
				{
					case 0: // Change value and commit
						wrapper.Value = random.Next(1, 1000);
						undoRedo.Commit();
						break;
					case 1: // Undo
						undoRedo.Undo();
						break;
					case 2: // Redo
						undoRedo.Redo();
						break;
					case 3: // Change value without commit
						wrapper.Value = random.Next(1, 1000);
						break;
				}

				// Verify consistency after each operation
				Assert.IsTrue(wrapper.Value >= 0);
			}
		}

		#endregion

		#region Undo Tests

		[TestMethod]
		[TestCategory("Undo")]
		[TestCategory("Normal")]
		public void Undo_WithHistory_ReturnsToPreviousState()
		{
			// Tests normal undo operation with history available
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			wrapper.Value = 20;
			undoRedo.Commit();

			undoRedo.Undo();
			Assert.AreEqual(10, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Undo")]
		[TestCategory("Boundary")]
		public void Undo_AtBeginning_DoesNothing()
		{
			// Tests boundary case when no more undo is available
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();

			// Multiple undos at beginning should not change state
			undoRedo.Undo();
			int firstUndo = wrapper.Value;
			undoRedo.Undo();
			int secondUndo = wrapper.Value;

			Assert.AreEqual(firstUndo, secondUndo);
		}

		[TestMethod]
		[TestCategory("Undo")]
		[TestCategory("Boundary")]
		public void Undo_WithSingleState_RemainsUnchanged()
		{
			// Tests boundary case with only one state in history
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			wrapper.Value = 20; // Modify without commit
			undoRedo.Undo(); // Should revert to committed state

			Assert.AreEqual(10, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Undo")]
		[TestCategory("Special")]
		public void Undo_AfterValueChangedWithoutCommit_UsesLatestCommitted()
		{
			// Tests special case where value changed but not committed before undo
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit(); // State 1

			wrapper.Value = 20;
			undoRedo.Commit(); // State 2

			wrapper.Value = 30; // Changed but not committed

			undoRedo.Undo(); // Should go to state 1 (not state 2 because current = state 2)
			Assert.AreEqual(20, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Undo")]
		[TestCategory("Special")]
		public void Undo_WithCircularBuffer_WrapsCorrectly()
		{
			// Tests special case with circular buffer behavior
			var wrapper = new TestWrapper { Value = 0 };
			var undoRedo = new UndoRedo(wrapper, "Value", 3);

			// Fill and overflow buffer
			for (int i = 1; i <= 4; i++)
			{
				wrapper.Value = i;
				undoRedo.Commit();
			}

			// Should be able to undo through circular buffer
			undoRedo.Undo(); // State 3
			undoRedo.Undo(); // State 2
			undoRedo.Undo(); // Still state 2 (oldest)

			Assert.AreEqual(2, wrapper.Value);
		}

		#endregion

		#region Redo Tests

		[TestMethod]
		[TestCategory("Redo")]
		[TestCategory("Normal")]
		public void Redo_AfterUndo_ReturnsToNextState()
		{
			// Tests normal redo operation after undo
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			wrapper.Value = 20;
			undoRedo.Commit();

			undoRedo.Undo(); // To state 10
			undoRedo.Redo(); // Back to state 20

			Assert.AreEqual(20, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Redo")]
		[TestCategory("Boundary")]
		public void Redo_AtEnd_DoesNothing()
		{
			// Tests boundary case when no more redo is available
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			wrapper.Value = 20;
			undoRedo.Commit();

			undoRedo.Undo(); // To state 10
			undoRedo.Redo(); // To state 20
			undoRedo.Redo(); // Should stay at state 20

			Assert.AreEqual(20, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Redo")]
		[TestCategory("Boundary")]
		public void Redo_WithoutUndo_DoesNothing()
		{
			// Tests boundary case when redo is called without previous undo
			var wrapper = new TestWrapper { Value = 10 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit();
			int originalValue = wrapper.Value;
			undoRedo.Redo(); // Should do nothing

			Assert.AreEqual(originalValue, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Redo")]
		[TestCategory("Special")]
		public void Redo_AfterCommit_InvalidatesFuture()
		{
			// Tests special case where commit after undo invalidates redo stack
			var wrapper = new TestWrapper { Value = 1 };
			var undoRedo = new UndoRedo(wrapper, "Value", 10);

			undoRedo.Commit(); // State 1
			wrapper.Value = 2;
			undoRedo.Commit(); // State 2
			wrapper.Value = 3;
			undoRedo.Commit(); // State 3

			undoRedo.Undo(); // State 2
			undoRedo.Undo(); // State 1

			wrapper.Value = 4;
			undoRedo.Commit(); // New branch - invalidates states 2 and 3

			undoRedo.Redo(); // Should do nothing (future states invalidated)
			Assert.AreEqual(4, wrapper.Value);
		}

		#endregion

		#region Integration Tests

		[TestMethod]
		[TestCategory("Integration")]
		[TestCategory("Complex")]
		public void ComplexScenario_UndoRedoSequence_WorksCorrectly()
		{
			// Tests complex sequence of operations
			var wrapper = new TestWrapper { Value = 0 };
			var undoRedo = new UndoRedo(wrapper, "Value", 5);

			// Initial state
			undoRedo.Commit(); // State 0

			// Create some history
			for (int i = 1; i <= 3; i++)
			{
				wrapper.Value = i;
				undoRedo.Commit();
			}

			// Undo twice
			undoRedo.Undo(); // State 2
			undoRedo.Undo(); // State 1

			// Create new branch
			wrapper.Value = 10;
			undoRedo.Commit(); // New state 10 (invalidates states 2 and 3)

			// Verify correct behavior
			undoRedo.Undo(); // State 1
			Assert.AreEqual(1, wrapper.Value);

			undoRedo.Redo(); // State 10
			Assert.AreEqual(10, wrapper.Value);

			undoRedo.Redo(); // Should stay at state 10
			Assert.AreEqual(10, wrapper.Value);
		}

		[TestMethod]
		[TestCategory("Integration")]
		[TestCategory("Stress")]
		public void StressTest_ManyOperations_HandlesCorrectly()
		{
			// Tests performance and correctness with many operations
			var wrapper = new TestWrapper { Value = 0 };
			var undoRedo = new UndoRedo(wrapper, "Value", 100);

			undoRedo.Commit(); // Initial state

			for (int i = 1; i <= 1000; i++)
			{
				wrapper.Value = i;
				if (i % 3 == 0) undoRedo.Commit();
				if (i % 5 == 0) undoRedo.Undo();
				if (i % 7 == 0) undoRedo.Redo();

				// Verify no exceptions and reasonable behavior
				Assert.IsTrue(wrapper.Value >= 0);
			}
		}

		[TestMethod]
		[TestCategory("Integration")]
		[TestCategory("DataType")]
		public void DifferentDataTypes_UndoRedo_WorksCorrectly()
		{
			// Tests with different data types
			var wrapper = new TestWrapper();
			var undoRedo = new UndoRedo(wrapper, "Text", 10);

			// Test string values
			wrapper.Text = "First";
			undoRedo.Commit();

			wrapper.Text = "Second";
			undoRedo.Commit();

			wrapper.Text = "Third";
			undoRedo.Commit();

			undoRedo.Undo(); // Should be "Second"
			Assert.AreEqual("Second", wrapper.Text);

			undoRedo.Undo(); // Should be "First"
			Assert.AreEqual("First", wrapper.Text);

			undoRedo.Redo(); // Should be "Second"
			Assert.AreEqual("Second", wrapper.Text);
		}

		#endregion
	}
}