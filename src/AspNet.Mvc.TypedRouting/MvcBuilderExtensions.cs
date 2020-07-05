using System;
using AspNet.Mvc.TypedRouting.LinkGeneration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
	public static class MvcBuilderExtensions
	{
		/// <summary>
		/// Adds typed expression based routes in ASP.NET Core MVC application.
		/// </summary>
		/// <param name="routesConfiguration">Typed routes configuration.</param>
		public static IMvcBuilder AddTypedLinkGeneration(this IMvcBuilder mvcBuilder)
		{
			var uniqueRouteKeysProvider = new UniqueRouteKeysProvider();

			var services = mvcBuilder.Services;

			services.AddSingleton<IUniqueRouteKeysProvider>(uniqueRouteKeysProvider);
			services.AddSingleton<IExpressionRouteHelper, ExpressionRouteHelper>();

			services.Configure<MvcOptions>(options =>
			{
				options.Conventions.Add(new LinkGenerationControllerModelConvention(uniqueRouteKeysProvider));
			});

			return mvcBuilder;
		}
	}
}
