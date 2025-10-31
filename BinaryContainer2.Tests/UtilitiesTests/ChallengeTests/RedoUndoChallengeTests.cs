using BinaryContainer2.Utilities;
using BinaryContainer2.Utilities.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace BinaryContainer2.Tests.UtilitiesTests.ChallengeTests
{
	[TestClass]
	public class RedoUndoChallengeTests
	{
		#region Edge Cases for Constructor

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_WithNullWrapper_ThrowsArgumentNullException()
		{
			var undoRedo = new UndoRedo(null, "TestProperty", 10);
		}

		[TestMethod]
		public void Constructor_WithMaxStepZero_SetsMaxStepToOne()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 0);

			// Tạo nhiều commit để kiểm tra xem nó chỉ lưu 1 bước
			testObj.TestProperty = "Value1";
			undoRedo.Commit();

			testObj.TestProperty = "Value2";
			undoRedo.Commit(); // Value1 sẽ bị ghi đè

			testObj.TestProperty = "Value3";
			undoRedo.Commit(); // Value2 sẽ bị ghi đè

			undoRedo.Undo(); // Nếu maxStep = 1, sẽ quay lại Value2 (không thể undo về Value1)
			Assert.AreEqual("Value3", testObj.TestProperty);
		}

		[TestMethod]
		[ExpectedException(typeof(MemberNotFoundException))]
		public void Constructor_WithInvalidMemberName_NoExceptionUntilUsage()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "NonExistentProperty", 10);
		}

		#endregion

		#region Complex Commit Scenarios

		[TestMethod]
		public void Commit_WithCircularReferences_HandlesGracefully()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "ComplexProperty", 10);

			var circularObject = new CircularReferenceClass();
			circularObject.SelfReference = circularObject;
			circularObject.Data = "Initial";

			testObj.ComplexProperty = circularObject;
			undoRedo.Commit();

			circularObject.Data = "Modified";
			undoRedo.Commit();

			undoRedo.Undo();
			var restored = testObj.ComplexProperty as CircularReferenceClass;
			Assert.AreEqual("Initial", restored.Data);
		}

		[TestMethod]
		public void Commit_LargeData_ExceedsMemoryLimits()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "LargeDataProperty", 5);

			// Dữ liệu lớn có thể làm tràn bộ nhớ khi serialize
			var largeData = new byte[1000000]; // 1MB
			new Random().NextBytes(largeData);

			testObj.LargeDataProperty = largeData;
			undoRedo.Commit();

			// Thay đổi chỉ 1 byte
			largeData[0] = (byte)(largeData[0] + 1);
			undoRedo.Commit();

			undoRedo.Undo();
			Assert.AreEqual(0, ((byte[])testObj.LargeDataProperty)[0] - (byte)(largeData[0] - 1));
		}

		[TestMethod]
		public void Commit_WithCustomSerializationLogic_ValidatesCorrectness()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "CustomSerializableProperty", 10);

			var customObj = new CustomSerializableClass
			{
				SecretValue = "SensitiveData",
				Version = 1
			};

			testObj.CustomSerializableProperty = customObj;
			undoRedo.Commit();

			customObj.Version = 2;
			undoRedo.Commit();

			undoRedo.Undo();
			var restored = testObj.CustomSerializableProperty as CustomSerializableClass;
			Assert.AreEqual(1, restored.Version);
			Assert.AreEqual("SensitiveData", restored.SecretValue);
		}

		#endregion

		#region Advanced Undo/Redo Mechanics

		[TestMethod]
		public void Undo_EmptyHistory_NoSideEffects()
		{
			var testObj = new TestClass { TestProperty = "Initial" };
			var undoRedo = new UndoRedo(testObj, "TestProperty", 10);

			undoRedo.Undo(); // Không có commit nào trước đó
			Assert.AreEqual("Initial", testObj.TestProperty);
		}

		[TestMethod]
		public void Redo_AtEndOfHistory_NoSideEffects()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 3);

			testObj.TestProperty = "Value1";
			undoRedo.Commit();

			testObj.TestProperty = "Value2";
			undoRedo.Commit();

			undoRedo.Undo(); // Về Value1
			undoRedo.Redo(); // Đến Value2
			undoRedo.Redo(); // Ở cuối history

			Assert.AreEqual("Value2", testObj.TestProperty);
		}

		[TestMethod]
		public void UndoRedo_ChainWithIdenticalValues_OptimizesCorrectly()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 5);

			// Chuỗi giá trị giống nhau
			testObj.TestProperty = "SameValue";
			undoRedo.Commit();

			testObj.TestProperty = "SameValue"; // Giá trị không đổi
			undoRedo.Commit();

			testObj.TestProperty = "Different";
			undoRedo.Commit();

			undoRedo.Undo();
			Assert.AreEqual("SameValue", testObj.TestProperty);

			undoRedo.Undo(); // Phải về SameValue đầu tiên
			Assert.AreEqual("SameValue", testObj.TestProperty);
		}

		[TestMethod]
		public void Undo_MiddleOfHistory_CreatesNewBranch()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 5);

			// Tạo history: V1 -> V2 -> V3
			testObj.TestProperty = "V1";
			undoRedo.Commit();

			testObj.TestProperty = "V2";
			undoRedo.Commit();

			testObj.TestProperty = "V3";
			undoRedo.Commit();

			// Undo về V2
			undoRedo.Undo();
			Assert.AreEqual("V2", testObj.TestProperty);

			// Thay đổi tạo branch: V1 -> V2 -> V4
			testObj.TestProperty = "V4";
			undoRedo.Commit();

			// V3 phải bị mất khỏi history
			undoRedo.Redo(); // Không thể redo vì đã tạo branch mới
			Assert.AreEqual("V4", testObj.TestProperty);
		}

		#endregion

		#region Concurrency and Thread Safety

		[TestMethod]
		public void UndoRedo_ConcurrentAccess_RaceConditions()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 100);

			testObj.TestProperty = "Initial";
			undoRedo.Commit();

			var exceptions = new ConcurrentBag<Exception>();
			var tasks = new List<Task>();

			// Multiple threads thực hiện undo/redo đồng thời
			for (int i = 0; i < 10; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						for (int j = 0; j < 10; j++)
						{
							if (j % 2 == 0)
								undoRedo.Undo();
							else
								undoRedo.Redo();
							Thread.Sleep(1);
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}));

				tasks.Add(Task.Run(() =>
				{
					try
					{
						for (int j = 0; j < 10; j++)
						{
							testObj.TestProperty = $"Value_{Thread.CurrentThread.ManagedThreadId}_{j}";
							undoRedo.Commit();
							Thread.Sleep(1);
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());

			Assert.AreEqual(0, exceptions.Count,
				$"Có {exceptions.Count} exceptions xảy ra: {string.Join(", ", exceptions)}");
		}

		[TestMethod]
		public void Commit_WhileUndoRedoInProgress_ConsistentState()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "TestProperty", 10);

			testObj.TestProperty = "V1";
			undoRedo.Commit();

			var commitTask = Task.Run(() =>
			{
				for (int i = 0; i < 5; i++)
				{
					testObj.TestProperty = $"Commit_{i}";
					undoRedo.Commit();
					Thread.Sleep(10);
				}
			});

			var undoTask = Task.Run(() =>
			{
				for (int i = 0; i < 5; i++)
				{
					undoRedo.Undo();
					Thread.Sleep(15);
				}
			});

			Task.WaitAll(commitTask, undoTask);

			// State phải consistent sau tất cả operations
			Assert.IsNotNull(testObj.TestProperty);
		}

		#endregion

		#region Memory Management and Performance

		[TestMethod]
		public void UndoRedo_MaxStepEviction_OldestItemsRemoved()
		{
			var testObj = new TestClass();
			const int maxSteps = 3;
			var undoRedo = new UndoRedo(testObj, "TestProperty", maxSteps);

			// Commit nhiều hơn maxSteps
			for (int i = 1; i <= maxSteps + 2; i++)
			{
				testObj.TestProperty = $"Value{i}";
				undoRedo.Commit();
			}

			// Undo nhiều lần - chỉ có thể undo tối đa maxSteps
			for (int i = 0; i < maxSteps; i++)
			{
				undoRedo.Undo();
			}

			// Thêm một undo nữa sẽ không có tác dụng
			var currentValue = testObj.TestProperty;
			undoRedo.Undo();
			Assert.AreEqual(currentValue, testObj.TestProperty);
		}

		[TestMethod]
		public void UndoRedo_MemoryLeakTest_ObjectsProperlyDisposed()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "DisposableProperty", 5);

			var weakRefs = new List<WeakReference>();

			for (int i = 0; i < 10; i++)
			{
				var disposableObj = new DisposableClass { Data = $"Data{i}" };
				weakRefs.Add(new WeakReference(disposableObj));

				testObj.DisposableProperty = disposableObj;
				undoRedo.Commit();

				disposableObj = null;
			}

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Các object cũ phải được collect
			var aliveCount = weakRefs.Count(wr => wr.IsAlive);
			Assert.IsTrue(aliveCount <= 5,
				$"Còn {aliveCount} objects sống, nhiều hơn maxSteps (5)");
		}

		#endregion

		#region Type System and Serialization Edge Cases

		[TestMethod]
		public void UndoRedo_WithDynamicTypes_HandlesTypeChanges()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "DynamicProperty", 10);

			// Thay đổi kiểu dữ liệu hoàn toàn
			testObj.DynamicProperty = "StringValue";
			undoRedo.Commit();

			testObj.DynamicProperty = 42;
			undoRedo.Commit();

			testObj.DynamicProperty = new List<int> { 1, 2, 3 };
			undoRedo.Commit();

			undoRedo.Undo();
			Assert.AreEqual(42, testObj.DynamicProperty);

			undoRedo.Undo();
			Assert.AreEqual("StringValue", testObj.DynamicProperty);
		}

		[TestMethod]
		public void UndoRedo_WithVersionedTypes_BackwardCompatibility()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "VersionedProperty", 10);

			var v1Object = new VersionedClassV1 { Data = "Old", Version = 1 };
			testObj.VersionedProperty = v1Object;
			undoRedo.Commit();

			// Giả sử sau này class thay đổi
			var v2Object = new VersionedClassV2 { Data = "New", Version = 2, NewField = "Additional" };
			testObj.VersionedProperty = v2Object;
			undoRedo.Commit();

			// Undo về V1 - phải handle được sự khác biệt version
			undoRedo.Undo();
			var restored = testObj.VersionedProperty as VersionedClassV1;
			Assert.IsNotNull(restored);
			Assert.AreEqual(1, restored.Version);
			Assert.AreEqual("Old", restored.Data);
		}

		[TestMethod]
		public void UndoRedo_WithNullableValueTypes_CorrectBehavior()
		{
			var testObj = new TestClass();
			var undoRedo = new UndoRedo(testObj, "NullableIntProperty", 5);

			testObj.NullableIntProperty = 100;
			undoRedo.Commit();

			testObj.NullableIntProperty = null;
			undoRedo.Commit();

			testObj.NullableIntProperty = 200;
			undoRedo.Commit();

			undoRedo.Undo();
			Assert.IsNull(testObj.NullableIntProperty);

			undoRedo.Undo();
			Assert.AreEqual(100, testObj.NullableIntProperty);
		}

		#endregion

		#region Helper Classes for Testing

		private class TestClass
		{
			public string TestProperty { get; set; }
			public object ComplexProperty { get; set; }
			public object LargeDataProperty { get; set; }
			public object CustomSerializableProperty { get; set; }
			public object DisposableProperty { get; set; }
			public object DynamicProperty { get; set; }
			public object VersionedProperty { get; set; }
			public int? NullableIntProperty { get; set; }
		}

		private class CircularReferenceClass
		{
			public CircularReferenceClass SelfReference { get; set; }
			public string Data { get; set; }
		}

		private class CustomSerializableClass
		{
			public string SecretValue { get; set; }
			public int Version { get; set; }
		}

		private class DisposableClass
		{
			public string Data { get; set; }
		}

		private class VersionedClassV1
		{
			public string Data { get; set; }
			public int Version { get; set; }
		}

		private class VersionedClassV2
		{
			public string Data { get; set; }
			public int Version { get; set; }
			public string NewField { get; set; }
		}

		#endregion
	}

	[TestClass]
	public class UndoRedoExtremeTests
	{
		#region Serialization Boundary Cases

		[TestMethod]
		public void UndoRedo_WithCustomSerializationSurrogates_ComplexObjects()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "CustomSerializedProperty", 10);

			var complexObj = new ComplexSerializableClass
			{
				DictionaryData = new Dictionary<string, ComplexSerializableClass>
				{
					["nested"] = new ComplexSerializableClass { Number = 42 }
				},
				ArrayData = new[] { new ComplexSerializableClass(), new ComplexSerializableClass() },
				Number = 100
			};

			testObj.CustomSerializedProperty = complexObj;
			undoRedo.Commit();

			complexObj.Number = 200;
			undoRedo.Commit();

			undoRedo.Undo();
			var restored = testObj.CustomSerializedProperty as ComplexSerializableClass;
			Assert.AreEqual(100, restored.Number);
			Assert.AreEqual(42, restored.DictionaryData["nested"].Number);
		}

		#endregion

		#region Advanced Concurrency Scenarios

		[TestMethod]
		public void UndoRedo_DeadlockPrevention_ConcurrentAccessPatterns()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "DeadlockProperty", 20);

			testObj.DeadlockProperty = "Initial";
			undoRedo.Commit();

			var deadlockDetected = false;
			var tasks = new List<Task>();

			for (int i = 0; i < 10; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						using (var mutex = new Mutex(false, "UndoRedoTestMutex"))
						{
							if (!mutex.WaitOne(1000)) // Timeout để phát hiện deadlock
							{
								deadlockDetected = true;
								return;
							}

							try
							{
								for (int j = 0; j < 5; j++)
								{
									testObj.DeadlockProperty = $"Task_{Task.CurrentId}_Value_{j}";
									undoRedo.Commit();
									undoRedo.Undo();
								}
							}
							finally
							{
								mutex.ReleaseMutex();
							}
						}
					}
					catch (AbandonedMutexException)
					{
						deadlockDetected = true;
					}
				}));
			}

			Assert.IsTrue(Task.WaitAll(tasks.ToArray(), 10000), "Timeout - có thể có deadlock");
			Assert.IsFalse(deadlockDetected, "Phát hiện deadlock");
		}

		#endregion

		#region Memory and Performance Extremes

		[TestMethod]
		public void UndoRedo_StressTest_HighFrequencyOperations()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "StressProperty", 1000);

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			// 10,000 operations với tốc độ cao
			for (int i = 0; i < 10000; i++)
			{
				testObj.StressProperty = $"Value_{i}";
				undoRedo.Commit();

				if (i % 10 == 0)
				{
					undoRedo.Undo();
					undoRedo.Redo();
				}

				if (i % 100 == 0)
				{
					// Tạo branch
					undoRedo.Undo();
					testObj.StressProperty = $"Branch_{i}";
					undoRedo.Commit();
				}
			}

			stopwatch.Stop();

			Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000,
				$"Performance issue: {stopwatch.ElapsedMilliseconds}ms cho 10,000 operations");
		}

		[TestMethod]
		public void UndoRedo_MemoryPressureTest_LargeObjectHeap()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "LargeObjectProperty", 10);

			// Tạo objects đủ lớn để vào Large Object Heap (>85KB)
			for (int i = 0; i < 20; i++)
			{
				var largeArray = new byte[90000]; // >85KB - vào LOH
				new Random().NextBytes(largeArray);
				testObj.LargeObjectProperty = largeArray;
				undoRedo.Commit();
			}

			// Kiểm tra memory usage
			var memoryBefore = GC.GetTotalMemory(true);

			// Thực hiện undo/redo để trigger GC
			for (int i = 0; i < 5; i++)
			{
				undoRedo.Undo();
				undoRedo.Redo();
			}

			var memoryAfter = GC.GetTotalMemory(true);

			// Memory không được tăng quá nhiều
			Assert.IsTrue(memoryAfter < memoryBefore * 1.5,
				$"Memory leak detected: {memoryBefore} -> {memoryAfter}");
		}

		#endregion

		#region Type System and Reflection Edge Cases

		[TestMethod]
		public void UndoRedo_WithGenericTypes_TypeParameterHandling()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "GenericProperty", 10);

			// Sử dụng generic types với các type parameters phức tạp
			var genericList = new List<Dictionary<string, List<int>>>
			{
				new Dictionary<string, List<int>>
				{
					["key1"] = new List<int> { 1, 2, 3 },
					["key2"] = new List<int> { 4, 5, 6 }
				}
			};

			testObj.GenericProperty = genericList;
			undoRedo.Commit();

			genericList[0]["key1"].Add(7);
			undoRedo.Commit();

			undoRedo.Undo();
			var restored = testObj.GenericProperty as List<Dictionary<string, List<int>>>;
			Assert.AreEqual(3, restored[0]["key1"].Count);
		}

		[TestMethod]
		public void UndoRedo_WithInterfaceTypes_RuntimeTypeResolution()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "InterfaceProperty", 10);

			// Lưu dưới dạng interface nhưng restore về concrete type
			IDisposable disposableObj = new CustomDisposable();
			testObj.InterfaceProperty = disposableObj;
			undoRedo.Commit();

			disposableObj.Dispose();
			testObj.InterfaceProperty = "NewValue";
			undoRedo.Commit();

			// Undo phải restore concrete type implement interface
			undoRedo.Undo();
			var restored = testObj.InterfaceProperty as IDisposable;
			Assert.IsNotNull(restored);
			Assert.IsTrue(restored.GetType() == typeof(CustomDisposable));
		}

		#endregion

		#region Exception Handling and Recovery

		[TestMethod]
		public void UndoRedo_ExceptionDuringRestore_StateConsistency()
		{
			var testObj = new AdvancedTestClass();
			var undoRedo = new UndoRedo(testObj, "ExceptionProperty", 10);

			var validObject = new ExceptionProneClass { Data = "Valid" };
			testObj.ExceptionProperty = validObject;
			undoRedo.Commit();

			var invalidObject = new ExceptionProneClass { Data = "Invalid" };
			testObj.ExceptionProperty = invalidObject;
			undoRedo.Commit();

			// Simulate exception during restore
			try
			{
				undoRedo.Undo();
			}
			catch (InvalidOperationException)
			{
				// Expected exception
			}

			// State phải remain consistent sau exception
			Assert.IsNotNull(testObj.ExceptionProperty);
		}

		#endregion

		#region Advanced Test Helper Classes

		private class AdvancedTestClass
		{
			public Delegate DelegateProperty { get; set; }
			public Stream StreamProperty { get; set; }
			public object CustomSerializedProperty { get; set; }
			public object ConcurrentProperty { get; set; }
			public object DeadlockProperty { get; set; }
			public object StressProperty { get; set; }
			public object LargeObjectProperty { get; set; }
			public object DynamicAssemblyProperty { get; set; }
			public object GenericProperty { get; set; }
			public object InterfaceProperty { get; set; }
			public object ExceptionProperty { get; set; }
			public object CorruptedProperty { get; set; }
		}

		private class ComplexSerializableClass
		{
			public Dictionary<string, ComplexSerializableClass> DictionaryData { get; set; }
			public ComplexSerializableClass[] ArrayData { get; set; }
			public int Number { get; set; }
		}

		private class CustomDisposable : IDisposable
		{
			public void Dispose()
			{
				// Implementation
			}
		}

		private class ExceptionProneClass
		{
			public string Data { get; set; }

			// Constructor có thể throw exception dựa trên data
			public ExceptionProneClass()
			{
				if (Data == "Invalid")
					throw new InvalidOperationException("Simulated exception during creation");
			}
		}

		// Sử dụng internal class HistoryItem từ UndoRedo (cần access internal)
		private class HistoryItem
		{
			public bool IsNull { get; set; }
			public string Name { get; set; }
			public byte[] Data { get; set; }

			public static HistoryItem NullItem => new HistoryItem { IsNull = true };
		}

		#endregion
	}
}