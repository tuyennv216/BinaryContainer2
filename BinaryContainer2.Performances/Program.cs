using BenchmarkDotNet.Running;
using BinaryContainer.Performances.Converter.DataConvertClass;
using BinaryContainer.Performances.Sizes;

//var summary1 = BenchmarkRunner.Run<GetBytesBenchmark>();
//var summary2 = BenchmarkRunner.Run<GetItemBenchmark>();

new GetBytes_OutputSizes().Run();
