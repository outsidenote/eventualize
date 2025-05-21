using MongoBenchmark;
using System.Diagnostics;

#pragma warning disable S125 // Sections of code should not be commented out

await ManualAsync();


//Benchmark();
Console.ReadKey(false);

//static void Benchmark()
//{
//    var summary = BenchmarkRunner.Run<MongoAccessPatternsBenchmarks>();
//    Console.WriteLine(summary.Table);
//}

static async Task ManualAsync()
{
    var tests = new MongoAccessPatternsBenchmarks();
    tests.GlobalSetup();

    for (int i = 0; i < 20; i++)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Compound");
        Console.ResetColor();

        tests.IterationSetup();
        var sw = Stopwatch.StartNew();
        await tests.Compound_GetBatch();
        sw.Stop();
        var complexDuration = sw.Elapsed.TotalSeconds;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("By Id");
        Console.ResetColor();

        tests.IterationSetup();
        sw = Stopwatch.StartNew();
        await tests.ById_GetBatch();
        sw.Stop();
        var byIdDuration = sw.Elapsed.TotalSeconds;


        //Console.ForegroundColor = ConsoleColor.Yellow;
        //Console.WriteLine("By Id Via Cursor");
        //Console.ResetColor();

        //tests.IterationSetup();
        //sw = Stopwatch.StartNew();
        //await tests.ById_GetViaCursor();
        //sw.Stop();
        //var byIdViaCursorDuration = sw.Elapsed.TotalSeconds;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"""
    Compound:           {complexDuration:N2}
    By Id:              {byIdDuration:N2}
    """);
        Console.ResetColor();
        Console.WriteLine("====================================");
    }
    tests.GlobalCleanup();
}
