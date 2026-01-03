using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Validation
{
    public class Validator
    {
        public IEnumerable<IValidationRule> Rules { get; } = new List<IValidationRule>();

        private List<IValidationRule> RulesList => Rules as List<IValidationRule>;

        public void Include(params IValidationRule[] rules)
        {
            foreach (var rule in rules)
            {
                RulesList.Add(rule);
            }
        }

        public void Validate(object subject)
        {
            var failures = new List<ValidationFailedException>();
            var successes = new List<IValidationRule>();

            foreach (var rule in Rules)
            {
                try
                {
                    var ruleType = rule.GetType();
                    var validatorForType = ruleType.BaseType.GetGenericArguments().First();
                    if (subject.GetType() != validatorForType)
                        continue;
                    rule.Validate(subject);
                    successes.Add(rule);
                }
                catch (ValidationFailedException e)
                {
                    failures.Add(e);
                }
            }

            if (failures.Any())
            {
                throw new ValidationResultFailedException($"Validation failed for {subject}. {failures.Count} failed and {successes.Count} passed.")
                {
                    Errors = failures,
                };
            }
        }
    }
}
