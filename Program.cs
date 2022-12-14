using Kernel;
using Kernel.Shared;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateHostBuilder(string[] args)
    {
        var config = Configuration.ParseConfig();

        return new WebHostBuilder()
            .UseKestrel()
            .UseConfiguration(config)
            .UseStaticWebAssets()
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            })
            .UseStartup<Startup>();
    }
}