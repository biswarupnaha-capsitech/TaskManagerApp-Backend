using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class XEnumNamesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum)
        {
            var enumNames = Enum.GetNames(type);

            var enumArray = new OpenApiArray();
            foreach (var name in enumNames)
            {
                enumArray.Add(new OpenApiString(name));
            }

            schema.Extensions.Add("x-enumNames", enumArray);
        }
    }
}