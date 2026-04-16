using JaTakTilbud.Client;
using JaTakTilbud.Http;
using JaTakTilbud.Http.Interfaces;
using JaTakTilbud.Http.Services;

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
        // This must match your API port
        // Example: https://localhost:8888/
        var apiClient = new ApiClient("https://localhost:8888/");

        IProductApi productApi = new ProductApi(apiClient);
        ICampaignApi campaignApi = new CampaignApi(apiClient);

        // =========================================================
        // START APP WITH DEPENDENCIES
        // =========================================================
        Application.Run(new MainForm(productApi));
    }
}