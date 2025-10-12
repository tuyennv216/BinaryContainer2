namespace BinaryContainer2.Abstraction
{
	public interface IContainer<TData>
	{
		/// <summary>
		/// Độ dài của container = số lượng bytes
		/// </summary>
		public int TotalExportBytes { get; }

		/// <summary>
		/// Xuất mảng dữ liệu
		/// </summary>
		/// <returns>Trả về mảng dữ liệu</returns>
		public TData[] Export();

		/// <summary>
		/// Nhập mảng dữ liệu
		/// </summary>
		/// <param name="buffer">Mảng dữ liệu</param>
		/// <param name="start">Vị trí bắt đầu</param>
		/// <returns>Vị trí bắt đầu tiếp theo</returns>
		public int Import(TData[] buffer, int start = 0);
	}
}
