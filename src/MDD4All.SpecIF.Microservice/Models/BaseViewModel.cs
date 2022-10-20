namespace MDD4All.SpecIF.Microservice.Models
{
    public class BaseViewModel
    {
        public string Version
        {
            get
            {
                string result = "N/A";

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                result = fileVersionInfo.FileVersion;

                return result;
            }
        }
    }
}
