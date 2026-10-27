using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Nclex.Api.ModelBinding;

/// <summary>
/// Reads enums in query strings by their API names (api.md: snake_case, e.g.
/// <c>itemTypes=multiple_choice</c>), the same names JSON bodies use. MVC's own binder only knows
/// the C# names, so "multiple_choice" would fail while "sata" happened to work.
/// </summary>
public sealed class SnakeCaseEnumModelBinder(Type enumType) : IModelBinder
{
    private readonly Dictionary<string, object> _valuesByApiName = Enum.GetValues(enumType)
        .Cast<object>()
        .ToDictionary(value => ApiName(enumType, value), value => value, StringComparer.OrdinalIgnoreCase);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None || string.IsNullOrEmpty(result.FirstValue))
        {
            return Task.CompletedTask; // not given: the property keeps its default (null)
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        if (_valuesByApiName.TryGetValue(result.FirstValue, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"'{result.FirstValue}' is not a valid value.");
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }

    // The name System.Text.Json writes: an explicit [JsonStringEnumMemberName] (e.g. "RN"), else snake_case.
    private static string ApiName(Type enumType, object value)
    {
        var name = value.ToString()!;
        var explicitName = enumType.GetField(name)?.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
        return explicitName ?? JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
    }
}

/// <summary>Uses <see cref="SnakeCaseEnumModelBinder"/> for every enum (and nullable enum) bound from the query string.</summary>
public sealed class SnakeCaseEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type.IsEnum ? new SnakeCaseEnumModelBinder(type) : null;
    }
}
