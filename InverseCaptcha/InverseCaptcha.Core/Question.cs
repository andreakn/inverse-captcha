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

    public bool HasBeenSkippedByPlayer { get; private set; }

    public bool Answer(string answer)
    {
        return AnswerCategories.Any(category => category.Answer(answer));
    }

    public void Skip()
    {
        if (!HasBeenCleared)
        {
            HasBeenSkippedByPlayer = true;
        }
    }
}

public class AnswerCategory
{
    public string[] Answers { get; private set; }
    public bool HasBeenAnswered { get; private set; }

    public AnswerCategory(string[] answers)
    {
        Answers = answers;
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