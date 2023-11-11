namespace InverseCaptcha.Core;

public class ColorQuestion: Question
{
    private readonly string[] _rainbow = new[] { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };
    private readonly string[] _colors = new[] { "Purple", "Black", "White", "Pink", "Gray", "Brown", "Cyan", "Magenta" };

    public ColorQuestion()
    {
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
        AnswerCategories = new List<AnswerCategory>
        {
            new("OtherColors", _colors, null),
            new("RainbowColors", _rainbow, null),
            new("Hex", new[]{""},new[]{"\\d{6}","#\\d{6}"})
        };
    }
}