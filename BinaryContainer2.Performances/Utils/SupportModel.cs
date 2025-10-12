namespace BinaryContainer.Performances.Models
{

	// Supporting classes for benchmark tests
	public class SimpleClass
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime CreatedDate { get; set; }
		public bool IsActive { get; set; }
		public decimal Price { get; set; }
	}

	public class ComplexClass
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Address Address { get; set; }
		public string[] Tags { get; set; }
		public Dictionary<string, string> Metadata { get; set; }
	}

	public class Address
	{
		public string Street { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
	}

	public class PrimitiveTypes
	{
		public int IntValue { get; set; }
		public long LongValue { get; set; }
		public double DoubleValue { get; set; }
		public float FloatValue { get; set; }
		public bool BoolValue { get; set; }
		public string StringValue { get; set; }
		public DateTime DateTimeValue { get; set; }
	}
}
