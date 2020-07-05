﻿namespace AspNet.Mvc.TypedRouting.Test.LinkGeneration
{
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.ApplicationModels;
	using Microsoft.AspNetCore.Mvc.ModelBinding;
	using Microsoft.AspNetCore.Mvc.Routing;
	using Microsoft.AspNetCore.Mvc.Testing;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using TypedRouting.LinkGeneration;
	using Xunit;

	using With = Microsoft.AspNetCore.Mvc.With;

	public class ExpressionRouteHelperTest : ICollectionFixture<WebApplicationFactory<TestStartup>>
	{
		private readonly WebApplicationFactory<TestStartup> _factory;

		public ExpressionRouteHelperTest(WebApplicationFactory<TestStartup> factory)
		{
			_factory = factory;
		}

		[Fact]
		public void Resolve_AddTypedRoutingShouldAddExpressionRouteHelper()
		{
			var serviceCollection = new ServiceCollection();

			serviceCollection.AddMvc().AddTypedRouting();

			var serviceProvider = serviceCollection.BuildServiceProvider();

			var expressionRouteHelper = serviceProvider.GetRequiredService<IExpressionRouteHelper>();
			Assert.IsAssignableFrom<ExpressionRouteHelper>(expressionRouteHelper);
		}

		[Theory]
		[MemberData(nameof(UsualActionsWithNoParametersData))]
		public void Resolve_ControllerAndActionWithoutParameters_ControllerAndActionNameAreResolved(
			Expression<Action<UsualController>> action, string controllerName, string actionName)
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve(action);

			// Assert
			Assert.Equal(controllerName, result.Controller);
			Assert.Equal(actionName, result.Action);
			Assert.Empty(result.RouteValues);
		}

		[Theory]
		[MemberData(nameof(UsualActionssWithParametersData))]
		public void Resolve_ControllerAndActionWithPrimitiveParameters_ControllerActionNameAndParametersAreResolved(
			Expression<Action<UsualController>> action, string controllerName, string actionName, IDictionary<string, object> routeValues)
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve(action);

			// Assert
			Assert.Equal(controllerName, result.Controller);
			Assert.Equal(actionName, result.Action);
			Assert.Equal(routeValues.Count, result.RouteValues.Count);

			foreach (var routeValue in routeValues)
			{
				Assert.True(result.RouteValues.ContainsKey(routeValue.Key));
				Assert.Equal(routeValue.Value, result.RouteValues[routeValue.Key]);
			}
		}

		[Fact]
		public void Resolve_ControllerAndActionWithObjectParameters_ControllerActionNameAndParametersAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<UsualController>(c => c.ActionWithMultipleParameters(1, "string", new RequestModel { Integer = 1, String = "Text" }));

			// Assert
			Assert.Equal("Usual", result.Controller);
			Assert.Equal("ActionWithMultipleParameters", result.Action);
			Assert.Equal(3, result.RouteValues.Count);
			Assert.Equal(1, result.RouteValues["id"]);
			Assert.Equal("string", result.RouteValues["text"]);
			Assert.IsAssignableFrom<RequestModel>(result.RouteValues["model"]);

			var model = (RequestModel)result.RouteValues["model"];
			Assert.Equal(1, model.Integer);
			Assert.Equal("Text", model.String);
		}

		[Fact]
		public void Resolve_PocoController_ControllerActionNameAndParametersAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<PocoController>(c => c.Action(1));

			// Assert
			Assert.Equal("Poco", result.Controller);
			Assert.Equal("Action", result.Action);
			Assert.Equal(1, result.RouteValues.Count);
			Assert.True(result.RouteValues.ContainsKey("id"));
			Assert.Equal(1, result.RouteValues["id"]);
		}

		[Fact]
		public void Resolve_PocoController_ControllerAsyncActionNameAndParametersAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<PocoController>(c => c.ActionAsync(1));

			// Assert
			Assert.Equal("Poco", result.Controller);
			Assert.Equal("ActionAsync", result.Action);
			Assert.Equal(1, result.RouteValues.Count);
			Assert.True(result.RouteValues.ContainsKey("id"));
			Assert.Equal(1, result.RouteValues["id"]);
		}

		[Fact]
		public void Resolve_InAreaController_ControllerActionNameAndAreaAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<InAreaController>(c => c.Action(1));

			// Assert
			Assert.Equal("InArea", result.Controller);
			Assert.Equal("Action", result.Action);
			Assert.Equal(2, result.RouteValues.Count);
			Assert.True(result.RouteValues.ContainsKey("id"));
			Assert.Equal(1, result.RouteValues["id"]);
			Assert.True(result.RouteValues.ContainsKey("area"));
			Assert.Equal("MyArea", result.RouteValues["area"]);
		}

		[Fact]
		public void Resolve_ActionWithCustomRouteConstraints_RouteConstraintsAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<RouteConstraintController>(c => c.Action(1, 2));

			// Assert
			Assert.Equal("CustomController", result.Controller);
			Assert.Equal("CustomAction", result.Action);
			Assert.Equal(3, result.RouteValues.Count);
			Assert.True(result.RouteValues.ContainsKey("id"));
			Assert.Equal("5", result.RouteValues["id"]);
			Assert.True(result.RouteValues.ContainsKey("key"));
			Assert.Equal("value", result.RouteValues["key"]);
			Assert.True(result.RouteValues.ContainsKey("anotherId"));
			Assert.Equal(2, result.RouteValues["anotherId"]);
		}

		[Fact]
		public void Resolve_CustomConventions_CustomConventionsAreResolved()
		{
			// Act
			var result = TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<ConventionsController>(c => c.ConventionsAction(1));

			// Assert
			Assert.Equal("ChangedController", result.Controller);
			Assert.Equal("ChangedAction", result.Action);
			Assert.Equal(1, result.RouteValues.Count);
			Assert.True(result.RouteValues.ContainsKey("ChangedParameter"));
			Assert.Equal(1, result.RouteValues["ChangedParameter"]);
		}

		[Fact]
		public void Resolve_StaticMethodCall_ThrowsInvalidOperationException()
		{
			// Act
			var exception = Assert.Throws<InvalidOperationException>(() =>
			{
				TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<UsualController>(c => UsualController.StaticCall());
			});

			// Assert
			Assert.Equal("Expression is not valid - expected instance method call but instead received static method call.", exception.Message);
		}

		[Fact]
		public void Resolve_NonMethodCallException_ThrowsInvalidOperationException()
		{
			// Act
			var exception = Assert.Throws<InvalidOperationException>(() =>
			{
				TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<UsualController>(c => new object());
			});

			// Assert
			Assert.Equal("Expression is not valid - expected instance method call but instead received other type of expression.", exception.Message);
		}

		[Fact]
		public void Resolve_NonControllerAction_ThrowsInvalidOperationException()
		{
			// Act
			var exception = Assert.Throws<InvalidOperationException>(() =>
			{
				TestServices.Global.GetService<IExpressionRouteHelper>().Resolve<RequestModel>(c => c.SomeMethod());
			});

			// Assert
			Assert.Equal("Method SomeMethod in class RequestModel is not a valid controller action.", exception.Message);
		}

		public static TheoryData<Expression<Action<UsualController>>, string, string> UsualActionsWithNoParametersData
		{
			get
			{
				var data = new TheoryData<Expression<Action<UsualController>>, string, string>();

				const string controllerName = "Usual";
				data.Add(c => c.ActionWithoutParameters(), controllerName, "ActionWithoutParameters");
				data.Add(c => c.ActionWithOverloads(), controllerName, "ActionWithOverloads");
				data.Add(c => c.VoidAction(), controllerName, "VoidAction");
				data.Add(c => c.ActionWithChangedName(), controllerName, "AnotherName");

				return data;
			}
		}

		public static TheoryData<
			Expression<Action<UsualController>>,
			string,
			string,
			IDictionary<string, object>> UsualActionssWithParametersData
		{
			get
			{
				var data = new TheoryData<Expression<Action<UsualController>>, string, string, IDictionary<string, object>>();

				const string controllerName = "Usual";
				data.Add(
					c => c.ActionWithOverloads(1),
					controllerName,
					"ActionWithOverloads",
					new Dictionary<string, object> { ["id"] = 1 });

				data.Add(
					c => c.ActionWithOverloads(With.No<int>()),
					controllerName,
					"ActionWithOverloads",
					new Dictionary<string, object>());

				data.Add(
					c => c.ActionWithOverloads(GetInt()),
					controllerName,
					"ActionWithOverloads",
					new Dictionary<string, object> { ["id"] = 1 });

				data.Add(
					c => c.ActionWithMultipleParameters(1, "string", null),
					controllerName,
					"ActionWithMultipleParameters",
					new Dictionary<string, object> { ["id"] = 1, ["text"] = "string" });

				return data;
			}
		}

		private static int GetInt()
		{
			return 1;
		}
	}

	public class RequestModel
	{
		public int Integer { get; set; }

		public string String { get; set; }

		public void SomeMethod()
		{
		}
	}

	public class UsualController : Controller
	{
		public static void StaticCall()
		{
		}

		public IActionResult ActionWithoutParameters()
		{
			return null;
		}

		public IActionResult ActionWithMultipleParameters(int id, string text, RequestModel model)
		{
			return null;
		}

		public IActionResult ActionWithOverloads()
		{
			return null;
		}

		public IActionResult ActionWithOverloads(int id)
		{
			return null;
		}

		[ActionName("AnotherName")]
		public IActionResult ActionWithChangedName()
		{
			return null;
		}

		public void VoidAction()
		{
		}
	}

	public class PocoController
	{
		public IActionResult Action(int id)
		{
			return null;
		}

		public async Task<IActionResult> ActionAsync(int id)
		{
			return await Task.FromResult(new EmptyResult());
		}
	}

	[Area("MyArea")]
	public class InAreaController
	{
		public IActionResult Action(int id)
		{
			return null;
		}
	}

	[MyRouteConstraint("controller", "CustomController")]
	public class RouteConstraintController
	{
		[MyRouteConstraint("action", "CustomAction")]
		[MyRouteConstraint("id", "5")]
		[MyRouteConstraint("key", "value")]
		public IActionResult Action(int id, int anotherId)
		{
			return null;
		}
	}

	[CustomControllerConvention]
	public class ConventionsController
	{
		[CustomActionConvention]
		public IActionResult ConventionsAction([CustomParameterConvention] int id)
		{
			return null;
		}
	}

	public class CustomControllerConventionAttribute : Attribute, IControllerModelConvention
	{
		public void Apply(ControllerModel controller)
		{
			controller.ControllerName = "ChangedController";
		}
	}

	public class CustomActionConventionAttribute : Attribute, IActionModelConvention
	{
		public void Apply(ActionModel action)
		{
			action.ActionName = "ChangedAction";
		}
	}

	public class CustomParameterConventionAttribute : Attribute, IParameterModelConvention
	{
		public void Apply(ParameterModel parameter)
		{
			parameter.BindingInfo = parameter.BindingInfo ?? new BindingInfo();
			parameter.BindingInfo.BinderModelName = "ChangedParameter";
		}
	}

	public class MyRouteConstraintAttribute : RouteValueAttribute
	{
		public MyRouteConstraintAttribute(string routeKey, string routeValue)
			: base(routeKey, routeValue)
		{
		}
	}
}
