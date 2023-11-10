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

    private Questions _questions = new();
    
    public IActionResult Captcha()
    {
        var session = HttpContext.Request.Cookies["session"] ?? Guid.NewGuid().ToString();
        HttpContext.Response.Cookies.Append("session",session);
        var sessionQuestions = _questions.GetSessionQuestions(session);
        
        return View(sessionQuestions);
    }
    
    public IActionResult Answer(CaptchaAnswer input)
    {
        var session = HttpContext.Request.Cookies["session"];
        if (session == null)
        {
            return Redirect("Boom");
        }
        var sessionQuestions = _questions.GetSessionQuestions(session);
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
        return result switch
        {
            AnswerResult.Done => Redirect("Cleared"),
            AnswerResult.Boom => Redirect("Boom"),
            AnswerResult.NeedMoreCaptcha => Redirect("Captcha"),
            _ => StatusCode((int) HttpStatusCode.BadRequest)
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class CaptchaAnswer
{
    public string Answer { get; set; }
}

internal class Questions
{
    private ConcurrentDictionary<string, SessionQuestions> _dict = new();
    public SessionQuestions GetSessionQuestions(string session)
    {
        return _dict.GetOrAdd(session, GenerateQuestions);
    }

    private SessionQuestions GenerateQuestions(string sessionKey)
    {
        var questions = new SessionQuestions();
        // david do your magic here
        return questions;
    }
}

public class SessionQuestions
{
    public List<Question?> Questions { get; set; } = new();
    public bool HasCurrentQuestion { get; set; }
    
    public AnswerResult TryAnswer(string answer)
    {
        return AnswerResult.Done;
    }

    public Question? GetCurrentQuestion()
    {
        return Questions.FirstOrDefault(x=>x.IsCurrent);
    }
}



