/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfDataProviderBase
	{
		ISpecIfServiceDescription DataSourceDescription { get; set; }
	}
}
