﻿using System.Text.Json;

namespace InverseCaptcha.Core;

public class CountryQuestion : Question
{
    private readonly List<Country> _countries;
    private readonly List<Continent> _continents;
    private AnswerCategory? otherInContinent = null;
    private AnswerCategory? otherInOtherContinents;

    public CountryQuestion() : base()
    {
        _countries = ReadCountries();
        _continents = CreateContinents();

        Regen();

    }

    private void Regen()
    {
        var random = new Random();
        var chosenCountry = _countries[random.Next(_countries.Count)];
        QuestionText = $"Name a neighbouring country to {chosenCountry.Name}";
        AnswerCategories = GenerateAnswerCategories(chosenCountry);
        HumanAnswers = _countries.Where(c => chosenCountry.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToList();
    }


    public override void RegenerateQuestion()
    {
        Regen();
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
        otherInContinent ??= new AnswerCategory("Other countries in continent", Array.Empty<string>());
        otherInContinent.Answers = continentForCountry.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code))
            .Select(c => c.Name).ToArray();
        
        otherInOtherContinents ??= new AnswerCategory("Other countries in other continents", Array.Empty<string>());
        otherInContinent.Answers = _continents.Where(x=>x.Code != country.Continent).SelectMany(x=>x.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code)))
            .Select(c => c.Name).ToArray();
        var otherInContinent = new AnswerCategory("Other countries in continent", continentForCountry.Countries.Where(c => !country.NeighbourCodes.Contains(c.Code)).Select(c => c.Name).ToArray());
        var countriesInOtherContinents = new AnswerCategory("Countries In Other Continents", _continents.Where(c => c.Code != country.Continent).SelectMany(c => c.Countries.Select(n => n.Name)).ToArray());
        var nameOfContinents = new AnswerCategory("Name of continents", _continents.Select(c => c.Name).ToArray());
        return new List<AnswerCategory>
        {
            otherInContinent, 
            otherInOtherContinents, 
            new ("Garbage", new string[0], new []{"\\w+"}),
            new ("Blank", new string[0], new []{""}),
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


