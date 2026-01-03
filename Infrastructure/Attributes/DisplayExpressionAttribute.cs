using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Extensions;

namespace Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DisplayExpressionAttribute : Attribute
    {
        public string FormatExpression { get; init; }
        public string[] PropertyNames { get; init; }

        public DisplayExpressionAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }

        public string GetValue(object o)
        {
            if(o == null || o.GetType().IsValueType) return null;

            if (PropertyNames == null || PropertyNames.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(FormatExpression))
                    return FormatExpression;
                return null;
            }

            var classType = o.GetType();
            var properties = PropertyNames.Select(p => classType.GetProperty(p));

            List<object> propertyValues = properties.Select(property => property.GetValue(o)).ToList();

            List<object> valueResults = propertyValues.Select(GetValueFromProperty).ToList();

            if (string.IsNullOrWhiteSpace(FormatExpression))
                return string.Join(" ", valueResults);

            return string.Format(FormatExpression, valueResults);

        }

        private object GetValueFromProperty(object o)
        {
            if (o == null || o.GetType().IsValueType) return null;

            DisplayExpressionAttribute displayExpressionAttribute = o.GetAttribute<DisplayExpressionAttribute>();
            if (displayExpressionAttribute != null)
            {
                return displayExpressionAttribute.GetValue(o);
            }

            return o;
        }
    }
}
