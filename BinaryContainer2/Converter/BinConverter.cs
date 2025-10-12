using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;

namespace BinaryContainer2.Converter
{
	public static class BinConverter
	{
		public static byte[] GetBytes<T>(T? target)
		{
			var op = TypeOperators.Instance.GetOperator<T>(); 

			var refPool = new RefPool();
			var container = new DataContainer();
			op.Write(container, target, refPool);

			var bytes = container.Export();
			return bytes;
		}

		public static T? GetItem<T>(byte[] binary)
		{
			var container = new DataContainer();
			container.Import(binary);
			var refPool = new RefPool();

			var op = TypeOperators.Instance.GetOperator<T>();

			var item = op.Read(container, refPool);
			return (T?)item;
		}
	}
}
