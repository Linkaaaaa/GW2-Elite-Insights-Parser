using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GW2EIParserCommons;
using GW2EIParserCommons.Properties;

namespace GW2EIParserAvalonia.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ProgramSettings _settings;

    public SettingsViewModel(ProgramSettings settings)
    {
        _settings = settings;
    }

    // Upload / Webhook
    [ObservableProperty]
    private bool sendEmbedToWebhook;
    [ObservableProperty]
    private bool sendSimpleMessageToWebhook;
    [ObservableProperty]
    private string webhookURL = string.Empty;
    [ObservableProperty]
    private bool uploadToDPSReports;
    [ObservableProperty]
    private string dPSReportUserToken = string.Empty;
    [ObservableProperty]
    private bool uploadToWingman;

    // Format
    [ObservableProperty]
    private bool saveOutCSV;
    [ObservableProperty]
    private bool saveOutHTML;
    [ObservableProperty]
    private bool saveOutJSON;
    [ObservableProperty]
    private bool saveOutTrace;

    // Multi-threading
    [ObservableProperty]
    private bool parseMultipleLogs;
    [ObservableProperty]
    private bool singleThreaded;

    // Parsing
    [ObservableProperty]
    private bool anonymous;
    [ObservableProperty]
    private bool skipFailedTries;
    [ObservableProperty]
    private long customTooShort;
    [ObservableProperty]
    private int customTooBig;
    [ObservableProperty]
    private bool detailledWvW;
    [ObservableProperty]
    private bool computePhases;
    [ObservableProperty]
    private bool computeCombatReplay;
    [ObservableProperty]
    private bool computeDamageModifiers;
    [ObservableProperty]
    private bool parseExtensions;
    [ObservableProperty]
    private bool computeDamage;
    [ObservableProperty]
    private bool computeBuff;
    [ObservableProperty]
    private bool computeCast;
    [ObservableProperty]
    private bool computeMechanics;

    // Save Location
    [ObservableProperty]
    private bool saveAtOut;
    [ObservableProperty]
    private string outLocation = string.Empty;

    // Output
    [ObservableProperty]
    private bool addDuration;
    [ObservableProperty]
    private bool addPoVProf;

    // HTML
    [ObservableProperty]
    private bool lightTheme;
    [ObservableProperty]
    private bool htmlExternalScripts;
    [ObservableProperty]
    private string htmlExternalScriptsPath = string.Empty;
    [ObservableProperty]
    private string htmlExternalScriptsCdn = string.Empty;
    [ObservableProperty]
    private bool htmlCompressJson;

    // JSON
    [ObservableProperty]
    private bool rawTimelineArrays;
    [ObservableProperty]
    private bool compressRaw;
    [ObservableProperty]
    private bool indentJSON;

    // Other
    [ObservableProperty]
    private int memoryLimit;

    // GUI
    [ObservableProperty]
    private bool autoAdd;
    [ObservableProperty]
    private string autoAddPath = string.Empty;
    [ObservableProperty]
    private bool autoParse;
    [ObservableProperty]
    private bool applicationTraces;
    [ObservableProperty]
    private long populateHourLimit;
    [ObservableProperty]
    private bool autoDiscordBatch;

    //
    public event EventHandler? SettingsApplied;

    public void LoadFromSettings()
    {
        // Upload / Webhook
        SendEmbedToWebhook = _settings.SendEmbedToWebhook;
        SendSimpleMessageToWebhook = _settings.SendSimpleMessageToWebhook;
        WebhookURL = _settings.WebhookURL ?? string.Empty;
        UploadToDPSReports = _settings.UploadToDPSReports;
        DPSReportUserToken = _settings.DPSReportUserToken ?? string.Empty;
        UploadToWingman = _settings.UploadToWingman;

        // Format
        SaveOutCSV = _settings.SaveOutCSV;
        SaveOutHTML = _settings.SaveOutHTML;
        SaveOutJSON = _settings.SaveOutJSON;
        SaveOutTrace = _settings.SaveOutTrace;

        // Multi-threading
        ParseMultipleLogs = _settings.ParseMultipleLogs;
        SingleThreaded = _settings.SingleThreaded;

        // Parsing
        Anonymous = _settings.Anonymous;
        SkipFailedTries = _settings.SkipFailedTries;
        CustomTooShort = _settings.CustomTooShort;
        CustomTooBig = _settings.CustomTooBig;
        DetailledWvW = _settings.DetailledWvW;
        ComputePhases = _settings.ComputePhases;
        ComputeCombatReplay = _settings.ComputeCombatReplay;
        ComputeDamageModifiers = _settings.ComputeDamageModifiers;
        ParseExtensions = _settings.ParseExtensions;
        ComputeDamage = _settings.ComputeDamage;
        ComputeBuff = _settings.ComputeBuff;
        ComputeCast = _settings.ComputeCast;
        ComputeMechanics = _settings.ComputeMechanics;

        // Save Location
        SaveAtOut = _settings.SaveAtOut;
        OutLocation = _settings.OutLocation ?? string.Empty;

        // Output
        AddDuration = _settings.AddDuration;
        AddPoVProf = _settings.AddPoVProf;

        // HTML
        LightTheme = _settings.LightTheme;
        HtmlExternalScripts = _settings.HtmlExternalScripts;
        HtmlExternalScriptsPath = _settings.HtmlExternalScriptsPath ?? string.Empty;
        HtmlExternalScriptsCdn = _settings.HtmlExternalScriptsCdn ?? string.Empty;
        HtmlCompressJson = _settings.HtmlCompressJson;

        // JSON
        RawTimelineArrays = _settings.RawTimelineArrays;
        CompressRaw = _settings.CompressRaw;
        IndentJSON = _settings.IndentJSON;

        // Other
        MemoryLimit = _settings.MemoryLimit;

        // GUI
        AutoAdd = Settings.Default.AutoAdd;
        AutoAddPath = Settings.Default.AutoAddPath ?? string.Empty;
        AutoParse = Settings.Default.AutoParse;
        ApplicationTraces = Settings.Default.ApplicationTraces;
        PopulateHourLimit = Settings.Default.PopulateHourLimit;
        AutoDiscordBatch = Settings.Default.AutoDiscordBatch;
    }

    public void ApplyToSettings()
    {
        // Upload / Webhook
        _settings.SendEmbedToWebhook = SendEmbedToWebhook;
        _settings.SendSimpleMessageToWebhook = SendSimpleMessageToWebhook;
        _settings.WebhookURL = WebhookURL;
        _settings.UploadToDPSReports = UploadToDPSReports;
        _settings.DPSReportUserToken = DPSReportUserToken;
        _settings.UploadToWingman = UploadToWingman;

        // Format
        _settings.SaveOutCSV = SaveOutCSV;
        _settings.SaveOutHTML = SaveOutHTML;
        _settings.SaveOutJSON = SaveOutJSON;
        _settings.SaveOutTrace = SaveOutTrace;

        // Multi-threading
        _settings.ParseMultipleLogs = ParseMultipleLogs;
        _settings.SingleThreaded = SingleThreaded;

        // Parsing
        _settings.Anonymous = Anonymous;
        _settings.SkipFailedTries = SkipFailedTries;
        _settings.CustomTooShort = CustomTooShort;
        _settings.CustomTooBig = CustomTooBig;
        _settings.DetailledWvW = DetailledWvW;
        _settings.ComputePhases = ComputePhases;
        _settings.ComputeCombatReplay = ComputeCombatReplay;
        _settings.ComputeDamageModifiers = ComputeDamageModifiers;
        _settings.ParseExtensions = ParseExtensions;
        _settings.ComputeDamage = ComputeDamage;
        _settings.ComputeBuff = ComputeBuff;
        _settings.ComputeCast = ComputeCast;
        _settings.ComputeMechanics = ComputeMechanics;

        // Save Location
        _settings.SaveAtOut = SaveAtOut;
        _settings.OutLocation = OutLocation;

        // Output
        _settings.AddDuration = AddDuration;
        _settings.AddPoVProf = AddPoVProf;

        // HTML
        _settings.LightTheme = LightTheme;
        _settings.HtmlExternalScripts = HtmlExternalScripts;
        _settings.HtmlExternalScriptsPath = HtmlExternalScriptsPath;
        _settings.HtmlExternalScriptsCdn = HtmlExternalScriptsCdn;
        _settings.HtmlCompressJson = HtmlCompressJson;

        // JSON
        _settings.RawTimelineArrays = RawTimelineArrays;
        _settings.CompressRaw = CompressRaw;
        _settings.IndentJSON = IndentJSON;

        // Other
        _settings.MemoryLimit = MemoryLimit;

        // GUI
        Settings.Default.AutoAdd = AutoAdd;
        Settings.Default.AutoAddPath = AutoAddPath;
        Settings.Default.AutoParse = AutoParse;
        Settings.Default.ApplicationTraces = ApplicationTraces;
        Settings.Default.PopulateHourLimit = PopulateHourLimit;
        Settings.Default.AutoDiscordBatch = AutoDiscordBatch;

        Settings.Default.Save();

        SettingsApplied?.Invoke(this, EventArgs.Empty);
    }
}
