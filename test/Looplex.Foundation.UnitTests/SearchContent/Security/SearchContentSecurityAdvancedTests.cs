using Microsoft.VisualStudio.TestTools.UnitTesting;
using Looplex.Foundation.SearchContent;
using Looplex.Foundation.SearchContent.SqlGenerator;
using Looplex.Foundation.SearchContent.Parser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Looplex.Foundation.UnitTests.SearchContent.Security;

/// <summary>
/// Advanced security tests for SearchContent functionality
/// Tests DoS Protection, Recursion Protection, and Thread Safety
/// </summary>
[TestClass]
public class SearchContentSecurityAdvancedTests
{
    private ISearchContentService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SearchContentService();
    }

    #region DoS Protection Tests

    [TestMethod]
    public void Parse_ExtremelyLongFilter_ShouldBeRejected()
    {
        // Arrange
        var longFilter = new string('a', 10000) + " eq \"test\"";

        // Act & Assert
        Assert.ThrowsException<FilterParseException>(() => _service.ConvertToSql(longFilter), "Extremely long filter should be rejected");
    }

    [TestMethod]
    public void Parse_ExcessiveNesting_ShouldBeRejected()
    {
        // Arrange
        var deeplyNestedFilter = "((((" + new string('(', 100) + "userName eq \"test\"" + new string(')', 100) + "))))";

        // Act & Assert
        Assert.ThrowsException<FilterParseException>(() => _service.ConvertToSql(deeplyNestedFilter), "Excessively nested filter should be rejected");
    }

    [TestMethod]
    public void Parse_ExcessiveOperators_ShouldBeRejected()
    {
        // Arrange
        var excessiveOperators = string.Join(" and ", new string[1000].Select((_, i) => $"field{i} eq \"value{i}\""));

        // Act & Assert
        Assert.ThrowsException<FilterParseException>(() => _service.ConvertToSql(excessiveOperators), "Filter with excessive operators should be rejected");
    }

    [TestMethod]
    public void Parse_ExcessiveParameters_ShouldBeRejected()
    {
        // Arrange
        var excessiveParameters = string.Join(" and ", new string[500].Select((_, i) => $"field eq \"{new string('x', 1000)}\""));

        // Act & Assert
        Assert.ThrowsException<FilterParseException>(() => _service.ConvertToSql(excessiveParameters), "Filter with excessive parameters should be rejected");
    }



    #endregion

    #region Recursion Protection Tests



    [TestMethod]
    public void Parse_DeeplyNestedLogicalOperators_ShouldBeRejected()
    {
        // Arrange
        var deeplyNestedLogical = "(" + new string('(', 50) + "userName eq \"test\"" + new string(')', 50) + ")";

        // Act & Assert
        Assert.ThrowsException<FilterParseException>(() => _service.ConvertToSql(deeplyNestedLogical), "Deeply nested logical operators should be rejected");
    }

    #endregion

    #region Thread Safety Tests

    [TestMethod]
    public void ConvertToSql_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var filters = new[]
        {
            "userName eq \"john\"",
            "department eq \"IT\"",
            "active eq true",
            "manager ne null",
            "name.givenName co \"John\"",
            "emails[type eq \"work\"].value co \"@company.com\""
        };

        var results = new List<SqlPredicateResult>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var filter = filters[i % filters.Length];
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var result = _service.ConvertToSql(filter);
                    lock (results)
                    {
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur during concurrent access");
        Assert.AreEqual(10, results.Count, "All tasks should complete successfully");
        
        foreach (var result in results)
        {
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.HasConditions, "Result should have conditions");
        }
    }

    [TestMethod]
    public void TryConvertToSql_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var filters = new[]
        {
            "userName eq \"john\"",
            "invalid filter syntax",
            "department eq \"IT\"",
            "",
            "active eq true"
        };

        var results = new List<bool>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var filter = filters[i % filters.Length];
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var success = _service.TryConvertToSql(filter, out var result);
                    lock (results)
                    {
                        results.Add(success);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur during concurrent access");
        Assert.AreEqual(10, results.Count, "All tasks should complete successfully");
    }

    [TestMethod]
    public void IsValidFilter_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var filters = new[]
        {
            "userName eq \"john\"",
            "invalid filter syntax",
            "department eq \"IT\"",
            "",
            "active eq true"
        };

        var results = new List<bool>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var filter = filters[i % filters.Length];
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var isValid = _service.IsValidFilter(filter);
                    lock (results)
                    {
                        results.Add(isValid);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur during concurrent access");
        Assert.AreEqual(10, results.Count, "All tasks should complete successfully");
    }

    [TestMethod]
    public void GetSupportedOperators_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var results = new List<IEnumerable<string>>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var operators = _service.GetSupportedOperators();
                    lock (results)
                    {
                        results.Add(operators);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur during concurrent access");
        Assert.AreEqual(10, results.Count, "All tasks should complete successfully");
        
        foreach (var operators in results)
        {
            Assert.IsNotNull(operators, "Operators should not be null");
            Assert.IsTrue(operators.Any(), "Should return at least one operator");
        }
    }

    [TestMethod]
    public void GetFilterExamples_ConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var results = new List<Dictionary<string, string>>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var examples = _service.GetFilterExamples();
                    lock (results)
                    {
                        results.Add(examples);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur during concurrent access");
        Assert.AreEqual(10, results.Count, "All tasks should complete successfully");
        
        foreach (var examples in results)
        {
            Assert.IsNotNull(examples, "Examples should not be null");
            Assert.IsTrue(examples.Count > 0, "Should return at least one example");
        }
    }

    #endregion

    #region Memory Safety Tests

    [TestMethod]
    public void Parse_LargeNumberOfParameters_ShouldNotExhaustMemory()
    {
        // Arrange
        var largeFilter = string.Join(" and ", new string[100].Select((_, i) => $"field{i} eq \"value{i}\""));

        // Act
        var result = _service.ConvertToSql(largeFilter);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsTrue(result.HasConditions, "Result should have conditions");
        Assert.AreEqual(100, result.Parameters.Count, "Should handle large number of parameters");
    }

    [TestMethod]
    public void Parse_RepeatedLargeFilters_ShouldNotExhaustMemory()
    {
        // Arrange
        var largeFilter = string.Join(" and ", new string[50].Select((_, i) => $"field{i} eq \"value{i}\""));

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            var result = _service.ConvertToSql(largeFilter);
            Assert.IsNotNull(result, $"Result {i} should not be null");
            Assert.IsTrue(result.HasConditions, $"Result {i} should have conditions");
        }
    }

    #endregion

    #region Performance Under Load Tests

    [TestMethod]
    public void Parse_ConcurrentHighLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var filters = new[]
        {
            "userName eq \"john\" and department eq \"IT\"",
            "active eq true and manager ne null",
            "name.givenName co \"John\" and name.familyName co \"Smith\"",
            "emails[type eq \"work\"].value co \"@company.com\"",
            "(userName eq \"john\" or userName eq \"jane\") and department eq \"IT\""
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = new List<SqlPredicateResult>();
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var filter = filters[i % filters.Length];
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var result = _service.ConvertToSql(filter);
                    lock (results)
                    {
                        results.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(0, exceptions.Count, "No exceptions should occur under high load");
        Assert.AreEqual(100, results.Count, "All tasks should complete successfully");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, "Should complete within reasonable time under high load");
        
        foreach (var result in results)
        {
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.HasConditions, "Result should have conditions");
        }
    }

    #endregion
}
