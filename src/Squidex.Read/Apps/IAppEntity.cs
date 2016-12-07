﻿// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Read.Apps
{
    public interface IAppEntity : IEntity
    {
        string Name { get; }

        Language MasterLanguage { get; }

        IEnumerable<IAppClientEntity> Clients { get; }

        IEnumerable<IAppContributorEntity> Contributors { get; }

        IEnumerable<Language> Languages { get; }
    }
}