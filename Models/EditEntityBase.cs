using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

// For admin editing staff and departments
public abstract class EditEntityBase : AdminBase
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    protected virtual async Task<bool> ConfirmAction(string message)
    {
        return await JSRuntime.InvokeAsync<bool>("confirm", message);
    }

    protected virtual void HandleCancel()
    {
        ErrorMessage = null;
        IsSuccessMessage = false;
    }

    protected virtual void ShowSuccessMessage(string message)
    {
        ErrorMessage = message;
        IsSuccessMessage = true;
        SuccessMessageTimer.Start();
    }
}