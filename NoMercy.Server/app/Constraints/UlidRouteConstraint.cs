using System.Globalization;

namespace NoMercy.Server.app.Constraints;

public class UlidRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var routeValue)) return false;
        
        var parameterValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);
        return Ulid.TryParse(parameterValueString, out _);

    }
}