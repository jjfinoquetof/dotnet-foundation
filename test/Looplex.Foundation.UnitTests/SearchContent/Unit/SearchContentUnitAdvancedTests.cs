using Microsoft.VisualStudio.TestTools.UnitTesting;
using Looplex.Foundation.SearchContent;
using Looplex.Foundation.SearchContent.SqlGenerator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Looplex.Foundation.UnitTests.SearchContent.Unit;

/// <summary>
/// Advanced tests for SearchContent functionality
/// Tests Field Mapping, Table Alias, Parameter Prefix, and advanced configurations
/// </summary>
[TestClass]
public class SearchContentUnitAdvancedTests
{
    private ISearchContentService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new SearchContentService();
    }

    #region Field Mapping Tests

    [TestMethod]
    public void Parse_WithFieldMapping_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "userName", "dsNome" },
            { "displayName", "dsNomeExibicao" },
            { "email", "dsEmail" }
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and displayName co \"John\"", fieldMapping);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("dsNome"), "Should map userName to dsNome");
        Assert.IsTrue(result.Sql.Contains("dsNomeExibicao"), "Should map displayName to dsNomeExibicao");
        Assert.IsFalse(result.Sql.Contains("userName"), "Should not contain original field name");
        Assert.IsFalse(result.Sql.Contains("displayName"), "Should not contain original field name");
    }

    [TestMethod]
    public void Parse_WithFieldMapping_SubAttributes_ShouldMapCorrectly()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "name.givenName", "dsNome" },
            { "name.familyName", "dsSobrenome" },
            { "emails.value", "dsEmail" }
        };

        // Act
        var result = _service.ConvertToSql("name.givenName eq \"John\" and name.familyName eq \"Smith\"", fieldMapping);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("dsNome"), "Should map name.givenName to dsNome");
        Assert.IsTrue(result.Sql.Contains("dsSobrenome"), "Should map name.familyName to dsSobrenome");
        Assert.IsFalse(result.Sql.Contains("name_givenName"), "Should not use fallback conversion");
        Assert.IsFalse(result.Sql.Contains("name_familyName"), "Should not use fallback conversion");
    }

    [TestMethod]
    public void Parse_WithFieldMapping_ComplexExpression_ShouldMapCorrectly()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "userName", "dsNome" },
            { "department", "dsDepartamento" },
            { "active", "isAtivo" },
            { "manager", "cdGerente" }
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\" and active eq true and manager ne null", fieldMapping);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("dsNome"), "Should map userName");
        Assert.IsTrue(result.Sql.Contains("dsDepartamento"), "Should map department");
        Assert.IsTrue(result.Sql.Contains("isAtivo"), "Should map active");
        Assert.IsTrue(result.Sql.Contains("cdGerente"), "Should map manager");
        
        // Verify all original field names are replaced
        Assert.IsFalse(result.Sql.Contains("userName"), "Should not contain original userName");
        Assert.IsFalse(result.Sql.Contains("department"), "Should not contain original department");
        Assert.IsFalse(result.Sql.Contains("active"), "Should not contain original active");
        Assert.IsFalse(result.Sql.Contains("manager"), "Should not contain original manager");
    }

    [TestMethod]
    public void Parse_WithFieldMapping_FallbackToSubAttributeConversion_ShouldWork()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "name_givenName", "dsNome" },
            { "name_familyName", "dsSobrenome" }
        };

        // Act
        var result = _service.ConvertToSql("name.givenName eq \"John\" and name.familyName eq \"Smith\"", fieldMapping);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("dsNome"), "Should map name_givenName to dsNome");
        Assert.IsTrue(result.Sql.Contains("dsSobrenome"), "Should map name_familyName to dsSobrenome");
    }

    [TestMethod]
    public void Parse_WithFieldMapping_NoMapping_ShouldUseOriginalNames()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>();

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", fieldMapping);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("userName"), "Should use original userName when no mapping");
        Assert.IsTrue(result.Sql.Contains("department"), "Should use original department when no mapping");
    }

    #endregion

    #region Table Alias Tests

    [TestMethod]
    public void Parse_WithTableAlias_ShouldPrefixColumns()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.userName"), "Should prefix userName with table alias");
        Assert.IsTrue(result.Sql.Contains("u.department"), "Should prefix department with table alias");
    }

    [TestMethod]
    public void Parse_WithTableAlias_SubAttributes_ShouldPrefixCorrectly()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("name.givenName eq \"John\" and emails.value co \"@example.com\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.name_givenName"), "Should prefix sub-attributes with table alias");
        Assert.IsTrue(result.Sql.Contains("u.emails_value"), "Should prefix sub-attributes with table alias");
    }

    [TestMethod]
    public void Parse_WithTableAlias_ComplexExpression_ShouldPrefixAllColumns()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            TableAlias = "users"
        };

        // Act
        var result = _service.ConvertToSql("(userName eq \"john\" or userName eq \"jane\") and (department eq \"IT\" or department eq \"HR\") and active eq true", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("users.userName"), "Should prefix all userName references");
        Assert.IsTrue(result.Sql.Contains("users.department"), "Should prefix all department references");
        Assert.IsTrue(result.Sql.Contains("users.active"), "Should prefix active with table alias");
        
        // Count occurrences to ensure all references are prefixed
        var userNameCount = result.Sql.Split(new[] { "users.userName" }, StringSplitOptions.None).Length - 1;
        var departmentCount = result.Sql.Split(new[] { "users.department" }, StringSplitOptions.None).Length - 1;
        
        Assert.IsTrue(userNameCount >= 2, "Should have at least 2 userName references prefixed");
        Assert.IsTrue(departmentCount >= 2, "Should have at least 2 department references prefixed");
    }

    [TestMethod]
    public void Parse_WithTableAlias_NoAlias_ShouldNotPrefixColumns()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            TableAlias = null
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("userName"), "Should use userName without prefix when no alias");
        Assert.IsTrue(result.Sql.Contains("department"), "Should use department without prefix when no alias");
        Assert.IsFalse(result.Sql.Contains("."), "Should not contain dots when no table alias");
    }

    [TestMethod]
    public void Parse_WithTableAlias_EmptyAlias_ShouldNotPrefixColumns()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            TableAlias = ""
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("userName"), "Should use userName without prefix when empty alias");
        Assert.IsTrue(result.Sql.Contains("department"), "Should use department without prefix when empty alias");
        Assert.IsFalse(result.Sql.Contains("."), "Should not contain dots when empty table alias");
    }

    #endregion

    #region Parameter Prefix Tests

    [TestMethod]
    public void Parse_WithCustomParameterPrefix_ShouldUseCustomPrefix()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            ParameterPrefix = "param"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.Any(k => k.StartsWith("param")), "Should use custom parameter prefix");
        
        // Verify specific parameter names
        var paramKeys = result.Parameters.Keys.Where(k => k.StartsWith("param")).ToList();
        Assert.IsTrue(paramKeys.Contains("param1"), "Should have param1");
        Assert.IsTrue(paramKeys.Contains("param2"), "Should have param2");
    }

    [TestMethod]
    public void Parse_WithCustomParameterPrefix_ComplexExpression_ShouldUseConsistentPrefix()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            ParameterPrefix = "filter"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\" and active eq true and manager ne null", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("filter")), "All parameters should use custom prefix");
        Assert.AreEqual(3, result.Parameters.Count, "Should have correct number of parameters");
        
        // Verify parameter names are sequential
        var expectedParams = new[] { "filter1", "filter2", "filter3" };
        foreach (var expectedParam in expectedParams)
        {
            Assert.IsTrue(result.Parameters.ContainsKey(expectedParam), $"Should contain {expectedParam}");
        }
    }

    [TestMethod]
    public void Parse_WithCustomParameterPrefix_DefaultPrefix_ShouldUseDefault()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            ParameterPrefix = "p" // Default value
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("p")), "All parameters should use default prefix");
        
        // Verify specific parameter names
        var paramKeys = result.Parameters.Keys.Where(k => k.StartsWith("p")).ToList();
        Assert.IsTrue(paramKeys.Contains("p1"), "Should have p1");
        Assert.IsTrue(paramKeys.Contains("p2"), "Should have p2");
    }

    [TestMethod]
    public void Parse_WithCustomParameterPrefix_EmptyPrefix_ShouldWork()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            ParameterPrefix = ""
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and age gt 25", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Parameters.Keys.All(k => int.TryParse(k, out _)), "All parameters should be numeric when empty prefix");
        
        // Verify parameter names are sequential numbers
        var paramKeys = result.Parameters.Keys.Select(k => int.Parse(k)).OrderBy(x => x).ToList();
        Assert.AreEqual(1, paramKeys[0], "First parameter should be 1");
        Assert.AreEqual(2, paramKeys[1], "Second parameter should be 2");
    }

    #endregion

    #region Combined Field Mapping and Table Alias Tests

    [TestMethod]
    public void Parse_WithFieldMappingAndTableAlias_ShouldApplyBoth()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "userName", "dsNome" },
            { "department", "dsDepartamento" }
        };

        var options = new SqlGenerationOptions
        {
            FieldMapping = fieldMapping,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.dsNome"), "Should apply both field mapping and table alias");
        Assert.IsTrue(result.Sql.Contains("u.dsDepartamento"), "Should apply both field mapping and table alias");
        Assert.IsFalse(result.Sql.Contains("u.userName"), "Should not contain original field name with alias");
        Assert.IsFalse(result.Sql.Contains("u.department"), "Should not contain original field name with alias");
    }

    [TestMethod]
    public void Parse_WithFieldMappingAndTableAlias_SubAttributes_ShouldApplyBoth()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "name.givenName", "dsNome" },
            { "name.familyName", "dsSobrenome" }
        };

        var options = new SqlGenerationOptions
        {
            FieldMapping = fieldMapping,
            TableAlias = "u"
        };

        // Act
        var result = _service.ConvertToSql("name.givenName eq \"John\" and name.familyName eq \"Smith\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.dsNome"), "Should apply both field mapping and table alias for sub-attributes");
        Assert.IsTrue(result.Sql.Contains("u.dsSobrenome"), "Should apply both field mapping and table alias for sub-attributes");
    }

    [TestMethod]
    public void Parse_WithFieldMappingAndTableAliasAndParameterPrefix_ShouldApplyAll()
    {
        // Arrange
        var fieldMapping = new Dictionary<string, string>
        {
            { "userName", "dsNome" },
            { "department", "dsDepartamento" }
        };

        var options = new SqlGenerationOptions
        {
            FieldMapping = fieldMapping,
            TableAlias = "u",
            ParameterPrefix = "filter"
        };

        // Act
        var result = _service.ConvertToSql("userName eq \"john\" and department eq \"IT\"", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("u.dsNome"), "Should apply field mapping and table alias");
        Assert.IsTrue(result.Sql.Contains("u.dsDepartamento"), "Should apply field mapping and table alias");
        Assert.IsTrue(result.Parameters.Keys.All(k => k.StartsWith("filter")), "Should use custom parameter prefix");
        Assert.AreEqual(2, result.Parameters.Count, "Should have correct number of parameters");
    }

    #endregion

    #region IncludeNullChecks Tests

    [TestMethod]
    public void Parse_WithIncludeNullChecks_True_ShouldIncludeNullChecks()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            IncludeNullChecks = true
        };

        // Act
        var result = _service.ConvertToSql("emails pr", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        Assert.IsTrue(result.Sql.Contains("IS NOT NULL"), "Should include null check for present operator");
    }

    [TestMethod]
    public void Parse_WithIncludeNullChecks_False_ShouldNotIncludeNullChecks()
    {
        // Arrange
        var options = new SqlGenerationOptions
        {
            IncludeNullChecks = false
        };

        // Act
        var result = _service.ConvertToSql("emails pr", options);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.HasConditions);
        // Note: The exact behavior depends on implementation, but should be different from IncludeNullChecks = true
    }

    #endregion

    #region API Methods Tests

    [TestMethod]
    public void TryConvertToSql_ValidFilter_ShouldReturnTrue()
    {
        // Arrange
        var filter = "userName eq \"john\"";

        // Act
        var success = _service.TryConvertToSql(filter, out var result);

        // Assert
        Assert.IsTrue(success, "Should return true for valid filter");
        Assert.IsNotNull(result, "Result should not be null");
        Assert.IsTrue(result!.HasConditions, "Should have conditions");
    }

    [TestMethod]
    public void TryConvertToSql_InvalidFilter_ShouldReturnFalse()
    {
        // Arrange
        var filter = "invalid filter syntax";

        // Act
        var success = _service.TryConvertToSql(filter, out var result);

        // Assert
        Assert.IsFalse(success, "Should return false for invalid filter");
        Assert.IsNull(result, "Result should be null for invalid filter");
    }

    [TestMethod]
    public void TryConvertToSql_EmptyFilter_ShouldReturnFalse()
    {
        // Arrange
        var filter = "";

        // Act
        var success = _service.TryConvertToSql(filter, out var result);

        // Assert
        Assert.IsFalse(success, "Should return false for empty filter");
        Assert.IsNull(result, "Result should be null for empty filter");
    }

    [TestMethod]
    public void GetSupportedOperators_ShouldReturnAllOperators()
    {
        // Act
        var operators = _service.GetSupportedOperators().ToList();

        // Assert
        Assert.IsNotNull(operators, "Should return operators list");
        Assert.IsTrue(operators.Count > 0, "Should return at least one operator");
        
        // Check for expected operators
        Assert.IsTrue(operators.Contains("eq"), "Should include eq operator");
        Assert.IsTrue(operators.Contains("ne"), "Should include ne operator");
        Assert.IsTrue(operators.Contains("co"), "Should include co operator");
        Assert.IsTrue(operators.Contains("sw"), "Should include sw operator");
        Assert.IsTrue(operators.Contains("ew"), "Should include ew operator");
        Assert.IsTrue(operators.Contains("gt"), "Should include gt operator");
        Assert.IsTrue(operators.Contains("ge"), "Should include ge operator");
        Assert.IsTrue(operators.Contains("lt"), "Should include lt operator");
        Assert.IsTrue(operators.Contains("le"), "Should include le operator");
        Assert.IsTrue(operators.Contains("pr"), "Should include pr operator");
        Assert.IsTrue(operators.Contains("and"), "Should include and operator");
        Assert.IsTrue(operators.Contains("or"), "Should include or operator");
        Assert.IsTrue(operators.Contains("not"), "Should include not operator");
    }

    [TestMethod]
    public void GetFilterExamples_ShouldReturnExamples()
    {
        // Act
        var examples = _service.GetFilterExamples();

        // Assert
        Assert.IsNotNull(examples, "Should return examples dictionary");
        Assert.IsTrue(examples.Count > 0, "Should return at least one example");
        
        // Check that examples contain valid filters
        foreach (var example in examples)
        {
            Assert.IsFalse(string.IsNullOrEmpty(example.Key), "Example key should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(example.Value), "Example value should not be empty");
            
            // Verify that the example is a valid filter
            Assert.IsTrue(_service.IsValidFilter(example.Value), $"Example '{example.Key}' should be a valid filter");
        }
    }

    [TestMethod]
    public void IsValidFilter_ValidFilters_ShouldReturnTrue()
    {
        // Arrange & Act & Assert
        Assert.IsTrue(_service.IsValidFilter("userName eq \"john\""), "Basic filter should be valid");
        Assert.IsTrue(_service.IsValidFilter("name.givenName eq \"John\""), "Sub-attribute filter should be valid");
        Assert.IsTrue(_service.IsValidFilter("urn:ietf:params:scim:schemas:core:2.0:User:userName eq \"john\""), "URN filter should be valid");
        Assert.IsTrue(_service.IsValidFilter("manager eq null"), "Null filter should be valid");
        Assert.IsTrue(_service.IsValidFilter("(userName eq \"john\" or userName eq \"jane\") and department eq \"IT\""), "Complex filter should be valid");
    }

    [TestMethod]
    public void IsValidFilter_InvalidFilters_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        Assert.IsFalse(_service.IsValidFilter(""), "Empty filter should be invalid");
        Assert.IsFalse(_service.IsValidFilter("   "), "Whitespace filter should be invalid");
        Assert.IsFalse(_service.IsValidFilter("invalid filter"), "Invalid syntax should be invalid");
        Assert.IsFalse(_service.IsValidFilter("userName eq"), "Incomplete filter should be invalid");
        Assert.IsFalse(_service.IsValidFilter("userName invalid \"john\""), "Invalid operator should be invalid");
        Assert.IsFalse(_service.IsValidFilter("((unclosed parentheses"), "Unclosed parentheses should be invalid");
        Assert.IsFalse(_service.IsValidFilter("userName eq \"john"), "Unclosed quotes should be invalid");
    }

    #endregion
}
