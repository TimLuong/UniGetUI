using System;

namespace UniGetUI.Configuration.Examples
{
    /// <summary>
    /// Utility class for detecting and managing application environment
    /// </summary>
    public static class EnvironmentDetector
    {
        /// <summary>
        /// Detects the current environment from multiple sources
        /// </summary>
        public static string DetectEnvironment()
        {
            // 1. Check DOTNET_ENVIRONMENT (highest priority)
            var envVar = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (!string.IsNullOrEmpty(envVar))
            {
                return envVar;
            }

            // 2. Check ASPNETCORE_ENVIRONMENT (for ASP.NET Core compatibility)
            envVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!string.IsNullOrEmpty(envVar))
            {
                return envVar;
            }

            // 3. Check UNIGETUI_ENVIRONMENT (application-specific)
            envVar = Environment.GetEnvironmentVariable("UNIGETUI_ENVIRONMENT");
            if (!string.IsNullOrEmpty(envVar))
            {
                return envVar;
            }

            // 4. Check if debugger is attached
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return "Development";
            }

            // 5. Check build configuration via conditional compilation
#if DEBUG
            return "Development";
#elif STAGING
            return "Staging";
#else
            return "Production";
#endif
        }

        /// <summary>
        /// Checks if running in Development environment
        /// </summary>
        public static bool IsDevelopment()
        {
            return DetectEnvironment().Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if running in Test environment
        /// </summary>
        public static bool IsTest()
        {
            return DetectEnvironment().Equals("Test", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if running in Staging environment
        /// </summary>
        public static bool IsStaging()
        {
            return DetectEnvironment().Equals("Staging", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if running in Production environment
        /// </summary>
        public static bool IsProduction()
        {
            return DetectEnvironment().Equals("Production", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the environment variable for the current process
        /// </summary>
        public static void SetEnvironment(string environment)
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", environment);
            Console.WriteLine($"Environment set to: {environment}");
        }

        /// <summary>
        /// Gets environment-specific information
        /// </summary>
        public static EnvironmentInfo GetEnvironmentInfo()
        {
            var env = DetectEnvironment();
            
            return new EnvironmentInfo
            {
                Name = env,
                IsDevelopment = IsDevelopment(),
                IsTest = IsTest(),
                IsStaging = IsStaging(),
                IsProduction = IsProduction(),
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                Is64BitOS = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                CLRVersion = Environment.Version.ToString()
            };
        }

        /// <summary>
        /// Validates that the environment is properly configured
        /// </summary>
        public static bool ValidateEnvironment(out string[] errors)
        {
            var errorList = new System.Collections.Generic.List<string>();
            var env = DetectEnvironment();

            // Validate environment name
            var validEnvironments = new[] { "Development", "Test", "Staging", "Production" };
            if (!Array.Exists(validEnvironments, e => e.Equals(env, StringComparison.OrdinalIgnoreCase)))
            {
                errorList.Add($"Invalid environment '{env}'. Must be one of: {string.Join(", ", validEnvironments)}");
            }

            // Production-specific validations
            if (IsProduction())
            {
                // Ensure debugger is not attached in production
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    errorList.Add("Debugger should not be attached in Production environment");
                }

                // Warn if running DEBUG build in production
#if DEBUG
                errorList.Add("WARNING: Running DEBUG build in Production environment");
#endif
            }

            errors = errorList.ToArray();
            return errorList.Count == 0;
        }

        /// <summary>
        /// Prints environment information to console
        /// </summary>
        public static void PrintEnvironmentInfo()
        {
            var info = GetEnvironmentInfo();
            
            Console.WriteLine("=== Environment Information ===");
            Console.WriteLine($"Environment:        {info.Name}");
            Console.WriteLine($"Machine Name:       {info.MachineName}");
            Console.WriteLine($"User Name:          {info.UserName}");
            Console.WriteLine($"OS Version:         {info.OSVersion}");
            Console.WriteLine($"Processor Count:    {info.ProcessorCount}");
            Console.WriteLine($"64-bit OS:          {info.Is64BitOS}");
            Console.WriteLine($"64-bit Process:     {info.Is64BitProcess}");
            Console.WriteLine($"CLR Version:        {info.CLRVersion}");
            Console.WriteLine("==============================");
        }
    }

    /// <summary>
    /// Contains detailed information about the current environment
    /// </summary>
    public class EnvironmentInfo
    {
        public string Name { get; set; }
        public bool IsDevelopment { get; set; }
        public bool IsTest { get; set; }
        public bool IsStaging { get; set; }
        public bool IsProduction { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public string OSVersion { get; set; }
        public int ProcessorCount { get; set; }
        public bool Is64BitOS { get; set; }
        public bool Is64BitProcess { get; set; }
        public string CLRVersion { get; set; }
    }

    /// <summary>
    /// Example usage of the EnvironmentDetector
    /// </summary>
    public static class EnvironmentDetectorExample
    {
        public static void DemonstrateEnvironmentDetection()
        {
            // Detect current environment
            var environment = EnvironmentDetector.DetectEnvironment();
            Console.WriteLine($"Current environment: {environment}");

            // Check specific environments
            if (EnvironmentDetector.IsDevelopment())
            {
                Console.WriteLine("Running in Development mode");
                // Enable development features
                EnableDevelopmentFeatures();
            }
            else if (EnvironmentDetector.IsProduction())
            {
                Console.WriteLine("Running in Production mode");
                // Enable production optimizations
                EnableProductionOptimizations();
            }

            // Validate environment
            if (EnvironmentDetector.ValidateEnvironment(out var errors))
            {
                Console.WriteLine("Environment validation passed");
            }
            else
            {
                Console.WriteLine("Environment validation failed:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            // Print detailed information
            EnvironmentDetector.PrintEnvironmentInfo();
        }

        private static void EnableDevelopmentFeatures()
        {
            // Development-specific configuration
            Console.WriteLine("Enabling:");
            Console.WriteLine("  - Debug logging");
            Console.WriteLine("  - Detailed error messages");
            Console.WriteLine("  - Hot reload");
            Console.WriteLine("  - Mock services");
        }

        private static void EnableProductionOptimizations()
        {
            // Production-specific configuration
            Console.WriteLine("Enabling:");
            Console.WriteLine("  - Performance optimizations");
            Console.WriteLine("  - Error logging only");
            Console.WriteLine("  - Connection pooling");
            Console.WriteLine("  - Caching");
        }
    }
}
