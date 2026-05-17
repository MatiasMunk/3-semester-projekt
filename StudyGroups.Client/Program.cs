using StudyGroups.Client;
using StudyGroups.Http;
using StudyGroups.Http.Interfaces;
using StudyGroups.Http.Services;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // =========================================================
        // SINGLE SOURCE OF TRUTH FOR API
        // =========================================================

        // IMPORTANT:
        // This must match your API port.
        // Example: https://localhost:8888/
        var apiClient = new ApiClient(
            "https://localhost:8888/",
            trustDevCert: true,
            adminApiKey: "dev-admin-key-change-me");

        IStudySessionApi sessionApi = new StudySessionApi(apiClient);
        ICategoryApi categoryApi = new CategoryApi(apiClient);
        IUserApi userApi = new UserApi(apiClient);

        // =========================================================
        // START APP WITH DEPENDENCIES
        // =========================================================
        Application.Run(new MainForm(sessionApi, categoryApi, userApi));
    }
}
