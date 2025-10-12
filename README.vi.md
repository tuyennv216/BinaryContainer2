
## Hỗ trợ kiểu dữ liệu:

Nullable, Boolean, Byte, Char, Decimal, Double, Guid, Int16, Int32, Int64, SByte, Single, String, UInt16, UInt32, UInt64.  
DateTime, DateTimeOffset, TimeSpan.  
Array, List, Stack, Queue, LinkedList, Array nhiều chiều, IEnumerable.  
KeyPairValue, Dictionary, ShortedList, Tuple.  
Enum, Class.  
Object: với object là kiểu buildin, enum, class.  

## Điểm nổi bật:

Hỗ trợ tham chiều vòng tròn, tham chiếu lặp.  
Hỗ trợ đa hình, kế thừa.  
Hỗ trợ Dictionary với Key là một class.  
Hỗ trợ danh sách lồng nhiều lớp.  

## Build
Đã pass: **538/538** testcases.  
Hỗ trợ: .NET Standard 2.0

## Đây là một số tests theo nhu cầu thực tế đã pass

```
TEST CASE 1: Xử lý Tham chiếu Vòng tròn (Circular Reference).
TEST CASE 2: Xử lý Kế thừa và Đa hình (Khôi phục kiểu lớp con).
TEST CASE 3: Xử lý Null, List rỗng, và Số thập phân/DateTime chính xác.
TEST CASE 5: Bằng cấu trúc lồng ghép SÂU (Kiểm tra giới hạn đệ quy).
TEST CASE 6: Xử lý Dictionary với Key là một Class (Non-primitive Key).
TEST CASE 8: Serialization/Deserialization qua Interface (Khôi phục kiểu thực tế).
TEST CASE 9: Bằng việc kết hợp List lồng List và Giá trị Default.
TEST CASE 10: Xử lý các Kiểu Dữ liệu .NET hiếm/khó (TimeSpan, Tuple, HashSet, Unicode 4-byte).
TEST CASE 13: Xử lý Tham chiếu Trùng lặp (Duplicate Object Reference).
TEST CASE 14: Tham chiếu Trùng lặp trong Collection.
TEST CASE 16: Bằng Dữ liệu NULL Lồng ghép và Kế thừa.
TEST CASE 18: Đa hình trong Collection (Khôi phục kiểu thực tế từng item).
TEST CASE 19: Dictionary với Value là Struct.
TEST CASE 20: Thuộc tính có kiểu Object (Đa hình tối đa).
TEST CASE 21: Với Kiểu Dữ liệu Nullable Struct lồng ghép sâu.
TEST CASE 22: Bằng String Lớn (Payload Size).
TEST CASE 23: Với các giá trị Zero/Default và các đối tượng có giá trị rỗng không phải null.
TEST CASE 24: Kế thừa Sâu kết hợp Đa hình trong List.
TEST CASE 25: Thuộc tính Tự Tham Chiếu (Self-Referencing Property).
TEST CASE 26: Bằng String Lớn Lồng ghép trong Dictionary (Boundary Check).
TEST CASE 27: Bằng List<object?> chứa hỗn hợp các kiểu (Boxing và Null).
TEST CASE 29: Bằng Mảng Lồng Ghép Sâu (3D Array) và Decimal chính xác.
TEST CASE 30: Bằng Ký tự Control (Non-printable Characters) trong Chuỗi.
TEST CASE 31: DateTimeOffset và Large TimeSpan với độ chính xác Ticks.
TEST CASE 32: Class Generic Nâng cao (List<GenericWrapper<Guid, DateTime?>>).
TEST CASE 33: Enum và Nullable Enum với giá trị 0 và giá trị lớn.
TEST CASE 34: Array và Dictionary với các Chuỗi Dữ liệu Cực Lớn và List Rỗng.
TEST CASE 35: Mảng 2 chiều và Char (Two-Dim Array).
TEST CASE 36: Bằng Kiểu Object Array lồng ghép (Polymorphism trong object[]).
TEST CASE 37: Sự khác biệt giữa List và Array, đặc biệt là các cấu trúc rỗng lồng ghép.
TEST CASE 39: Bằng Dictionary<TKey, TValue> với TValue là một Array.
TEST CASE 40: Với Mảng NULL và Mảng Kích thước Lớn.
TEST CASE 41: Với các Kiểu Boolean/Byte ở giới hạn (FALSE, 0, 255).
TEST CASE 43: Với Struct Lồng ghép (Size) và Mảng Boolean.
TEST CASE 44: Bằng Chuỗi Byte Đặc biệt (0xFF và 0x00).
TEST CASE 45: Bằng List<List<T>> với T là Decimal (Lồng ghép và Độ chính xác).
TEST CASE 46: Với SortedList (Collection có thứ tự).
TEST CASE 47: Kiểu Dữ liệu Nullable Lồng ghép Sâu (Struct trong List).
TEST CASE 48: Tham chiếu Vòng tròn Gián tiếp qua Collection.
TEST CASE 49: Với List<object> chứa dữ liệu hỗn hợp.
TEST CASE 50: Với Lớp Cơ sở KHÔNG có Thuộc tính Public.
TEST CASE 51: Bằng một cấu trúc rỗng sâu và không cần thiết.
TEST CASE 52: KeyValuePair (Struct Generic) trong cả thuộc tính đơn và List.
TEST CASE 53: Object Array chứa Kiểu Enum (Boxing/Unboxing Enum).
TEST CASE 54: Cấu trúc Rỗng Sâu và Giá trị Mặc định của Tuple.
```

## Cách sử dụng

```
var bin = BinConverter.GetBytes(target);
var item = BinConverter.GetItem<ItemType>(bin);
```

## Hiệu năng so với JsonSerializer

### Lấy bytes

| Method                           | Mean          | Error        | StdDev       | Gen0    | Gen1   | Allocated |
|--------------------------------- |--------------:|-------------:|-------------:|--------:|-------:|----------:|
| GetBytes_SimpleObject            |     989.66 ns |    18.805 ns |    20.121 ns |  0.4101 | 0.0019 |    3440 B |
| JsonSerializer_SimpleObject      |     416.58 ns |     5.720 ns |     5.070 ns |  0.0181 |      - |     152 B |
| GetBytes_ComplexObject           |   2,691.08 ns |    53.280 ns |    61.357 ns |  0.7248 | 0.0038 |    6080 B |
| JsonSerializer_ComplexObject     |   1,025.01 ns |    20.289 ns |    25.660 ns |  0.0744 |      - |     632 B |
| GetBytes_SimpleList              |  54,182.62 ns | 1,039.368 ns |   972.225 ns | 12.4512 | 0.0610 |  104544 B |
| JsonSerializer_SimpleList        |  42,330.42 ns |   137.116 ns |   107.051 ns |  1.5869 |      - |   13760 B |
| GetBytes_ComplexList             | 110,954.01 ns | 2,053.677 ns | 1,714.913 ns | 22.5830 | 3.1738 |  189440 B |
| JsonSerializer_ComplexList       |  47,977.19 ns |   376.641 ns |   333.883 ns |  1.7700 |      - |   15288 B |
| GetBytes_Dictionary              |  13,002.12 ns |   182.236 ns |   152.175 ns |  2.9755 | 0.0610 |   24952 B |
| JsonSerializer_Dictionary        |   8,554.34 ns |   169.212 ns |   201.435 ns |  0.3662 |      - |    3096 B |
| GetBytes_Null                    |     364.16 ns |     5.844 ns |     6.253 ns |  0.2770 | 0.0019 |    2320 B |
| JsonSerializer_Null              |      87.74 ns |     0.438 ns |     0.388 ns |  0.0038 |      - |      32 B |
| GetBytes_PrimitiveTypes          |   1,035.93 ns |     7.412 ns |     7.280 ns |  0.4234 | 0.0019 |    3544 B |
| JsonSerializer_PrimitiveTypes    |     676.49 ns |     8.886 ns |     7.420 ns |  0.0324 |      - |     272 B |
| GetBytes_LargeString             |   2,228.34 ns |    20.555 ns |    18.221 ns |  1.6785 | 0.0267 |   14040 B |
| JsonSerializer_LargeString       |   2,857.74 ns |    57.059 ns |   109.933 ns |  0.2556 |      - |    2144 B |
| GetBytes_ArrayOfPrimitives       |  79,907.28 ns |   988.545 ns |   876.320 ns | 10.6201 | 0.2441 |   89024 B |
| JsonSerializer_ArrayOfPrimitives |  17,671.76 ns |    99.517 ns |    88.219 ns |  1.2817 |      - |   10984 B |

### Lấy item

| Method                            | Mean          | Error        | StdDev       | Gen0    | Gen1   | Allocated |
|---------------------------------- |--------------:|-------------:|-------------:|--------:|-------:|----------:|
| GetItem_SimpleObject              |     879.23 ns |     8.601 ns |     7.182 ns |  0.3538 | 0.0019 |    2960 B |
| JsonDeserialize_SimpleObject      |     652.74 ns |     4.640 ns |     3.875 ns |  0.0134 |      - |     112 B |
| GetItem_ComplexObject             |   2,650.08 ns |    36.302 ns |    32.181 ns |  0.6599 | 0.0076 |    5520 B |
| JsonDeserialize_ComplexObject     |   2,121.66 ns |    27.759 ns |    25.966 ns |  0.2136 |      - |    1800 B |
| GetItem_SimpleList                |  57,784.33 ns |   694.265 ns |   649.416 ns | 11.8408 | 1.2207 |   99801 B |
| JsonDeserialize_SimpleList        |  69,595.65 ns |   489.255 ns |   408.550 ns |  1.4648 |      - |   13064 B |
| GetItem_ComplexList               | 125,647.49 ns | 1,260.907 ns | 1,117.761 ns | 23.9258 | 4.8828 |  202086 B |
| JsonDeserialize_ComplexList       | 100,260.10 ns | 1,135.784 ns |   948.431 ns |  7.0801 | 0.9766 |   60232 B |
| GetItem_Dictionary                |  13,209.50 ns |   188.504 ns |   176.327 ns |  2.7008 | 0.0763 |   22670 B |
| JsonDeserialize_Dictionary        |  15,962.22 ns |   187.164 ns |   165.916 ns |  0.6409 |      - |    5512 B |
| GetItem_Null                      |     314.87 ns |     6.278 ns |     5.873 ns |  0.2475 | 0.0014 |    2072 B |
| JsonDeserialize_Null              |      72.36 ns |     0.262 ns |     0.232 ns |       - |      - |         - |
| GetItem_LargeString               |   2,270.12 ns |    40.225 ns |    47.884 ns |  1.4534 | 0.0076 |   12184 B |
| JsonDeserialize_LargeString       |   2,799.16 ns |    54.077 ns |    47.938 ns |  0.5035 |      - |    4240 B |
| GetItem_PrimitiveTypes            |     997.64 ns |    17.648 ns |    15.644 ns |  0.3586 | 0.0019 |    3008 B |
| JsonDeserialize_PrimitiveTypes    |     941.73 ns |    14.625 ns |    13.680 ns |  0.0210 |      - |     176 B |
| GetItem_ArrayOfPrimitives         |  26,854.68 ns |   528.755 ns |   587.710 ns |  8.4534 | 0.3052 |   70800 B |
| JsonDeserialize_ArrayOfPrimitives |  47,826.88 ns |   529.952 ns |   469.788 ns |  1.4648 |      - |   12448 B |

### Kích thước bytes output

|Object                   |bin converter  |bytes     |  percent|   >   |json converter |bytes     |  percent
|-------------------------|---------------|----------|---------|-------|---------------|----------|---------
|SimpleObject             |bindata(*)     |64        |49,612404|   <   |jsondata       |129       |
|ComplexObject            |bindata(*)     |230       | 71,20743|   <   |jsondata       |323       |
|SimpleList               |bindata(*)     |4610      |35,711517|   <   |jsondata       |12909     |
|ComplexList              |bindata(*)     |9515      |  63,0049|   <   |jsondata       |15102     |
|Dictionary               |bindata(*)     |1077      |39,135174|   <   |jsondata       |2752      |
|Null                     |bindata        |11        |         |   >   |jsondata(*)    |4         |36,363636
|LargeString              |bindata(*)     |2174      |99,954025|   <   |jsondata       |2175      |
|Primitive                |bindata(*)     |110       |42,801556|   <   |jsondata       |257       |
|PrimitiveArr             |bindata(*)     |4269      |38,802036|   <   |jsondata       |11002     |
