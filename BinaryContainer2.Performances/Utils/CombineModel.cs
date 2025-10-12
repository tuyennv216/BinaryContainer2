namespace BinaryContainer.Performances.Models
{
	public class BenchmarkItem<T>
	{
		public T? Source { get; set; }
		public byte[]? BinaryBytes { get; set; }
		public byte[]? JsonBytes { get; set; }
	}
}
