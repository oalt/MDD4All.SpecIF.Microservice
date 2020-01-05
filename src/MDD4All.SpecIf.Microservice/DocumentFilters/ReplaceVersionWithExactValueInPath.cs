using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace MDD4All.SpecIf.Microservice.DocumentFilters
{
    public class ReplaceVersionWithExactValueInPath : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Paths = swaggerDoc.Paths
                .ToDictionary(
                    path => path.Key.Replace("v{version}", swaggerDoc.Info.Version),
                    path => path.Value
                );
        }
    }
}
