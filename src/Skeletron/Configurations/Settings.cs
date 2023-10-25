using System;

namespace Skeletron.Configurations
{
    public class Settings
    {   
        // Discord credential
        public string Token { get; set; }

        // Bancho credentials
        public int BanchoClientId { get; set; }
        
        public string BanchoSecret { get; set; }

        public string VkSecret { get; set; }
        
        public string PGConnectionString { get; set; }

        public Settings()
        {
            var settingsType = typeof(Settings);
            
            foreach (var property in settingsType.GetProperties())
            {
                var stringValue = Environment.GetEnvironmentVariable(property.Name);
                var type = property.PropertyType.ToString();
                
                switch(type)
                {
                    case "System.Int32":
                        var intValue = Convert.ToInt32(stringValue);
                        property.SetValue(this, intValue);
                        break;
                    case "System.String":
                        property.SetValue(this, stringValue);
                        break;
                }
            }
        }
    }
}
