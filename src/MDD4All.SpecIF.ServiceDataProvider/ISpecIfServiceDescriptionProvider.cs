/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.ServiceDataProvider
{
	public interface ISpecIfServiceDescriptionProvider
	{
		List<SpecIfServiceDescription> GetAvailableServices();

		void Refresh();
	}
}
