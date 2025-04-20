using HMS.Services;
using HMS.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

// To get the visit records
public abstract class RecordsBase : ComponentBase
{
    [Inject]
    protected UserStateService UserStateService { get; set; }

    [Inject]
    protected NavigationManager Navigation { get; set; }

    protected List<VisitRecord> VisitRecords { get; set; } = new List<VisitRecord>();
    protected string ErrorMessage { get; set; }

    // Abstract property: Subclasses specify the expected role
    protected abstract Role ExpectedRole { get; }

    // Virtual property: Subclasses can override to specify extra columns
    protected virtual bool ShowExtraColumns => false;

    // Virtual method: Subclasses implement to load records
    protected virtual void LoadRecords(int patientId)
    {
    }

    protected override void OnInitialized()
    {
        var currentUser = UserStateService.GetCurrentUser();
        if (currentUser == null || currentUser.Role != ExpectedRole)
        {
            Navigation.NavigateTo("/", forceLoad: true);
            return;
        }
    }

    // Virtual method: Subclasses can override to customize empty records message
    protected virtual string EmptyRecordsMessage => "No visit records found.";

    // Virtual method: Subclasses can override to add extra columns
    protected virtual RenderFragment RenderExtraColumns(VisitRecord record) => builder =>
    {
    };
}