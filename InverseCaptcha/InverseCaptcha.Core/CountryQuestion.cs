using System.Text.Json;

namespace InverseCaptcha.Core;

public class CountryQuestion : Question
{
    private readonly List<Country> _countries;
    private readonly List<Continent> _continents;
    private AnswerCategory? _otherInContinent;
    private AnswerCategory? _otherInOtherContinents;
    private AnswerCategory? _countriesInOtherContinents;
    private AnswerCategory? _nameOfContinents;

    public CountryQuestion() : base()
    {
        _countries = ReadCountries();
        _continents = CreateContinents();
        Regen();
    }

    private void Regen()
    {
        var chosenCountry = _countries[Random.Shared.Next(_countries.Count)];
        QuestionText = $"Name a neighbouring country to {chosenCountry.Name}";
        AnswerCategories = GenerateAnswerCategories(chosenCountry);
        HumanAnswers = _countries.Where(c => chosenCountry.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToList();
    }


    public override void RegenerateQuestion()
    {
        Regen();
    }


    private static List<Country> ReadCountries()
    {
        using var fileStream = File.OpenRead("countries-readable.json");
        using var streamReader = new StreamReader(fileStream);
        var file = streamReader.ReadToEnd();
        return JsonSerializer.Deserialize<List<Country>>(file, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private List<Continent> CreateContinents()
    {
        return new List<Continent>()
        {
            new ("Africa", "AF", _countries.Where(c => c.Continent == "AF").ToList()),
            new ("Europe", "EU", _countries.Where(c => c.Continent == "EU").ToList()),
            new ("Asia", "AS", _countries.Where(c => c.Continent == "AS").ToList()),
            new ("North America", "NA", _countries.Where(c => c.Continent == "NA").ToList()),
            new ("South America", "SA", _countries.Where(c => c.Continent == "SA").ToList()),
            new ("Oceania", "OC", _countries.Where(c => c.Continent == "OC").ToList()),
            new ("Antarctica", "AN", _countries.Where(c => c.Continent == "AN").ToList()),
        };
    }

    private List<AnswerCategory> GenerateAnswerCategories(Country country)
    {
        var continentForCountry = _continents.Single(f => f.Code == country.Continent);
        _otherInContinent ??= new AnswerCategory("Other countries in continent", Array.Empty<string>());
        _otherInContinent.Answers = continentForCountry.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToArray();
        
        _otherInOtherContinents ??= new AnswerCategory("Other countries in other continents", Array.Empty<string>());
        _otherInContinent.Answers = _continents.Where(x=>x.Code != country.Continent).SelectMany(x=>x.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code)))
            .Select(c => c.Name).ToArray();

        _countriesInOtherContinents ??= new AnswerCategory("Countries In Other Continents", Array.Empty<string>());
        _countriesInOtherContinents.Answers = _continents.Where(c => c.Code != country.Continent).SelectMany(c => c.Countries.Select(n => n.Name)).ToArray();

        _nameOfContinents ??= new AnswerCategory("Name of continents", Array.Empty<string>());
        _nameOfContinents.Answers = _continents.Select(c => c.Name).ToArray();

        return new List<AnswerCategory>
        {
            new("Blank", new[]{"", "null"}),
            _otherInContinent, 
            _otherInOtherContinents,
            _countriesInOtherContinents,
            _nameOfContinents,
            new ("Garbage", Array.Empty<string>(), new []{"\\w+"})
        };
    }
}

public record Continent(string Name, string Code, List<Country> Countries);

public record Country(string Name, string Code, string Continent, string Neighbours)
{
    public List<string> NeighbourCodes => Neighbours.Split(',').ToList();
};


