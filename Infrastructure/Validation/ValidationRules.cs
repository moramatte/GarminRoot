using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infrastructure.Extensions;

namespace Infrastructure.Validation
{
    public static class ValidationRules
    {
        public static void AddFromThisAssemby()
        {
            var assembly = Assembly.GetCallingAssembly();
            AddFromAssembly(assembly);
        }

        public static void Validate(this object subject)
        {
            var validator = ServiceLocator.Resolve<Validator>();
            validator.Validate(subject);
        }

        public static void AddFromAssembly(Assembly assembly)
        {
            var ruleTypes = assembly.GetTypes()
                .Where(t => ReflectionExtensions.DerivesFrom(t, typeof(IValidationRule)));

            AddRuleTypes(ruleTypes);
        }

        private static void AddRuleTypes(IEnumerable<Type> ruleTypes)
        {
            var validator = ServiceLocator.Resolve<Validator>();
            foreach (var ruleType in ruleTypes)
            {
                var r = Activator.CreateInstance(ruleType);
                validator.Include(r as IValidationRule);
            }
        }

        public static void Add(params Type[] types)
        {
            AddRuleTypes(types);
        }
    }
}