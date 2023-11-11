using System.Text.Json;

namespace InverseCaptcha.Core;

public class CountryQuestion : Question
{
    private readonly List<Country> _countries;
    private readonly List<Continent> _continents;

    public CountryQuestion() : base()
    {
        _countries = ReadCountries();
        _continents = CreateContinents();

        var countryToChose = Random.Shared.Next(0, _countries.Count);
        var chosenCountry = _countries[countryToChose];
        QuestionText = $"Name a neighbouring country to {chosenCountry.Name}";
        AnswerCategories = GenerateAnswerCategories(chosenCountry);
        RequiredCategoriesToPass = AnswerCategories.Count;
        HumanAnswers = _countries.Where(c => chosenCountry.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToList();
    }

    private List<Country> ReadCountries()
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
            new Continent("Africa", "AF", _countries.Where(c => c.Continent == "AF").ToList()),
            new Continent("Europe", "EU", _countries.Where(c => c.Continent == "EU").ToList()),
            new Continent("Asia", "AS", _countries.Where(c => c.Continent == "AS").ToList()),
            new Continent("North America", "NA", _countries.Where(c => c.Continent == "NA").ToList()),
            new Continent("South America", "SA", _countries.Where(c => c.Continent == "SA").ToList()),
            new Continent("Oceania", "OC", _countries.Where(c => c.Continent == "OC").ToList()),
            new Continent("Antarctica", "AN", _countries.Where(c => c.Continent == "AN").ToList()),
        };
    }

    private List<AnswerCategory> GenerateAnswerCategories(Country country)
    {
        var continentForCountry = _continents.Single(f => f.Code == country.Continent);
        var otherInContinent = new AnswerCategory("Other countries in continent", continentForCountry.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToArray());
        var countriesInOtherContinents = new AnswerCategory("Countries In Other Continents", _continents.Where(c => c.Code != country.Continent).SelectMany(c => c.Countries.Select(n => n.Name)).ToArray());
        var nameOfContinents = new AnswerCategory("Name of continents", _continents.Select(c => c.Name).ToArray());
        return new List<AnswerCategory>
        {
            otherInContinent,
            countriesInOtherContinents,
            nameOfContinents,
            new ("Garbage", Array.Empty<string>(), new []{"\\w+"}),
        };
    }
}

public record Continent(string Name, string Code, List<Country> Countries);

public record Country(string Name, string Code, string Continent, string Neighbours)
{
    public List<string> NeighbourCodes => Neighbours.Split(',').ToList();
};


