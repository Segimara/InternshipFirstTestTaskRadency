using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MainService.Extension
{
    public static class ConfigChekerExtension
    {
        public static T CheckHasValidConfig<T>(this IConfigurationSection config)
        {
            try
            {

                var cfg = config.Get<T>();
                if (cfg == null)
                    throw new Exception("Empty Config");
                return cfg;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Configuration is not valid for type T.", ex);
            }
            
        }
    }
}
