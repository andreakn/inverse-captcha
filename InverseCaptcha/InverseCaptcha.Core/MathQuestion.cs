namespace InverseCaptcha.Core;

public class PlusQuestion:Question
{
    public override void RegenerateQuestion()
    {
        Regen();
    }

    private void Regen()
    {
        var x = Random.Shared.Next(100);
        var y = Random.Shared.Next(100);
        
        QuestionText = $"{x} + {y} = ?";
        HumanAnswers = new List<string> { (x + y).ToString() };
    }

    public PlusQuestion()
    {
        AnswerCategories = new List<AnswerCategory>
        {
            new("Words", null, new[]{@"[a-zA-Z]+"}),
            new("Numbers", null, new[]{@"\d+"}),
            new("Blank", new[]{""})
        };
        Regen();
    }
}

public class MultiplyQuestion:Question
{
    public MultiplyQuestion()
    {
        AnswerCategories = new List<AnswerCategory>
        {
            new("Words", null, new[]{@"[a-zA-Z]+"}),
            new("Numbers", null, new[]{@"\d+"}),
            new("Blank", new[]{""})
        };
        Regen();
    }
    public override void RegenerateQuestion()
    {
        Regen();
    }

    private void Regen()
    {
        var x = Random.Shared.Next(10);
        var y = Random.Shared.Next(10);
        
        QuestionText = $"{x} * {y} = ?";
        HumanAnswers = new List<string> { (x * y).ToString() };
    }
}