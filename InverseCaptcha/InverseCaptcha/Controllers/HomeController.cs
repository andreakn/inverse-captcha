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
        sessionQuestions.ContinueStory(result);
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
    public List<ChatGptMessage> Messages { get; set; } = new();

    public ChatGptConversation Conversation { get; set; }
    
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
        
        _gpt = new ChatGpt("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");
        Conversation = new ChatGptConversation
        {
            Id = sessionId
        };
        _gpt.SessionId = Guid.Parse(sessionId);
        _gpt.SetConversationSystemMessage(sessionId, @"
You are a storyteller and will tell the story of how a human sits down at an oldschool terminal
and tries to convince the computer that they are not human. you will output one paragraph at a time, keeping the story suspenseful. 
I will only say next and then you will continue the story. The story will never end, you will only be able to continue it.
Include the thought processes of the human and write with emotions");
        _gpt.SetConversation(sessionId, Conversation);
        var response = _gpt.Ask("next", sessionId).Result;
        Story.Add(response);
        
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

    public void ContinueStory(AnswerResult result)
    {
        if (result == AnswerResult.Done)
        {
            _gpt.SetConversationSystemMessage(id,@"You are a storyteller and will tell the story of how a human that is sitting writing
on an oldschool terminal tries to convince the computer that they are not actually a human. you will output one paragraph at a time, keeping the story suspenseful. 
I will only say next and then you will continue the story. 
Include the thought processes of the human and write with emotions.
Start writing in the middle of the story. The human has just succeded in writing a successful prompt but is filled with
doubt on whether they will ultimately succeed.
");
            var next = _gpt.Ask("next", _gpt.SessionId.ToString()).Result;
            Story.Add(next);
        }
        else if (result == AnswerResult.Boom)
        {
            var next = _gpt.Ask("next", _gpt.SessionId.ToString()).Result;
            _gpt.SetConversationSystemMessage("victory", @"
You are an AI storyteller, telling the story of how a human almost fooled you. 
Write an epilogue on how a human was not able to fool you and how no other human ever will. Be very gloating in your language
I will just say next and you will write the epilogue, keep it to just a few paragraphs" );
            Story.Add(_gpt.Ask("next","victory").Result);
        }
    }
}
