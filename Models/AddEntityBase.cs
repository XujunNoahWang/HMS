using Microsoft.AspNetCore.Components;

// For admin adding staff and departments
public abstract class AddEntityBase : AdminBase
{
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