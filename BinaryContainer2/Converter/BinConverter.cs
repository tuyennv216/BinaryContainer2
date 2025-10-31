using BinaryContainer2.Container;
using BinaryContainer2.Others;
using BinaryContainer2.Types;
using System;

namespace BinaryContainer2.Converter
{
	public static class BinConverter
	{
		public static byte[] GetBytes(object? target, bool useRefPool = true)
		{
			if (target is null)
			{
				var nullContainer = new DataContainer(useRefPool);
				nullContainer.SetSetting(Settings.Is_Root_Null, true);
				var nullBytes = nullContainer.Export();
				return nullBytes;
			}

			var type = target.GetType();
			var op = TypeOperators.Instance.GetOperator(type);

			var refPool = new RefPool();
			var container = new DataContainer(useRefPool);
			op.Write(container, target, refPool);

			var bytes = container.Export();
			return bytes;
		}

		public static T? GetItem<T>(byte[] binary, bool useRefPool = true)
		{
			var item = GetItem(typeof(T), binary, useRefPool);
			return (T?)item;
		}

		public static object? GetItem(Type type, byte[] binary, bool useRefPool = true)
		{
			var container = new DataContainer(useRefPool);
			container.Import(binary);

			if (container.Settings.Is(Settings.Is_Root_Null, true))
			{
				return null;
			}

			var refPool = new RefPool();

			var op = TypeOperators.Instance.GetOperator(type);

			var item = op.Read(container, refPool);
			return item;
		}
	}
}
