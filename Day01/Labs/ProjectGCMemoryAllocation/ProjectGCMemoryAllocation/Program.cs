using System;
using System.Collections.Generic;

Console.WriteLine("=== Garbage Collection Demo ===");

long before = GC.GetTotalMemory(true);

Console.WriteLine($"Memory Before Allocation: {before / 1024} KB");

CreatePatients();

GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

long after = GC.GetTotalMemory(true);

Console.WriteLine($"Memory After Cleanup: {after / 1024} KB");
Console.WriteLine($"Difference From Start: {(after - before) / 1024} KB");

static void CreatePatients()
{
    var patients = new List<string>();

    for (int i = 0; i < 50_000; i++)
    {
        patients.Add($"Patient-{i}");
    }

    long during = GC.GetTotalMemory(true);

    Console.WriteLine($"Memory During Allocation: {during / 1024} KB");
}
