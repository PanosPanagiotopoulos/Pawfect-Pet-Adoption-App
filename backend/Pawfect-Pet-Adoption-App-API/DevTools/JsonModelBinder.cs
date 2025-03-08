using Microsoft.AspNetCore.Mvc.ModelBinding;

using Newtonsoft.Json;

public class JsonModelBinder<T> : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		if (bindingContext == null)
		{
			throw new ArgumentNullException(nameof(bindingContext));
		}

		// Try to get the value from the value provider (i.e. the form field)
		ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
		if (valueProviderResult == ValueProviderResult.None)
		{
			bindingContext.Result = ModelBindingResult.Failed();
			return Task.CompletedTask;
		}

		bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

		String json = valueProviderResult.FirstValue;
		if (String.IsNullOrEmpty(json))
		{
			bindingContext.Result = ModelBindingResult.Success(null);
			return Task.CompletedTask;
		}

		try
		{
			T result = JsonConvert.DeserializeObject<T>(json);
			bindingContext.Result = ModelBindingResult.Success(result);
		}
		catch (JsonException ex)
		{
			bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
			bindingContext.Result = ModelBindingResult.Failed();
		}

		return Task.CompletedTask;
	}
}
