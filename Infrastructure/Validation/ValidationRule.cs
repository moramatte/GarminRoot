namespace Infrastructure.Validation
{
    public interface IValidationRule
    {
        void Validate(object subject);
    }

    public class ValidationRule<T> : IValidationRule
    {
        public virtual void Validate(object subject){ Validate((T) subject); }

        protected virtual void Validate(T subject) { }

        public virtual void Fail(string reason)
        {
            var exception = new ValidationFailedException(reason)
            {
                Rule = this,
            };
            throw exception;
        }
    }
}
