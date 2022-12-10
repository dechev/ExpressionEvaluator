using FastExpressionCompiler;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace ExpressionEvaluator;

/// <summary>
/// Extensions for compiling and dynamically evaluating to Boolean string expressions.
/// </summary>
public static class BooleanExpressionExtensions
{
    private static readonly ConcurrentDictionary<string, string[]> _detectedIdentifiersCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Compiles and evaluates to <see cref="Boolean"/> a <see cref="String"/> <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="String"/> expression to evaluate.</param>
    /// <param name="parameters">The <see cref="Expression"/> arguments.</param>
    /// <returns>The dynamically evaluated (late-bound) <see cref="Expression"/> result.</returns>
    public static bool CompileAndEvaluateBooleanExpression(this string expression, Dictionary<string, object?> parameters)
    {
        var identifiers = expression.DetectIdentifiers();
        var filteredInput = identifiers.Distinct().ToDictionary(x => x, x => parameters.TryGetValue(x, out var v) ? v : null, StringComparer.Ordinal);
        return expression
            .CompileBooleanExpression(filteredInput)
            .EvaluateCompiledBooleanExpression(filteredInput);
    }

    /// <summary>
    /// Builds and compiles a <see cref="Boolean"/> <see cref="String"/> <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="String"/> <see cref="Expression"/> to build and compile.</param>
    /// <param name="parameters">The <see cref="Expression"/> arguments.</param>
    /// <returns>The compiled <see cref="Delegate"/>.</returns>
    public static Delegate CompileBooleanExpression(this string expression, Dictionary<string, object?> parameters)
    {
        var parameterExpressions = new List<ParameterExpression>();
        foreach (var val in parameters)
        {
            parameterExpressions.Add(Expression.Parameter(val.Value?.GetType() ?? typeof(object), val.Key));
        }

        Type tResult = typeof(bool);

        var e = DynamicExpressionParser.ParseLambda(parameterExpressions.ToArray(), tResult, expression, null);

        return e.CompileFast();
    }

    /// <summary>
    /// Dynamically invokes (late-bound) the method represented by a compiled <see cref="Boolean"/> <see cref="Expression"/> <see cref="Delegate"/>.
    /// </summary>
    /// <param name="compiledDelegate">The compiled <see cref="Delegate"/>.</param>
    /// <param name="parameters">The arguments to pass to the method represented by the <paramref name="compiledDelegate"/>.</param>
    /// <returns>The object returned by the method represented by the <paramref name="compiledDelegate"/> cast to <see cref="Boolean"/>.</returns>
    public static bool EvaluateCompiledBooleanExpression(this Delegate compiledDelegate, Dictionary<string, object?> parameters) =>
        (bool)(compiledDelegate.DynamicInvoke(parameters.Values.Select(x => x).ToArray()) ?? false);

    /// <summary>
    /// Detects the identifiers used in the expression.
    /// </summary>
    /// <param name="expression">The extression.</param>
    /// <returns>The detected identifiers in the expression.</returns>
    public static string[] DetectIdentifiers(this string expression)
    {
        if (!_detectedIdentifiersCache.TryGetValue(expression, out var result))
        {
            result = new DynamicExpresso.Interpreter().DetectIdentifiers(expression).UnknownIdentifiers.ToArray();
            _detectedIdentifiersCache.AddOrUpdate(expression, result, (_, _) => result);
        }
        return result;
    }
}