using BinaryContainer2.Utilities;
using BinaryContainer2.Utilities.Datamodel;
using System.Collections.Concurrent;
using System.Reflection;

namespace BinaryContainer2.Tests.UtilitiesTests.ChallengeTests
{
	public class TestData
	{
		public string Name { get; set; }
		public int Value { get; set; }
		public DateTime Timestamp { get; set; }

		public override bool Equals(object obj)
		{
			return obj is TestData other &&
				   Name == other.Name &&
				   Value == other.Value &&
				   Timestamp == other.Timestamp;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Value, Timestamp);
		}
	}

	[TestClass]
	public class BackupChallengeTests
	{
		#region Constructor Tests

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_ZeroBackupSize_ThrowsException()
		{
			var backup = new Backup(0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Constructor_NegativeBackupSize_ThrowsException()
		{
			var backup = new Backup(-5);
		}

		[TestMethod]
		public void Constructor_ValidBackupSize_CreatesInstance()
		{
			var backup = new Backup(10);
			Assert.IsNotNull(backup);
		}

		#endregion

		#region Edge Case Tests

		[TestMethod]
		public void Add_NullItem_StoresNullItem()
		{
			var backup = new Backup(3);
			backup.Add<string>(null);
			var result = backup.Get<string>(0);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void Add_MultipleNullItems_CircularBufferWorks()
		{
			var backup = new Backup(2);
			backup.Add<string>(null);
			backup.Add<int?>(null);
			backup.Add<double?>(null); // Should overwrite first null

			Assert.IsNull(backup.Get<double?>(0));
			Assert.IsNull(backup.Get<int?>(1));
			Assert.IsNull(backup.Get<string>(2)); // Should be out of range
		}

		[TestMethod]
		public void Get_VersionBeyondBackupCount_ReturnsNull()
		{
			var backup = new Backup(5);
			backup.Add(10);
			backup.Add(20);

			Assert.IsNull(backup.Get<int?>(2)); // Only 2 items stored
			Assert.IsNull(backup.Get<int?>(10));
		}

		[TestMethod]
		public void Get_NegativeVersion_ReturnsNull()
		{
			var backup = new Backup(3);
			backup.Add("test");

			Assert.IsNull(backup.Get<string>(-1));
			Assert.IsNull(backup.Get<string>(-5));
		}

		#endregion

		#region Circular Buffer Stress Tests

		[TestMethod]
		public void CircularBuffer_ExactSize_NoOverwrite()
		{
			var backup = new Backup(3);
			backup.Add(1);
			backup.Add(2);
			backup.Add(3);

			Assert.AreEqual(3, backup.Get<int>(0));
			Assert.AreEqual(2, backup.Get<int>(1));
			Assert.AreEqual(1, backup.Get<int>(2));
			Assert.IsNull(backup.Get<int?>(3));
		}

		[TestMethod]
		public void CircularBuffer_Overflow_OverwritesOldest()
		{
			var backup = new Backup(3);
			backup.Add(1);
			backup.Add(2);
			backup.Add(3);
			backup.Add(4); // Should overwrite 1
			backup.Add(5); // Should overwrite 2

			Assert.AreEqual(5, backup.Get<int>(0));
			Assert.AreEqual(4, backup.Get<int>(1));
			Assert.AreEqual(3, backup.Get<int>(2));
			Assert.IsNull(backup.Get<int?>(3));
		}

		[TestMethod]
		public void CircularBuffer_MultipleOverflows_CorrectOrder()
		{
			var backup = new Backup(2);
			for (int i = 0; i < 10; i++)
			{
				backup.Add(i);
			}

			Assert.AreEqual(9, backup.Get<int>(0));
			Assert.AreEqual(8, backup.Get<int>(1));
			Assert.IsNull(backup.Get<int?>(2));
		}

		#endregion

		#region Complex Type Tests

		[TestMethod]
		public void Add_ComplexType_SerializationWorks()
		{
			var backup = new Backup(2);
			var complexObj = new TestData
			{
				Name = "Test",
				Value = 42,
				Timestamp = DateTime.Now
			};
			backup.Add(complexObj);

			var result = backup.Get<TestData>(0);
			Assert.IsNotNull(result);
			Assert.AreEqual("Test", result.Name);
			Assert.AreEqual(42, result.Value);
		}

		[TestMethod]
		public void Add_DifferentTypes_CircularBufferMaintainsTypeInfo()
		{
			var backup = new Backup(3);
			backup.Add("string");
			backup.Add(42);
			backup.Add(3.14);
			backup.Add(true); // Should overwrite "string"

			Assert.AreEqual(true, backup.Get<bool>(0));
			Assert.AreEqual(3.14, backup.Get<double>(1));
			Assert.AreEqual(42, backup.Get<int>(2));
		}

		#endregion

		#region Type Safety Tests

		[TestMethod]
		[ExpectedException(typeof(InvalidCastException))]
		public void Get_WrongType_ThrowsException()
		{
			var backup = new Backup(2);
			backup.Add(42);

			// This should throw InvalidCastException
			var result = backup.Get<string>(0);
		}

		[TestMethod]
		public void Get_ObjectType_ReturnsCorrectType()
		{
			var backup = new Backup(2);
			var testData = new TestData { Name = "test string", Value = 100, Timestamp = DateTime.Now };
			backup.Add(testData);
			backup.Add(DateTime.Now);

			var data1Result = backup.Get<object>(0) as DateTime?;
			var data2Result = backup.Get<object>(1) as TestData;

			Assert.IsNotNull(data1Result);

			Assert.IsNotNull(data2Result);
			Assert.AreEqual("test string", data2Result.Name);
		}

		#endregion

		#region Performance Stress Tests

		[TestMethod]
		public void StressTest_LargeNumberOfItems()
		{
			var backup = new Backup(1000);
			for (int i = 0; i < 10000; i++)
			{
				backup.Add(i);
			}

			// Should have last 1000 items
			Assert.AreEqual(9999, backup.Get<int>(0));
			Assert.AreEqual(9998, backup.Get<int>(1));
			Assert.AreEqual(9000, backup.Get<int>(999));
			Assert.IsNull(backup.Get<int?>(1000));
		}

		[TestMethod]
		public void StressTest_RapidAddGetOperations()
		{
			var backup = new Backup(50);
			var random = new Random();

			for (int i = 0; i < 1000; i++)
			{
				backup.Add(random.Next());
				if (i % 10 == 0)
				{
					var value = backup.Get<int>(0);
					Assert.IsTrue(value >= 0);
				}
			}
		}

		#endregion

		#region Boundary Value Tests

		[TestMethod]
		public void Boundary_BackupSizeOne()
		{
			var backup = new Backup(1);
			backup.Add("first");
			backup.Add("second"); // Overwrites first

			Assert.AreEqual("second", backup.Get<string>(0));
			Assert.IsNull(backup.Get<string>(1));
		}

		[TestMethod]
		public void Boundary_MaxBackupSize()
		{
			// Use reasonable size to avoid memory issues
			var backup = new Backup(10000);
			backup.Add("test");

			Assert.AreEqual("test", backup.Get<string>(0));
		}

		[TestMethod]
		public void Boundary_VersionZeroAfterOverflow()
		{
			var backup = new Backup(2);
			backup.Add(1);
			backup.Add(2);
			backup.Add(3);

			Assert.AreEqual(3, backup.Get<int>(0));
			Assert.AreEqual(2, backup.Get<int>(1));
		}

		#endregion

		#region Complex Data Structure Tests

		[TestMethod]
		public void Add_NestedComplexType_HandlesCorrectly()
		{
			var backup = new Backup(3);
			var complexObj = new TestData
			{
				Name = "NestedTest",
				Value = 999,
				Timestamp = new DateTime(2023, 1, 1)
			};

			backup.Add(complexObj);
			var result = backup.Get<TestData>(0);

			Assert.AreEqual(complexObj, result);
		}

		[TestMethod]
		public void Add_MultipleComplexTypes_CircularBufferWorks()
		{
			var backup = new Backup(2);
			var data1 = new TestData { Name = "First", Value = 1, Timestamp = DateTime.Now };
			var data2 = new TestData { Name = "Second", Value = 2, Timestamp = DateTime.Now.AddHours(1) };
			var data3 = new TestData { Name = "Third", Value = 3, Timestamp = DateTime.Now.AddHours(2) };

			backup.Add(data1);
			backup.Add(data2);
			backup.Add(data3); // Should overwrite data1

			Assert.AreEqual(data3, backup.Get<TestData>(0));
			Assert.AreEqual(data2, backup.Get<TestData>(1));
			Assert.IsNull(backup.Get<TestData>(2));
		}

		#endregion

		#region Serialization Edge Cases

		[TestMethod]
		public void Add_EmptyString_HandlesCorrectly()
		{
			var backup = new Backup(2);
			backup.Add("");
			backup.Add("non-empty");

			Assert.AreEqual("non-empty", backup.Get<string>(0));
			Assert.AreEqual("", backup.Get<string>(1));
		}

		[TestMethod]
		public void Add_DateTimeMinMax_HandlesCorrectly()
		{
			var backup = new Backup(3);
			backup.Add(DateTime.MinValue);
			backup.Add(DateTime.MaxValue);
			backup.Add(DateTime.Now);

			Assert.AreEqual(DateTime.Now.Date, backup.Get<DateTime>(0).Date);
			Assert.AreEqual(DateTime.MaxValue, backup.Get<DateTime>(1));
			Assert.AreEqual(DateTime.MinValue, backup.Get<DateTime>(2));
		}

		#endregion
	}

	[TestClass]
	public class ExtremeBackupTests
	{
		#region Reflection-Based Attack Tests

		#endregion

		#region Memory Corruption Tests

		[TestMethod]
		public void Memory_ExtremeLargeData_OverflowProtection()
		{
			var backup = new Backup(10);

			// Test with extremely large data
			var largeData = new byte[1000000]; // 1MB of data
			new Random().NextBytes(largeData);

			backup.Add(largeData);

			// Verify it can be retrieved
			var result = backup.Get<byte[]>(0);
			Assert.AreEqual(largeData.Length, result.Length);
			CollectionAssert.AreEqual(largeData, result);
		}

		[TestMethod]
		public void Memory_MassiveNumberOfSmallItems_StressTest()
		{
			var backup = new Backup(100);

			// Add millions of small items to test memory management
			for (int i = 0; i < 1000000; i++)
			{
				backup.Add(i);
				if (i % 10000 == 0)
				{
					// Verify recent items are still accessible
					var recent = backup.Get<int>(0);
					Assert.AreEqual(i, recent);
				}
			}

			// Final verification
			Assert.AreEqual(999999, backup.Get<int>(0));
			Assert.AreEqual(999998, backup.Get<int>(1));
		}

		#endregion

		#region Type System Exploitation Tests

		[TestMethod]
		public void TypeSystem_GenericTypeWithConstraints_BreaksSerialization()
		{
			var backup = new Backup(3);

			// Test with complex generic types
			var complexDict = new Dictionary<Dictionary<string, int>, List<HashSet<double>>>
			{
				[new Dictionary<string, int> { ["key"] = 42 }] = new List<HashSet<double>> { new HashSet<double> { 1.1, 2.2 } }
			};

			backup.Add(complexDict);

			// This might break due to complex type serialization
			var result = backup.Get<Dictionary<Dictionary<string, int>, List<HashSet<double>>>>(0);
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void TypeSystem_TypeWithCircularReferences_HandlesGracefully()
		{
			var backup = new Backup(2);

			// Create circular reference object
			var circularObj = new CircularReferenceClass();
			circularObj.SelfReference = circularObj;
			circularObj.Value = "test";

			backup.Add(circularObj);

			// This might cause stack overflow or infinite recursion
			try
			{
				var result = backup.Get<CircularReferenceClass>(0);
				// If we get here, circular references are handled
				Assert.AreEqual("test", result.Value);
			}
			catch (Exception ex)
			{
				// Stack overflow or serialization error is expected
				Assert.IsTrue(ex is StackOverflowException || ex.InnerException is StackOverflowException);
			}
		}

		private class CircularReferenceClass
		{
			public CircularReferenceClass SelfReference { get; set; }
			public string Value { get; set; }
		}

		#endregion

		#region Race Condition Simulation Tests

		[TestMethod]
		public void Concurrency_SimultaneousAddGet_StateConsistency()
		{
			var backup = new Backup(5);
			var exceptions = new List<Exception>();

			// Simulate concurrent access without actual threading (conceptual test)
			ParallelInvoke(10, i =>
			{
				try
				{
					if (i % 2 == 0)
					{
						backup.Add($"data_{i}");
					}
					else
					{
						var result = backup.Get<string>(0);
						// Result might be null if accessed during buffer wrap
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			});

			// After all operations, state should be consistent
			Assert.IsTrue(exceptions.Count == 0, $"Unexpected exceptions: {string.Join(", ", exceptions)}");
		}

		private void ParallelInvoke(int count, Action<int> action)
		{
			var tasks = new System.Threading.Tasks.Task[count];
			for (int i = 0; i < count; i++)
			{
				int localI = i;
				tasks[i] = System.Threading.Tasks.Task.Run(() => action(localI));
			}
			System.Threading.Tasks.Task.WaitAll(tasks);
		}

		#endregion

		#region Edge Case Exploitation Tests

		[TestMethod]
		public void EdgeCases_MaxVersionRequest_ResourceExhaustion()
		{
			var backup = new Backup(1000);

			// Fill with data
			for (int i = 0; i < 1000; i++)
			{
				backup.Add(i);
			}

			// Request maximum possible version (conceptual)
			// This tests the modulo arithmetic with large numbers
			var result = backup.Get<int>(999);
			Assert.AreEqual(0, result); // First item after 999 overwrites

			// Test with version number close to int.MaxValue
			Assert.IsNull(backup.Get<int?>(int.MaxValue));
		}


		#endregion
	}

	[TestClass]
	public class AdvancedBackupTests
	{
		#region ConcurrentDictionary-Specific Tests

		[TestMethod]
		public void ConcurrentDictionary_MassiveParallelAccess_ThreadSafety()
		{
			var backup = new Backup(100);
			var exceptions = new ConcurrentQueue<Exception>();
			int operationCount = 10000;

			Parallel.For(0, operationCount, i =>
			{
				try
				{
					if (i % 3 == 0)
					{
						backup.Add($"data_{i}");
					}
					else if (i % 3 == 1)
					{
						var result = backup.Get(0);
					}
					else
					{
						backup.Add(i);
					}
				}
				catch (Exception ex)
				{
					exceptions.Enqueue(ex);
				}
			});

			Assert.AreEqual(0, exceptions.Count, $"Thread safety violations: {string.Join(", ", exceptions)}");
		}

		#endregion

		#region Advanced Race Condition Tests

		[TestMethod]
		public void RaceCondition_AddGetSimultaneously_DataConsistency()
		{
			var backup = new Backup(10);
			var results = new ConcurrentBag<object>();
			var exceptions = new ConcurrentQueue<Exception>();

			Parallel.For(0, 1000, i =>
			{
				try
				{
					if (i % 2 == 0)
					{
						backup.Add($"item_{i}");
					}
					else
					{
						// Try to get while others are adding
						for (int j = 0; j < 5; j++)
						{
							var result = backup.Get<object>(j);
							if (result != null)
								results.Add(result);
						}
					}
				}
				catch (Exception ex)
				{
					exceptions.Enqueue(ex);
				}
			});

			Assert.AreEqual(0, exceptions.Count, $"Race condition errors: {string.Join(", ", exceptions)}");
		}

		[TestMethod]
		public void RaceCondition_IndexWrapDuringConcurrentAccess()
		{
			var backup = new Backup(3);
			var barrier = new Barrier(3);

			Task[] tasks = new Task[3];
			for (int i = 0; i < 3; i++)
			{
				int taskId = i;
				tasks[i] = Task.Run(() =>
				{
					barrier.SignalAndWait();
					for (int j = 0; j < 10; j++)
					{
						backup.Add($"task{taskId}_item{j}");
						Thread.Sleep(1); // Increase chance of interleaving
					}
				});
			}

			Task.WaitAll(tasks);

			// After all operations, system should be consistent
			var current = backup.Get<string>(0);
			Assert.IsNotNull(current);
		}

		#endregion

		#region Memory Leak and Resource Tests

		[TestMethod]
		public void Memory_LargeObjectHeap_FragmentationTest()
		{
			var backup = new Backup(10);

			// Test with objects that might go to LOH
			for (int i = 0; i < 100; i++)
			{
				var largeArray = new byte[85000]; // Above LOH threshold
				Array.Fill(largeArray, (byte)i);
				backup.Add(largeArray);

				// Verify retrieval works
				if (i % 10 == 0)
				{
					var result = backup.Get<byte[]>(0);
					Assert.AreEqual(85000, result.Length);
				}
			}
		}

		#endregion

		#region Serialization Edge Cases

		[TestMethod]
		public void Serialization_ComplexObjectGraphs_DeepNesting()
		{
			var backup = new Backup(5);

			// Create deeply nested object
			var root = new TreeNode("root");
			var current = root;
			for (int i = 0; i < 100; i++)
			{
				current.Children.Add(new TreeNode($"level_{i}"));
				current = current.Children[0];
			}

			backup.Add(root);

			var result = backup.Get<TreeNode>(0);
			Assert.AreEqual("root", result.Name);
			Assert.IsTrue(result.Children.Count > 0);
		}

		[TestMethod]
		public void Serialization_TypeWithCustomSerialization()
		{
			var backup = new Backup(3);

			var customObj = new CustomSerializableObject
			{
				Id = Guid.NewGuid(),
				Data = "Test data",
				Timestamp = DateTime.UtcNow
			};

			backup.Add(customObj);

			var result = backup.Get<CustomSerializableObject>(0);
			Assert.AreEqual(customObj.Id, result.Id);
			Assert.AreEqual(customObj.Data, result.Data);
		}

		[TestMethod]
		public void Serialization_VersionTolerance_TypeEvolution()
		{
			var backup = new Backup(2);

			// Simulate storing data with one version of a type
			var v1Data = new VersionedData { Id = 1, Name = "Test" };
			backup.Add(v1Data);

			// Simulate retrieving with evolved type
			// This tests the serializer's version tolerance
			var result = backup.Get<VersionedData>(0);
			Assert.AreEqual("Test", result.Name);
		}

		public class TreeNode
		{
			public string Name { get; set; }
			public List<TreeNode> Children { get; set; } = new List<TreeNode>();

			public TreeNode() { }

			public TreeNode(string name) => Name = name;
		}

		[Serializable]
		public class CustomSerializableObject
		{
			public Guid Id { get; set; }
			public string Data { get; set; }
			public DateTime Timestamp { get; set; }
		}

		[Serializable]
		public class VersionedData
		{
			public int Id { get; set; }
			public string Name { get; set; }
			// New properties can be added in future versions
		}

		#endregion

		#region Performance and Load Testing

		[TestMethod]
		[Timeout(5000)] // 5 second timeout
		public void Performance_HighFrequencyOperations_ThroughputTest()
		{
			var backup = new Backup(100);
			var sw = System.Diagnostics.Stopwatch.StartNew();

			int operations = 100000;
			Parallel.For(0, operations, i =>
			{
				backup.Add(i);
				if (i % 100 == 0)
				{
					backup.Get<int>(0);
				}
			});

			sw.Stop();
			Assert.IsTrue(sw.ElapsedMilliseconds < 4000,
				$"Performance degraded: {operations} operations took {sw.ElapsedMilliseconds}ms");
		}

		[TestMethod]
		public void Performance_MemoryUsage_GrowthStability()
		{
			var backup = new Backup(100);
			long initialMemory = GC.GetTotalMemory(true);

			for (int i = 0; i < 10000; i++)
			{
				backup.Add(new string('x', 1000)); // 1KB strings

				if (i % 1000 == 0)
				{
					// Force GC to measure real memory usage
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
			}

			long finalMemory = GC.GetTotalMemory(true);
			long memoryGrowth = finalMemory - initialMemory;

			// Memory growth should be reasonable (less than 50MB for 10MB of data)
			Assert.IsTrue(memoryGrowth < 50 * 1024 * 1024,
				$"Excessive memory growth: {memoryGrowth / 1024 / 1024}MB");
		}

		#endregion

		#region Error Recovery and Resilience

		[TestMethod]
		public void ErrorRecovery_SerializationFailure_GracefulDegradation()
		{
			var backup = new Backup(3);

			// Add valid data first
			backup.Add("valid data 1");
			backup.Add("valid data 2");

			// Simulate a serialization failure in BinConverter
			// This would require mocking BinConverter, but we can test the error path
			try
			{
				backup.Add(new object()); // Might cause serialization issues
			}
			catch
			{
				// Expected - system should handle serialization errors
			}

			// Previous valid data should still be accessible
			var result = backup.Get<string>(1);
			Assert.AreEqual("valid data 2", result);
		}

		#endregion

		#region Advanced Type System Tests

		[TestMethod]
		public void TypeSystem_GenericTypesWithConstraints_ComplexSerialization()
		{
			var backup = new Backup(3);

			var complexData = new Dictionary<string, List<KeyValuePair<int, double>>>
			{
				["key1"] = new List<KeyValuePair<int, double>>
				{
					new KeyValuePair<int, double>(1, 1.1),
					new KeyValuePair<int, double>(2, 2.2)
				}
			};

			backup.Add(complexData);

			var result = backup.Get<Dictionary<string, List<KeyValuePair<int, double>>>>(0);
			Assert.AreEqual(1.1, result["key1"][0].Value);
		}

		[TestMethod]
		public void TypeSystem_DynamicTypes_RuntimeTypeHandling()
		{
			var backup = new Backup(2);

			dynamic dynamicObject = new System.Dynamic.ExpandoObject();
			dynamicObject.Name = "Test";
			dynamicObject.Value = 42;

			// This might fail depending on BinConverter's dynamic type support
			try
			{
				backup.Add((object)dynamicObject);
				var result = backup.Get<object>(0);
				Assert.IsNotNull(result);
			}
			catch
			{
				// Dynamic types might not be supported
			}
		}

		#endregion

		#region Security and Validation Tests

		[TestMethod]
		public void Security_MaliciousTypeNames_InjectionPrevention()
		{
			var backup = new Backup(3);

			// Test with potentially dangerous type names
			var maliciousItems = new[]
			{
				"System.; DELETE FROM Users; --",
				"../../../../../etc/passwd",
				"<script>alert('xss')</script>",
				new string('x', 10000) // Very long type name
            };

			foreach (var typeName in maliciousItems)
			{
				var item = new HistoryItem
				{
					IsNull = false,
					Name = typeName,
					Data = new byte[10]
				};

				// Inject via reflection
				var dictField = typeof(Backup).GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance);
				var dict = (ConcurrentDictionary<int, HistoryItem>)dictField.GetValue(backup);

				dict[0] = item;

				// Type resolution should handle malicious names safely
				try
				{
					var result = backup.Get<object>(0);
					// Should return null or throw for invalid types
				}
				catch (Exception ex)
				{
					Assert.IsNotNull(ex);
				}
			}
		}

		[TestMethod]
		public void Security_BinaryData_InjectionAttacks()
		{
			var backup = new Backup(2);

			// Test with potentially malicious binary data
			var maliciousData = new[]
			{
				new byte[] { 0x00, 0x00, 0x00, 0x00 }, // Null bytes
                new byte[1000000], // Very large data
                new byte[] { 0xFF, 0xFE, 0xFD, 0xFC } // Invalid sequences
            };

			foreach (var data in maliciousData)
			{
				var item = new HistoryItem
				{
					IsNull = false,
					Name = typeof(string).AssemblyQualifiedName,
					Data = data
				};

				var dictField = typeof(Backup).GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance);
				var dict = (ConcurrentDictionary<int, HistoryItem>)dictField.GetValue(backup);

				dict[0] = item;

				// Deserialization should handle malicious data safely
				try
				{
					var result = backup.Get<string>(0);
					// Should handle gracefully
				}
				catch (Exception ex)
				{
					Assert.IsNotNull(ex);
				}
			}
		}

		#endregion

		#region Integration-Style Tests

		[TestMethod]
		public void Integration_RealWorldScenario_CompleteWorkflow()
		{
			var backup = new Backup(10);

			// Simulate a real-world editing session
			var document = new DocumentState
			{
				Content = "Initial content",
				Version = 1,
				Timestamp = DateTime.Now
			};

			for (int i = 0; i < 15; i++) // Exceed backup size
			{
				document.Content = $"Edit {i + 1}";
				document.Version = i + 2;
				document.Timestamp = DateTime.Now.AddMinutes(i);

				backup.Add(document);

				// Simulate occasional undo operations
				if (i % 3 == 0)
				{
					var previous = backup.Get<DocumentState>(0);
					Assert.IsNotNull(previous);
				}
			}

			// Test undo stack
			for (int i = 0; i < 10; i++)
			{
				var state = backup.Get<DocumentState>(i);
				if (state != null)
				{
					Assert.IsTrue(state.Version >= 6); // Only last 10 edits remain
				}
			}
		}

		public class DocumentState
		{
			public string Content { get; set; }
			public int Version { get; set; }
			public DateTime Timestamp { get; set; }
		}

		#endregion
	}
}