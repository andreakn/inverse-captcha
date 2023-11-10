using System.Text.RegularExpressions;

namespace InverseCaptcha.Core;

public class Question
{
    public Question(string questionText, List<string> humanAnswers, List<AnswerCategory> answerCategories, int requiredCategoriesToPass)
    {
        QuestionText = questionText;
        AnswerCategories = answerCategories;
        RequiredCategoriesToPass = requiredCategoriesToPass;
        HumanAnswers = humanAnswers;
    }


    protected Question() { }
    public string QuestionText { get; protected set; }
    public List<string> HumanAnswers { get; set; }

    public List<AnswerCategory> AnswerCategories { get; protected set;}

    public int RequiredCategoriesToPass { get; protected set;}

    public bool HasBeenCleared => AnswerCategories.Count(a => a.HasBeenAnswered) >= RequiredCategoriesToPass;
    public bool IsCurrent { get; set; }

    public bool Answer(string answer)
    {
        return AnswerCategories.Any(category => category.Answer(answer));
    }

    public AnswerResult TryAnswer(string inputAnswer)
    {
        if(HumanAnswers.Any(h=>h.Equals(inputAnswer, StringComparison.InvariantCultureIgnoreCase)))
        {
            return AnswerResult.Boom;
        }

        if (AnswerCategories.Any(c => c.Answer(inputAnswer)))
        {
            return AnswerResult.Done;            
        }
        return AnswerResult.Boom;
    }
}
public enum AnswerResult
{
    Unknown,
    Done,
    Boom,
}
public class AnswerCategory
{
    public string Description { get; }
    public string[] Answers { get; }
    
    public string[] Patterns { get; }
    public bool HasBeenAnswered { get; private set; }

    public AnswerCategory(string description, string[]? answers, string[]? patterns = null )
    {
        Answers = answers ?? Array.Empty<string>();
        Patterns = patterns ?? Array.Empty<string>();
        Description = description;
        HasBeenAnswered = false;
    }

    public bool Answer(string incomingAnswer)
    {
        if (HasBeenAnswered)
        {
            return false;
        }

        if (Answers.Any(answer => answer.Equals(incomingAnswer, StringComparison.InvariantCultureIgnoreCase)))
        {
            HasBeenAnswered = true;
            return true;
        }

        if (Patterns.Any(p => Regex.IsMatch(incomingAnswer, p)))
        {
            HasBeenAnswered = true;
            return true;
        }

        return false;
    }
};