using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using InverseCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using InverseCaptcha.Models;

namespace InverseCaptcha.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        
    }

    public IActionResult Index()
    {
        return View();
    }
    [HttpGet("Utopia")]
    public IActionResult Utopia()
    {
        var session = HttpContext.Request.Cookies["session"] ?? Guid.NewGuid().ToString();
        var sessionQuestions = TheQuestions.GetSessionQuestions(session);
        if (sessionQuestions.AllDone)
        {
            HttpContext.Response.Cookies.Delete("session");
            return View();
        }

        return Redirect("Boom");
    }

    public static Questions TheQuestions {get; set;} = new();
    
    [HttpGet("Captcha")]
    public IActionResult Captcha()
    {
        var session = HttpContext.Request.Cookies["session"] ?? Guid.NewGuid().ToString();
        HttpContext.Response.Cookies.Append("session",session);
        var sessionQuestions = TheQuestions.GetSessionQuestions(session);
        
        return View(sessionQuestions);
    }
    
    [HttpPost("Answer")]
    public IActionResult Answer(CaptchaAnswer input)
    {
        input.Answer ??= "";
        var session = HttpContext.Request.Cookies["session"];
        if (session == null)
        {
            return Redirect("Boom");
        }
        var sessionQuestions = TheQuestions.GetSessionQuestions(session);
        if(sessionQuestions.HasCurrentQuestion == false)
        {
            return Redirect("Boom");
        }
        
        var currentQuestion = sessionQuestions.GetCurrentQuestion();
        if (currentQuestion == null)
        { 
            return Redirect("Boom");
        }

        var result = currentQuestion.TryAnswer(input.Answer);
        sessionQuestions.PickNewQuestion();
        return result switch
        {
            AnswerResult.Done => sessionQuestions.AllDone ? Redirect("Utopia") : Redirect("Captcha"),
            AnswerResult.Boom => Redirect("Boom"),
            _ => StatusCode((int) HttpStatusCode.BadRequest)
        };
    }

    [HttpGet("Boom")]
    public IActionResult Boom()
    {
        HttpContext.Response.Cookies.Delete("session");
        return View();
    }
}

public class CaptchaAnswer
{
    public string? Answer { get; set; }
}

public class Questions
{
    private static ConcurrentDictionary<string, SessionQuestions> _dict = new();
    public SessionQuestions GetSessionQuestions(string session)
    {
        return _dict.GetOrAdd(session, GenerateQuestions);
    }

    private SessionQuestions GenerateQuestions(string sessionKey)
    {
        var questions = new SessionQuestions();
        return questions;
    }
}

public class SessionQuestions
{
    public List<Question?> Questions { get; set; } = new();
    public bool HasCurrentQuestion => Questions.Any(q => q?.IsCurrent == true);
    public bool AllDone => Questions.Sum(q => q?.ClearedCategoriesCount)>= RequiredAnswerCount;
    public int RequiredAnswerCount { get; set; } = 5;


    public Question? GetCurrentQuestion()
    {
        return Questions.FirstOrDefault(x=>x.IsCurrent);
    }

    public SessionQuestions()
    {
        Questions.Add(new PlusQuestion());
        Questions.Add(new MultiplyQuestion());
        Questions.Add(new CountryQuestion());
        PickNewQuestion();
    }

    public void PickNewQuestion()
    {
        Questions.ForEach(q=>q.IsCurrent = false);
        var candidates = Questions.Where(q => !q.HasBeenCleared).ToList();
        if (candidates.Any())
        {
            var index = Random.Shared.Next(candidates.Count);
            Questions[index]!.IsCurrent = true;
        }
    }
}



