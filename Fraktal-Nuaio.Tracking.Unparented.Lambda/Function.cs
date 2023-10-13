using Amazon.Lambda.Core;
using Fraktal_Nuaio.Tracking.Unparented.Shared.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Fraktal_Nuaio.Tracking.Unparented.Lambda;

public class Function
{
    private readonly string _connectionString;
    private ILambdaContext _context;

    public Function()
    {
        _connectionString = Environment.GetEnvironmentVariable("ConnectionString") ?? throw new ArgumentException("Missing ConnectionString variable");

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        try
        {
            _context.Logger.LogInformation($"FunctionHandler() :: Start method...");
            _context.Logger.LogInformation($"FunctionHandler() :: Get Unparented Ucid List from Database...");
            var unparentedUcidList = await SqlServerHelper.GetUnparentedUcidList(_connectionString);
            if (unparentedUcidList.Count < 1)
            {
                _context.Logger.LogInformation($"FunctionHandler() :: There are no items to process...");
                return;
            }

            foreach (var item in unparentedUcidList)
            {
                if (DifferenceInHours(item.GenerationDate) > 8)
                {
                    _context.Logger.LogInformation($"FunctionHandler() :: Item with Ucid [{item.Ucid}] got deleted due 8 hours validation...");
                    await SqlServerHelper.DeleteUnparentedUcid(_connectionString, item.Id);
                    continue;
                }

                var proccessDetailId = await SqlServerHelper.IsUcidInBookingProcess(_connectionString, item.Ucid);
                if (!proccessDetailId.HasValue)
                {
                    _context.Logger.LogInformation($"FunctionHandler() :: Item with Ucid [{item.Ucid}] still is Unparented (Doing nothing)...");
                    continue;
                }

                _context.Logger.LogInformation($"FunctionHandler() :: Item with Ucid [{item.Ucid}] Is going to be send to Queue for {item.Origin}...");
                await SendUcidToQueue(item.Origin, item.JsonToQueue);
                _context.Logger.LogInformation($"FunctionHandler() :: Item with Ucid [{item.Ucid}] got deleted after sending to Queue...");
                await SqlServerHelper.DeleteUnparentedUcid(_connectionString, item.Id);
            }

            _context.Logger.LogInformation($"FunctionHandler() :: End method...");
        }
        catch (Exception ex)
        {
            _context.Logger.LogError($"FunctionHandler() :: There was an error during execution. Error Message: [{ex.Message}] - StackTrace: [{ex.StackTrace}]...");
        }
    }

    private int DifferenceInHours(DateTime sourceDate)
    {
        return (int)(sourceDate - DateTime.Now).TotalHours;
    }

    private async Task SendUcidToQueue(string origin, string jsonToQueue)
    {
        switch (origin)
        {
            //TODO: Send to specific Queue
            //Get url from Environment Variables
            default:
                throw new NotImplementedException($"Origin {origin} is not implemented.");
        }
    }

}
