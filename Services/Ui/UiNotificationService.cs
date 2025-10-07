namespace ProposalWebApp.Services.Ui;

public interface IUiNotificationService
{
    event Func<string, Task>? OnError;
    Task ShowError(string message);
}

public class UiNotificationService : IUiNotificationService
{
    public event Func<string, Task>? OnError;

    public async Task ShowError(string message)
    {
        if (OnError != null)
            await OnError.Invoke(message);
    }
}