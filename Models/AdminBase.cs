using HMS.Services;
using HMS.Data;
using HMS.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Timers; 

public abstract class AdminBase : ComponentBase, IDisposable
{
    [Inject]
    protected UserStateService UserStateService { get; set; }

    [Inject]
    protected NavigationManager Navigation { get; set; }

    [Inject]
    protected DatabaseConnection DatabaseConnection { get; set; }

    protected string ErrorMessage { get; set; }
    protected bool IsSuccessMessage { get; set; }
    protected System.Timers.Timer SuccessMessageTimer { get; set; } // Explicitly specify System.Timers.Timer

    protected override void OnInitialized()
    {
        var currentUser = UserStateService.GetCurrentUser();
        if (currentUser == null || currentUser.Role != Role.Admin)
        {
            Navigation.NavigateTo("/", forceLoad: true);
            return;
        }

        // Initialize timer for success message
        SuccessMessageTimer = new System.Timers.Timer(3000); // Explicitly specify System.Timers.Timer
        SuccessMessageTimer.Elapsed += (sender, e) =>
        {
            ErrorMessage = null;
            IsSuccessMessage = false;
            SuccessMessageTimer.Stop();
            InvokeAsync(StateHasChanged);
        };
        SuccessMessageTimer.AutoReset = false;
    }

    public virtual void Dispose()
    {
        SuccessMessageTimer?.Dispose();
    }
}