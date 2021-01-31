namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ProjectDescriptorManipulationExtensions
    {
        public static string GetTitleValue(this ProjectDescriptor projectDescriptor, string language = "de")
        {
            string result = "";

            if(projectDescriptor.Title is string)
            {
                result = projectDescriptor.Title as string;
            }
            // TODO Language value

            return result;
        }
    }
}
