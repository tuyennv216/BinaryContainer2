namespace BinaryContainer2.Tests.ChallengeTests
{
	using BinaryContainer2.Converter;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	[TestClass]
	public class SpecialTests
	{
		/// <summary>
		/// TEST CASE 1: Đánh bại xử lý Tham chiếu Vòng tròn (Circular Reference).
		/// Đây là lỗi kinh điển gây StackOverflow hoặc tạo bytes không giới hạn.
		/// </summary>
		[TestMethod]
		public void T01_CircularReference_ShouldFailOrHandleCorrectly()
		{
			// 1. Setup Input: Tạo vòng tròn
			var nodeA = new Node { Name = "A" };
			var nodeB = new Node { Name = "B" };
			nodeA.Child = nodeB;
			nodeB.Parent = nodeA; // Vòng tròn ở đây

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(nodeA);

			// 2. Lấy item
			var item = BinConverter.GetItem<Node>(bytes);

			// 3. So sánh: Phải đảm bảo tham chiếu được khôi phục CHÍNH XÁC.
			// Tức là item.Child.Parent PHẢI là cùng tham chiếu item.
			Assert.AreSame(item, item.Child!.Parent); // KIỂM TRA THAM CHIẾU!
		}

		/// <summary>
		/// TEST CASE 2: Đánh bại xử lý Kế thừa và Đa hình.
		/// Thư viện phải lưu trữ kiểu dữ liệu thực tế (Dog), không phải kiểu cơ sở (Animal).
		/// </summary>
		[TestMethod]
		public void T02_Polymorphism_ShouldRestoreDerivedType()
		{
			// 1. Setup Input: Gán đối tượng Dog vào thuộc tính Animal
			var input = new Zoo
			{
				Pet = new Dog { Name = "Buddy", Age = 5, Breed = "Golden Retriever" }
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Zoo>(bytes);

			// 3. So sánh
			Assert.IsNotNull(item.Pet);
			// THỬ THÁCH: Kiểm tra kiểu dữ liệu thực tế!
			Assert.IsInstanceOfType(item.Pet, typeof(Dog), "Pet phải được deserialize về kiểu Dog.");

			var restoredDog = (Dog)item.Pet;
			Assert.AreEqual("Golden Retriever", restoredDog.Breed, "Dữ liệu riêng của lớp con (Breed) bị mất.");
		}

		/// <summary>
		/// TEST CASE 3: Đánh bại xử lý Null, List rỗng, và Số thập phân chính xác.
		/// Kiểm tra sự khác biệt giữa 'null' và 'rỗng' (count=0).
		/// </summary>
		[TestMethod]
		public void T03_NullAndEmpty_And_PrecisionData_ShouldBeAccurate()
		{
			// 1. Setup Input: Giá trị góc
			var preciseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var input = new DataHolder
			{
				NullList = null,
				NullString = null,
				NullableInt = null,
				EmptyList = new List<int>(), // List rỗng (không null)
				UtcTime = preciseTime, // Ngày giờ với Kind=Utc
				PreciseAmount = 9876543210.123456789m, // Số thập phân chính xác
				UniqueId = Guid.NewGuid()
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<DataHolder>(bytes);

			// 3. So sánh: Kiểm tra các giá trị góc
			Assert.IsNull(item.NullList, "NullList phải là NULL.");
			Assert.IsNull(item.NullString, "NullString phải là NULL.");
			Assert.IsNull(item.NullableInt, "NullableInt phải là NULL.");

			Assert.IsNotNull(item.EmptyList, "EmptyList phải không NULL.");
			Assert.AreEqual(0, item.EmptyList.Count, "EmptyList phải có Count = 0.");

			Assert.AreEqual(input.PreciseAmount, item.PreciseAmount, "Số Decimal bị mất độ chính xác.");

			// THỬ THÁCH: Kiểm tra DateKind
			Assert.AreEqual(DateTimeKind.Utc, item.UtcTime.Kind, "DateTime.Kind bị thay đổi (không còn là Utc).");
			Assert.AreEqual(input.UtcTime, item.UtcTime, "Thời gian bị sai lệch.");
		}

		/// <summary>
		/// TEST CASE 5: Đánh bại bằng cấu trúc lồng ghép SÂU.
		/// Kiểm tra giới hạn đệ quy và bộ nhớ.
		/// </summary>
		[TestMethod]
		public void T05_DeeplyNestedStructure_ShouldRestoreAllLevels()
		{
			const int depth = 500; // Độ sâu lớn để kiểm tra giới hạn Stack/Bộ nhớ

			// Setup: Tạo 500 Node lồng vào nhau
			var head = new Node { Name = "Head" };
			var current = head;
			for (int i = 1; i <= depth; i++)
			{
				var newNode = new Node { Name = $"Node_{i}" };
				current.Child = newNode;
				current = newNode;
			}

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(head);

			// 2. Lấy item
			var item = BinConverter.GetItem<Node>(bytes);

			// 3. So sánh: Kiểm tra độ sâu và dữ liệu ở mức sâu nhất
			var restored = item;
			for (int i = 1; i <= depth; i++)
			{
				Assert.IsNotNull(restored.Child, $"Mất node ở cấp độ {i}.");
				Assert.AreEqual($"Node_{i}", restored.Child!.Name, $"Sai tên node ở cấp độ {i}.");
				restored = restored.Child;
			}

			Assert.IsNull(restored.Child, "Vượt quá độ sâu dự kiến.");
		}

		/// <summary>
		/// TEST CASE 6: Đánh bại xử lý Dictionary với Key là một Class (Non-primitive Key).
		/// Thư viện phải serialize/deserialize cả Key và Value một cách chính xác.
		/// </summary>
		[TestMethod]
		public void T06_DictionaryWithComplexKey_ShouldBeRestored()
		{
			// 1. Setup Input
			var input = new MapData();
			var coord1 = new Coordinate { X = 10, Y = 20 };
			var coord2 = new Coordinate { X = 30, Y = 40 };

			input.Locations.Add(coord1, "Cửa hàng A");
			input.Locations.Add(coord2, "Trường học B");

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<MapData>(bytes);

			// 3. So sánh
			Assert.AreEqual(input.Locations.Count, item.Locations.Count, "Số lượng item trong Dictionary bị sai.");

			// THỬ THÁCH: Kiểm tra xem các Key phức tạp có được khôi phục chính xác (X=10, Y=20)
			var restoredCoord1 = item.Locations.Keys.FirstOrDefault(c => c.X == 10 && c.Y == 20);

			Assert.IsNotNull(restoredCoord1, "Key phức tạp không được khôi phục.");
			Assert.AreEqual("Cửa hàng A", item.Locations[restoredCoord1], "Value của Key bị sai.");
		}

		/// <summary>
		/// TEST CASE 8: Đánh bại Serialization/Deserialization qua Interface.
		/// Thư viện phải lưu trữ kiểu thực tế (Book) và khôi phục nó, bao gồm cả thuộc tính của lớp con.
		/// </summary>
		[TestMethod]
		public void T08_SerializationViaInterface_ShouldRestoreActualClass()
		{
			// 1. Setup Input
			var input = new Inventory
			{
				ItemA = new Book { Name = "Clean Code", Price = 500, Author = "Uncle Bob" }
			};
			input.Items.Add(new Book { Name = "Design Patterns", Price = 700, Author = "Gamma et al." });

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Inventory>(bytes);

			// 3. So sánh
			Assert.IsInstanceOfType(item.ItemA, typeof(Book), "ItemA phải được khôi phục thành kiểu Book.");

			var restoredBook = (Book)item.ItemA;
			Assert.AreEqual("Uncle Bob", restoredBook.Author, "Thuộc tính lớp con (Author) bị mất khi serialize qua Interface.");

			// THỬ THÁCH: Kiểm tra List<Interface>
			Assert.AreEqual(1, item.Items.Count);
			Assert.IsInstanceOfType(item.Items[0], typeof(Book), "Item trong List<IItem> phải được khôi phục thành kiểu Book.");
			Assert.AreEqual("Gamma et al.", ((Book)item.Items[0]).Author, "Dữ liệu trong List Interface bị mất.");
		}

		/// <summary>
		/// TEST CASE 9: Đánh bại bằng việc kết hợp List lồng List và Giá trị Default.
		/// Kiểm tra khả năng xử lý các List rỗng và không rỗng trong cấu trúc phức tạp.
		/// </summary>
		[TestMethod]
		public void T09_NestedCollectionsAndDefaultValues_ShouldBeAccurate()
		{
			// Setup: List lồng List, và một thuộc tính không được set (default value)
			var input = new List<List<int>>
		{
			new List<int> { 1, 2, 3 },        // List có data
            new List<int> { },                // List rỗng
            null!                             // Null list (Nếu thư viện xử lý null trong collection)
        };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<List<int>>>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Count);

			// THỬ THÁCH 1: List đầu tiên phải có dữ liệu
			Assert.IsNotNull(item[0]);
			Assert.AreEqual(3, item[0].Count);
			Assert.AreEqual(2, item[0][1]);

			// THỬ THÁCH 2: List thứ hai phải là đối tượng rỗng
			Assert.IsNotNull(item[1]);
			Assert.AreEqual(0, item[1].Count, "List rỗng bị khôi phục sai (có thể thành null).");

			// THỬ THÁCH 3: List thứ ba phải là NULL (Nếu thư viện hỗ trợ null trong collection)
			Assert.IsNull(item[2], "List null không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 10 (Viết lại): Đánh bại xử lý các Kiểu Dữ liệu .NET hiếm/khó, Tuple, và HashSet.
		/// Kiểm tra array rỗng, TimeSpan chính xác, chuỗi Unicode 4-byte, và HashSet.
		/// </summary>
		[TestMethod]
		public void T10_EdgeCaseDotNetTypes_ShouldBeAccurate()
		{
			// 1. Setup Input: Sử dụng class EdgeCaseData
			var input = new EdgeCaseData();
			input.UniqueItems.Add("Cherry");

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item: Sử dụng kiểu EdgeCaseData rõ ràng
			var item = BinConverter.GetItem<EdgeCaseData>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: byte[] rỗng
			Assert.IsNotNull(item.EmptyBytes, "byte[] rỗng không được khôi phục.");
			Assert.AreEqual(0, item.EmptyBytes.Length, "byte[] rỗng bị sai kích thước.");

			// THỬ THÁCH 2: TimeSpan (Kiểm tra độ chính xác microsecond)
			Assert.AreEqual(input.Duration, item.Duration, "TimeSpan bị mất độ chính xác.");

			// THỬ THÁCH 3: Tuple (Kiểm tra serialization/deserialization của Tuple)
			Assert.AreEqual(input.DataTuple.Item1, item.DataTuple.Item1, "Giá trị Tuple Item1 bị sai.");
			Assert.AreEqual(input.DataTuple.Item2, item.DataTuple.Item2, "Giá trị Tuple Item2 bị sai.");

			// THỬ THÁCH 4: Chuỗi Unicode 4-byte (Phải bảo toàn ký tự 💩)
			Assert.AreEqual(input.SpecialString, item.SpecialString, "Chuỗi Unicode 4 byte (Surrogate Pair) bị sai hoặc mất ký tự.");

			// THỬ THÁCH 5: HashSet (Kiểm tra sự khác biệt giữa List và HashSet - không có thứ tự, không có trùng lặp)
			Assert.IsInstanceOfType(item.UniqueItems, typeof(HashSet<string>), "UniqueItems không được khôi phục thành HashSet.");
			Assert.AreEqual(3, item.UniqueItems.Count, "HashSet bị sai số lượng phần tử.");
			Assert.IsTrue(item.UniqueItems.Contains("Banana"), "Phần tử HashSet bị thiếu.");
		}

		/// <summary>
		/// TEST CASE 13: Đánh bại Xử lý Tham chiếu Trùng lặp (Duplicate Reference Handling).
		/// Nếu thư viện serialize 2 lần (thay vì ghi nhớ tham chiếu), nó sẽ tạo ra 2 đối tượng khác nhau khi deserialize.
		/// </summary>
		[TestMethod]
		public void T13_DuplicateObjectReference_ShouldMaintainReferenceIdentity()
		{
			// 1. Setup Input: RefA và RefB trỏ đến CÙNG một đối tượng trong bộ nhớ
			var shared = new SharedItem { Value = 500 };
			var input = new ReferenceHolder
			{
				RefA = shared,
				RefB = shared // Cùng tham chiếu!
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ReferenceHolder>(bytes);

			// 3. So sánh

			// THỬ THÁCH QUAN TRỌNG: Kiểm tra xem RefA và RefB có phải là CÙNG tham chiếu đối tượng KHÔNG.
			// Nếu thư viện serialize hai lần, item.RefA sẽ KHÔNG phải là item.RefB.
			Assert.AreSame(item.RefA, item.RefB, "Tham chiếu đối tượng trùng lặp không được bảo toàn.");

			// Kiểm tra thay đổi: Nếu thay đổi RefA, RefB cũng phải thay đổi (chứng minh cùng tham chiếu)
			item.RefA!.Value = 999;
			Assert.AreEqual(999, item.RefB!.Value, "Kiểm tra giá trị để xác nhận trùng tham chiếu.");
		}

		/// <summary>
		/// TEST CASE 14: Đánh bại Tham chiếu Trùng lặp trong Collection.
		/// Kiểm tra khi một đối tượng được thêm nhiều lần vào một Collection.
		/// </summary>
		[TestMethod]
		public void T14_CollectionWithRepeatedReference_ShouldMaintainReferenceIdentity()
		{
			// 1. Setup Input
			var shared = new SharedItem { Value = 10 };
			var input = new ReferenceHolder
			{
				RefA = shared // Tham chiếu 1
			};
			input.Items.Add(shared); // Tham chiếu 2
			input.Items.Add(shared); // Tham chiếu 3

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ReferenceHolder>(bytes);

			// 3. So sánh

			// THỬ THÁCH: Cả ba vị trí (RefA, Items[0], Items[1]) phải cùng tham chiếu đến một đối tượng.
			Assert.AreSame(item.RefA, item.Items[0], "Tham chiếu giữa thuộc tính và Collection bị mất.");
			Assert.AreSame(item.Items[0], item.Items[1], "Tham chiếu trùng lặp trong Collection bị mất.");

			// Kiểm tra tính chất tham chiếu: Thay đổi 1, tất cả 3 phải thay đổi
			item.RefA!.Value = 777;
			Assert.AreEqual(777, item.Items[1].Value, "Thay đổi không đồng bộ sau Deserialization.");
		}

		/// <summary>
		/// TEST CASE 16: Đánh bại Bằng Dữ liệu NULL Lồng ghép và Kế thừa.
		/// Kết hợp thử thách đa hình và xử lý null sâu.
		/// </summary>
		[TestMethod]
		public void T16_PolymorphismWithNullNested_ShouldBeCorrect()
		{
			// Giả sử Dog và Animal đã được định nghĩa

			// 1. Setup Input: Animal là Dog, nhưng Dog có một thuộc tính Nullable (ví dụ: ParentDog)
			var input = new Zoo // Zoo có thuộc tính Pet là Animal
			{
				Pet = new Dog
				{
					Name = "ChildDog",
					Age = 2,
					Breed = "Poodle",
					// Giả định thêm thuộc tính:
					// ParentDog = null 
				}
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Zoo>(bytes);

			// 3. So sánh

			// THỬ THÁCH: Đảm bảo kiểu vẫn là Dog
			Assert.IsInstanceOfType(item.Pet, typeof(Dog), "Pet bị khôi phục về kiểu cơ sở (Animal).");

			var restoredDog = (Dog)item.Pet;
			Assert.AreEqual("Poodle", restoredDog.Breed, "Dữ liệu riêng của lớp con bị mất.");

			// Giả định: Nếu Dog có thuộc tính 'string? FavoriteToy = null;'
			// Assert.IsNull(restoredDog.FavoriteToy, "Thuộc tính null của lớp con không được khôi phục là null.");
		}

		/// <summary>
		/// TEST CASE 18: Đánh bại Đa hình trong Collection.
		/// THỬ THÁCH: Thư viện phải serialize TỪNG item trong List<Document> theo kiểu thực tế của nó (Report), không phải kiểu cơ sở (Document).
		/// </summary>
		[TestMethod]
		public void T18_PolymorphismInCollection_ShouldRestoreDerivedTypes()
		{
			// 1. Setup Input
			var input = new Library();
			input.Items.Add(new Document { Title = "Base Doc" });
			input.Items.Add(new Report { Title = "Annual Report", ReportDate = new DateTime(2025, 1, 1) });
			input.Items.Add(null!); // Thử thách null trong collection đa hình

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Library>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Items.Count, "Số lượng item bị sai.");

			// THỬ THÁCH 1: Kiểm tra phần tử đầu tiên (kiểu Document)
			Assert.IsInstanceOfType(item.Items[0], typeof(Document), "Item 0 phải là Document.");
			Assert.AreEqual("Base Doc", item.Items[0].Title);

			// THỬ THÁCH 2: Kiểm tra phần tử thứ hai (phải là kiểu Report)
			Assert.IsInstanceOfType(item.Items[1], typeof(Report), "Item 1 bị khôi phục sai (mất đa hình, thành Document).");
			var restoredReport = (Report)item.Items[1];
			Assert.AreEqual("Annual Report", restoredReport.Title);
			Assert.AreEqual(2025, restoredReport.ReportDate.Year, "Dữ liệu riêng của lớp con bị mất.");

			// THỬ THÁCH 3: Kiểm tra phần tử thứ ba (null)
			Assert.IsNull(item.Items[2], "Null trong List đa hình không được khôi phục.");
		}

		/// <summary>
		/// TEST CASE 19: Đánh bại Dictionary với Value là Struct.
		/// THỬ THÁCH: Struct không có null, phải được khởi tạo đúng cách trong Dictionary, đặc biệt nếu Struct triển khai một Interface (không liên quan trực tiếp đến serialization nhưng gây nhiễu Reflection).
		/// </summary>
		[TestMethod]
		public void T19_DictionaryWithStructValue_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new Config();
			input.Settings.Add("Enter", new ConfigStruct { KeyCode = 13 });
			input.Settings.Add("Escape", new ConfigStruct { KeyCode = 27 });

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Config>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Settings.Count, "Số lượng item trong Dictionary bị sai.");

			// THỬ THÁCH: Kiểm tra Struct Value
			Assert.IsTrue(item.Settings.ContainsKey("Escape"), "Key bị mất.");
			Assert.AreEqual(13, item.Settings["Enter"].KeyCode, "Giá trị Struct bị sai.");
			Assert.AreEqual("Key 27", item.Settings["Escape"].DisplayValue, "Struct property bị sai.");
		}

		/// <summary>
		/// TEST CASE 20: Đánh bại Thuộc tính có kiểu Object (Serialization sâu).
		/// Đây là thử thách đa hình tối đa, vì kiểu khai báo là `object` nhưng kiểu thực tế là `Report`.
		/// </summary>
		[TestMethod]
		public void T20_ObjectPropertyPolymorphism_ShouldRestoreActualType()
		{
			// 1. Setup Input: Payload là object nhưng thực tế là Report
			var input = new DataWrapper
			{
				Payload = new Report { Title = "Secret Data", ReportDate = new DateTime(2026, 1, 1) }
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<DataWrapper>(bytes);

			// 3. So sánh
			Assert.IsNotNull(item.Payload, "Payload bị khôi phục thành null.");

			// THỬ THÁCH: Kiểm tra kiểu thực tế phải là Report
			Assert.IsInstanceOfType(item.Payload, typeof(Report), "Payload bị khôi phục sai (có thể là object hoặc kiểu sai).");

			var restoredReport = (Report)item.Payload;
			Assert.AreEqual("Secret Data", restoredReport.Title, "Dữ liệu cơ sở bị sai.");
			Assert.AreEqual(2026, restoredReport.ReportDate.Year, "Dữ liệu lớp con (ReportDate) bị mất.");
		}

		/// <summary>
		/// TEST CASE 21: Đánh bại với Kiểu Dữ liệu Nullable Struct lồng ghép sâu.
		/// Thử thách khả năng xử lý null cho Struct trong List.
		/// </summary>
		[TestMethod]
		public void T21_NullableStructInCollection_ShouldMaintainNullState()
		{
			// 1. Setup Input: List chứa các DateTime? (struct có thể null)
			var input = new List<DateTime?>
			{
				DateTime.UtcNow,        // Giá trị hợp lệ
				null,                   // Null
				new DateTime(2000, 1, 1) // Giá trị hợp lệ
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<DateTime?>>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Count);

			// THỬ THÁCH 1: Giá trị đầu tiên (Phải giống nhau)
			Assert.IsTrue(item[0].HasValue);
			Assert.AreEqual(input[0]!.Value.Year, item[0]!.Value.Year, "DateTime? value bị sai.");

			// THỬ THÁCH 2: Giá trị thứ hai (Phải là NULL)
			Assert.IsFalse(item[1].HasValue, "Nullable Struct không được khôi phục là NULL.");
			Assert.IsNull(item[1], "Giá trị thứ hai phải là null.");

			// THỬ THÁCH 3: Giá trị thứ ba
			Assert.IsTrue(item[2].HasValue);
			Assert.AreEqual(2000, item[2]!.Value.Year, "DateTime? value bị sai.");
		}

		/// <summary>
		/// TEST CASE 22: Đánh bại bằng String Lớn (Payload Size).
		/// Kiểm tra hiệu suất và giới hạn bộ nhớ/kích thước chuỗi byte, đặc biệt nếu serialization sử dụng kiểu int 32-bit để lưu trữ độ dài.
		/// </summary>
		[TestMethod]
		public void T22_LargeStringPayload_ShouldBeAccurate()
		{
			// 1. Setup Input: Tạo một chuỗi lớn (ví dụ: 1MB)
			const int sizeInKB = 1024; // 1MB
			var largeString = new string('A', sizeInKB * 1024);

			var input = new Dictionary<int, string>
		{
			{ 1, "Small" },
			{ 2, largeString },
			{ 3, "Another small" }
		};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Dictionary<int, string>>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Count);

			// THỬ THÁCH: Kiểm tra độ dài và nội dung của chuỗi lớn
			Assert.AreEqual(largeString.Length, item[2].Length, "Độ dài chuỗi lớn bị sai.");
			Assert.IsTrue(item[2].StartsWith("AAAAA"), "Nội dung chuỗi lớn bị hỏng.");
			Assert.IsTrue(item[2].EndsWith("AAAAA"), "Nội dung chuỗi lớn bị hỏng.");
		}

		/// <summary>
		/// TEST CASE 23: Đánh bại với các giá trị Zero/Default và các đối tượng có giá trị rỗng không phải null.
		/// THỬ THÁCH: Đảm bảo IPAddress.Any (0.0.0.0) và Array rỗng được khôi phục chính xác, không thành null.
		/// </summary>
		[TestMethod]
		public void T23_ZeroValuesAndEmptyArrays_ShouldNotBecomeNull()
		{
			// 1. Setup Input
			var input = new ZeroValueData();
			input.EmptyArray = new int[0]; // Mảng rỗng
			input.MaxCount = 100; // Giá trị DefaultValue

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ZeroValueData>(bytes);

			// 3. So sánh

			// THỬ THÁCH 2: Kiểm tra Array Rỗng
			Assert.IsNotNull(item.EmptyArray, "Array rỗng bị khôi phục thành NULL.");
			Assert.AreEqual(0, item.EmptyArray.Length, "Array rỗng bị khôi phục với kích thước sai.");

			// THỬ THÁCH 3: Kiểm tra giá trị default (100)
			Assert.AreEqual(100, item.MaxCount, "Giá trị 100 (DefaultValue) bị sai.");
		}

		/// <summary>
		/// TEST CASE 24: Đánh bại Kế thừa Sâu kết hợp Đa hình trong List.
		/// THỬ THÁCH: Serialization phải xử lý 3 cấp độ kế thừa (BaseMessage -> Command -> CommandGroup) và các đối tượng đa hình lồng ghép.
		/// </summary>
		[TestMethod]
		public void T24_DeepInheritanceWithPolymorphicNesting_ShouldRestoreAllLevels()
		{
			// 1. Setup Input
			var cmd1 = new Command { CommandName = "Run", MessageId = Guid.NewGuid() };
			var cmd2 = new Command { CommandName = "Stop", MessageId = Guid.NewGuid() };

			var input = new CommandGroup
			{
				CommandName = "MainGroup",
				MessageId = Guid.NewGuid(),
				// Thêm Command và đối tượng cùng kiểu (CommandGroup) vào List<Command>
				Commands = new List<Command> { cmd1, cmd2, new CommandGroup { CommandName = "SubGroup", MessageId = Guid.NewGuid() } }
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<CommandGroup>(bytes);

			// 3. So sánh
			Assert.AreEqual(input.MessageId, item.MessageId, "ID lớp cơ sở bị sai.");
			Assert.AreEqual("MainGroup", item.CommandName, "Thuộc tính lớp cha bị sai.");

			// THỬ THÁCH 1: Kiểm tra đa hình lồng ghép
			Assert.AreEqual(3, item.Commands.Count);

			// THỬ THÁCH 2: Kiểm tra phần tử là CommandGroup
			Assert.IsInstanceOfType(item.Commands[2], typeof(CommandGroup), "Đối tượng cùng kiểu bị khôi phục sai (mất đa hình).");
			var restoredGroup = (CommandGroup)item.Commands[2];
			Assert.AreEqual("SubGroup", restoredGroup.CommandName, "Dữ liệu lớp con lồng ghép bị mất.");
		}

		/// <summary>
		/// TEST CASE 25: Đánh bại Thuộc tính Tự Tham Chiếu (Self-Referencing Property).
		/// THỬ THÁCH: Đảm bảo thuộc tính `Self` được serialize thành một tham chiếu (NULL) chứ không gây vòng lặp.
		/// </summary>
		[TestMethod]
		public void T25_SelfReferencingProperty_ShouldBeNull()
		{
			// 1. Setup Input
			var input = new SelfRef
			{
				Data = "Root",
				Self = null // Thuộc tính trỏ đến chính nó, nhưng set là NULL
			};

			// Tình huống nguy hiểm hơn: Lấy chính nó làm tham chiếu (tạo vòng tròn đơn lẻ)
			input.Self = input;

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<SelfRef>(bytes);

			// 3. So sánh
			Assert.AreEqual("Root", item.Data);

			// THỬ THÁCH: Kiểm tra xem Self có trỏ về chính đối tượng cha (tham chiếu trùng lặp) không
			Assert.IsNotNull(item.Self, "Tham chiếu Self không được khôi phục.");
			Assert.AreSame(item, item.Self, "Thuộc tính Self không trỏ về chính đối tượng cha (mất tham chiếu trùng lặp).");
		}

		/// <summary>
		/// TEST CASE 26: Đánh bại bằng String Lớn Lồng ghép trong Dictionary (Boundary Check).
		/// Kiểm tra khi cả Key và Value đều là chuỗi lớn, kiểm tra giới hạn 2GB.
		/// </summary>
		[TestMethod]
		public void T26_LargeStringKeyAndValue_ShouldBeRestored()
		{
			// 1. Setup Input: Chuỗi lớn (ví dụ: 512KB)
			const int sizeInKB = 512;
			var largeString = new string('B', sizeInKB * 1024);
			var largeKey = new string('K', 10 * 1024); // Key 10KB

			var input = new Dictionary<string, string>
		{
			{ largeKey, largeString },
			{ "smallKey", "smallValue" }
		};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Dictionary<string, string>>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Count);

			// THỬ THÁCH 1: Kiểm tra Key lớn
			Assert.IsTrue(item.ContainsKey(largeKey), "Key chuỗi lớn bị mất.");

			// THỬ THÁCH 2: Kiểm tra Value lớn
			Assert.AreEqual(largeString.Length, item[largeKey].Length, "Độ dài Value chuỗi lớn bị sai.");
			Assert.IsTrue(item[largeKey].EndsWith("B"), "Nội dung Value chuỗi lớn bị hỏng.");
		}

		/// <summary>
		/// TEST CASE 27: Đánh bại bằng List<object?> chứa hỗn hợp các kiểu (Boxing và Null).
		/// Kiểm tra khả năng xử lý Boxing/Unboxing và null trong một List<object>.
		/// </summary>
		[TestMethod]
		public void T27_ListOfObjectWithMixedTypesAndNull_ShouldBeAccurate()
		{
			// 1. Setup Input: List<object> chứa Struct, Class và Null
			var input = new List<object?>
			{
				100,                        // int (Struct, bị Boxing)
				"String Value",             // string (Class)
				null,                       // null
				new ZeroValueData(),        // Class tùy chỉnh
				new DateTime(2023, 1, 1),   // DateTime (Struct, bị Boxing)
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<object?>>(bytes);

			// 3. So sánh
			Assert.AreEqual(5, item.Count);

			// THỬ THÁCH 1: Kiểm tra Int (Boxing/Unboxing)
			Assert.IsInstanceOfType(item[0], typeof(int), "Int không được Unbox đúng kiểu.");
			Assert.AreEqual(100, (int)item[0]!);

			// THỬ THÁCH 2: Kiểm tra Null
			Assert.IsNull(item[2], "Phần tử null trong List<object> không được khôi phục.");

			// THỬ THÁCH 3: Kiểm tra Class tùy chỉnh
			Assert.IsInstanceOfType(item[3], typeof(ZeroValueData), "Class tùy chỉnh bị sai kiểu.");

			// THỬ THÁCH 4: Kiểm tra DateTime (Boxing/Unboxing)
			Assert.IsInstanceOfType(item[4], typeof(DateTime), "DateTime không được Unbox đúng kiểu.");
			Assert.AreEqual(2023, ((DateTime)item[4]!).Year);
		}

		/// <summary>
		/// TEST CASE 29: Đánh bại bằng Mảng Lồng Ghép Sâu (3D Array).
		/// THỬ THÁCH: Kiểm tra khả năng xử lý kích thước mảng đa chiều và dữ liệu chính xác (Decimal) ở mọi vị trí.
		/// </summary>
		[TestMethod]
		public void T29_ThreeDimensionalArray_ShouldBeRestored()
		{
			// 1. Setup Input: Mảng 2x3x2 chứa Decimal
			var input = new DeepArray();
			var data = new Decimal[2, 3, 2];
			data[0, 1, 1] = 123.45m;
			data[1, 2, 0] = 987.65m;
			input.DataGrid = data;

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<DeepArray>(bytes);

			// 3. So sánh
			Assert.IsNotNull(item.DataGrid, "Mảng 3D bị khôi phục thành NULL.");
			Assert.AreEqual(3, item.DataGrid.Rank, "Rank của mảng 3D bị sai.");
			Assert.AreEqual(2, item.DataGrid.GetLength(0), "Kích thước chiều thứ nhất bị sai.");

			// THỬ THÁCH: Kiểm tra giá trị ngẫu nhiên trong mảng 3D
			Assert.AreEqual(123.45m, item.DataGrid[0, 1, 1], "Dữ liệu Decimal tại [0, 1, 1] bị sai.");
			Assert.AreEqual(987.65m, item.DataGrid[1, 2, 0], "Dữ liệu Decimal tại [1, 2, 0] bị sai.");
			Assert.AreEqual(0m, item.DataGrid[0, 0, 0], "Giá trị mặc định bị sai.");
		}

		/// <summary>
		/// TEST CASE 30: Đánh bại bằng Ký tự Control (Non-printable Characters) trong Chuỗi.
		/// THỬ THÁCH: Đảm bảo các ký tự điều khiển (như Null, Tab, Newline) được bảo toàn trong chuỗi.
		/// </summary>
		[TestMethod]
		public void T30_ControlCharactersInString_ShouldBePreserved()
		{
			// 1. Setup Input: Chuỗi chứa các ký tự control
			var input = new Dictionary<string, string>
		{
			{"Key", "Value 1\tValue 2\nValue 3\r\nValue 4" }, // Tab, Newline, Carriage Return
            {"Zero", "\0"} // Null Character
        };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Dictionary<string, string>>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra ký tự Tab, Newline, Carriage Return
			string expectedValue = "Value 1\tValue 2\nValue 3\r\nValue 4";
			Assert.AreEqual(expectedValue, item["Key"], "Ký tự điều khiển bị mất hoặc bị biến đổi.");

			// THỬ THÁCH 2: Kiểm tra ký tự Null ('\0')
			Assert.AreEqual("\0", item["Zero"], "Ký tự Null ('\\0') bị mất.");
			Assert.AreEqual(1, item["Zero"].Length, "Ký tự Null ('\\0') bị sai kích thước chuỗi.");
		}

		/// <summary>
		/// TEST CASE 31: Đánh bại DateTimeOffset và TimeSpan với giá trị góc.
		/// THỬ THÁCH: Đảm bảo cả DateTime và Offset đều được khôi phục chính xác, và TimeSpan lớn không mất độ chính xác.
		/// </summary>
		[TestMethod]
		public void T31_DateTimeOffsetAndLargeTimeSpan_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new TimeData();

			// DateTimeOffset: Thời điểm cụ thể với offset không phải 0
			input.StandardOffset = new DateTimeOffset(2050, 10, 20, 15, 30, 0, new TimeSpan(5, 30, 0));

			// TimeSpan: Giá trị rất lớn (ví dụ: 1000 ngày)
			input.LargeTimeSpan = TimeSpan.FromDays(1000) + TimeSpan.FromTicks(12345);

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<TimeData>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra DateTimeOffset
			Assert.AreEqual(input.StandardOffset.Year, item.StandardOffset.Year, "Năm bị sai.");
			// Kiểm tra Offset (Phải giữ nguyên +05:30:00)
			Assert.AreEqual(input.StandardOffset.Offset, item.StandardOffset.Offset, "Offset bị mất hoặc sai.");

			// THỬ THÁCH 2: Kiểm tra LargeTimeSpan (đến độ chính xác Ticks)
			Assert.AreEqual(input.LargeTimeSpan, item.LargeTimeSpan, "TimeSpan bị mất độ chính xác (Ticks).");
		}

		/// <summary>
		/// TEST CASE 32: Đánh bại Class Generic Nâng cao.
		/// THỬ THÁCH: Thư viện phải xử lý các kiểu generic phức tạp như List<GenericWrapper<Guid, DateTime?>>.
		/// </summary>
		[TestMethod]
		public void T32_DeeplyNestedGenerics_ShouldRestoreAllTypes()
		{
			// 1. Setup Input: List chứa GenericWrapper<Guid, DateTime?>
			var input = new List<GenericWrapper<Guid, DateTime?>>
			{
				new GenericWrapper<Guid, DateTime?>
				{
					Key = Guid.NewGuid(),
					Value = DateTime.UtcNow // DateTime? có giá trị
				},
				new GenericWrapper<Guid, DateTime?>
				{
					Key = Guid.NewGuid(),
					Value = null // DateTime? là null
				}
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<GenericWrapper<Guid, DateTime?>>>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Count);

			// THỬ THÁCH 1: Kiểm tra phần tử đầu tiên
			Assert.AreEqual(input[0].Key, item[0].Key, "Key Guid bị sai.");
			Assert.IsTrue(item[0].Value.HasValue, "Nullable DateTime bị mất giá trị.");

			// THỬ THÁCH 2: Kiểm tra phần tử thứ hai (Nullable là null)
			Assert.IsFalse(item[1].Value.HasValue, "Nullable DateTime không được khôi phục là null.");
			Assert.IsNull(item[1].Value, "Value bị sai.");
		}

		/// <summary>
		/// TEST CASE 33: Đánh bại Enum và Nullable Enum với giá trị 0 và giá trị lớn.
		/// THỬ THÁCH: Đảm bảo giá trị Enum = 0 và giá trị lớn (255) được khôi phục chính xác, và Enum có thể là null.
		/// </summary>
		[TestMethod]
		public void T33_EnumEdgeCases_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new EnumData();
			input.CurrentStatus = Status.Deleted; // Giá trị lớn (255)
			input.NullableStatus = Status.Pending; // Giá trị 0

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<EnumData>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra Enum với giá trị lớn (255)
			Assert.AreEqual(Status.Deleted, item.CurrentStatus, "Giá trị Enum lớn (255) bị sai.");

			// THỬ THÁCH 2: Kiểm tra Nullable Enum với giá trị 0
			Assert.IsTrue(item.NullableStatus.HasValue, "Nullable Enum bị mất giá trị.");
			Assert.AreEqual(Status.Pending, item.NullableStatus, "Giá trị Enum 0 bị sai.");

			// Setup và kiểm tra Nullable Enum là NULL
			var nullEnumInput = new EnumData { NullableStatus = null };
			byte[] nullEnumBytes = BinConverter.GetBytes(nullEnumInput);
			var nullEnumItem = BinConverter.GetItem<EnumData>(nullEnumBytes);
			Assert.IsNull(nullEnumItem.NullableStatus, "Nullable Enum NULL không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 34 (Sửa đổi): Đánh bại Array và Dictionary với các Chuỗi Dữ liệu Cực Lớn và List Rỗng.
		/// THỬ THÁCH: Kiểm tra khả năng xử lý áp lực bộ nhớ và Dictionary khi cả Key và Value đều là chuỗi lớn.
		/// </summary>
		[TestMethod]
		public void T34_LargeStringsInArrayAndDictionary_ShouldBeRestored()
		{
			// 1. Setup Input

			// Tạo chuỗi lớn (ví dụ: 100KB)
			const int stringSize = 100 * 1024;
			var largeStringA = new string('A', stringSize);
			var largeStringB = new string('B', stringSize);
			var largeKey = new string('K', 50 * 1024); // Key 50KB

			var input = new LargeDataContainer();

			// Array of Large Strings
			input.LargeStrings = new string[] { largeStringA, largeStringB, null! }; // Thử thách: Chuỗi lớn và NULL trong mảng

			// Dictionary of Large Strings
			input.LargeMap.Add(largeKey, largeStringB);
			input.LargeMap.Add("smallKey", largeStringA);

			// List Rỗng
			input.EmptyGuidList = new List<Guid>();

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<LargeDataContainer>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra Array of Large Strings
			Assert.AreEqual(3, item.LargeStrings.Length, "Kích thước mảng chuỗi bị sai.");
			Assert.AreEqual(stringSize, item.LargeStrings[0].Length, "Chuỗi lớn trong mảng bị mất kích thước.");
			Assert.IsNull(item.LargeStrings[2], "NULL trong mảng chuỗi không được khôi phục.");

			// THỬ THÁCH 2: Kiểm tra Dictionary of Large Strings
			Assert.AreEqual(2, item.LargeMap.Count, "Số lượng item trong Dictionary bị sai.");
			Assert.IsTrue(item.LargeMap.ContainsKey(largeKey), "Key chuỗi lớn bị mất.");
			Assert.AreEqual(stringSize, item.LargeMap[largeKey].Length, "Value chuỗi lớn bị mất kích thước.");

			// THỬ THÁCH 3: Kiểm tra List rỗng
			Assert.IsNotNull(item.EmptyGuidList, "List rỗng bị khôi phục thành NULL.");
			Assert.AreEqual(0, item.EmptyGuidList.Count, "List rỗng bị sai kích thước.");

			// Kiểm tra Equal tổng thể
			Assert.IsTrue(input.Equals(item), "Đối tượng không so sánh bằng sau Deserialization.");
		}

		/// <summary>
		/// TEST CASE 35: Đánh bại Mảng 2 chiều.
		/// THỬ THÁCH: Thư viện phải xử lý mảng 2D.
		/// </summary>
		[TestMethod]
		public void T35_StringBuilderAndTwoDimArray_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new CodeSnippet();

			// Mảng 2 chiều 3x4
			input.CharMatrix = new char[3, 4];
			input.CharMatrix[1, 2] = 'X';
			input.CharMatrix[2, 3] = 'Y';

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<CodeSnippet>(bytes);

			// 3. So sánh

			// THỬ THÁCH 2: Kiểm tra Mảng 2 chiều
			Assert.IsNotNull(item.CharMatrix, "Mảng 2D bị khôi phục thành NULL.");
			Assert.AreEqual(2, item.CharMatrix.Rank, "Rank của mảng 2D bị sai.");
			Assert.AreEqual(3, item.CharMatrix.GetLength(0), "Kích thước chiều 1 bị sai.");
			Assert.AreEqual('X', item.CharMatrix[1, 2], "Dữ liệu mảng 2D bị sai.");
			Assert.AreEqual(default(char), item.CharMatrix[0, 0], "Giá trị mặc định bị sai.");
		}

		/// <summary>
		/// TEST CASE 36: Đánh bại bằng Kiểu Object Array lồng ghép (Array of Object).
		/// THỬ THÁCH: Khả năng của thư viện trong việc khôi phục kiểu thực tế (polymorphism) cho TỪNG phần tử trong một mảng có kiểu khai báo là object[].
		/// </summary>
		[TestMethod]
		public void T36_ArrayOfObjectPolymorphism_ShouldRestoreActualTypes()
		{
			// 1. Setup Input
			var doc = new Document { Title = "Object Array Test" }; // Base class
			var rep = new Report { Title = "Final Report", ReportDate = DateTime.Now.Date }; // Derived class

			var input = new object?[]
			{
			100,            // Int (Struct, bị Boxing)
            doc,            // Class cơ sở
            rep,            // Class dẫn xuất
            null            // NULL
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<object?[]>(bytes);

			// 3. So sánh
			Assert.AreEqual(4, item.Length);

			// THỬ THÁCH 1: Kiểm tra Int (Boxing/Unboxing)
			Assert.IsInstanceOfType(item[0], typeof(int), "Phần tử mảng[0] không được khôi phục thành int.");
			Assert.AreEqual(100, (int)item[0]!);

			// THỬ THÁCH 2: Kiểm tra Class Dẫn xuất (Polymorphism)
			Assert.IsInstanceOfType(item[2], typeof(Report), "Phần tử mảng[2] bị khôi phục sai kiểu (mất đa hình).");
			var restoredReport = (Report)item[2]!;
			Assert.AreEqual(DateTime.Now.Date, restoredReport.ReportDate, "Dữ liệu lớp con bị mất.");

			// THỬ THÁCH 3: Kiểm tra NULL
			Assert.IsNull(item[3], "Phần tử null trong object[] không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 37: Đánh bại sự khác biệt giữa List và Array, đặc biệt là các cấu trúc rỗng lồng ghép.
		/// THỬ THÁCH: Đảm bảo List rỗng, Array rỗng, và sự kết hợp của chúng được khôi phục chính xác về kiểu List hay Array.
		/// </summary>
		[TestMethod]
		public void T37_ListVsArrayNesting_ShouldMaintainTypeAndEmptiness()
		{
			// 1. Setup Input
			var input = new ListArrayChallenge();

			// Thử thách 3: List chứa mảng (vừa rỗng, vừa có dữ liệu, vừa null)
			input.ListOfArrays.Add(new int[] { 10, 20 });
			input.ListOfArrays.Add(Array.Empty<int>()); // Mảng rỗng
			input.ListOfArrays.Add(null!); // Null mảng

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ListArrayChallenge>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: DeeplyEmptyList (List lồng List)
			Assert.IsNotNull(item.DeeplyEmptyList, "List ngoài bị null.");
			Assert.AreEqual(1, item.DeeplyEmptyList.Count, "Kích thước List ngoài bị sai.");
			Assert.AreEqual(0, item.DeeplyEmptyList[0].Count, "List trong bị khôi phục sai (phải là rỗng).");

			// THỬ THÁCH 2: DeeplyEmptyArray (Array lồng Array)
			Assert.IsNotNull(item.DeeplyEmptyArray, "Array ngoài bị null.");
			Assert.AreEqual(1, item.DeeplyEmptyArray.Length, "Kích thước Array ngoài bị sai.");
			Assert.AreEqual(0, item.DeeplyEmptyArray[0].Length, "Array trong bị khôi phục sai (phải là rỗng).");

			// THỬ THÁCH 3: ListOfArrays
			Assert.AreEqual(3, item.ListOfArrays.Count);

			// Kiểm tra phần tử [1] (Array rỗng)
			Assert.IsNotNull(item.ListOfArrays[1], "Mảng rỗng trong List bị khôi phục thành null.");
			Assert.AreEqual(0, item.ListOfArrays[1]!.Length, "Mảng rỗng bị sai kích thước.");

			// Kiểm tra phần tử [2] (Null mảng)
			Assert.IsNull(item.ListOfArrays[2], "NULL mảng trong List không được khôi phục là null.");
		}

		/// <summary>
		/// TEST CASE 39: Đánh bại bằng Dictionary<TKey, TValue> với TValue là một Array.
		/// THỬ THÁCH: Đảm bảo Dictionary có thể chứa các giá trị là mảng (byte[]).
		/// </summary>
		[TestMethod]
		public void T39_DictionaryWithValueAsArray_ShouldBeRestored()
		{
			// 1. Setup Input
			var input = new Dictionary<string, byte[]>
		{
			{"KeyA", new byte[] { 1, 2, 3 } },
			{"KeyB", new byte[] { } }, // Array rỗng
            {"KeyC", null! } // Null array
        };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<Dictionary<string, byte[]>>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Count);

			// THỬ THÁCH 1: Kiểm tra Array có dữ liệu
			Assert.IsTrue(input["KeyA"].SequenceEqual(item["KeyA"]), "Array có dữ liệu bị sai nội dung.");

			// THỬ THÁCH 2: Kiểm tra Array rỗng
			Assert.IsNotNull(item["KeyB"], "Array rỗng bị khôi phục thành null.");
			Assert.AreEqual(0, item["KeyB"].Length, "Array rỗng bị sai kích thước.");

			// THỬ THÁCH 3: Kiểm tra Null array
			Assert.IsNull(item["KeyC"], "Null array không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 40: Đánh bại với Mảng NULL và Mảng Kích thước Lớn.
		/// THỬ THÁCH: Đảm bảo mảng (kiểu tham chiếu) được khôi phục thành NULL thay vì mảng rỗng, và kiểm tra khả năng xử lý mảng lớn.
		/// </summary>
		[TestMethod]
		public void T40_NullArrayAndLargeByteArray_ShouldMaintainState()
		{
			// 1. Setup Input
			var input = new ArrayBoundary();
			input.NullArray = null;

			// Khởi tạo LargeByteArray với một vài giá trị đặc biệt ở đầu và cuối
			input.LargeByteArray[0] = 100;
			input.LargeByteArray[99999] = 200;

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ArrayBoundary>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra Null Array
			Assert.IsNull(item.NullArray, "Mảng NULL bị khôi phục thành một mảng rỗng (int[0]).");

			// THỬ THÁCH 2: Kiểm tra Large ByteArray
			Assert.IsNotNull(item.LargeByteArray, "Mảng lớn bị khôi phục thành NULL.");
			Assert.AreEqual(100000, item.LargeByteArray.Length, "Kích thước mảng lớn bị sai.");

			// Kiểm tra dữ liệu biên
			Assert.AreEqual(100, item.LargeByteArray[0], "Giá trị đầu mảng lớn bị sai.");
			Assert.AreEqual(200, item.LargeByteArray[99999], "Giá trị cuối mảng lớn bị sai.");
		}

		/// <summary>
		/// TEST CASE 41: Đánh bại với các Kiểu Boolean/Byte ở giới hạn.
		/// THỬ THÁCH: Đảm bảo giá trị FALSE (0) không bị nhầm lẫn với giá trị mặc định của serialization, và Byte 0/255 được bảo toàn.
		/// </summary>
		[TestMethod]
		public void T41_BooleanAndByteBoundaries_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new LogicPrimitives();
			input.IsEnabled = false; // Mặc định: FALSE
			input.MaxByte = 255;
			input.MinByte = 0;

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<LogicPrimitives>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra Boolean FALSE
			Assert.IsFalse(item.IsEnabled, "Boolean FALSE bị khôi phục thành TRUE (hoặc giá trị mặc định sai).");

			// THỬ THÁCH 2: Kiểm tra Byte Max (255)
			Assert.AreEqual((byte)255, item.MaxByte, "Giá trị Max Byte (255) bị sai.");

			// THỬ THÁCH 3: Kiểm tra Byte Min (0)
			Assert.AreEqual((byte)0, item.MinByte, "Giá trị Min Byte (0) bị sai.");
		}

		/// <summary>
		/// TEST CASE 43: Đánh bại với Struct Lồng ghép (Size) và Mảng Boolean.
		/// THỬ THÁCH: Đảm bảo Struct phức tạp (Size) và mảng Boolean được khôi phục chính xác, bao gồm cả giá trị NULL trong List.
		/// </summary>
		[TestMethod]
		public void T43_NestedStructAndBooleanArray_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new ItemContainer();

			// Thử thách 1: Struct có giá trị hợp lệ
			input.Points.Add(new ColorPoint
			{
				Dimensions = new Size(100, 200),
				Flags = new bool[] { true, false, true }
			});

			// Thử thách 2: Null Struct
			input.Points.Add(null);

			// Thử thách 3: Struct với giá trị mặc định/rỗng
			input.Points.Add(new ColorPoint { Flags = Array.Empty<bool>() });

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<ItemContainer>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Points.Count);

			// THỬ THÁCH 1: Kiểm tra Struct Lồng ghép (Size)
			Assert.IsTrue(item.Points[0].HasValue);
			Assert.AreEqual(100, item.Points[0]!.Value.Dimensions.Width, "Struct Size.Width bị sai.");
			Assert.IsTrue(item.Points[0]!.Value.Flags[0], "Boolean Array bị sai.");

			// THỬ THÁCH 2: Kiểm tra Null Struct
			Assert.IsFalse(item.Points[1].HasValue, "Nullable Struct không được khôi phục là NULL.");

			// THỬ THÁCH 3: Kiểm tra Struct rỗng
			Assert.IsTrue(item.Points[2].HasValue);
			Assert.AreEqual(0, item.Points[2]!.Value.Flags.Length, "Mảng boolean rỗng bị sai.");
		}

		/// <summary>
		/// TEST CASE 44: Đánh bại bằng Chuỗi Byte Đặc biệt (FF và 00).
		/// THỬ THÁCH: Đảm bảo các giá trị 0xFF (thường là -1 trong byte) và 0x00 (Null Byte) được lưu trữ chính xác.
		/// </summary>
		[TestMethod]
		public void T44_ByteArrayEdgeValues_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new BinaryBlock();
			input.Data = new byte[] { 0x00, 0xFF, 0x01, 0xFF, 0x00 };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<BinaryBlock>(bytes);

			// 3. So sánh
			Assert.AreEqual(5, item.Data.Length, "Kích thước byte array bị sai.");

			// THỬ THÁCH: Kiểm tra giá trị ở các vị trí khác nhau
			Assert.AreEqual(0x00, item.Data[0], "Byte 0x00 bị sai.");
			Assert.AreEqual(0xFF, item.Data[1], "Byte 0xFF bị sai.");
			Assert.AreEqual(0xFF, item.Data[3], "Byte 0xFF bị sai.");
			Assert.IsTrue(input.Data.SequenceEqual(item.Data), "Nội dung byte array không khớp.");
		}

		/// <summary>
		/// TEST CASE 45: Đánh bại bằng List<List<T>> với T là Decimal (Lồng ghép và Độ chính xác).
		/// THỬ THÁCH: Kiểm tra khả năng xử lý lồng ghép List sâu và bảo toàn độ chính xác của Decimal.
		/// </summary>
		[TestMethod]
		public void T45_DeeplyNestedDecimalList_ShouldMaintainPrecision()
		{
			// 1. Setup Input
			var input = new List<List<decimal>>
		{
			new List<decimal> { 12345.678901234m, 0m, 99.99m },
			new List<decimal> { }, // List rỗng
            null! // Null List
        };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<List<decimal>>>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.Count, "Số lượng List ngoài bị sai.");

			// THỬ THÁCH 1: Kiểm tra List có dữ liệu (Độ chính xác Decimal)
			Assert.IsNotNull(item[0]);
			Assert.AreEqual(3, item[0].Count);
			Assert.AreEqual(12345.678901234m, item[0][0], "Độ chính xác Decimal bị mất.");

			// THỬ THÁCH 2: Kiểm tra List rỗng
			Assert.IsNotNull(item[1], "List rỗng bị khôi phục thành NULL.");
			Assert.AreEqual(0, item[1].Count, "List rỗng bị sai kích thước.");

			// THỬ THÁCH 3: Kiểm tra Null List
			Assert.IsNull(item[2], "List NULL không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 46: Đánh bại với SortedList (Collection có thứ tự).
		/// THỬ THÁCH: Thư viện phải khôi phục cấu trúc dữ liệu chính xác (SortedList, KHÔNG phải Dictionary), và các Key phải được sắp xếp đúng sau khi deserialize.
		/// </summary>
		[TestMethod]
		public void T46_SortedList_ShouldMaintainKeyOrder()
		{
			// 1. Setup Input: Key được thêm vào theo thứ tự không sắp xếp
			var input = new SortedCollectionData();
			input.RankedData.Add("Zeta", 5);
			input.RankedData.Add("Alpha", 1);
			input.RankedData.Add("Gamma", 3);

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<SortedCollectionData>(bytes);

			// 3. So sánh
			Assert.IsInstanceOfType(item.RankedData, typeof(SortedList<string, int>), "Cấu trúc bị khôi phục sai (không phải SortedList).");
			Assert.AreEqual(3, item.RankedData.Count);

			// THỬ THÁCH: Kiểm tra thứ tự Key (phải là Alpha, Gamma, Zeta)
			var keys = item.RankedData.Keys.ToList();
			Assert.AreEqual("Alpha", keys[0], "Thứ tự Key của SortedList bị sai.");
			Assert.AreEqual("Zeta", keys[2], "Thứ tự Key của SortedList bị sai.");
			Assert.AreEqual(5, item.RankedData["Zeta"], "Giá trị không khớp.");
		}

		/// <summary>
		/// TEST CASE 47 (Sửa đổi): Đánh bại Kiểu Dữ liệu Nullable Lồng ghép Sâu (Struct trong List).
		/// THỬ THÁCH: Đảm bảo khả năng xử lý NULL ở 3 cấp độ: List<T?>, Struct? và thuộc tính Struct bên trong (int?, Guid?).
		/// </summary>
		[TestMethod]
		public void T47_DeeplyNestedNullableStruct_ShouldMaintainNullStates()
		{
			// 1. Setup Input
			var input = new DeeplyNullableContainer();

			// Case 1: Struct hợp lệ, có giá trị đầy đủ
			input.NestedList.Add(new InnerData
			{
				Id = Guid.NewGuid(),
				Value = 100
			});

			// Case 2: Struct hợp lệ, nhưng các thuộc tính bên trong là NULL
			input.NestedList.Add(new InnerData
			{
				Id = null,
				Value = null
			});

			// Case 3: Phần tử List là NULL (InnerData? là NULL)
			input.NestedList.Add(null);

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<DeeplyNullableContainer>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.NestedList.Count);

			// THỬ THÁCH 1: Kiểm tra Case 1 (Giá trị đầy đủ)
			Assert.IsTrue(item.NestedList[0].HasValue, "Phần tử [0] bị mất giá trị.");
			Assert.IsTrue(item.NestedList[0]!.Value.Id.HasValue, "Guid? bị mất giá trị.");
			Assert.AreEqual(100, item.NestedList[0]!.Value.Value, "int? bị mất giá trị.");

			// THỬ THÁCH 2: Kiểm tra Case 2 (Thuộc tính bên trong là NULL)
			Assert.IsTrue(item.NestedList[1].HasValue, "Phần tử [1] bị mất giá trị.");
			Assert.IsFalse(item.NestedList[1]!.Value.Id.HasValue, "Guid? không được khôi phục là NULL.");
			Assert.IsFalse(item.NestedList[1]!.Value.Value.HasValue, "int? không được khôi phục là NULL.");

			// THỬ THÁCH 3: Kiểm tra Case 3 (Phần tử List là NULL)
			Assert.IsFalse(item.NestedList[2].HasValue, "InnerData? NULL không được khôi phục là NULL.");
			Assert.IsNull(item.NestedList[2], "InnerData? NULL không được khôi phục là NULL.");
		}

		/// <summary>
		/// TEST CASE 48: Đánh bại Tham chiếu Vòng tròn Gián tiếp qua Collection.
		/// THỬ THÁCH: Đảm bảo thư viện nhận ra đối tượng cha được thêm vào List<T> của chính nó, ngăn chặn StackOverflow và khôi phục tham chiếu.
		/// </summary>
		[TestMethod]
		public void T48_CircularReferenceViaList_ShouldMaintainReferenceIdentity()
		{
			// 1. Setup Input: Tạo vòng tròn gián tiếp
			var parent = new CollectionRef { Name = "Parent" };
			var child = new CollectionRef { Name = "Child" };

			parent.Children!.Add(child);
			parent.Children!.Add(parent); // Vòng tròn ở đây (Parent -> Children[1] -> Parent)

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(parent);

			// 2. Lấy item
			var item = BinConverter.GetItem<CollectionRef>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Children!.Count, "Số lượng phần tử trong List bị sai.");

			// THỬ THÁCH QUAN TRỌNG: Kiểm tra tham chiếu
			// item PHẢI là cùng tham chiếu với item.Children[1].
			Assert.AreSame(item, item.Children[1], "Tham chiếu vòng tròn qua List không được bảo toàn.");

			// Kiểm tra tính chất tham chiếu: Thay đổi tên, cả hai phải thay đổi
			item.Name = "New Name";
			Assert.AreEqual("New Name", item.Children[1].Name, "Thay đổi không đồng bộ sau Deserialization.");
		}

		/// <summary>
		/// TEST CASE 49: Đánh bại với Kiểu System.Type và List<object> chứa dữ liệu hỗn hợp.
		/// THỬ THÁCH: Serialization phải lưu trữ tên Type của chính nó (hoặc Assembly Qualified Name) và khôi phục nó, và xử lý Boxing/Unboxing trong List<object> phức tạp.
		/// </summary>
		[TestMethod]
		public void T49_SystemTypeAndMixedList_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new TypeReferenceData();

			input.MixedList.Add(42);                        // int
			input.MixedList.Add("Final Test");              // string
			input.MixedList.Add(new SimpleBase());          // Class
			input.MixedList.Add(null);                      // null

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<TypeReferenceData>(bytes);

			// 3. So sánh
			// THỬ THÁCH 2: Kiểm tra List<object> hỗn hợp
			Assert.AreEqual(4, item.MixedList.Count);

			Assert.IsInstanceOfType(item.MixedList[0], typeof(int), "Phần tử int bị sai kiểu.");
			Assert.AreEqual(42, (int)item.MixedList[0]!);

			Assert.IsInstanceOfType(item.MixedList[2], typeof(SimpleBase), "Phần tử Class tùy chỉnh bị sai kiểu.");
			Assert.IsNull(item.MixedList[3], "Phần tử NULL bị sai.");
		}

		/// <summary>
		/// TEST CASE 50: Đánh bại với Lớp Cơ sở KHÔNG có Thuộc tính Public.
		/// THỬ THÁCH: Nếu thư viện chỉ dựa vào việc tìm các thuộc tính có thể serialize, nó có thể gặp lỗi khi serialize một đối tượng rỗng (trừ các thuộc tính kế thừa từ Object).
		/// </summary>
		[TestMethod]
		public void T50_BaseClassWithNoPublicProperties_ShouldBeSerialized()
		{
			// 1. Setup Input
			var input = new EmptyBaseContainer();
			input.Items.Add(new SimpleBase());
			input.Items.Add(new SimpleBase());

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<EmptyBaseContainer>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Items.Count, "Số lượng item bị sai.");
			Assert.IsInstanceOfType(item.Items[0], typeof(SimpleBase), "Kiểu dữ liệu bị sai.");

			// THỬ THÁCH: Kiểm tra xem các đối tượng có được khôi phục thành công không.
			Assert.IsNotNull(item.Items[0], "Đối tượng SimpleBase bị khôi phục thành NULL.");
		}

		/// <summary>
		/// TEST CASE 51: Đánh bại bằng một cấu trúc rỗng sâu và không cần thiết.
		/// THỬ THÁCH: Kiểm tra khả năng xử lý việc List<List<T>> được khởi tạo nhưng hoàn toàn rỗng.
		/// </summary>
		[TestMethod]
		public void T51_DeeplyInitializedButEmptyList_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new List<List<string>>
		{
			new List<string>(), // List rỗng 1
            new List<string>(), // List rỗng 2
        };

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<List<List<string>>>(bytes);

			// 3. So sánh
			Assert.AreEqual(2, item.Count, "Số lượng List ngoài bị sai.");

			// THỬ THÁCH: Đảm bảo các List bên trong được khôi phục thành đối tượng List RỖNG, không phải NULL
			Assert.IsNotNull(item[0], "List lồng ghép bị khôi phục thành NULL.");
			Assert.AreEqual(0, item[0].Count, "List lồng ghép bị khôi phục với kích thước sai.");
			Assert.AreEqual(0, item[1].Count, "List lồng ghép bị khôi phục với kích thước sai.");

			// Kiểm tra kiểu: phải là List<string>
			Assert.IsInstanceOfType(item[0], typeof(List<string>), "Kiểu lồng ghép bị khôi phục sai.");
		}

		/// <summary>
		/// TEST CASE 52: Đánh bại KeyValuePair (Struct Generic) trong cả thuộc tính đơn và List.
		/// THỬ THÁCH: Struct Generic có cấu trúc phức tạp, phải được khôi phục chính xác mà không mất dữ liệu.
		/// </summary>
		[TestMethod]
		public void T52_KeyValuePairStructGeneric_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new KeyValuePairContainer();

			// Gán thuộc tính đơn
			input.Item = new KeyValuePair<string, int>("Version", 2);

			// Gán List
			var guid1 = Guid.NewGuid();
			input.ListItems.Add(new KeyValuePair<Guid, string>(guid1, "First"));
			input.ListItems.Add(new KeyValuePair<Guid, string>(Guid.Empty, "Zero Guid"));

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<KeyValuePairContainer>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra thuộc tính đơn KeyValuePair
			Assert.AreEqual("Version", item.Item.Key, "Key của KeyValuePair bị sai.");
			Assert.AreEqual(2, item.Item.Value, "Value của KeyValuePair bị sai.");

			// THỬ THÁCH 2: Kiểm tra KeyValuePair trong List
			Assert.AreEqual(2, item.ListItems.Count, "Số lượng ListItems bị sai.");
			Assert.AreEqual(Guid.Empty, item.ListItems[1].Key, "Zero Guid bị khôi phục sai.");
			Assert.AreEqual("First", item.ListItems[0].Value, "Value trong List bị sai.");
		}

		/// <summary>
		/// TEST CASE 53: Đánh bại Object Array chứa Kiểu Enum (Boxing/Unboxing Enum).
		/// THỬ THÁCH: Đảm bảo Enum (Struct) được Box và Unbox chính xác qua object[] mà không bị nhầm thành giá trị số nguyên.
		/// </summary>
		[TestMethod]
		public void T53_ObjectArrayWithBoxedEnums_ShouldRestoreType()
		{
			// Giả định Enum Status (Pending=0, Active=1, Deleted=255) từ T33

			// 1. Setup Input
			var input = new EnumArrayContainer();
			input.MixedEnums = new object[]
			{
				Status.Active,      // Enum
				(byte)255,          // Byte (có giá trị giống Enum.Deleted)
				Status.Deleted      // Enum
			};

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<EnumArrayContainer>(bytes);

			// 3. So sánh
			Assert.AreEqual(3, item.MixedEnums.Length);

			// THỬ THÁCH 1: Kiểm tra phần tử đầu tiên (Phải là Enum)
			Assert.IsInstanceOfType(item.MixedEnums[0], typeof(Status), "Boxed Enum bị khôi phục thành kiểu số nguyên.");
			Assert.AreEqual(Status.Active, (Status)item.MixedEnums[0]);

			// THỬ THÁCH 2: Kiểm tra phần tử thứ hai (Phải là Byte)
			Assert.IsInstanceOfType(item.MixedEnums[1], typeof(byte), "Boxed Byte bị khôi phục sai kiểu.");
			Assert.AreEqual((byte)255, item.MixedEnums[1]);

			// THỬ THÁCH 3: Kiểm tra phần tử thứ ba (Phải là Enum, giá trị lớn)
			Assert.AreEqual(Status.Deleted, (Status)item.MixedEnums[2]);
		}

		/// <summary>
		/// TEST CASE 54: Đánh bại Cấu trúc Rỗng Sâu và Giá trị Mặc định của Struct (Zero Value).
		/// THỬ THÁCH: Đảm bảo List<List<T>> được khôi phục thành đối tượng List RỖNG, không phải NULL. Và Tuple với các giá trị zero-value của Struct/Primitive.
		/// </summary>
		[TestMethod]
		public void T54_DeepEmptinessAndZeroValueTuple_ShouldBeAccurate()
		{
			// 1. Setup Input
			var input = new FinalDeepEmptiness();
			input.OuterList.Clear();
			input.OuterList.Add(new List<int>()); // List ngoài chứa List rỗng

			// Tuple mặc định (DateTime.MinValue, 0, false)
			input.DefaultTuple = Tuple.Create(0, DateTime.MinValue, false);

			// 1. Lấy bytes
			byte[] bytes = BinConverter.GetBytes(input);

			// 2. Lấy item
			var item = BinConverter.GetItem<FinalDeepEmptiness>(bytes);

			// 3. So sánh

			// THỬ THÁCH 1: Kiểm tra List lồng List rỗng
			Assert.IsNotNull(item.OuterList, "List ngoài bị NULL.");
			Assert.AreEqual(1, item.OuterList.Count, "Kích thước List ngoài bị sai.");
			Assert.IsNotNull(item.OuterList[0], "List lồng ghép bị khôi phục thành NULL.");
			Assert.AreEqual(0, item.OuterList[0].Count, "List lồng ghép bị sai kích thước.");

			// THỬ THÁCH 2: Kiểm tra Tuple với giá trị mặc định
			Assert.IsNotNull(item.DefaultTuple);
			Assert.AreEqual(0, item.DefaultTuple.Item1, "Giá trị int mặc định bị sai.");
			Assert.AreEqual(DateTime.MinValue, item.DefaultTuple.Item2, "Giá trị DateTime mặc định bị sai.");
			Assert.IsFalse(item.DefaultTuple.Item3, "Giá trị bool mặc định bị sai.");
		}
	}
}
