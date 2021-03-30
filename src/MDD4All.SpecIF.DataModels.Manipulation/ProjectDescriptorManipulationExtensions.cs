namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ProjectDescriptorManipulationExtensions
    {
        public static string GetTitleValue(this ProjectDescriptor projectDescriptor, string language = "en")
        {
            string result = "";

            foreach(MultilanguageText multilanguageText in projectDescriptor.Title)
            {
                if(multilanguageText.Language == language)
                {
                    result = multilanguageText.Text;
                }
            }

            return result;
        }
    }
}
