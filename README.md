## Data Type Support:

Nullable, Boolean, Byte, Char, Decimal, Double, Guid, Int16, Int32, Int64, SByte, Single, String, UInt16, UInt32, UInt64.  
DateTime, DateTimeOffset, TimeSpan.  
Array, List, Stack, Queue, LinkedList, Multidimensional Array, IEnumerable.  
KeyValuePair, Dictionary, SortedList, Tuple.  
Enum, Class.  
Object: where object is a build-in type, enum, or class.  

-----

## Highlights:

Supports circular references, iterative references.  
Supports polymorphism, inheritance.  
Supports Dictionary with a Class Key.  
Supports multi-level nested lists.  

-----

## Build

Passed: **538/538** test cases.  
Support: **.NET Standard 2.0**  

-----

## These are some real-world test cases that have passed

```
TEST CASE 1: Handling Circular Reference.
TEST CASE 2: Handling Inheritance and Polymorphism (Restoring subclass type).
TEST CASE 3: Handling Nulls, Empty Lists, and Decimal/DateTime Precision.
TEST CASE 5: Using DEEP nested structures (Recursion limit check).
TEST CASE 6: Handling Dictionary with a Class Key (Non-primitive Key).
TEST CASE 8: Serialization/Deserialization via Interface (Restoring actual type).
TEST CASE 9: Using a combination of List nested within a List and Default Values.
TEST CASE 10: Handling rare/difficult .NET Data Types (TimeSpan, Tuple, HashSet, 4-byte Unicode).
TEST CASE 13: Handling Duplicate Object Reference.
TEST CASE 14: Duplicate Reference in Collection.
TEST CASE 16: Using Nested NULL Data and Inheritance.
TEST CASE 18: Polymorphism in Collection (Restoring actual type of each item).
TEST CASE 19: Dictionary with Struct Value.
TEST CASE 20: Property with Object type (Maximum polymorphism).
TEST CASE 21: With Deeply Nested Nullable Struct type.
TEST CASE 22: Using a Large String (Payload Size).
TEST CASE 23: With Zero/Default values and empty-valued non-null objects.
TEST CASE 24: Deep Inheritance combined with Polymorphism in a List.
TEST CASE 25: Self-Referencing Property.
TEST CASE 26: Using a Large String Nested in a Dictionary (Boundary Check).
TEST CASE 27: Using List<object?> containing a mix of types (Boxing and Null).
TEST CASE 29: Using a Deeply Nested Array (3D Array) and Decimal Precision.
TEST CASE 30: Using Control Characters (Non-printable Characters) in String.
TEST CASE 31: DateTimeOffset and Large TimeSpan with Ticks precision.
TEST CASE 32: Advanced Generic Class (List<GenericWrapper<Guid, DateTime?>>).
TEST CASE 33: Enum and Nullable Enum with value 0 and large values.
TEST CASE 34: Array and Dictionary with Extremely Large Data Strings and Empty Lists.
TEST CASE 35: 2-Dimensional Array and Char (Two-Dim Array).
TEST CASE 36: Using Nested Object Array Type (Polymorphism in object[]).
TEST CASE 37: The difference between List and Array, especially in nested empty structures.
TEST CASE 39: Using Dictionary<TKey, TValue> where TValue is an Array.
TEST CASE 40: With NULL Array and Large Size Array.
TEST CASE 41: With Boolean/Byte types at limits (FALSE, 0, 255).
TEST CASE 43: With Nested Struct (Size) and Boolean Array.
TEST CASE 44: Using Special Byte Strings (0xFF and 0x00).
TEST CASE 45: Using List<List<T>> where T is Decimal (Nesting and Precision).
TEST CASE 46: With SortedList (Ordered Collection).
TEST CASE 47: Deeply Nested Nullable Type (Struct in List).
TEST CASE 48: Indirect Circular Reference via Collection.
TEST CASE 49: With List<object> containing mixed data.
TEST CASE 50: With a Base Class having NO Public Properties.
TEST CASE 51: Using a deep and unnecessary empty structure.
TEST CASE 52: KeyValuePair (Generic Struct) in both single property and List.
TEST CASE 53: Object Array containing Enum Types (Boxing/Unboxing Enum).
TEST CASE 54: Deep Empty Structure and Default Value of Tuple.
```

-----

## How to use

```
var bin = BinConverter.GetBytes(target);
var item = BinConverter.GetItem<ItemType>(bin);
```

-----

## Performance comparison with JsonSerializer

### Get bytes

| Method | Mean | Error | StdDev | Gen0 | Gen1 | Allocated |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: |
| GetBytes\_SimpleObject | 989.66 ns | 18.805 ns | 20.121 ns | 0.4101 | 0.0019 | 3440 B |
| JsonSerializer\_SimpleObject | 416.58 ns | 5.720 ns | 5.070 ns | 0.0181 | - | 152 B |
| GetBytes\_ComplexObject | 2,691.08 ns | 53.280 ns | 61.357 ns | 0.7248 | 0.0038 | 6080 B |
| JsonSerializer\_ComplexObject | 1,025.01 ns | 20.289 ns | 25.660 ns | 0.0744 | - | 632 B |
| GetBytes\_SimpleList | 54,182.62 ns | 1,039.368 ns | 972.225 ns | 12.4512 | 0.0610 | 104544 B |
| JsonSerializer\_SimpleList | 42,330.42 ns | 137.116 ns | 107.051 ns | 1.5869 | - | 13760 B |
| GetBytes\_ComplexList | 110,954.01 ns | 2,053.677 ns | 1,714.913 ns | 22.5830 | 3.1738 | 189440 B |
| JsonSerializer\_ComplexList | 47,977.19 ns | 376.641 ns | 333.883 ns | 1.7700 | - | 15288 B |
| GetBytes\_Dictionary | 13,002.12 ns | 182.236 ns | 152.175 ns | 2.9755 | 0.0610 | 24952 B |
| JsonSerializer\_Dictionary | 8,554.34 ns | 169.212 ns | 201.435 ns | 0.3662 | - | 3096 B |
| GetBytes\_Null | 364.16 ns | 5.844 ns | 6.253 ns | 0.2770 | 0.0019 | 2320 B |
| JsonSerializer\_Null | 87.74 ns | 0.438 ns | 0.388 ns | 0.0038 | - | 32 B |
| GetBytes\_PrimitiveTypes | 1,035.93 ns | 7.412 ns | 7.280 ns | 0.4234 | 0.0019 | 3544 B |
| JsonSerializer\_PrimitiveTypes | 676.49 ns | 8.886 ns | 7.420 ns | 0.0324 | - | 272 B |
| GetBytes\_LargeString | 2,228.34 ns | 20.555 ns | 18.221 ns | 1.6785 | 0.0267 | 14040 B |
| JsonSerializer\_LargeString | 2,857.74 ns | 57.059 ns | 109.933 ns | 0.2556 | - | 2144 B |
| GetBytes\_ArrayOfPrimitives | 79,907.28 ns | 988.545 ns | 876.320 ns | 10.6201 | 0.2441 | 89024 B |
| JsonSerializer\_ArrayOfPrimitives | 17,671.76 ns | 99.517 ns | 88.219 ns | 1.2817 | - | 10984 B |

### Get item

| Method | Mean | Error | StdDev | Gen0 | Gen1 | Allocated |
| :--- | ---: | ---: | ---: | ---: | ---: | ---: |
| GetItem\_SimpleObject | 879.23 ns | 8.601 ns | 7.182 ns | 0.3538 | 0.0019 | 2960 B |
| JsonDeserialize\_SimpleObject | 652.74 ns | 4.640 ns | 3.875 ns | 0.0134 | - | 112 B |
| GetItem\_ComplexObject | 2,650.08 ns | 36.302 ns | 32.181 ns | 0.6599 | 0.0076 | 5520 B |
| JsonDeserialize\_ComplexObject | 2,121.66 ns | 27.759 ns | 25.966 ns | 0.2136 | - | 1800 B |
| GetItem\_SimpleList | 57,784.33 ns | 694.265 ns | 649.416 ns | 11.8408 | 1.2207 | 99801 B |
| JsonDeserialize\_SimpleList | 69,595.65 ns | 489.255 ns | 408.550 ns | 1.4648 | - | 13064 B |
| GetItem\_ComplexList | 125,647.49 ns | 1,260.907 ns | 1,117.761 ns | 23.9258 | 4.8828 | 202086 B |
| JsonDeserialize\_ComplexList | 100,260.10 ns | 1,135.784 ns | 948.431 ns | 7.0801 | 0.9766 | 60232 B |
| GetItem\_Dictionary | 13,209.50 ns | 188.504 ns | 176.327 ns | 2.7008 | 0.0763 | 22670 B |
| JsonDeserialize\_Dictionary | 15,962.22 ns | 187.164 ns | 165.916 ns | 0.6409 | - | 5512 B |
| GetItem\_Null | 314.87 ns | 6.278 ns | 5.873 ns | 0.2475 | 0.0014 | 2072 B |
| JsonDeserialize\_Null | 72.36 ns | 0.262 ns | 0.232 ns | - | - | - |
| GetItem\_LargeString | 2,270.12 ns | 40.225 ns | 47.884 ns | 1.4534 | 0.0076 | 12184 B |
| JsonDeserialize\_LargeString | 2,799.16 ns | 54.077 ns | 47.938 ns | 0.5035 | - | 4240 B |
| GetItem\_PrimitiveTypes | 997.64 ns | 17.648 ns | 15.644 ns | 0.3586 | 0.0019 | 3008 B |
| JsonDeserialize\_PrimitiveTypes | 941.73 ns | 14.625 ns | 13.680 ns | 0.0210 | - | 176 B |
| GetItem\_ArrayOfPrimitives | 26,854.68 ns | 528.755 ns | 587.710 ns | 8.4534 | 0.3052 | 70800 B |
| JsonDeserialize\_ArrayOfPrimitives | 47,826.88 ns | 529.952 ns | 469.788 ns | 1.4648 | - | 12448 B |

### Output bytes size

| Object | bin converter | bytes | percent | \> | json converter | bytes | percent |
| :--- | --- | --- | --- | :--- | --- | --- | --- |
| SimpleObject | bindata(\*) | 64 | 49.612404 | \< | jsondata | 129 | |
| ComplexObject | bindata(\*) | 230 | 71.20743 | \< | jsondata | 323 | |
| SimpleList | bindata(\*) | 4610 | 35.711517 | \< | jsondata | 12909 | |
| ComplexList | bindata(\*) | 9515 | 63.0049 | \< | jsondata | 15102 | |
| Dictionary | bindata(\*) | 1077 | 39.135174 | \< | jsondata | 2752 | |
| Null | bindata | 11 | | \> | jsondata(\*) | 4 | 36.363636 |
| LargeString | bindata(\*) | 2174 | 99.954025 | \< | jsondata | 2175 | |
| Primitive | bindata(\*) | 110 | 42.801556 | \< | jsondata | 257 | |
| PrimitiveArr | bindata(\*) | 4269 | 38.802036 | \< | jsondata | 11002 | |