namespace InverseCaptcha.Core;

public class ColorQuestion: Question
{
    string[] rainbow = new[] { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };
    string[] colors = new[] { "Purple", "Black", "White", "Pink", "Gray", "Brown", "Cyan", "Magenta" };

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
        var rand = new Random();
        var rainbowColor = rainbow[rand.Next(rainbow.Length)];
        QuestionText = $"What is a color in the rainbow next to the color {rainbowColor}";
        var previousColor = rainbow[(Array.IndexOf(rainbow, rainbowColor) - 1 + rainbow.Length) % rainbow.Length];
        var nextColor = rainbow[(Array.IndexOf(rainbow, rainbowColor) + 1) % rainbow.Length];
        
        HumanAnswers = new List<string> { previousColor, nextColor };
        AnswerCategories = new List<AnswerCategory>
        {
            new("OtherColors", colors, null),
            new("RainbowColors", rainbow, null),
            new("Hex", new[]{""},new[]{"\\d{6}","#\\d{6}"})
        };
    }
}