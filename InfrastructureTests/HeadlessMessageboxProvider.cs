using System.Collections.Generic;
using Infrastructure;

namespace InfrastructureTests
{
    public class HeadlessMessageboxProvider : IMessageBoxProvider
    {
        private Dictionary<string, string> explicitAnswers = new();

        public IEnumerable<(string, string)> DeliveredAnswers
        {
            get { return _deliveredAnswers; }
        }
        private List<(string, string)> _deliveredAnswers = new();

        public string GetResponse(string question, string caption, params string[] discreteChoices)
        {
			return GetResponseImpl(question);
		}

        public string GetPassword(string question, string caption)
        {
            return GetResponseImpl(question);
        }

        public string GetSuggestedResponse(string question, string caption, string suggestion)
        {
            return GetResponseImpl(question);
        }

        public bool GetBoolResponse(string question, string caption)
        {
            return GetResponseImpl(question) == "Yes";
        }

        private string GetResponseImpl(string question)
        {
            foreach (var answer in explicitAnswers)
            {
                if (question.Contains(answer.Key))
                {
                    _deliveredAnswers.Add((question, answer.Value));
                    return answer.Value;
                }
            }

            return string.Empty;
        }

        public void AnswerFor(string partOfQuestion, string answer)
        {
            explicitAnswers[partOfQuestion] = answer;
        }

		public void Inform(string title, string message)
		{
			
		}
	}
}
