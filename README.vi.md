
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
Hỗ trợ lồng nhiều kiểu dữ liệu.  

## Build
Đã pass: **692/692** testcases.  
Phiên bản: **.NET Standard 2.0**  

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
...
```

## Cách sử dụng

```
var bin = BinConverter.GetBytes(target);
var item = BinConverter.GetItem<ItemType>(bin);
```

## Hiệu năng so với JsonSerializer

### Lấy bytes

| Method                           | Mean         | Error        | StdDev       | Median       | Gen0    | Gen1   | Allocated |
|--------------------------------- |-------------:|-------------:|-------------:|-------------:|--------:|-------:|----------:|
| GetBytes_SimpleObject            |    922.87 ns |    18.327 ns |    26.285 ns |    922.98 ns |  0.4063 | 0.0019 |    3400 B |
| JsonSerializer_SimpleObject      |    420.59 ns |     8.150 ns |     8.004 ns |    417.81 ns |  0.0191 |      - |     160 B |
| GetBytes_ComplexObject           |  1,959.70 ns |    23.159 ns |    20.530 ns |  1,952.94 ns |  0.6447 | 0.0038 |    5400 B |
| JsonSerializer_ComplexObject     |  1,021.60 ns |    20.506 ns |    21.059 ns |  1,019.33 ns |  0.0763 |      - |     640 B |
| GetBytes_SimpleList              | 38,655.29 ns |   727.712 ns |   996.100 ns | 38,411.80 ns |  8.8501 | 0.3052 |   74136 B |
| JsonSerializer_SimpleList        | 38,640.88 ns |   543.558 ns |   508.445 ns | 38,413.91 ns |  1.5259 |      - |   12952 B |
| GetBytes_ComplexList             | 63,460.96 ns | 1,194.750 ns | 1,467.260 ns | 63,554.36 ns | 12.9395 | 0.7324 |  108672 B |
| JsonSerializer_ComplexList       | 49,888.94 ns |   599.060 ns |   500.242 ns | 49,828.89 ns |  1.8311 |      - |   15488 B |
| GetBytes_Dictionary              |  9,541.98 ns |   183.778 ns |   211.639 ns |  9,526.38 ns |  2.3956 | 0.0305 |   20112 B |
| JsonSerializer_Dictionary        |  8,791.93 ns |   171.792 ns |   210.976 ns |  8,789.13 ns |  0.3510 |      - |    3032 B |
| GetBytes_Null                    |     90.39 ns |     1.879 ns |     4.241 ns |     88.40 ns |  0.0526 |      - |     440 B |
| JsonSerializer_Null              |     96.02 ns |     1.963 ns |     3.921 ns |     94.52 ns |  0.0038 |      - |      32 B |
| GetBytes_PrimitiveTypes          |    972.59 ns |    19.039 ns |    26.061 ns |    974.05 ns |  0.4158 |      - |    3480 B |
| JsonSerializer_PrimitiveTypes    |    697.90 ns |    13.506 ns |    15.012 ns |    696.78 ns |  0.0296 |      - |     248 B |
| GetBytes_LargeString             |  1,413.90 ns |    29.511 ns |    83.236 ns |  1,385.06 ns |  2.1820 | 0.0362 |   18272 B |
| JsonSerializer_LargeString       |  2,462.38 ns |    50.605 ns |   149.211 ns |  2,456.63 ns |  0.2022 |      - |    1712 B |
| GetBytes_ArrayOfPrimitives       | 86,718.33 ns | 1,385.149 ns | 1,482.094 ns | 86,772.27 ns | 10.6201 | 0.3662 |   88944 B |
| JsonSerializer_ArrayOfPrimitives | 18,335.04 ns |   361.243 ns |   386.525 ns | 18,261.74 ns |  1.3123 |      - |   10992 B |

### Lấy item

| Method                            | Mean          | Error        | StdDev       | Gen0    | Gen1   | Allocated |
|---------------------------------- |--------------:|-------------:|-------------:|--------:|-------:|----------:|
| GetItem_SimpleObject              |     823.66 ns |    16.255 ns |    25.307 ns |  0.3462 | 0.0019 |    2896 B |
| JsonDeserialize_SimpleObject      |     688.69 ns |    13.647 ns |    13.404 ns |  0.0124 |      - |     104 B |
| GetItem_ComplexObject             |   2,423.26 ns |    45.954 ns |    86.313 ns |  0.6714 | 0.0038 |    5632 B |
| JsonDeserialize_ComplexObject     |   2,082.58 ns |    41.529 ns |    62.159 ns |  0.2098 |      - |    1776 B |
| GetItem_SimpleList                |  41,131.37 ns |   541.948 ns |   452.551 ns |  8.2397 | 0.4883 |   69107 B |
| JsonDeserialize_SimpleList        |  70,627.62 ns |   799.426 ns |   708.671 ns |  1.5869 |      - |   13864 B |
| GetItem_ComplexList               |  88,006.76 ns | 1,658.766 ns | 2,270.537 ns | 15.8691 | 2.3193 |  132989 B |
| JsonDeserialize_ComplexList       | 103,485.21 ns | 1,300.564 ns | 1,086.030 ns |  7.3242 | 0.9766 |   61432 B |
| GetItem_Dictionary                |  10,373.22 ns |   167.814 ns |   218.206 ns |  2.3193 | 0.0458 |   19464 B |
| JsonDeserialize_Dictionary        |  16,022.08 ns |   290.119 ns |   345.366 ns |  0.6104 |      - |    5352 B |
| GetItem_Null                      |      29.08 ns |     0.769 ns |     2.242 ns |  0.0287 |      - |     240 B |
| JsonDeserialize_Null              |      72.28 ns |     1.282 ns |     1.199 ns |       - |      - |         - |
| GetItem_LargeString               |   2,802.18 ns |    56.014 ns |   135.280 ns |  1.6823 | 0.0191 |   14088 B |
| JsonDeserialize_LargeString       |   2,646.16 ns |    50.467 ns |    49.566 ns |  0.4997 |      - |    4208 B |
| GetItem_PrimitiveTypes            |     904.18 ns |    17.955 ns |    40.527 ns |  0.3605 | 0.0019 |    3024 B |
| JsonDeserialize_PrimitiveTypes    |     930.20 ns |    11.391 ns |    10.098 ns |  0.0257 |      - |     216 B |
| GetItem_ArrayOfPrimitives         |  26,434.80 ns |   521.064 ns |   730.459 ns |  8.4534 | 0.2747 |   70720 B |
| JsonDeserialize_ArrayOfPrimitives |  47,628.61 ns |   493.229 ns |   411.869 ns |  1.4648 |      - |   12448 B |

### Kích thước bytes output

|Object                   |bin converter  |bytes     |  percent|   >   |json converter |bytes     |  percent
|-------------------------|---------------|----------|---------|-------|---------------|----------|---------
|SimpleObject             |bindata(*)     |61        |48,412697|   <   |jsondata       |126       |
|ComplexObject            |bindata(*)     |215       |68,910255|   <   |jsondata       |312       |
|SimpleList               |bindata(*)     |4285      |33,937904|   <   |jsondata       |12626     |
|ComplexList              |bindata(*)     |9289      | 61,96798|   <   |jsondata       |14990     |
|Dictionary               |bindata(*)     |1010      | 37,49072|   <   |jsondata       |2694      |
|Null                     |bindata        |5         |         |   >   |jsondata(*)    |4         |       80
|LargeString              |bindata(*)     |1953      | 99,94882|   <   |jsondata       |1954      |
|Primitive                |bindata(*)     |90        | 38,29787|   <   |jsondata       |235       |
|PrimitiveArr             |bindata(*)     |4269      |38,703537|   <   |jsondata       |11030     |
