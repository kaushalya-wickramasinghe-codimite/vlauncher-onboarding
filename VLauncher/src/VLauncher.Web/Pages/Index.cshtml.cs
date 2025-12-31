using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VLauncher.Application.Auth.Commands;

namespace VLauncher.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        // If already logged in, redirect to dashboard
        if (HttpContext.Session.GetString("Username") != null)
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Username and password are required";
            return Page();
        }

        var result = await _mediator.Send(new LoginCommand(Username, Password));

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }

        // Store user info in session
        HttpContext.Session.SetString("Username", result.Data!.Username);
        HttpContext.Session.SetString("DisplayName", result.Data.DisplayName);
        HttpContext.Session.SetString("IsAdmin", result.Data.IsAdmin.ToString());

        return RedirectToPage("/Admin/Dashboard");
    }
}
