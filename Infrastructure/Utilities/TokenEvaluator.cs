using Infrastructure.Logger;
using Newtonsoft.Json.Linq;
using TokenEvaluator.Net;
using TokenEvaluator.Net.Models;

namespace Infrastructure.Utilities
{
    public static class TokenEvaluator
    {
        public static int Evaluate(string str)
        {
            var evaluator = TokenEvaluatorClientFactory.Create();
            evaluator.SetDefaultTokenEncoding(EncodingType.Cl100kBase);
            return evaluator.EncodedTokenCount(str);
        }

        public static string Reduce(string str, int tokenLimit)
        {
            var tokens = Evaluate(str);
            while (tokens > tokenLimit)
            {
                Log.Warning(typeof(TokenEvaluator), $"Log text exceeds token limit ({tokenLimit}): {tokens} tokens. Truncating to fit.");
                str = str.Substring(0, (int)(str.Length * 0.8));
                tokens = Evaluate(str);
            }
            return str;
        }
    }
}
