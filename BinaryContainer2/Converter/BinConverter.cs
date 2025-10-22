using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;

namespace BinaryContainer2.Converter
{
	public static class BinConverter
	{
		public static byte[] GetBytes<T>(T? target, bool useRefPool = true)
		{
			if (target is null)
			{
				var nullContainer = new DataContainer(useRefPool);
				nullContainer.SetSetting(Settings.Is_Root_Null, true);
				var nullBytes = nullContainer.Export();
				return nullBytes;
			}

			var op = TypeOperators.Instance.GetOperator<T>(); 

			var refPool = new RefPool();
			var container = new DataContainer(useRefPool);
			op.Write(container, target, refPool);

			var bytes = container.Export();
			return bytes;
		}

		public static T? GetItem<T>(byte[] binary, bool useRefPool = true)
		{
			var container = new DataContainer(useRefPool);
			container.Import(binary);

			if (container.Settings.Is(Settings.Is_Root_Null, true))
			{
				return (T?)(object?)null;
			}

			var refPool = new RefPool();

			var op = TypeOperators.Instance.GetOperator<T>();

			var item = op.Read(container, refPool);
			return (T?)item;
		}
	}
}
