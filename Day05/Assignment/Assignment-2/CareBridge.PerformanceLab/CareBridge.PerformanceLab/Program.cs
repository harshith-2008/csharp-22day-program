using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using CareBridge.PerformanceLab.Models;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

while (true)
{
    Console.Clear();

    Console.WriteLine("=================================");
    Console.WriteLine(" CAREBRIDGE PERFORMANCE LAB");
    Console.WriteLine("=================================");
    Console.WriteLine();

    Console.WriteLine("1. View Patient");
    Console.WriteLine("2. View Patient Encounters");
    Console.WriteLine("3. Simulate N+1 Problem");
    Console.WriteLine("4. Eager Loading Demo");
    Console.WriteLine("5. Explicit Loading Demo");
    Console.WriteLine("6. AsNoTracking Demo");
    Console.WriteLine("7. Revenue At risk Dashboard");
    Console.WriteLine("8. Cartesion Explosion demo");
    Console.WriteLine("9. Exit");

    Console.WriteLine();
    Console.Write("Choose Option: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            ShowPatient();
            break;

        case "2":
            ShowEncounters();
            break;

        case "3":
            SimulateNPlusOne();
            break;

        case "4":
            EagerLoadingDemo();
            break;

        case "5":
            ExplicitLoadingDemo();
            break;

        case "6":
            AsNoTrackingDemo();
            break;

        case "7":
            RevenueAtRisk();
            break;

        case "8":
            CartesionExplosionDemo();
               break;
        case "9":
            return;

        default:
            Console.WriteLine("Invalid Option");
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

static void ShowPatient()
{
    using var db = new CareBridgeContext();

    var patient =
        db.Patients
          .FirstOrDefault(p => p.Mrn == "MRN999999");

    if (patient == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("PATIENT DETAILS");
    Console.WriteLine("----------------------------");

    Console.WriteLine($"Patient Id : {patient.PatientId}");
    Console.WriteLine($"MRN        : {patient.Mrn}");
    Console.WriteLine($"Name       : {patient.FullName}");
    Console.WriteLine($"City       : {patient.City}");
    Console.WriteLine($"Active     : {patient.IsActive}");
}

static void ShowEncounters()
{
    using var db = new CareBridgeContext();

    var patient =
        db.Patients
          .FirstOrDefault(p => p.Mrn == "MRN999999");

    if (patient == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    var encounters =
        db.Encounters
          .Where(e => e.PatientId == patient.PatientId)
          .ToList();

    Console.WriteLine();
    Console.WriteLine("PATIENT ENCOUNTERS");
    Console.WriteLine("----------------------------");

    Console.WriteLine($"Patient Name    : {patient.FullName}");
    Console.WriteLine($"Encounter Count : {encounters.Count}");
}

static void SimulateNPlusOne()
{
    using var db = new CareBridgeContext();

    Console.WriteLine();
    Console.WriteLine("SIMULATING N+1 QUERY PROBLEM");
    Console.WriteLine("----------------------------");

    var patient =
        db.Patients
          .FirstOrDefault(p => p.Mrn == "MRN999999");

    if (patient == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    Stopwatch stopwatch = Stopwatch.StartNew();

    var encounters =
        db.Encounters
          .Where(e => e.PatientId == patient.PatientId)
          .ToList();

    int totalClaims = 0;

    foreach (var encounter in encounters)
    {
        var claims =
            db.Claims
              .Where(c => c.EncounterId == encounter.EncounterId)
              .ToList();

        totalClaims += claims.Count;
    }

    stopwatch.Stop();

    Console.WriteLine();
    Console.WriteLine($"Patient Name      : {patient.FullName}");
    Console.WriteLine($"Encounters Loaded : {encounters.Count}");
    Console.WriteLine($"Claims Loaded     : {totalClaims}");

    Console.WriteLine();
    Console.WriteLine("PERFORMANCE SUMMARY");
    Console.WriteLine("----------------------------");

    Console.WriteLine("Patient Queries    : 1");
    Console.WriteLine("Encounter Queries  : 1");
    Console.WriteLine($"Claim Queries      : {encounters.Count}");

    Console.WriteLine();

    Console.WriteLine(
        $"Approx Total Queries : {encounters.Count + 2}");

    Console.WriteLine(
        $"Elapsed Time         : {stopwatch.ElapsedMilliseconds} ms");
}

static void EagerLoadingDemo()
{
    using var db = new CareBridgeContext();

    Console.WriteLine();
    Console.WriteLine("EAGER LOADING DEMO");
    Console.WriteLine("----------------------------");

    Stopwatch stopwatch = Stopwatch.StartNew();

    var patient =
        db.Patients
          .Include(p => p.Encounters)
          .ThenInclude(e => e.Claims)
          .FirstOrDefault(p => p.Mrn == "MRN999999");

    stopwatch.Stop();

    if (patient == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    int encounterCount =
        patient.Encounters.Count;

    int claimCount =
        patient.Encounters
               .SelectMany(e => e.Claims)
               .Count();

    Console.WriteLine();
    Console.WriteLine($"Patient Name      : {patient.FullName}");
    Console.WriteLine($"Encounters Loaded : {encounterCount}");
    Console.WriteLine($"Claims Loaded     : {claimCount}");

    Console.WriteLine();

    Console.WriteLine("PERFORMANCE SUMMARY");
    Console.WriteLine("----------------------------");

    Console.WriteLine("Single Include Query : 1");

    Console.WriteLine();

    Console.WriteLine(
        $"Elapsed Time        : {stopwatch.ElapsedMilliseconds} ms");
}

static void ExplicitLoadingDemo()
{
    using var db = new CareBridgeContext();

    Console.WriteLine();
    Console.WriteLine("EXPLICIT LOADING DEMO");
    Console.WriteLine("----------------------------");

    Stopwatch stopwatch = Stopwatch.StartNew();

    var patient =
        db.Patients
          .FirstOrDefault(p => p.Mrn == "MRN999999");

    if (patient == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    db.Entry(patient)
      .Collection(p => p.Encounters)
      .Load();

    stopwatch.Stop();

    Console.WriteLine();
    Console.WriteLine($"Patient Name      : {patient.FullName}");
    Console.WriteLine($"Encounters Loaded : {patient.Encounters.Count}");

    Console.WriteLine();

    Console.WriteLine("PERFORMANCE SUMMARY");
    Console.WriteLine("----------------------------");

    Console.WriteLine("Patient Query     : 1");
    Console.WriteLine("Encounter Query   : 1");
    Console.WriteLine("Total Queries     : 2");

    Console.WriteLine();

    Console.WriteLine(
        $"Elapsed Time      : {stopwatch.ElapsedMilliseconds} ms");
}

static void AsNoTrackingDemo()
{
    Console.WriteLine();
    Console.WriteLine("ASNOTRACKING PERFORMANCE DEMO");
    Console.WriteLine("------------------------------------------------");

    long normalMemoryConsumed = 0;
    long noTrackingMemoryConsumed = 0;

    //
    // Cleanup before measurement
    //
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    //
    // SCENARIO 1
    // Normal Query
    //
    using (var db = new CareBridgeContext())
    {
        Console.WriteLine();
        Console.WriteLine("SCENARIO 1 - NORMAL QUERY");
        Console.WriteLine("------------------------------------------------");

        long memoryBefore =
            GC.GetTotalMemory(true);

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        var claims =
            db.Claims
              .ToList();

        stopwatch.Stop();

        long memoryAfter =
            GC.GetTotalMemory(false);

        normalMemoryConsumed =
            memoryAfter - memoryBefore;

        Console.WriteLine();

        Console.WriteLine(
            $"Claims Loaded      : {claims.Count}");
        Console.WriteLine(
            $"Tracked Entities   : {db.ChangeTracker.Entries().Count()}");
        Console.WriteLine(
            $"Elapsed Time       : {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine(
            $"Memory Consumed    : {normalMemoryConsumed:N0} bytes");
    }

    Console.WriteLine();
    Console.WriteLine();

    //
    // Cleanup before second test
    //
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    //
    // SCENARIO 2
    // AsNoTracking
    //
    using (var db = new CareBridgeContext())
    {
        Console.WriteLine();
        Console.WriteLine("SCENARIO 2 - ASNOTRACKING()");
        Console.WriteLine("------------------------------------------------");

        long memoryBefore =
            GC.GetTotalMemory(true);

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        var claims =
            db.Claims

              // Disable Change Tracking
              .AsNoTracking()

              .ToList();

        stopwatch.Stop();

        long memoryAfter =
            GC.GetTotalMemory(false);

        noTrackingMemoryConsumed =
            memoryAfter - memoryBefore;

        Console.WriteLine();

        Console.WriteLine(
            $"Claims Loaded      : {claims.Count}");

        Console.WriteLine(
            $"Tracked Entities   : {db.ChangeTracker.Entries().Count()}");

        Console.WriteLine(
            $"Elapsed Time       : {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine(
            $"Memory Consumed    : {noTrackingMemoryConsumed:N0} bytes");
    }

    //
    // Final Comparison
    //
    double memorySavingsPercent =
        ((double)(normalMemoryConsumed - noTrackingMemoryConsumed)
         / normalMemoryConsumed) * 100;

    Console.WriteLine();
    Console.WriteLine("PERFORMANCE COMPARISON");
    Console.WriteLine("------------------------------------------------");

    Console.WriteLine(
        $"Normal Memory Usage       : {normalMemoryConsumed:N0} bytes");

    Console.WriteLine(
        $"AsNoTracking Memory Usage : {noTrackingMemoryConsumed:N0} bytes");

    Console.WriteLine(
        $"Memory Saved              : {(normalMemoryConsumed - noTrackingMemoryConsumed):N0} bytes");

    Console.WriteLine(
        $"Memory Saved (%)          : {memorySavingsPercent:F2}%");

    Console.WriteLine();

    Console.WriteLine("Key Learning");
    Console.WriteLine("------------------------------------------------");

    Console.WriteLine(
        "Same SQL Query");

    Console.WriteLine(
        "Same Data Returned");

    Console.WriteLine(
        "Less EF Core Tracking");

    Console.WriteLine(
        "Lower Memory Usage");
}

static void RevenueAtRisk()
{
    Console.WriteLine("Assignment 1 - Revenue at risk dashboard");


    using (var db = new CareBridgeContext())
    {
        Stopwatch stopwatch =
          Stopwatch.StartNew();


        var summary = db.Claims
            .AsNoTracking()
            .GroupBy(c => c.Status)
            .Select(g => new { 
                Status = g.Key,
                ClaimCount = g.Count(),
                TotalBilled = g.Sum(c=>c.BilledAmount),
                TotalReimbursed = g.Sum(c => c.ReimbursedAmt),
                Gap = g.Sum(c => c.BilledAmount) - g.Sum(c => c.ReimbursedAmt)
            })
            .ToList();

        stopwatch.Stop();

        Console.WriteLine("SUMMARY BY STATUS");

        foreach (var row in summary)
        {
            Console.WriteLine($"Status         : {row.Status}  ");
            Console.WriteLine($"Claim Count    : {row.ClaimCount}   ");
            Console.WriteLine($"Total Billed   : {row.TotalBilled:C}  ");
            Console.WriteLine($"Total Reimbursed: {row.TotalReimbursed:C}  ");
            Console.WriteLine($"Gap            : {row.Gap:C}  ");
            Console.WriteLine();
        }

        var revenueAtRisk = summary
         .Where(s => s.Status != "Paid")
         .Sum(s => s.TotalBilled);

        Console.WriteLine($"Revenue at Risk : {revenueAtRisk:C}");

        Console.WriteLine();
        Console.WriteLine($"Tracked Entities : {db.ChangeTracker.Entries().Count()}");
        Console.WriteLine($"Elapsed Time     : {stopwatch.ElapsedMilliseconds} ms");

    }
}
static void CartesionExplosionDemo()
{
    Console.WriteLine();
    Console.WriteLine("CARTESIAN EXPLOSION ");

    using var db = new CareBridgeContext();

    // --- Single Query ---
    Stopwatch sw1 = Stopwatch.StartNew();

    var patientCartesian =
        db.Patients
          .Where(p => p.Mrn == "MRN888888")
          .Include(p => p.Encounters)
            .ThenInclude(e => e.Diagnoses)
          .Include(p => p.Encounters)
            .ThenInclude(e => e.Claims)
          .AsNoTracking()
          .FirstOrDefault();

    sw1.Stop();

    if (patientCartesian == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    int encounterCount1 = patientCartesian.Encounters.Count();
    int diagnosisCount1 = patientCartesian.Encounters.SelectMany(e => e.Diagnoses).Count();
    int claimCount1 = patientCartesian.Encounters.SelectMany(e => e.Claims).Count();

    // --- Split Query ---
    Stopwatch sw2 = Stopwatch.StartNew();

    var patientSplit =
        db.Patients
          .Where(p => p.Mrn == "MRN888888")
          .Include(p => p.Encounters)
            .ThenInclude(e => e.Diagnoses)
          .Include(p => p.Encounters)
            .ThenInclude(e => e.Claims)
          .AsSplitQuery()
          .AsNoTracking()
          .FirstOrDefault();

    sw2.Stop();

    if (patientSplit == null)
    {
        Console.WriteLine("Patient not found.");
        return;
    }

    int encounterCount2 = patientSplit.Encounters.Count();
    int diagnosisCount2 = patientSplit.Encounters.SelectMany(e => e.Diagnoses).Count();
    int claimCount2 = patientSplit.Encounters.SelectMany(e => e.Claims).Count();

    Console.WriteLine();

    // --- Summary ---
    Console.WriteLine("SINGLE QUERY ");
    Console.WriteLine("------------------------------------------------");
    Console.WriteLine($"Encounters : {encounterCount1}   Diagnoses : {diagnosisCount1}   Claims : {claimCount1}");
    Console.WriteLine($"Elapsed Time                : {sw1.ElapsedMilliseconds} ms");

    Console.WriteLine();
    Console.WriteLine("SPLIT QUERY ");
    Console.WriteLine("------------------------------------------------");
    Console.WriteLine($"Encounters : {encounterCount2}   Diagnoses : {diagnosisCount2}   Claims : {claimCount2}");
    Console.WriteLine($"Elapsed Time                : {sw2.ElapsedMilliseconds} ms");

    Console.WriteLine();
    Console.WriteLine($"Object counts identical : {encounterCount1 == encounterCount2 && diagnosisCount1 == diagnosisCount2 && claimCount1 == claimCount2}");
}
