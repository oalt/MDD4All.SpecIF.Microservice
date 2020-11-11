using MDD4All.SpecIF.Apps.ServiceStarter.DataModels;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDD4All.SpecIF.Apps.ServiceStarter.Controllers
{
    public class SettingsController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string _settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SpecIFicator";

        private string _settingsFileName = "ServiceStarterSettings.json";

        public SettingsController()
        {
            LoadSettings();
        }

        public ServiceStarterSettings Settings { get; set; } = new ServiceStarterSettings();


        public void LoadSettings()
        {
            

            if (!Directory.Exists(_settingsDirectory))
            {
                Directory.CreateDirectory(_settingsDirectory);
            }

            try
            {

                StreamReader file = new StreamReader(_settingsDirectory + "/" + _settingsFileName);

                JsonSerializer serializer = new JsonSerializer();

                Settings = (ServiceStarterSettings)serializer.Deserialize(file, typeof(ServiceStarterSettings));

                file.Close();
            }
            catch (Exception exception)
            {
                
            }
        }

        public void SaveSettings()
        {
            if (!Directory.Exists(_settingsDirectory))
            {
                Directory.CreateDirectory(_settingsDirectory);
            }

            StreamWriter sw = new StreamWriter(_settingsDirectory + "/" + _settingsFileName);
            JsonWriter writer = new JsonTextWriter(sw)
            {
                Formatting = Formatting.Indented
            };

            JsonSerializer serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            serializer.Serialize(writer, Settings);

            writer.Flush();
            writer.Close();

        }
    }
}
