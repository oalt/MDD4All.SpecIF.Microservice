using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Helpers;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
    public class SpecIfElementCreator
    {

        public static Resource CreateResource(Key resourceClassKey, ISpecIfMetadataReader metadataReader)
        {
            Resource result = new Resource();

            ResourceClass resourceType = metadataReader.GetResourceClassByKey(resourceClassKey);

            result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
            result.Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
            result.Properties = new List<Property>();

            result.Class = resourceClassKey;

            foreach (Key propertyClassReference in resourceType.PropertyClasses)
            {
                PropertyClass propertyClass = metadataReader.GetPropertyClassByKey(propertyClassReference);

                Property property = new Property()
                {
                    ID = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                    Revision = SpecIfGuidGenerator.CreateNewRevsionGUID(),
                    Title = propertyClass.Title,
                    PropertyClass = propertyClassReference,
                    Description = propertyClass.Description,
                    Value = ""
                };

                result.Properties.Add(property);
            }

            result.ChangedAt = DateTime.Now;
            // TODO changeBy implementation
            result.ChangedBy = "";

            return result;
        }
    }
}
