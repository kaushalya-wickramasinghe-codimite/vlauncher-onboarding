using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VLauncher.Application.DTOs;
using VLauncher.Application.Users.Commands;
using VLauncher.Application.Users.Queries;
using VLauncher.Domain.Enums;

namespace VLauncher.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IAntiforgery _antiforgery;

    public DashboardModel(IMediator mediator, IAntiforgery antiforgery)
    {
        _mediator = mediator;
        _antiforgery = antiforgery;
    }

    public string DisplayName { get; set; } = string.Empty;
    public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if logged in
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToPage("/Index");
        }

        DisplayName = HttpContext.Session.GetString("DisplayName") ?? username;
        Users = await _mediator.Send(new GetAllUsersQuery());

        return Page();
    }

    public async Task<IActionResult> OnGetUserDetailsAsync(int userId)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(userId));
        if (user == null)
        {
            return Content("<p class='alert alert-danger'>User not found</p>");
        }

        var availableGroups = await _mediator.Send(new GetAvailableSecurityGroupsQuery());

        var html = GenerateUserDetailsHtml(user, availableGroups);
        return Content(html, "text/html");
    }

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        var result = await _mediator.Send(new DeleteUserCommand(userId));

        if (result.IsSuccess)
        {
            TempData["Success"] = "User deleted successfully";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRegisterAsync(int userId, string adUserPrincipalName, List<string> selectedGroups)
    {
        var result = await _mediator.Send(new RegisterUserCommand(userId, adUserPrincipalName, selectedGroups ?? new List<string>()));

        if (result.IsSuccess)
        {
            TempData["Success"] = "User registered successfully";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateGroupsAsync(int userId, List<string> groupsToAdd, List<string> groupsToRemove)
    {
        var result = await _mediator.Send(new UpdateUserGroupsCommand(userId, groupsToAdd ?? new List<string>(), groupsToRemove ?? new List<string>()));

        if (result.IsSuccess)
        {
            TempData["Success"] = "User groups updated successfully";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(int userId)
    {
        var result = await _mediator.Send(new ResetUserPasswordCommand(userId));

        if (result.IsSuccess)
        {
            TempData["Success"] = $"Password reset successfully. New password: {result.Data}";
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToPage();
    }

    private string GenerateUserDetailsHtml(UserDetailDto user, IEnumerable<AdSecurityGroupDto> availableGroups)
    {
        var isPending = user.Status == UserStatus.Pending;
        var buttonText = isPending ? "Register User" : "Update Groups";
        var handlerName = isPending ? "Register" : "UpdateGroups";

        // Get anti-forgery token
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        var tokenValue = tokens.RequestToken;

        var statusBadge = isPending
            ? "<span class='inline-flex items-center gap-1.5 px-3 py-1.5 bg-amber-100 text-amber-700 text-xs font-semibold rounded-full'><span class='w-1.5 h-1.5 bg-amber-500 rounded-full'></span>Pending</span>"
            : "<span class='inline-flex items-center gap-1.5 px-3 py-1.5 bg-green-100 text-green-700 text-xs font-semibold rounded-full'><i data-lucide=\"check\" class=\"w-3 h-3\"></i>Registered</span>";

        var html = $@"
            <form method='post' action='/Admin/Dashboard?handler={handlerName}' class='space-y-5'>
                <input type='hidden' name='__RequestVerificationToken' value='{tokenValue}' />
                <input type='hidden' name='userId' value='{user.Id}' />

                <div>
                    <label class='block text-sm font-medium text-gray-700 mb-2'>
                        Google Email
                    </label>
                    <input type='text' class='w-full bg-gray-100 border border-gray-300 text-gray-600 px-4 py-2.5 rounded-lg focus:outline-none' value='{user.GoogleEmail}' disabled />
                </div>

                <div>
                    <label class='block text-sm font-medium text-gray-700 mb-2'>
                        AD User Principal Name
                    </label>
                    <input type='text' name='adUserPrincipalName'
                           class='w-full border border-gray-300 text-gray-800 px-4 py-2.5 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 {(isPending ? "" : "bg-gray-100 cursor-not-allowed")}'
                           value='{user.AdUserPrincipalName ?? ""}'
                           placeholder='user@kaushalya.local'
                           {(isPending ? "" : "disabled")} />
                </div>

                <div>
                    <label class='block text-sm font-medium text-gray-700 mb-2'>
                        Status
                    </label>
                    <div class='py-2'>
                        {statusBadge}
                    </div>
                </div>

                <div>
                    <label class='block text-sm font-medium text-gray-700 mb-2'>
                        Security Groups
                    </label>
                    <div class='border border-gray-300 rounded-lg max-h-48 overflow-y-auto'>";

        foreach (var group in availableGroups)
        {
            var isMember = user.AdGroups.Any(g => string.Equals(g.DistinguishedName, group.DistinguishedName, StringComparison.OrdinalIgnoreCase));
            var checkedAttr = isMember ? "checked" : "";
            var memberBg = isMember ? "bg-green-50" : "bg-white";
            var memberBadge = isMember ? "<span class='ml-auto text-xs text-green-600 font-semibold bg-green-100 px-2 py-0.5 rounded-full'>Member</span>" : "";

            if (isPending)
            {
                html += $@"
                        <label class='flex items-center gap-3 px-4 py-3 border-b border-gray-200 last:border-b-0 hover:bg-gray-50 cursor-pointer transition-colors {memberBg}'>
                            <input type='checkbox' name='selectedGroups' value='{group.DistinguishedName}' {checkedAttr}
                                   class='w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500' />
                            <span class='text-gray-800 text-sm font-medium'>{group.Name}</span>
                            {memberBadge}
                        </label>";
            }
            else
            {
                html += $@"
                        <label class='flex items-center gap-3 px-4 py-3 border-b border-gray-200 last:border-b-0 hover:bg-gray-50 cursor-pointer transition-colors {memberBg}'>
                            <input type='checkbox' class='group-checkbox w-4 h-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500'
                                   data-dn='{group.DistinguishedName}' data-member='{isMember.ToString().ToLower()}' {checkedAttr} />
                            <span class='text-gray-800 text-sm font-medium'>{group.Name}</span>
                            {memberBadge}
                        </label>";
            }
        }

        html += @"
                    </div>
                </div>

                <div class='flex items-center gap-3 pt-5 border-t border-gray-200'>";

        if (!isPending)
        {
            html += $@"
                    <button type='button' onclick=""resetPassword({user.Id})""
                            class='px-4 py-2.5 bg-amber-100 text-amber-700 hover:bg-amber-200 font-semibold text-sm rounded-lg transition-all shadow-sm hover:shadow flex items-center gap-2'>
                        <i data-lucide='key' class='w-4 h-4'></i>
                        Reset Password
                    </button>";
        }

        html += $@"
                    <div class='ml-auto flex items-center gap-3'>
                        <button type='button' onclick='closeModal()'
                                class='px-5 py-2.5 bg-gray-100 text-gray-700 hover:bg-gray-200 font-semibold text-sm rounded-lg transition-all shadow-sm border border-gray-200'>
                            Cancel
                        </button>
                        <button type='submit'
                                class='px-5 py-2.5 bg-primary-600 text-white hover:bg-primary-700 font-semibold text-sm rounded-lg transition-all shadow-md hover:shadow-lg flex items-center gap-2'>
                            <i data-lucide='{(isPending ? "plus" : "save")}' class='w-4 h-4'></i>
                            {buttonText}
                        </button>
                    </div>
                </div>
            </form>";

        return html;
    }
}
