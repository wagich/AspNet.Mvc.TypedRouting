namespace AspNet.Mvc.TypedRouting.Test.LinkGeneration
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Text.Encodings.Web;
    using Xunit;

    using With = Microsoft.AspNetCore.Mvc.With;
    using TypedRouting.LinkGeneration;
	using Microsoft.AspNetCore.Mvc.Testing;
	using System.Runtime.InteropServices.ComTypes;
	using Microsoft.Extensions.DependencyInjection;

	[Collection("TypedRoutingTests")]
    public class UrlHelperExtensionsTest : ICollectionFixture<WebApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _factory;

        public UrlHelperExtensionsTest(WebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void UrlActionWithExpressionAndAllParameters_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action<NormalController>(c => c.ActionWithoutParameters(),
                values: null,
                protocol: "https",
                host: "remotelyhost",
                fragment: "somefragment");

            // Assert
            Assert.Equal("https://remotelyhost/app/Normal/ActionWithoutParameters#somefragment", url);
        }

        [Fact]
        public void UrlActionWithExpressionActionWithParameters_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action<NormalController>(c => c.ActionWithParameters(1, "sometext"));

            // Assert
            Assert.Equal("/app/Normal/ActionWithParameters/1?text=sometext", url);
        }

        [Fact]
        public void UrlActionWithExpressionActionWithParametersAndAdditionalValues_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action<NormalController>(c => c.ActionWithParameters(1, "sometext"), new { text = "othertext" });

            // Assert
            Assert.Equal("/app/Normal/ActionWithParameters/1?text=othertext", url);
        }

        [Fact]
        public void UrlActionWithExpressionActionWithNoParameterssAndAdditionalValues_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action<NormalController>(c => c.ActionWithParameters(With.No<int>(), With.No<string>()), new { id = 1, text = "othertext" });

            // Assert
            Assert.Equal("/app/Normal/ActionWithParameters/1?text=othertext", url);
        }

        [Fact]
        public void LinkWithAllParameters_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Link<NormalController>("namedroute", c => c.ActionWithParameters(1, "sometext"));

            // Assert
            Assert.Equal("http://localhost/app/named/Normal/ActionWithParameters/1?text=sometext", url);
        }

        [Fact]
        public void LinkWithNullRouteName_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Link<NormalController>(null, c => c.ActionWithParameters(1, "sometext"));

            // Assert
            Assert.Equal("http://localhost/app/Normal/ActionWithParameters/1?text=sometext", url);
        }

        [Fact]
        public void LinkWithAdditionalRouteValues_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Link<NormalController>(null, c => c.ActionWithParameters(1, "sometext"), new { text = "othertext" });

            // Assert
            Assert.Equal("http://localhost/app/Normal/ActionWithParameters/1?text=othertext", url);
        }

        [Fact]
        public void NormalControllerToAreaController_GeneratesCorrectLink()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");
            var controller = new NormalController { Url = urlHelper };

            // Act
            var result = controller.ToAreaAction() as ContentResult;

            // Assert
            Assert.Equal("/app/Admin/Area/ToEmptyAreaAction", result.Content);
        }
        
        [Fact]
        public void AreaControllerToNormalController_GeneratesCorrectLink()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");
            var controller = new AreaController { Url = urlHelper };

            // Act
            var result = controller.ToEmptyAreaAction() as ContentResult;

            // Assert
            Assert.Equal("/app/Normal/ActionWithoutParameters", result.Content);
        }

        [Fact]
        public void AreaControllerToAreaController_GeneratesCorrectLink()
        {
            // Arrange
            var controller = _factory.Services.GetService<AreaController>();

            // Act
            var result = controller.ToOtherAreaAction() as ContentResult;

            // Assert
            Assert.Equal("/app/Support/AnotherArea", result.Content);
        }
    }
    
    public class NormalController : Controller
    {
        public IActionResult ActionWithoutParameters()
        {
            return null;
        }

        public IActionResult ActionWithParameters(int id, string text)
        {
            return null;
        }

        public IActionResult ToAreaAction()
        {
            return Content(Url.Action<AreaController>(c => c.ToEmptyAreaAction()));
        }
    }

    [Area("Admin")]
    public class AreaController : Controller
    {
        public IActionResult ToEmptyAreaAction()
        {
            return Content(Url.Action<NormalController>(c => c.ActionWithoutParameters()));
        }

        public IActionResult ToOtherAreaAction()
        {
            return Content(Url.Action<AnotherAreaController>(c => c.Index()));
        }
    }

    [Area("Support")]
    public class AnotherAreaController : Controller
    {
        public IActionResult Index()
        {
            return null;
        }
    }

    internal class LoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger();
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }

    internal class Logger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }
    }

    internal class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}