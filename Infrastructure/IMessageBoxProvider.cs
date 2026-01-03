namespace Infrastructure
{
    public interface IMessageBoxProvider
    {
        public void Inform(string title, string message);
        public string GetPassword(string question, string caption);
        public string GetResponse(string question, string caption, params string[] descreteChoices);
        public string GetSuggestedResponse(string question, string caption, string suggestion);
        public bool GetBoolResponse(string question, string caption);
    }

    public static class Show
    {
        public static void Info(string message, string title) => ServiceLocator.Resolve<IMessageBoxProvider>().Inform(title, message);
    }

    public static class Ask
    {
        public static string UserAboutPassword(string question, string caption)
        {
            return ServiceLocator.Resolve<IMessageBoxProvider>().GetPassword(question, caption);
        }

        public static string User(string question, string caption, string suggestion = null)
        {
            if (suggestion != null)
            {
                return ServiceLocator.Resolve<IMessageBoxProvider>().GetSuggestedResponse(question, caption, suggestion);
			}
            else
            {
                return ServiceLocator.Resolve<IMessageBoxProvider>().GetResponse(question, caption);
			}
        }

        public static string UserToChoose(string question, string caption, params string[] descreteChoices)
        {
            return ServiceLocator.Resolve<IMessageBoxProvider>().GetResponse(question, caption, descreteChoices);
        }

		public static bool IfUserWantsToProceed(string caption, string question = "Proceed?")
        {
            return ServiceLocator.Resolve<IMessageBoxProvider>().GetBoolResponse(question, caption);
        }

        /// <summary>
        /// Returns true if we currently have a user to ask questions to.
        /// </summary>
        public static bool IsPossible()
        {
            return ServiceLocator.HasRegistration<IMessageBoxProvider>();
        }
    }
}
