using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Validation
{
    public class ValidationFailedException : Exception
    {
        public ValidationFailedException(string reason): base(reason){}
        public IValidationRule Rule { get; init; }
    }

    public class ValidationResultFailedException : Exception
    {
        public ValidationResultFailedException(string summary) : base(summary) { }
        public List<ValidationFailedException> Errors { get; init; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var failure in Errors)
            {
                sb.AppendLine($"\t{failure.Rule.GetType().Name}: {failure.Message}");
            }
            return $"{Message}. Failed validations: {Errors.Count}: {Environment.NewLine}\t{sb.ToString()}";
        }
    }
}