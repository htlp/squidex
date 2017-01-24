﻿// ==========================================================================
//  MongoDbModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Replay;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Contents.Repositories;
using Squidex.Read.History.Repositories;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Users.Repositories;
using Squidex.Read.MongoDb.Apps;
using Squidex.Read.MongoDb.Contents;
using Squidex.Read.MongoDb.History;
using Squidex.Read.MongoDb.Infrastructure;
using Squidex.Read.MongoDb.Schemas;
using Squidex.Read.MongoDb.Users;

namespace Squidex.Read.MongoDb
{
    public class MongoDbModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RefTokenSerializer.Register();

            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<MyMongoDbOptions>>().Value;

                var mongoDbClient = new MongoClient(options.ConnectionString);
                var mongoDatabase = mongoDbClient.GetDatabase(options.DatabaseName);

                return mongoDatabase;
            }).SingleInstance();

            builder.Register<IUserStore<IdentityUser>>(context =>
            {
                var usersCollection = context.Resolve<IMongoDatabase>().GetCollection<IdentityUser>("Identity_Users");

                IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                return new UserStore<IdentityUser>(usersCollection);
            }).SingleInstance();

            builder.Register<IRoleStore<IdentityRole>>(context =>
            {
                var rolesCollection = context.Resolve<IMongoDatabase>().GetCollection<IdentityRole>("Identity_Roles");

                IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                return new RoleStore<IdentityRole>(rolesCollection);
            }).SingleInstance();

            builder.RegisterType<MongoPersistedGrantStore>()
                .As<IPersistedGrantStore>()
                .SingleInstance();

            builder.RegisterType<MongoUserRepository>()
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MongoContentRepository>()
                .As<IContentRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoHistoryEventRepository>()
                .As<IHistoryEventRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoSchemaRepository>()
                .As<ISchemaRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();

            builder.RegisterType<MongoAppRepository>()
                .As<IAppRepository>()
                .As<ICatchEventConsumer>()
                .As<IReplayableStore>()
                .SingleInstance();
        }
    }
}