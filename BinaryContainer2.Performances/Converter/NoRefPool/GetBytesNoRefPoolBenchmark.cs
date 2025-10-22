using BenchmarkDotNet.Attributes;
using BinaryContainer.Performances.Models;
using BinaryContainer2.Converter;
using Bogus;
using System.Text.Json;

namespace BinaryContainer.Performances.Converter.NoRefPool;

[MemoryDiagnoser]
public class GetBytesNoRefPoolBenchmark
{
	private readonly Faker _faker;

	private BenchmarkItem<SimpleClass> _simpleObject_item;
	private BenchmarkItem<ComplexClass> _complexObject_item;
	private BenchmarkItem<List<SimpleClass>> _simpleList_item;
	private BenchmarkItem<List<ComplexClass>> _complexList_item;
	private BenchmarkItem<Dictionary<string, SimpleClass>> _dictionary_item;
	private BenchmarkItem<object?> _null_item;
	private BenchmarkItem<string> _string_item;
	private BenchmarkItem<PrimitiveTypes> _primitiveTypes_item;
	private BenchmarkItem<int[]> _primitiveTypeList_item;

	public GetBytesNoRefPoolBenchmark()
	{
		_faker = new Faker();
	}

	[GlobalSetup]
	public void GlobalSetup()
	{
		// Simple object with basic types
		var simpleObject = new SimpleClass
		{
			Id = _faker.Random.Int(1, 1000),
			Name = _faker.Person.FullName,
			CreatedDate = _faker.Date.Past(),
			IsActive = _faker.Random.Bool(),
			Price = _faker.Random.Decimal(1, 1000)
		};

		_simpleObject_item = new BenchmarkItem<SimpleClass>
		{
			Source = simpleObject,
			BinaryBytes = BinConverter.GetBytes(simpleObject, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(simpleObject),
		};

		// Complex object with nested objects
		var complexObject = new ComplexClass
		{
			Id = _faker.Random.Guid(),
			Name = _faker.Company.CompanyName(),
			Address = new Address
			{
				Street = _faker.Address.StreetAddress(),
				City = _faker.Address.City(),
				Country = _faker.Address.Country()
			},
			Tags = _faker.Random.WordsArray(5, 10),
			Metadata = new Dictionary<string, string>
			{
				["version"] = _faker.System.Version().ToString(),
				["timestamp"] = DateTime.UtcNow.Ticks.ToString()
			}
		};
		_complexObject_item = new BenchmarkItem<ComplexClass>
		{
			Source = complexObject,
			BinaryBytes = BinConverter.GetBytes(complexObject, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(complexObject),
		};

		// List of simple objects
		var simpleList = new List<SimpleClass>();
		for (int i = 0; i < 100; i++)
		{
			simpleList.Add(new SimpleClass
			{
				Id = _faker.Random.Int(1, 1000),
				Name = _faker.Person.FullName,
				CreatedDate = _faker.Date.Past(),
				IsActive = _faker.Random.Bool(),
				Price = _faker.Random.Decimal(1, 1000)
			});
		}
		_simpleList_item = new BenchmarkItem<List<SimpleClass>>
		{
			Source = simpleList,
			BinaryBytes = BinConverter.GetBytes(simpleList, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(simpleList),
		};

		// List of complex objects
		var complexList = new List<ComplexClass>();
		for (int i = 0; i < 50; i++)
		{
			complexList.Add(new ComplexClass
			{
				Id = _faker.Random.Guid(),
				Name = _faker.Company.CompanyName(),
				Address = new Address
				{
					Street = _faker.Address.StreetAddress(),
					City = _faker.Address.City(),
					Country = _faker.Address.Country()
				},
				Tags = _faker.Random.WordsArray(3, 8),
				Metadata = new Dictionary<string, string>
				{
					["index"] = i.ToString(),
					["created"] = DateTime.UtcNow.AddDays(-i).Ticks.ToString()
				}
			});
		}
		_complexList_item = new BenchmarkItem<List<ComplexClass>>
		{
			Source = complexList,
			BinaryBytes = BinConverter.GetBytes(complexList, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(complexList),
		};

		// Dictionary with string keys and object values
		var dictionary = new Dictionary<string, SimpleClass>();
		for (int i = 0; i < 20; i++)
		{
			dictionary[$"key_{i}"] = new SimpleClass
			{
				Id = _faker.Random.Int(1, 1000),
				Name = _faker.Person.FullName,
				CreatedDate = _faker.Date.Past(),
				IsActive = _faker.Random.Bool(),
				Price = _faker.Random.Decimal(1, 1000)
			};
		}
		_dictionary_item = new BenchmarkItem<Dictionary<string, SimpleClass>>
		{
			Source = dictionary,
			BinaryBytes = BinConverter.GetBytes(dictionary, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(dictionary),
		};

		object? nullVariable = null;
		_null_item = new BenchmarkItem<object?>
		{
			Source = nullVariable,
			BinaryBytes = BinConverter.GetBytes(_null_item, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(_null_item),
		};

		var largeString = _faker.Lorem.Paragraphs(10);
		_string_item = new BenchmarkItem<string>
		{
			Source = largeString,
			BinaryBytes = BinConverter.GetBytes(largeString, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(largeString),
		};

		var primitiveTypes = new PrimitiveTypes
		{
			IntValue = _faker.Random.Int(),
			LongValue = _faker.Random.Long(),
			DoubleValue = _faker.Random.Double(),
			FloatValue = _faker.Random.Float(),
			BoolValue = _faker.Random.Bool(),
			StringValue = _faker.Lorem.Sentence(),
			DateTimeValue = _faker.Date.Recent()
		};
		_primitiveTypes_item = new BenchmarkItem<PrimitiveTypes>
		{
			Source = primitiveTypes,
			BinaryBytes = BinConverter.GetBytes(primitiveTypes, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(primitiveTypes),
		};

		var arrayOfPrimitiveTypes = new int[1000];
		for (int i = 0; i < arrayOfPrimitiveTypes.Length; i++)
		{
			arrayOfPrimitiveTypes[i] = _faker.Random.Int();
		}
		_primitiveTypeList_item = new BenchmarkItem<int[]>
		{
			Source = arrayOfPrimitiveTypes,
			BinaryBytes = BinConverter.GetBytes(arrayOfPrimitiveTypes, false),
			JsonBytes = JsonSerializer.SerializeToUtf8Bytes(arrayOfPrimitiveTypes),
		};
	}

	[Benchmark]
	public byte[]? GetBytes_SimpleObject() => BinConverter.GetBytes(_simpleObject_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_SimpleObject() => JsonSerializer.SerializeToUtf8Bytes(_simpleObject_item.Source);

	[Benchmark]
	public byte[]? GetBytes_ComplexObject() => BinConverter.GetBytes(_complexObject_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_ComplexObject() => JsonSerializer.SerializeToUtf8Bytes(_complexObject_item.Source);

	[Benchmark]
	public byte[]? GetBytes_SimpleList() => BinConverter.GetBytes(_simpleList_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_SimpleList() => JsonSerializer.SerializeToUtf8Bytes(_simpleList_item.Source);

	[Benchmark]
	public byte[]? GetBytes_ComplexList() => BinConverter.GetBytes(_complexList_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_ComplexList() => JsonSerializer.SerializeToUtf8Bytes(_complexList_item.Source);

	[Benchmark]
	public byte[]? GetBytes_Dictionary() => BinConverter.GetBytes(_dictionary_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_Dictionary() => JsonSerializer.SerializeToUtf8Bytes(_dictionary_item.Source);

	[Benchmark]
	public byte[]? GetBytes_Null() => BinConverter.GetBytes(_null_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_Null() => JsonSerializer.SerializeToUtf8Bytes(_null_item.Source);

	[Benchmark]
	public byte[]? GetBytes_PrimitiveTypes() => BinConverter.GetBytes(_primitiveTypes_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_PrimitiveTypes() => JsonSerializer.SerializeToUtf8Bytes(_primitiveTypes_item.Source);

	[Benchmark]
	public byte[]? GetBytes_LargeString() => BinConverter.GetBytes(_string_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_LargeString() => JsonSerializer.SerializeToUtf8Bytes(_string_item.Source);

	[Benchmark]
	public byte[]? GetBytes_ArrayOfPrimitives() => BinConverter.GetBytes(_primitiveTypeList_item.Source, false);

	[Benchmark]
	public byte[]? JsonSerializer_ArrayOfPrimitives() => JsonSerializer.SerializeToUtf8Bytes(_primitiveTypeList_item.Source);
}
