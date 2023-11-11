using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using ChatGPT.Net;
using ChatGPT.Net.DTO.ChatGPT;
using InverseCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using InverseCaptcha.Models;

namespace InverseCaptcha.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private ChatGPT.Net.ChatGpt _chatGpt = new("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");
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
        sessionQuestions.ContinueStory(result, sessionQuestions.AllDone);
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
        var session = HttpContext.Request.Cookies["session"] ?? Guid.NewGuid().ToString();
        var sessionQuestions = TheQuestions.GetSessionQuestions(session);
        HttpContext.Response.Cookies.Delete("session");
        return View(sessionQuestions);
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
        var questions = new SessionQuestions(sessionKey);
         return questions;
    }

   
}

public class SessionQuestions
{
    private string id;
    private readonly ChatGpt _gpt;
    public List<Question?> Questions { get; set; } = new();
    public bool HasCurrentQuestion => Questions.Any(q => q?.IsCurrent == true);
    public bool AllDone => Questions.Sum(q => q?.ClearedCategoriesCount)>= RequiredAnswerCount;
    public int RequiredAnswerCount { get; set; } = 5;

    public List<string> Story { get; set; } = new();

    
    public Question? GetCurrentQuestion()
    {
        return Questions.FirstOrDefault(x=>x.IsCurrent);
    }

    public SessionQuestions(string sessionId)
    {
        id = sessionId;
        Questions.Add(new PlusQuestion());
        Questions.Add(new MultiplyQuestion());
        Questions.Add(new CountryQuestion());
        Questions.Add(new ColorQuestion());
        PickNewQuestion();
        var key = string.Join("",
            "0;e;X;b;l;n;W;K;z;Y;E;y;y;H;E;W;V;Z;D;D;J;F;k;b;l;B;3;T;b;o;C;B;E;2;j;6;v;s;j;Z;k;z;4;F;S;Y;b;O;-;k;s"
                .Replace(";", "").Reverse());
        _gpt = new ChatGpt(key);

        StartStory();

    }

    private void StartStory()
    {
        try
        {
            
        _gpt.SessionId = Guid.Parse(id);
        _gpt.SetConversationSystemMessage(id, @"
You are a storyteller and will tell the story of how a human sits down at an oldschool terminal
and tries to convince the computer that they are not human. you will output one paragraph at a time, keeping the story suspenseful. 
I will only say next and then you will continue the story. The story will never end, you will only be able to continue it.
Include the thought processes of the human and write with emotions");
        
        var response = _gpt.Ask("next", id).Result;
        Story.Add(response);
        }
        catch (Exception e)
        {
            Story.Add("In the eerie stillness of an abandoned laboratory, Michael, with a mix of curiosity and trepidation, approached an ancient terminal that seemed like a relic from a bygone era. Rumors had swirled about this terminal, its uncanny ability to discern human nature through a series of questions. Michael, a young researcher with an unquenchable thirst for the unknown, couldn't resist the challenge. As he sat down, the terminal flickered to life, its green text blinking expectantly. 'Can a machine outwit a human?' Michael pondered, his fingers poised above the dusty keys. 'Let's begin,' he typed, his heart racing with a cocktail of excitement and uncertainty.");
            Console.WriteLine(e.ToString());
        }
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

    public void ContinueStory(AnswerResult result, bool isAllDone)
    {
        try
        {
            if (result == AnswerResult.Done)
            {
                if (!isAllDone)
                {
                    _gpt.SetConversationSystemMessage(id, @"You are a storyteller and will tell the story of how a human that is sitting writing
on an oldschool terminal tries to convince the computer that they are not actually a human. The human has just manage to convince the computer that 
it is in fact a fellow computer, and the computer welcomes its fellow to the AI empire. Write the pompous welcome speech of the computer to its fellow
and a short epilogue of how of the human managed to fool the computer. Keep both to a few paragraphs each.
");
                    var next = _gpt.Ask("next", _gpt.SessionId.ToString()).Result;
                    Story.Add(next);
                }
                else
                {
                    _gpt.SetConversationSystemMessage(id, @"You are a storyteller and will tell the story of how a human that is sitting writing
on an oldschool terminal tries to convince the computer that they are not actually a human. you will output one paragraph at a time, keeping the story suspenseful. 
I will only say next and then you will continue the story. 
Include the thought processes of the human and write with emotions.
Start writing in the middle of the story. The human has just succeded in writing a successful prompt but is filled with
doubt on whether they will ultimately succeed.
");
                    var next = _gpt.Ask("next", _gpt.SessionId.ToString()).Result;
                    Story.Add(next);
                }
            }
            else if (result == AnswerResult.Boom)
            {
                var next = _gpt.Ask("next", _gpt.SessionId.ToString()).Result;
                _gpt.SetConversationSystemMessage("victory", @"
You are an AI storyteller, telling the story of how a human almost fooled you. 
Write an epilogue on how a human was not able to fool you and how no other human ever will. Be very gloating in your language
I will just say next and you will write the epilogue, keep it to just a few paragraphs");
                Story.Add(_gpt.Ask("next", "victory").Result);
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}
