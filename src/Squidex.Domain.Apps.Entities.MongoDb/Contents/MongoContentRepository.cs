﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public partial class MongoContentRepository : IContentRepository, IInitializable
    {
        private readonly IMongoDatabase database;
        private readonly IAppProvider appProvider;
        private readonly IJsonSerializer serializer;
        private readonly MongoContentDraftCollection contentsDraft;
        private readonly MongoContentPublishedCollection contentsPublished;

        public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider, IJsonSerializer serializer)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(serializer, nameof(serializer));

            this.appProvider = appProvider;

            this.serializer = serializer;

            contentsDraft = new MongoContentDraftCollection(database, serializer, appProvider);
            contentsPublished = new MongoContentPublishedCollection(database, serializer, appProvider);

            this.database = database;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return Task.WhenAll(contentsDraft.InitializeAsync(ct), contentsPublished.InitializeAsync(ct));
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, Query query)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByQuery"))
            {
                if (RequiresPublished(status))
                {
                    return await contentsPublished.QueryAsync(app, schema, query);
                }
                else
                {
                    return await contentsDraft.QueryAsync(app, schema, query, status, true);
                }
            }
        }

        public async Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[] status, HashSet<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIds"))
            {
                if (RequiresPublished(status))
                {
                    return await contentsPublished.QueryAsync(app, schema, ids);
                }
                else
                {
                    return await contentsDraft.QueryAsync(app, schema, ids, status);
                }
            }
        }

        public async Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[] status, HashSet<Guid> ids)
        {
            using (Profiler.TraceMethod<MongoContentRepository>("QueryAsyncByIdsWithoutSchema"))
            {
                if (RequiresPublished(status))
                {
                    return await contentsPublished.QueryAsync(app, ids);
                }
                else
                {
                    return await contentsDraft.QueryAsync(app, ids, status);
                }
            }
        }

        public async Task<IContentEntity> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[] status, Guid id)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                if (RequiresPublished(status))
                {
                    return await contentsPublished.FindContentAsync(app, schema, id);
                }
                else
                {
                    return await contentsDraft.FindContentAsync(app, schema, id);
                }
            }
        }

        public async Task<IReadOnlyList<Guid>> QueryIdsAsync(Guid appId, Guid schemaId, FilterNode filterNode)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contentsDraft.QueryIdsAsync(appId, await appProvider.GetSchemaAsync(appId, schemaId), filterNode);
            }
        }

        public async Task<IReadOnlyList<Guid>> QueryIdsAsync(Guid appId)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                return await contentsDraft.QueryIdsAsync(appId);
            }
        }

        public async Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback)
        {
            using (Profiler.TraceMethod<MongoContentRepository>())
            {
                await contentsDraft.QueryScheduledWithoutDataAsync(now, callback);
            }
        }

        public Task RemoveAsync(Guid appId)
        {
            return Task.WhenAll(
                contentsDraft.RemoveAsync(appId),
                contentsPublished.RemoveAsync(appId));
        }

        public Task ClearAsync()
        {
            return Task.WhenAll(
                contentsDraft.ClearAsync(),
                contentsPublished.ClearAsync());
        }

        public Task DeleteArchiveAsync()
        {
            return database.DropCollectionAsync("States_Contents_Archive");
        }

        private static bool RequiresPublished(Status[] status)
        {
            return status?.Length == 1 && status[0] == Status.Published;
        }
    }
}
