using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNet.Mvc.TypedRouting
{
    public interface IQueryValueProvider
    {
		RouteValueDictionary GetRouteValues();
    }
}
