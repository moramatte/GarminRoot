using System;

namespace Infrastructure.Attributes
{
    public static class Requirements
    {
        public static class AU
        {
            /// <summary>
            /// Indicates that the method implements or tests a requirements from our SubSystemSpecification document
            /// </summary>
            /// <param name="Ids">Requirement Id</param>
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            public class SSSAttribute(params object[] ids) : Attribute
            {
                /// <summary>
                /// Requirement Id
                /// </summary>
                public object[] Ids { get; init; } = ids;
            }

            [AttributeUsage(AttributeTargets.Method)]
            public class TMDS_PDRAttribute(string Id) : Attribute
            {
                public string Id { get; init; } = Id;
            }

        }
    }
}
