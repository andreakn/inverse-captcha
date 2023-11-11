namespace InverseCaptcha.Core;

public class ColorQuestion: Question
{
    private readonly string[] _rainbow = { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };
    private readonly string[] _colors = { "Purple", "Black", "White", "Pink", "Gray", "Brown", "Cyan", "Magenta" };

    public ColorQuestion()
    {
        AnswerCategories = new List<AnswerCategory>
        {
            new("Blank", new[]{"", "null"}),
            new("OtherColors", _colors),
            new("RainbowColors", _rainbow),
            new("Hex", Array.Empty<string>(), new[]{"\\d{6}","#\\d{6}"}),
            new("Garbage", Array.Empty<string>(), new []{"\\w+"}),
        };
        Regen();
    }

    public override void RegenerateQuestion()
    {
        Regen();
    }

    private void Regen()
    {
        var rainbowColor = _rainbow[Random.Shared.Next(_rainbow.Length)];
        QuestionText = $"What is a color in the rainbow next to the color {rainbowColor}";
        
        var previousColor = _rainbow[(Array.IndexOf(_rainbow, rainbowColor) - 1 + _rainbow.Length) % _rainbow.Length];
        var nextColor = _rainbow[(Array.IndexOf(_rainbow, rainbowColor) + 1) % _rainbow.Length];
        HumanAnswers = new List<string> { previousColor, nextColor };
    }
}