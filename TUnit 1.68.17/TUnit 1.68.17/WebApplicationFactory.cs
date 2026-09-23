using TUnit.AspNetCore;

namespace TUnit_1._68._17;

// TestWebApplicationFactory (TUnit.AspNetCore) replaces the vanilla WebApplicationFactory: clients it creates
// propagate traceparent/baggage/X-TUnit-TestId, server-side ILogger output is routed to the calling test, and
// TestContext.Current resolves inside request handlers. Analyzer TUnit0064 flags the vanilla base class.
public class WebApplicationFactory : TestWebApplicationFactory<Program>;
