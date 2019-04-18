/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels
{
	[JsonConverter(typeof(RevisionConverter))]
	public class Revision
	{
		public Revision()
		{ }

		public Revision(string revisionString)
		{
			StringValue = revisionString;
		}

		[JsonIgnore]
		[BsonIgnore]
		public string StringValue
		{
			get
			{
				return BranchName + "/" + RevsionNumber;
			}

			set
			{
				char[] separator = { '/' };

				if(value.Contains("/"))
				{
					string[] tokens = value.Split(separator);

					if(tokens.Length == 2)
					{
						BranchName = tokens[0];
						int revisionNumber = 1;

						if(int.TryParse(tokens[1], out revisionNumber))
						{
							RevsionNumber = revisionNumber;
						}
					}
				}
			}
		}

		[JsonIgnore]
		[BsonElement("branch")]
		public string BranchName { get; set; } = "main";

		[JsonIgnore]
		[BsonElement("revisionNumber")]
		public int RevsionNumber { get; set; } = 1;


		public override string ToString()
		{
			return StringValue;
		}
	}
}
