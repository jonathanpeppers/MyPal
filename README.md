# MyPal

A .NET MAUI sample application that uses omni-input LLMs to create "My
Pal", a friendly koala that insults you based on a selfie!

![Screenshot of MyPal application](assets/screenshot.png)

This sample runs on iOS and Android, but could be adapted for desktop
in the future.

## Azure Setup

This app requires 3 Azure services:

* Azure OpenAI: `gpt-4o` instance
* Azure OpenAI: `tts` instance
* Azure Application Insights: for telemetry

I created these in North Central US, as that region supports both gpt-4o and tts:

* https://learn.microsoft.com/azure/ai-services/openai/concepts/models

When creating a model deployment in Azure OpenAI Studio, name them
exactly `gpt-4o` and `tts`, otherwise you will need to use different
names in `MyPalWebClient.cs`.

You can also try a `tts-hd` model, but that seemed to just increase
the latency of each request. It would be more useful for making
high-quality voiceovers, though.

Lastly, I made an Azure Log Analytics (dependency of Application
Insights) and Azure Application Insights instance in North Central US.
If you want to skip this step, you can remove the Application Insights
code from the .NET MAUI app.

## Secrets

For any of the three apps to work:

* `MyPal.Benchmarks`
* `MyPal.Console`
* `MyPal.MauiApp`

Create a `runtimeconfig.template.json` with the contents:

```json
{
  "configProperties": {
    "APPLICATIONINSIGHTS_CONNECTION_STRING": "secret here",
    "OPENAI_API_KEY": "secret here"
  }
}
```

`APPLICATIONINSIGHTS_CONNECTION_STRING` is only required for the .NET MAUI app.
