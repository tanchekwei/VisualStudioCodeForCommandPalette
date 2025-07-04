# GitHub Actions for Publishing to Windows Store and winget

This report details the GitHub Actions workflow created to automate the publishing of the Workspace Launcher for VSCode to the Microsoft Store and winget.

## Workflow File

The workflow is defined in `.github/workflows/publish.yml`.

## Triggers

The workflow is triggered when a new release is created in the GitHub repository.

## Jobs

The workflow consists of a single job named `build` that runs on a Windows environment.

### Steps

1.  **Checkout**: Checks out the repository code.
2.  **Setup .NET**: Sets up the specified .NET version.
3.  **Restore dependencies**: Restores the .NET dependencies.
4.  **Build**: Builds the project in Release mode.
5.  **Get Version**: Extracts the version number from the `WorkspaceLauncherForVSCode.csproj` file.
6.  **Get SHA256**: Calculates the SHA256 hash of the MSIX package.
7.  **Publish to Microsoft Store**: Publishes the MSIX package to the Microsoft Store using the `microsoft/msstore-devcenter-cli-action` action.
8.  **Publish to winget**: Publishes the application to winget using the `vedantmgoyal2009/winget-releaser` action.

## Required Secrets

The following secrets must be configured in the GitHub repository for the workflow to function correctly:

*   `TENANT_ID`: The tenant ID for your Azure AD application.
*   `CLIENT_ID`: The client ID for your Azure AD application.
*   `CLIENT_SECRET`: The client secret for your Azure AD application.
*   `APP_ID`: The application ID of your app in the Microsoft Partner Center.
*   `WINGET_TOKEN`: A personal access token with the `repo` scope to publish to the winget-pkgs repository.

## Conclusion

The created GitHub Actions workflow automates the process of publishing the Workspace Launcher for VSCode to the Microsoft Store and winget. However, due to the dependency on secrets and the requirement of a live release, the workflow cannot be fully tested in the current environment. The workflow has been designed to be as robust as possible, but it may require adjustments after the first live run.