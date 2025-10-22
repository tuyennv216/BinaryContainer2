using System;

namespace BinaryContainer2.Others
{
	[Flags]
	public enum Settings : byte
	{
		Has_Flags = 1 << 0,
		Has_Items = 1 << 1,
		Has_Arrays = 1 << 2,
		Using_RefPool = 1 << 3,
		Is_Root_Null = 1 << 4,
	}

	public static class SettingsExtensions
	{
		/// <summary>
		/// Thiết lập trạng thái (bật hoặc tắt) của cờ tại vị trí 'type'.
		/// </summary>
		/// <param name="settings">Giá trị Settings hiện tại.</param>
		/// <param name="type">Cờ Settings cần thiết lập.</param>
		/// <param name="isEnable">True để bật cờ (Enable), False để tắt cờ (Disable).</param>
		/// <returns>Giá trị Settings mới sau khi thiết lập.</returns>
		public static Settings Set(this Settings settings, Settings type, bool isEnable)
		{
			if (isEnable)
			{
				// Nếu muốn bật (isEnable == true), sử dụng phép toán OR (|)
				return settings | type;
			}
			else
			{
				// Nếu muốn tắt (isEnable == false), sử dụng phép toán AND (&) với NOT (~)
				return settings & (~type);
			}
		}

		/// <summary>
		/// Kiểm tra xem cờ tại vị trí 'type' có khớp với trạng thái mong muốn ('isEnable') hay không.
		/// Nó tương đương với việc gọi 'settings.IsEnable(type) == isEnable'.
		/// </summary>
		/// <param name="settings">Giá trị Settings hiện tại.</param>
		/// <param name="type">Cờ Settings cần kiểm tra.</param>
		/// <param name="isEnable">Trạng thái mong muốn (true nếu muốn cờ BẬT, false nếu muốn cờ TẮT).</param>
		/// <returns>True nếu cờ hiện tại khớp với trạng thái mong muốn, ngược lại là False.</returns>
		public static bool Is(this Settings settings, Settings type, bool isEnable)
		{
			// Kiểm tra xem cờ hiện tại có đang BẬT không.
			bool isCurrentlyEnabled = (settings & type) == type;

			// So sánh trạng thái hiện tại với trạng thái mong muốn.
			return isCurrentlyEnabled == isEnable;
		}
	}
}
