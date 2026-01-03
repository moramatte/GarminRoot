using Infrastructure;
using Infrastructure.Validation;
using InfrastructureTests.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure.Extensions;

namespace InfrastructureTests
{
    [TestClass]
    public class ValidationTest
    {
        [TestMethod]
        public void StringPropertyCanBeValidated()
        {
            var rule = new ModelHasAName();
            var subject = new Model();

            Assertions.AssertException<ValidationFailedException>(() => rule.Validate(subject), "Name is null");

            subject.Name = "N";
            rule.Validate(subject);
        }

        [TestMethod]
        public void SeveralPropertiesCanBeValidatedTogether()
        {
            var rule = new ModelHasANameThatDescribesItsAge();
            var subject = new Model();

            Assertions.AssertException<ValidationFailedException>(() => rule.Validate(subject), "Name does not describe age");

            subject.Name = "0";
            rule.Validate(subject);
        }

        [TestMethod]
        public void ValidationRulesAreRegisteredGlobally()
        {
            ServiceLocator.Reset();
            ValidationRules.Add(typeof(ModelHasANameThatDescribesItsAge), typeof(ModelHasAName));
           
            var subject = new Model();
            
            Assertions.AssertException<ValidationResultFailedException>(() => subject.Validate(), "2 failed");

            subject.Name = "0";
            subject.Validate();
        }
    }

    public class ModelHasAName : ValidationRule<Model>
    {
        protected override void Validate(Model subject)
        {
            if (subject.Name.IsNullOrEmpty())
                Fail("Name is null");
        }
    }

    public class ModelHasANameThatDescribesItsAge : ValidationRule<Model>
    {
        protected override void Validate(Model subject)
        {
            var ageAsString = subject.Age.ToString();
            if (subject.Name != ageAsString)
                Fail("Name does not describe age");
        }
    }
}
