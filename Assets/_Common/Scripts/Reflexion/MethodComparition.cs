using Com.GitHub.Knose1.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Com.GitHub.Knose1.Common.Reflexion
{
	public struct MethodComparition
	{
		static readonly Type VOID_TYPE = typeof(void);

		private ParameterInfo[] methodParameters;
		private ParameterInfo[] modelParameters;

		public readonly MethodInfo method, model;
		public readonly object[] sendedParameters;
		public readonly ReturnComparition returnComparition;
		public readonly ParameterComparition[] parameterComparitions;

		public bool IsParametterMissingOrExtra()
		{
			return parameterComparitions.All((p) => !p.IsMissingOrExtra());
		}

		public bool IsCompatible()
		{
			var sendedParameters = this.sendedParameters;

			bool returnCompatible = returnComparition.IsCompatible();
			bool parametersCompatible = parameterComparitions.All((comparition) => comparition.IsCompatible(sendedParameters != null));

			return returnCompatible && parametersCompatible;
		}

		public object[] GetValues() => parameterComparitions.Map((p) => p.GetValue()).ToArray();
		public object Call(object target, object[] sendedParameters) => method.Invoke(target, sendedParameters);
		public object Call(object target) => method.Invoke(target, GetValues());

		public MethodComparition(MethodInfo method, MethodInfo model, object[] sendedParameters = null)
		{
			this.method = method;
			this.model = model;
			this.sendedParameters = sendedParameters;

			methodParameters = method.GetParameters();
			modelParameters = model.GetParameters();

			int count = Math.Max(methodParameters.Length, modelParameters.Length);
			parameterComparitions = new ParameterComparition[count];

			returnComparition = new ReturnComparition(method.ReturnType, model.ReturnType);

			for (int i = 0; i < count; i++)
			{
				parameterComparitions[i] = new ParameterComparition(methodParameters.ElementAtOrDefault(i), modelParameters.ElementAtOrDefault(i), sendedParameters.ElementAtOrDefault(i));
			}

		}

		public struct ReturnComparition
		{
			public readonly Type method, model;

			public ReturnComparition(Type method, Type model)
			{
				this.method = method;
				this.model = model;
			}

			/// <summary>
			/// Return true if A can be called like B
			/// </summary>
			public bool IsCompatible()
			{
				if (model == VOID_TYPE)
					return method == VOID_TYPE;

				return method == model || method.IsSubclassOf(model);
			}
		}

		public struct ParameterComparition
		{
			public ParameterInfo method, model;
			public object sendedParameter;
			public Type sendedParameterType;

			public ParameterComparition(ParameterInfo method, ParameterInfo model, object sendedParameter) : this()
			{
				this.method = method;
				this.model = model;
				this.sendedParameter = sendedParameter;
				this.sendedParameterType = sendedParameter is null ? typeof(void) : sendedParameter.GetType();
			}

			public bool IsOut => method.IsOut || model.IsOut;
			public bool IsIn => method.IsIn || model.IsIn;
			public bool IsRef => method.IsRef() || model.IsRef();

			public object GetValue() => sendedParameter ?? 
				(model.IsOptional ? model.DefaultValue : 
						method.IsOptional ? method.DefaultValue : null
				);

			internal bool IsMissingOrExtra() => !IsOptional() && method == null && model == null;

			/// <summary>
			/// True if less arguments are declared by <see cref="method"/>
			/// </summary>
			/// <returns></returns>
			public bool IsMissing() => !IsOptional() && method == null;

			/// <summary>
			/// True if more arguments are declared by <see cref="method"/>
			/// </summary>
			/// <returns></returns>
			public bool IsExtra() => !IsOptional() && model == null;

			public bool IsOptional()
			{
				if (model is null && method is null)
				{
					UnityEngine.Debug.LogWarning(new Exception($"{nameof(model)} and {nameof(method)} are null"));
					return true;
				}

				//Optional if one is null and the other is optional
				if (model is null && method.IsOptional) return true;
				if (method is null && model.IsOptional) return true;
				return false;
			}

			/// <summary>
			/// Return true if A can be called like B
			/// </summary>
			public bool IsCompatible(bool checkSendedParameters = true) => IsCompatible(out _, out _, out _, checkSendedParameters);
			/// <summary>
			/// Return true if A can be called like B
			/// </summary>
			public bool IsCompatible(out bool methodSubOfModel, out bool parameterSubOfMethod, out bool parameterSubOfModel, bool checkSendedParameters = true)
			{
				Type methodParameterType = method.ParameterType;
				Type modelParameterType = method.ParameterType;

				if (IsOptional())
					return methodSubOfModel = parameterSubOfMethod = parameterSubOfModel = true;

				if (IsMissingOrExtra())
					return methodSubOfModel = parameterSubOfMethod = parameterSubOfModel = false;

				// Check if class are equals or if it's a subClass
				//		The Method's parameter must be compatible with the Model's parameter
				//		The Method's parameter must be compatible with the Parameter
				//		The Parameter must be compatible with the Model's parameter
				methodSubOfModel		=							methodParameterType == modelParameterType	|| methodParameterType.IsSubclassOf(modelParameterType);
				parameterSubOfMethod	= !checkSendedParameters || methodParameterType == sendedParameterType	|| sendedParameterType.IsSubclassOf(methodParameterType);
				parameterSubOfModel		= !checkSendedParameters || sendedParameterType == modelParameterType	|| sendedParameterType.IsSubclassOf(modelParameterType);

				return
					//They must be out or not out
					method.IsOut == model.IsOut &&
					//They must be in or not in
					method.IsIn == model.IsIn &&
					//They must be ref or not ref
					method.IsRef() == model.IsRef() &&
					
					methodSubOfModel && parameterSubOfMethod && parameterSubOfModel;
			}
		}
	}
}
