---
title: "Performance Improvements With Code Generation"
date: 2025-08-10T18:03:26+01:00
cover : "bacon-ipsum-banner1.jpg"
image : "bacon-ipsum-banner1.jpg"
useRelativeCover : true
draft: true
tags:
  - performance
  - reflection
toc : true
summary: "An example how using code generation improved data pipeline execution time."

---




# Title

something something





A system

```json
{
  "CustomFunction": "SetFullTimeValidationStatus",
  "RuleCondition": {
    "SchemaItemPath": "User.IsFullTime",
    "Condition": "Equals",
    "ComparisonValue": "True",
    "NextOperation": {
      "OperatorType": "AND",
      "RuleCondition": {
        "SchemaItemPath": "User.Title",
        "Condition": "IsNotBlank"
      }
    }
  }
}
```



```csharp
public class User
{
    public bool IsFullTime { get; set; }

    public string? Title { get; set; }

    public string? ValidationResult { get; set; }
}
```

```csharp
public static class RecalculationHandler
{
    public static void SetFullTimeValidationStatus(User value)
    {
        /* Do Some Work here */

        value.ValidationResult = "full time employee validated";
    }
}
```




```
BenchmarkDotNet v0.15.2, Linux KDE neon User Edition
Intel Core Ultra 7 258V 4.80GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]    : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2
  .NET 10.0 : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX2
  .NET 6.0  : .NET 6.0.36 (6.0.3624.51421), X64 RyuJIT AVX2
  .NET 7.0  : .NET 7.0.20 (7.0.2024.26716), X64 RyuJIT AVX2
  .NET 8.0  : .NET 8.0.19 (8.0.1925.36514), X64 RyuJIT AVX2
  .NET 9.0  : .NET 9.0.7 (9.0.725.31616), X64 RyuJIT AVX2


| Method      | Job       | Runtime   | Mean        | Error      | StdDev     | Ratio    | RatioSD |
|------------ |---------- |---------- |------------:|-----------:|-----------:|---------:|--------:|
| Interpreted | .NET 6.0  | .NET 6.0  | 866.3737 ns | 15.0672 ns | 14.0938 ns | 1,882.52 |   51.16 |
| Compiled    | .NET 6.0  | .NET 6.0  |   0.4605 ns |  0.0115 ns |  0.0107 ns |     1.00 |    0.03 |
|             |           |           |             |            |            |          |         |
| Interpreted | .NET 7.0  | .NET 7.0  | 462.0918 ns |  8.2606 ns |  7.3228 ns |   647.23 |   23.58 |
| Compiled    | .NET 7.0  | .NET 7.0  |   0.7147 ns |  0.0262 ns |  0.0245 ns |     1.00 |    0.05 |
|             |           |           |             |            |            |          |         |
| Interpreted | .NET 8.0  | .NET 8.0  | 344.1804 ns |  2.0669 ns |  1.8322 ns |   504.49 |    5.91 |
| Compiled    | .NET 8.0  | .NET 8.0  |   0.6823 ns |  0.0090 ns |  0.0075 ns |     1.00 |    0.01 |
|             |           |           |             |            |            |          |         |
| Interpreted | .NET 9.0  | .NET 9.0  | 316.2857 ns |  1.6904 ns |  1.4985 ns |   945.94 |   14.13 |
| Compiled    | .NET 9.0  | .NET 9.0  |   0.3344 ns |  0.0052 ns |  0.0049 ns |     1.00 |    0.02 |
|             |           |           |             |            |            |          |         |
| Interpreted | .NET 10.0 | .NET 10.0 | 270.1915 ns |  4.3739 ns |  3.8773 ns |   782.91 |   51.82 |
| Compiled    | .NET 10.0 | .NET 10.0 |   0.3466 ns |  0.0245 ns |  0.0241 ns |     1.00 |    0.09 |




```





![Ratio ](ratio.png)



![Interpreted code duration distribution](interpreded_comparision.png)
