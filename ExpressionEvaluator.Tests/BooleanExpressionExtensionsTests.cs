namespace ExpressionEvaluator.Tests;

[TestClass]
public class BooleanExpressionExtensionsTests
{
    [TestMethod]
    [DataRow(true, "1 == 1", new string[0], new object?[0])]
    [DataRow(true, "1 == x", new string[] { "x" }, new object?[] { 1 })]
    [DataRow(false, "1 == x", new string[] { "x" }, new object?[] { 2 })]
    [DataRow(true, "1 == x", new string[] { "x", "y" }, new object?[] { 1, 3 })]
    [DataRow(false, "1 == x", new string[] { "x", "y" }, new object?[] { 2, "irrelevant" })]
    public void TestMethod1(bool expected, string expression, string[] parameterNames, object?[] parameterValues)
    {
        var parameters = new Dictionary<string, object?>();
        for (int i = 0; i < parameterNames.Length && i < parameterValues.Length; i++)
        {
            parameters[parameterNames[i]] = parameterValues[i];
        }
        var actual = expression.CompileAndEvaluateBooleanExpression(parameters);
        Assert.AreEqual(expected, actual);
    }
}