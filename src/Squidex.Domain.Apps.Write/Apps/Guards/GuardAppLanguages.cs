﻿// ==========================================================================
//  GuardApp.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Guards
{
    public static class GuardAppLanguages
    {
        public static void CanAdd(LanguagesConfig languages, AddLanguage command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add language.", error =>
            {
                if (command.Language == null)
                {
                    error(new ValidationError("Language cannot be null.", nameof(command.Language)));
                }
                else if (languages.Contains(command.Language))
                {
                    error(new ValidationError("Language already added.", nameof(command.Language)));
                }
            });
        }

        public static void CanRemove(LanguagesConfig languages, RemoveLanguage command)
        {
            Guard.NotNull(command, nameof(command));

            var languageConfig = GetLanguageConfigOrThrow(languages, command.Language);

            Validate.It(() => "Cannot remove language.", error =>
            {
                if (languages.Master == languageConfig)
                {
                    error(new ValidationError("Language config is master.", nameof(command.Language)));
                }
            });
        }

        public static void CanUpdate(LanguagesConfig languages, UpdateLanguage command)
        {
            Guard.NotNull(command, nameof(command));

            var languageConfig = GetLanguageConfigOrThrow(languages, command.Language);

            Validate.It(() => "Cannot update language.", error =>
            {
                if ((languages.Master == languageConfig || command.IsMaster) && command.IsOptional)
                {
                    error(new ValidationError("Cannot make master language optional.", nameof(command.IsMaster)));
                }

                if (command.Fallback != null)
                {
                    foreach (var fallback in command.Fallback)
                    {
                        if (!languages.Contains(fallback))
                        {
                            error(new ValidationError($"Config does not contain fallback language {fallback}.", nameof(command.Fallback)));
                        }
                    }
                }
            });
        }

        private static LanguageConfig GetLanguageConfigOrThrow(LanguagesConfig languages, Language language)
        {
            if (language == null)
            {
                throw new DomainObjectNotFoundException(language, "Languages", typeof(AppDomainObject));
            }

            if (!languages.TryGetConfig(language, out var languageConfig))
            {
                throw new DomainObjectNotFoundException(language, "Languages", typeof(AppDomainObject));
            }

            return languageConfig;
        }
    }
}
