
using System;
using Microsoft.Extensions.DependencyInjection;

namespace WorkspaceLauncherForVSCode.Classes;
public class Dependencies
{
  public IServiceProvider Services { get; }

  public Dependencies(IServiceProvider services)
  {
    Services = services;
  }

  public T Get<T>() where T : notnull => Services.GetRequiredService<T>();
}
