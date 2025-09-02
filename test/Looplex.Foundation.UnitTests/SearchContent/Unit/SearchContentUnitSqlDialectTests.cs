using Microsoft.VisualStudio.TestTools.UnitTesting;
using Looplex.Foundation.SearchContent;
using Looplex.Foundation.SearchContent.SqlGenerator;
using Looplex.Foundation.SearchContent.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Looplex.Foundation.UnitTests.SearchContent.Unit;

/// <summary>
/// SQL Dialect tests for SearchContent functionality
/// Tests different SQL dialects: SQLite, Oracle, SqlServer, PostgreSQL, MySQL
/// </summary>
[TestClass]
public class SearchContentUnitSqlDialectTests
{
    private ISearchContentService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SearchContentService();
    }

    #region SQLite Tests

    [TestMethod]
    public void Parse_SqliteDialect_ShouldUseSqliteSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SQLite
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LIKE"), "SQLite should use LIKE for string comparison");
        Assert.IsTrue(result.Sql.Contains("="), "SQLite should use = for equality");
        Assert.IsFalse(result.Sql.Contains("LOWER"), "SQLite should not use LOWER by default");
    }

    [TestMethod]
    public void Parse_SqliteDialect_CaseInsensitive_ShouldUseLOWER()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SQLite,
            CaseSensitive = false
        };

        // Act
        var result = _service.ConvertToSql("userName co \"john\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LOWER"), "SQLite should use LOWER for case-insensitive comparison");
    }

    [TestMethod]
    public void Parse_SqliteDialect_ValuePathFilter_ShouldUseSqliteSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SQLite
        };

        // Act
        var result = _service.ConvertToSql("emails[type eq \"work\"].value co \"@company.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("EXISTS"), "Should generate EXISTS clause");
        Assert.IsTrue(result.Sql.Contains("LIKE"), "SQLite should use LIKE for string comparison");
    }

    #endregion

    #region Oracle Tests

    [TestMethod]
    public void Parse_OracleDialect_ShouldUseOracleSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.Oracle
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "Oracle should use = for equality");
        Assert.IsFalse(result.Sql.Contains("LOWER"), "Oracle should not use LOWER by default");
    }

    [TestMethod]
    public void Parse_OracleDialect_CaseInsensitive_ShouldUseLOWER()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.Oracle,
            CaseSensitive = false
        };

        // Act
        var result = _service.ConvertToSql("userName co \"john\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LOWER"), "Oracle should use LOWER for case-insensitive comparison");
    }

    [TestMethod]
    public void Parse_OracleDialect_ValuePathFilter_ShouldUseOracleSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.Oracle
        };

        // Act
        var result = _service.ConvertToSql("emails[type eq \"work\"].value co \"@company.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("EXISTS"), "Should generate EXISTS clause");
        Assert.IsTrue(result.Sql.Contains("LIKE"), "Oracle should use LIKE for string comparison");
    }

    [TestMethod]
    public void Parse_OracleDialect_WithTableAlias_ShouldUseOracleSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.Oracle,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.userName"), "Should use table alias");
        Assert.IsTrue(result.Sql.Contains("u.department"), "Should use table alias");
    }

    #endregion

    #region SqlServer Tests

    [TestMethod]
    public void Parse_SqlServerDialect_ShouldUseSqlServerSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "SQL Server should use = for equality");
        Assert.IsFalse(result.Sql.Contains("LOWER"), "SQL Server should not use LOWER by default");
    }

    [TestMethod]
    public void Parse_SqlServerDialect_CaseInsensitive_ShouldUseLOWER()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer,
            CaseSensitive = false
        };

        // Act
        var result = _service.ConvertToSql("userName co \"john\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LOWER"), "SQL Server should use LOWER for case-insensitive comparison");
    }

    [TestMethod]
    public void Parse_SqlServerDialect_ValuePathFilter_ShouldUseSqlServerSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer
        };

        // Act
        var result = _service.ConvertToSql("emails[type eq \"work\"].value co \"@company.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("EXISTS"), "Should generate EXISTS clause");
        Assert.IsTrue(result.Sql.Contains("LIKE"), "SQL Server should use LIKE for string comparison");
    }

    [TestMethod]
    public void Parse_SqlServerDialect_WithTableAlias_ShouldUseSqlServerSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.userName"), "Should use table alias");
        Assert.IsTrue(result.Sql.Contains("u.department"), "Should use table alias");
    }

    [TestMethod]
    public void Parse_SqlServerDialect_WithCustomParameterPrefix_ShouldUseSqlServerSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer,
            ParameterPrefix = "@filter"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("@filter")), "Should use custom parameter prefix");
    }

    #endregion

    #region PostgreSQL Tests

    [TestMethod]
    public void Parse_PostgreSqlDialect_ShouldUsePostgreSqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "PostgreSQL should use = for equality");
        Assert.IsFalse(result.Sql.Contains("LOWER"), "PostgreSQL should not use LOWER by default");
    }

    [TestMethod]
    public void Parse_PostgreSqlDialect_CaseInsensitive_ShouldUseLOWER()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql,
            CaseSensitive = false
        };

        // Act
        var result = _service.ConvertToSql("userName co \"john\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LOWER"), "PostgreSQL should use LOWER for case-insensitive comparison");
    }

    [TestMethod]
    public void Parse_PostgreSqlDialect_ValuePathFilter_ShouldUsePostgreSqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql
        };

        // Act
        var result = _service.ConvertToSql("emails[type eq \"work\"].value co \"@company.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("EXISTS"), "Should generate EXISTS clause");
        Assert.IsTrue(result.Sql.Contains("LIKE"), "PostgreSQL should use LIKE for string comparison");
    }

    [TestMethod]
    public void Parse_PostgreSqlDialect_WithTableAlias_ShouldUsePostgreSqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.userName"), "Should use table alias");
        Assert.IsTrue(result.Sql.Contains("u.department"), "Should use table alias");
    }

    [TestMethod]
    public void Parse_PostgreSqlDialect_WithCustomParameterPrefix_ShouldUsePostgreSqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql,
            ParameterPrefix = "$filter"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("$filter")), "Should use custom parameter prefix");
    }

    #endregion

    #region MySQL Tests

    [TestMethod]
    public void Parse_MySqlDialect_ShouldUseMySqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "MySQL should use = for equality");
        Assert.IsFalse(result.Sql.Contains("LOWER"), "MySQL should not use LOWER by default");
    }

    [TestMethod]
    public void Parse_MySqlDialect_CaseInsensitive_ShouldUseLOWER()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql,
            CaseSensitive = false
        };

        // Act
        var result = _service.ConvertToSql("userName co \"john\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("LOWER"), "MySQL should use LOWER for case-insensitive comparison");
    }

    [TestMethod]
    public void Parse_MySqlDialect_ValuePathFilter_ShouldUseMySqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql
        };

        // Act
        var result = _service.ConvertToSql("emails[type eq \"work\"].value co \"@company.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("EXISTS"), "Should generate EXISTS clause");
        Assert.IsTrue(result.Sql.Contains("LIKE"), "MySQL should use LIKE for string comparison");
    }

    [TestMethod]
    public void Parse_MySqlDialect_WithTableAlias_ShouldUseMySqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.userName"), "Should use table alias");
        Assert.IsTrue(result.Sql.Contains("u.department"), "Should use table alias");
    }

    [TestMethod]
    public void Parse_MySqlDialect_WithCustomParameterPrefix_ShouldUseMySqlSyntax()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql,
            ParameterPrefix = "?filter"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("?filter")), "Should use custom parameter prefix");
    }

    #endregion

    #region Cross-Dialect Comparison Tests

    [TestMethod]
    public void Parse_CrossDialectComparison_ShouldGenerateDifferentSyntax()
    {
        // Arrange
        var filter = "userName co \"john\" and department eq \"IT\"";
        var results = new Dictionary<SqlDialect, SqlPredicateResult>();

        // Act
        foreach (SqlDialect dialect in Enum.GetValues(typeof(SqlDialect)))
        {
            var options = new SqlGenerationOptions
            {
                Dialect = dialect,
                CaseSensitive = false
            };
            results[dialect] = _service.ConvertToSql(filter, options);
        }

        // Assert
        Assert.AreEqual(6, results.Count, "Should test all SQL dialects");
        
        foreach (var result in results.Values)
        {
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.HasConditions, "Result should have conditions");
            Assert.IsTrue(result.Sql.Contains("LOWER"), "All dialects should use LOWER for case-insensitive comparison");
        }
    }

    [TestMethod]
    public void Parse_CrossDialectValuePath_ShouldGenerateConsistentStructure()
    {
        // Arrange
        var filter = "emails[type eq \"work\"].value co \"@company.com\"";
        var results = new Dictionary<SqlDialect, SqlPredicateResult>();

        // Act
        foreach (SqlDialect dialect in Enum.GetValues(typeof(SqlDialect)))
        {
            var options = new SqlGenerationOptions
            {
                Dialect = dialect
            };
            results[dialect] = _service.ConvertToSql(filter, options);
        }

        // Assert
        Assert.AreEqual(6, results.Count, "Should test all SQL dialects");
        
        foreach (var result in results.Values)
        {
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.HasConditions, "Result should have conditions");
            Assert.IsTrue(result.Sql.Contains("EXISTS"), "All dialects should generate EXISTS clause");
            Assert.IsTrue(result.Sql.Contains("LIKE"), "All dialects should use LIKE for string comparison");
        }
    }

    [TestMethod]
    public void Parse_CrossDialectParameterPrefix_ShouldUseDialectSpecificPrefix()
    {
        // Arrange
        var filter = "userName eq \"john\" and age gt 25";
        var dialectPrefixes = new Dictionary<SqlDialect, string>
        {
            { SqlDialect.SqlServer, "@filter" },
            { SqlDialect.PostgreSql, "$filter" },
            { SqlDialect.MySql, "?filter" },
            { SqlDialect.SQLite, "filter" },
            { SqlDialect.Oracle, "filter" }
        };

        // Act & Assert
        foreach (var kvp in dialectPrefixes)
        {
            var options = new SqlGenerationOptions
            {
                Dialect = kvp.Key,
                ParameterPrefix = kvp.Value
            };
            
            var result = _service.ConvertToSql(filter, options);
            
            Assert.IsNotNull(result, $"Result for {kvp.Key} should not be null");
            Assert.IsTrue(result.HasConditions, $"Result for {kvp.Key} should have conditions");
            Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith(kvp.Value)), 
                $"{kvp.Key} should use {kvp.Value} parameter prefix");
        }
    }

    #endregion

    #region Edge Cases for Each Dialect

    [TestMethod]
    public void Parse_SqliteDialect_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SQLite
        };

        // Act
        var result = _service.ConvertToSql("name eq \"João\" and city eq \"São Paulo\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "SQLite should handle Unicode characters");
    }

    [TestMethod]
    public void Parse_OracleDialect_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.Oracle
        };

        // Act
        var result = _service.ConvertToSql("name eq \"João\" and city eq \"São Paulo\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "Oracle should handle Unicode characters");
    }

    [TestMethod]
    public void Parse_SqlServerDialect_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.SqlServer
        };

        // Act
        var result = _service.ConvertToSql("name eq \"João\" and city eq \"São Paulo\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "SQL Server should handle Unicode characters");
    }

    [TestMethod]
    public void Parse_PostgreSqlDialect_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.PostgreSql
        };

        // Act
        var result = _service.ConvertToSql("name eq \"João\" and city eq \"São Paulo\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "PostgreSQL should handle Unicode characters");
    }

    [TestMethod]
    public void Parse_MySqlDialect_UnicodeCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            Dialect = SqlDialect.MySql
        };

        // Act
        var result = _service.ConvertToSql("name eq \"João\" and city eq \"São Paulo\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("="), "MySQL should handle Unicode characters");
    }

    #endregion
}
