using BinaryContainer2.Abstraction;
using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayGenericTests
{
	// Lớp mẫu để kiểm tra serialization của phần tử
	public class MyTestClass2
	{
		public int ID { get; set; }
		public string Name { get; set; } = string.Empty;

		// Bằng cách ghi đè Equals, ta đảm bảo các test so sánh giá trị hoạt động chính xác
		public override bool Equals(object? obj)
		{
			if (obj is MyTestClass2 other)
			{
				return ID == other.ID && Name == other.Name;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(ID, Name);
		}
	}

	[TestClass]
	public class TypeArrayGeneric_ListClass_Tests
	{
		private ITypeOperator _typeArrayOperator;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo TypeArrayGeneric với kiểu List<MyTestClass2>
			_typeArrayOperator = new TypeArrayGeneric(typeof(List<MyTestClass2>));
			_typeArrayOperator.Build();
			// Chỉ giữ lại Random, RefPool sẽ được khởi tạo trong từng test case
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh List<MyTestClass2> ---
		private void AssertListEqual(List<MyTestClass2> expected, object actual, string message, bool checkIdentity = false)
		{
			Assert.IsNotNull(actual);
			Assert.IsInstanceOfType(actual, typeof(List<MyTestClass2>), "Kiểu đọc ra phải là List<MyTestClass2>.");

			List<MyTestClass2> actualList = (List<MyTestClass2>)actual;
			Assert.AreEqual(expected.Count, actualList.Count, $"Độ dài List không khớp: {message}");

			for (int i = 0; i < expected.Count; i++)
			{
				var expectedItem = expected[i];
				var actualItem = actualList[i];

				if (expectedItem == null)
				{
					Assert.IsNull(actualItem, $"Phần tử thứ {i} phải là NULL: {message}");
				}
				else
				{
					Assert.IsNotNull(actualItem, $"Phần tử thứ {i} không được là NULL: {message}");

					// Kiểm tra giá trị bên trong đối tượng
					Assert.AreEqual(expectedItem.ID, actualItem!.ID, $"ID phần tử {i} không khớp: {message}");
					Assert.AreEqual(expectedItem.Name, actualItem.Name, $"Name phần tử {i} không khớp: {message}");

					// Nếu yêu cầu kiểm tra định danh tham chiếu
					if (checkIdentity)
					{
						// Kiểm tra xem các đối tượng tham chiếu cùng một instance trong mảng ban đầu
						// có đọc ra cùng một instance trong mảng mới không.
						if (i > 0 && expectedItem == expected[i - 1])
						{
							// Sử dụng ReferenceEquals để kiểm tra tham chiếu
							Assert.IsTrue(ReferenceEquals(actualItem, actualList[i - 1]), $"Định danh tham chiếu tại {i} phải được bảo toàn: {message}");
						}
					}
				}
			}
		}

		// ---------------------------------------------
		// I. Các Test Case Cơ Bản (Kiểm tra Chức năng)
		// ---------------------------------------------

		/// <summary>
		/// Test case cho một List<MyTestClass2> điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalListClass_ShouldReturnCorrectValue()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			List<MyTestClass2> originalList = new List<MyTestClass2>
			{
				new MyTestClass2 { ID = 1, Name = "Alpha" },
				new MyTestClass2 { ID = 2, Name = "Beta" },
				new MyTestClass2 { ID = 3, Name = "Gamma" }
			};
			object originalData = originalList;
			var originalContainer = new DataContainer();

			// Ghi với writePool
			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// Đọc với readPool mới
			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertListEqual(originalList, readData!, "List<class> điển hình.");
		}

		// ---------------------------------------------
		// II. Trường hợp Biên & Đặc biệt (Edge & Special Cases)
		// ---------------------------------------------

		/// <summary>
		/// Trường hợp đặc biệt: List chứa các phần tử NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_ListWithNullElements_ShouldPreserveNulls()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			List<MyTestClass2?> originalList = new List<MyTestClass2?>
			{
				new MyTestClass2 { ID = 1, Name = "First" },
				null,
				new MyTestClass2 { ID = 2, Name = "Third" },
				null,
				null
			};
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertListEqual(originalList!, readData!, "List chứa các phần tử NULL.");
		}

		/// <summary>
		/// Trường hợp đặc biệt: List chứa các tham chiếu lặp lại đến cùng một đối tượng.
		/// Việc này kiểm tra tính đúng đắn của operator bên trong (cho MyTestClass2) sử dụng RefPool.
		/// </summary>
		[TestMethod]
		public void WriteRead_ListWithSameReferenceElements_ShouldPreserveIdentity()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			var sharedObject = new MyTestClass2 { ID = 99, Name = "Shared" };
			List<MyTestClass2> originalList = new List<MyTestClass2>
			{
				sharedObject, // Index 0
				new MyTestClass2 { ID = 10, Name = "Unique" }, // Index 1
				sharedObject // Index 2 -> Tham chiếu lặp lại
			};
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			List<MyTestClass2> readList = (List<MyTestClass2>)readData!;

			// 1. Kiểm tra giá trị
			AssertListEqual(originalList, readData!, "List chứa các tham chiếu lặp lại.", checkIdentity: true);

			// 2. Kiểm tra định danh (quan trọng nhất cho RefPool)
			// Kiểm tra xem readList[0] và readList[2] có phải là cùng một instance không.
			Assert.IsTrue(ReferenceEquals(readList[0], readList[2]), "RefPool phải bảo toàn định danh tham chiếu lặp lại.");
			// Kiểm tra readList[0] và readList[1] không phải là cùng một instance.
			Assert.IsFalse(ReferenceEquals(readList[0], readList[1]), "Các đối tượng khác nhau không được tham chiếu cùng một instance.");
		}

		/// <summary>
		/// Trường hợp biên: List là NULL.
		/// </summary>
		[TestMethod]
		public void WriteRead_NullList_ShouldReturnNull()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		/// <summary>
		/// Trường hợp biên: List Rỗng (Count = 0).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyList_ShouldReturnEmptyList()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			List<MyTestClass2> originalList = new List<MyTestClass2>();
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			Assert.IsNotNull(readData);
			Assert.IsInstanceOfType(readData, typeof(List<MyTestClass2>));
			Assert.AreEqual(0, ((List<MyTestClass2>)readData).Count, "List rỗng phải được đọc ra là List có độ dài 0.");
		}

		/// <summary>
		/// Test giới hạn (Stress Test): Write/Read một List lớn với các đối tượng ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomList_ShouldBeStable()
		{
			var writePool = new RefPool();
			var readPool = new RefPool();

			const int listLength = 1000;
			List<MyTestClass2> originalList = new List<MyTestClass2>(listLength);

			for (int i = 0; i < listLength; i++)
			{
				originalList.Add(new MyTestClass2
				{
					ID = _random.Next(1, 1000000),
					Name = "Object_" + i.ToString() + "_" + _random.NextDouble()
				});
			}
			object originalData = originalList;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, writePool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, readPool);

			AssertListEqual(originalList, readData!, $"List ngẫu nhiên lớn ({listLength} phần tử).");
		}
	}
}
