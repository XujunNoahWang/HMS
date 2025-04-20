using HMS.Services;
using HMS.Models;
using Microsoft.AspNetCore.Components;
using System;

// This class is a base component for dashboards for all the roles, and it can reduce code duplication.
public abstract class DashboardBase : ComponentBase, IDisposable
{
    [Inject]
    protected UserStateService UserStateService { get; set; }

    [Inject]
    protected NavigationManager Navigation { get; set; }

    protected string DisplayName { get; set; }

    protected abstract Role ExpectedRole { get; }

    protected abstract void SetDisplayName(User user);

    protected override void OnInitialized()
    {
        UpdateUserState();
        UserStateService.OnUserChanged += UpdateUserState;
    }

    protected virtual void UpdateUserState()
    {
        var user = UserStateService.GetCurrentUser();
        if (user == null || user.Role != ExpectedRole)
        {
            Navigation.NavigateTo("/", forceLoad: true);
        }
        else
        {
            SetDisplayName(user);
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        UserStateService.OnUserChanged -= UpdateUserState;
    }
}