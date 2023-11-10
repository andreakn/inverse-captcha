namespace InverseCaptcha.Core;

public class Question
{
    public Question(string questionText, List<AnswerCategory> answerCategories, int requiredCategoriesToPass)
    {
        QuestionText = questionText;
        AnswerCategories = answerCategories;
        RequiredCategoriesToPass = requiredCategoriesToPass;
    }

    public string QuestionText { get; }

    public List<AnswerCategory> AnswerCategories { get; }

    public int RequiredCategoriesToPass { get; }

    public bool HasBeenCleared => AnswerCategories.Count(a => a.HasBeenAnswered) >= RequiredCategoriesToPass;
    public bool IsCurrent { get; set; }

    public bool Answer(string answer)
    {
        return AnswerCategories.Any(category => category.Answer(answer));
    }

    public AnswerResult TryAnswer(string inputAnswer)
    {
        return AnswerResult.Done;
    }
}
public enum AnswerResult
{
    Unknown,
    Done,
    Boom,
    NeedMoreCaptcha
}
public class AnswerCategory
{
    public string Description { get; }
    public string[] Answers { get; }
    public bool HasBeenAnswered { get; private set; }

    public AnswerCategory(string description, string[] answers)
    {
        Answers = answers;
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

        return false;
    }
};