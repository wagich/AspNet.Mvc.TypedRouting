using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNet.Mvc.TypedRouting.LinkGeneration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNet.Mvc.TypedRouting.Test
{
	public class TestStartup
	{
		public TestStartup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IUniqueRouteKeysProvider, UniqueRouteKeysProvider>();
			services.AddSingleton<IExpressionRouteHelper, ExpressionRouteHelper>();

			services
				.AddControllersWithViews()
				.AddApplicationPart(typeof(TestStartup).Assembly);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				
				endpoints.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}");
				endpoints.MapControllerRoute(string.Empty, "{controller}/{action}/{id}", new RouteValueDictionary(new { id = "defaultid" }));
				endpoints.MapControllerRoute("namedroute", "named/{controller}/{action}/{id}", new RouteValueDictionary(new { id = "defaultid" }));
			});
		}
	}
}
