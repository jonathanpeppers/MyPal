# MyPal

Using omni-input LLMs to create "my pal"

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

`APPLICATIONINSIGHTS_CONNECTION_STRING` is only required for the MAUI app.
