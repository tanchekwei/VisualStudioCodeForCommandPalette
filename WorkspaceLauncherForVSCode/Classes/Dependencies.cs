using System;
using Microsoft.Extensions.DependencyInjection;

namespace WorkspaceLauncherForVSCode.Classes;

public class Dependencies
{
    public IServiceProvider Services { get; }

    public Dependencies(IServiceProvider services)
    {
        try
        {
            Services = services;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public T Get<T>() where T : notnull
    {
        try
        {
            return Services.GetRequiredService<T>();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }
}
