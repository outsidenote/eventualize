using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using BenchmarkDotNet.Running;
using MongoBenchmark;

await ManualAsync();

//Benchmark();
Console.ReadKey(false);

static void Benchmark()
{
    var summary = BenchmarkRunner.Run<MongoAccessPatternsBenchmarks>();
    Console.WriteLine(summary.Table);
}

static async Task ManualAsync()
{
    var x = new MongoAccessPatternsBenchmarks();

    for (int i = 0; i < 20; i++)
    {

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("By Id");
        Console.ResetColor();

        x.GlobalSetup();
        x.IterationSetup();
        var sw = Stopwatch.StartNew();
        await x.ById_GetBatch();
        sw.Stop();
        var byIdDuration = sw.Elapsed.TotalSeconds;
        x.GlobalCleanup();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Complex");
        Console.ResetColor();

        x.GlobalSetup();
        x.IterationSetup();
        sw = Stopwatch.StartNew();
        await x.Composed_GetBatch();
        sw.Stop();
        var complexDuration = sw.Elapsed.TotalSeconds;
        x.GlobalCleanup();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"""
    By Id:      {byIdDuration:N2}
    Complex:    {complexDuration:N2}
    """);
        Console.ResetColor();
        Console.WriteLine("====================================");
    }
}
