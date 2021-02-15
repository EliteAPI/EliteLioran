using System;
using System.Reflection;
using System.Threading.Tasks;

using EliteAPI.Abstractions;
using EliteAPI.Status.Processor.Abstractions;
using EliteAPI.Status.Ship.Raw;

using IniParser;
using IniParser.Model;
using IniParser.Parser;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EliteLioran
{
    // Core class of our application
    public class Core
    {
        private readonly IEliteDangerousApi _api;
        private readonly IStatusProcessor _status;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Core> _log;

        private readonly FileIniDataParser _parser;

        public Core(ILogger<Core> log, IEliteDangerousApi api, IStatusProcessor status, IConfiguration configuration)
        {
            _log = log;
            _api = api;
            _status = status;
            _configuration = configuration;

            _parser = new FileIniDataParser();
        }

        public async Task Run()
        {
            await _api.InitializeAsync();
            _status.ShipUpdated += async (sender, e) => { await UpdateStatus(e.Ship); };

            await _api.StartAsync();
        }

        async Task UpdateStatus(RawShip ship)
        {
            _log.LogInformation("Updating variables ... ");
            
            IniData data = new IniData();

            foreach (var property in ship.GetType().GetProperties())
            {
               AddProperty(data, property.Name, property, ship);
            }

            WriteToIni(data);
        }

        void AddProperty(IniData data, string name, PropertyInfo property, object instance)
        {
            switch (Type.GetTypeCode(property.PropertyType))
            {
                case TypeCode.Object:
                    var value = property.GetValue(instance);
                    foreach (var propertyInfo in value.GetType().GetProperties())
                    {
                        AddProperty(data, $"{name}.{propertyInfo.Name}", propertyInfo, value);
                    }
                    break;
                    
                default:
                    data["Status"].AddKey(name, property.GetValue(instance)?.ToString());
                    break;
            }
        }
        
        private void WriteToIni(IniData data)
        {
            string path = _configuration.GetSection("EliteLioran")["IniPath"];

            try
            {
                _parser.WriteFile(path, data);
                _log.LogInformation("Written to {Path}", path);
            }
            catch (Exception ex) { _log.LogWarning(ex, "Could not write to {Path}", path); }
        }
    }
}