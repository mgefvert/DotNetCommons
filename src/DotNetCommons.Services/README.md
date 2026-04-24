# DotNetCommons.Services

This library provides various integration services for email and SMS messaging capabilities.

## Email Integrations

All email integrations use the IEmailIntegration interface which provides a basic method for sending emails. The method is
async and operates on a batch of emails for efficiency. For more involved scenarios, individual classes may expose further
functionality not covered by the interface.

### DebugIntegration

A development-focused email integration that logs messages instead of sending them. Useful for testing and debugging.

### SmtpClientIntegration

Production-ready SMTP email integration for sending emails through specified SMTP servers.

## SMS Integrations

### DebugIntegration

Development SMS integration that logs messages instead of sending them. Perfect for testing scenarios.

### SpiriusIntegration

Production SMS integration using the Spirius service provider.

## Configuration & Registration

### IntegrationConfiguration

Configure your integrations using the `IntegrationConfiguration` class:

```csharp
    services.Configure<IntegrationConfiguration>(configuration.GetSection("Integration"));
```

### Service Registration

Register the integrations as transient services:

```csharp
    // Email Services
    services.AddTransient<IEmailIntegration, Email.DebugIntegration>();
    // or
    services.AddTransient<IEmailIntegration, SmtpClientIntegration>();
    
    // SMS Services
    services.AddTransient<ISmsIntegration, Sms.DebugIntegration>();
    // or
    services.AddTransient<ISmsIntegration, Sms.SpiriusIntegration>();
```
