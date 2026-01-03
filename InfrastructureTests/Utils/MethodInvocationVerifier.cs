using System.Linq;

namespace InfrastructureTests
{
    public class InvokeResult
    {
        public InvokeResult() { }
        public InvokeResult(string error) { Error = error; }
        public bool Correct => string.IsNullOrWhiteSpace(Error);
        public string Error { get; private set; }
    }

    public class MethodInvocationVerifier
    {
        public static InvokeResult IsInvokedCorrectly<T>(string methodName, params object[] parameters)
        {
            var methods = typeof(T).GetMethods().ToList();
            var method = methods.First(x => x.Name == methodName && x.GetParameters().Length == parameters.Length);
            if (method == null)
            {
                return new InvokeResult($"Invoked method:'{methodName}' with parameter counter {parameters.Length} does not exist in '{nameof(T)}'");
            }

            var methodParameters = method.GetParameters().Select(x => x.ParameterType).ToList();
            var givenParameters = parameters.Select(x => x.GetType()).ToList();
            for (int i = 0; i < methodParameters.Count; i++)
            {
                if (methodParameters[i] != givenParameters[i] && !givenParameters[i].IsSubclassOf(methodParameters[i]))
                {
                    return new InvokeResult($"Parameter missmatch in invoke for method {method.Name}");
                }
            }

            return new InvokeResult();
        }
    }
}
