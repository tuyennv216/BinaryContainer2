using System;

namespace BinaryContainer2.Utilities.Datamodel
{
	public class SementIndexes
	{
		// Số lượng tối đa
		// Giá trị tối thiểu là 1
		public int Length { get; protected set; }

		// Vị trí bắt đầu
		// Giá trị trong khoảng [0; Length)
		public int StartIndex { get; protected set; }

		// Số lượng đang sử dụng
		// Giá trị trong khoảng [0; Length]
		public int Use { get; protected set; }

		// Vị trí phần tử đang làm việc
		// Giá trị trong khoảng [StartIndex; StartIndex + Use)
		public int WorkingIndex { get; protected set; }

		public SementIndexes(int length = 1, int startIndex = 0, int use = 0, int workingIndex = 0)
		{
			if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
			if (use < 0 || use > length) throw new ArgumentOutOfRangeException(nameof(use));

			Length = length;
			Use = use;
			StartIndex = CorrectIndex(startIndex);
			WorkingIndex = CorrectIndex(workingIndex);

			if (use == 0 || use == 1) WorkingIndex = StartIndex;
		}

		// Di chuyển working index đến vị trí tiếp theo
		// Không được > end index
		public void MoveNext()
		{
			if (Length == 1 || Use == 0) return;

			var endIndex = CorrectIndex(StartIndex + Use - 1);
			if (WorkingIndex == endIndex) return;

			var newIndex = CorrectIndex(WorkingIndex + 1);

			WorkingIndex = newIndex;
		}

		// Di chuyển working index đến vị trí trước đó
		// Không được < start index
		public void MovePrevious()
		{
			if (Length == 1 || Use == 0 || WorkingIndex == StartIndex) return;

			var newIndex = CorrectIndex(WorkingIndex - 1);

			WorkingIndex = newIndex;
		}

		// Di chuyển working index lên phía trước
		// Nếu <= end index thì đặt end index là working index
		// Nếu > end index thì tăng Used hoặc StartIndex
		public void NewIndex()
		{
			if (Length == 1) return;
			if (Use == 0)
			{
				Use = 1;
				return;
			}

			var endIndex = CorrectIndex(StartIndex + Use - 1);

			// Nếu đang ở vị trí cuối cùng
			if (WorkingIndex == endIndex)
			{
				// Nếu đã dùng hết số lượng thì tăng StartIndex
				if (Use == Length) StartIndex++;
				// Nếu chưa dùng hết thì tăng Use
				else Use++;

				WorkingIndex++;
			}
			// Nếu không ở vị trí cuối cùng và >= StartIndex
			else if (WorkingIndex >= StartIndex)
			{
				WorkingIndex++;
				Use = WorkingIndex - StartIndex + 1;
			}
			// Nếu không ở vị trí cuối cùng và < StartIndex
			else
			{
				WorkingIndex++;
				Use = (WorkingIndex + Length) - StartIndex + 1;
			}

			StartIndex = CorrectIndex(StartIndex);
			WorkingIndex = CorrectIndex(WorkingIndex);
		}

		private int CorrectIndex(int index)
		{
			return (index % Length + Length) % Length;
		}
	}
}
