using BenchmarkDotNet.Running;
using BinaryContainer.Performances.Converter.NoRefPool;
using BinaryContainer.Performances.Sizes;

var summary1 = BenchmarkRunner.Run<GetBytesNoRefPoolBenchmark>();
var summary2 = BenchmarkRunner.Run<GetItemNoRefPoolBenchmark>();

//new GetBytesNoRefPool_OutputSizes().Run();
