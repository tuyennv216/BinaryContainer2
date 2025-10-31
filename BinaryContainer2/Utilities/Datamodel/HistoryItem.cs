namespace BinaryContainer2.Utilities.Datamodel
{
	public class HistoryItem
	{
		public bool IsNull { get; set; }
		public string Name { get; set; }
		public byte[] Data { get; set; }

		private static HistoryItem _nullItem = new HistoryItem { IsNull = true };
		public static HistoryItem NullItem => _nullItem;
	}
}
