global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading.Tasks;

global using Microsoft.Playwright;
global using Microsoft.Playwright.NUnit;

global using NUnit.Framework;

global using PTS.Automation.Infrastructure.Config;
global using PTS.Automation.Infrastructure.Fixtures;
global using PTS.Automation.Infrastructure.Reporting;

// Alias Serilog's ILogger so individual files don't need `using Serilog;` which
// would collide with our own `Log` static class.
global using ILogger = Serilog.ILogger;
