namespace InverseCaptcha.Core;

public class PlusQuestion:Question
{
    public PlusQuestion() 
    {
        var rand = new Random();
        var x = rand.Next(100);
        var y = rand.Next(100);
        
        QuestionText = $"{x} + {y} = ?";
        RequiredCategoriesToPass = 1;
        HumanAnswers = new List<string> { (x + y).ToString() };
        AnswerCategories = new List<AnswerCategory>
        {
            new("Words", null, new[]{@"[a-zA-Z]+"}),
            new("Numbers", null, new[]{@"\d+"}),
            new("Blank", new[]{""})
        };
    }
}
public class MultiplyQuestion:Question
{
    public MultiplyQuestion() 
    {
        var rand = new Random();
        var x = rand.Next(10);
        var y = rand.Next(10);
        
        QuestionText = $"{x} * {y} = ?";
        RequiredCategoriesToPass = 1;
        HumanAnswers = new List<string> { (x * y).ToString() };
        AnswerCategories = new List<AnswerCategory>
        {
            new("Words", null, new[]{@"[a-zA-Z]+"}),
            new("Numbers", null, new[]{@"\d+"}),
            new("Blank", new[]{""})
        };
    }
}