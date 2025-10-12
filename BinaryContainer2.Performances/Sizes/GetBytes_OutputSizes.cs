using BinaryContainer.Performances.Converter.DataConvertClass;

namespace BinaryContainer.Performances.Sizes
{
	internal class GetBytes_OutputSizes
	{
	
		private static string RowFornat = "|{0,-25}|{1,-15}|{2,-10}|{3,9}|   {4,1}   |{5,-15}|{6,-10}|{7,9}";
		private static string SeparatorFormat = RowFornat.Replace(' ', '-');

		private GetBytesBenchmark benchmark = new();

		public void Run()
		{
			benchmark.GlobalSetup();
			var simpleObject = new SizeItem("SimpleObject", benchmark.GetBytes_SimpleObject, benchmark.JsonSerializer_SimpleObject);
			var simpleList = new SizeItem("ComplexObject", benchmark.GetBytes_ComplexObject, benchmark.JsonSerializer_ComplexObject);
			var complexObject = new SizeItem("SimpleList", benchmark.GetBytes_SimpleList, benchmark.JsonSerializer_SimpleList);
			var complexist = new SizeItem("ComplexList", benchmark.GetBytes_ComplexList, benchmark.JsonSerializer_ComplexList);
			var dictionary = new SizeItem("Dictionary", benchmark.GetBytes_Dictionary, benchmark.JsonSerializer_Dictionary);
			var nullItem = new SizeItem("Null", benchmark.GetBytes_Null, benchmark.JsonSerializer_Null);
			var largeString = new SizeItem("LargeString", benchmark.GetBytes_LargeString, benchmark.JsonSerializer_LargeString);
			var primitive = new SizeItem("Primitive", benchmark.GetBytes_PrimitiveTypes, benchmark.JsonSerializer_PrimitiveTypes);
			var primitiveArr = new SizeItem("PrimitiveArr", benchmark.GetBytes_ArrayOfPrimitives, benchmark.JsonSerializer_ArrayOfPrimitives);

			Console.WriteLine(RowFornat, "Object", "bin converter", "bytes", "percent", ">", "json converter", "bytes", "percent");
			Console.WriteLine(SeparatorFormat,
				new string('-', 25),
				new string('-', 15),
				new string('-', 10),
				new string('-', 9),
				new string('-', 1),
				new string('-', 15),
				new string('-', 10),
				new string('-', 9));

			simpleObject.Display();
			simpleList.Display();
			complexObject.Display();
			complexist.Display();
			dictionary.Display();
			nullItem.Display();
			largeString.Display();
			primitive.Display();
			primitiveArr.Display();
		}

		internal class SizeItem()
		{
			private string name;
			private Func<byte[]?> binConverter;
			private Func<byte[]?> jsonConveter;

			public SizeItem(string name, Func<byte[]?> binConverter, Func<byte[]?> jsonConveter) : this()
			{
				this.name = name;
				this.binConverter = binConverter;
				this.jsonConveter = jsonConveter;
			}

			public void Display()
			{
				var bindata = binConverter();
				var jsondata = jsonConveter();

				var binlength = bindata?.Length ?? 0;
				var jsonlength = jsondata?.Length ?? 0;
				if (binlength > jsonlength)
				{
					var binPercent = "";
					var jsonPercent = jsonlength * 100f / binlength;
					Console.WriteLine(RowFornat, name, "bindata", binlength, binPercent, ">", "jsondata(*)", jsonlength, jsonPercent);
				}
				else
				{
					var binPercent = binlength * 100f / jsonlength;
					var jsonPercent = "";
					Console.WriteLine(RowFornat, name, "bindata(*)", binlength, binPercent, "<", "jsondata", jsonlength, jsonPercent);
				}
			}
		}
	}
}
