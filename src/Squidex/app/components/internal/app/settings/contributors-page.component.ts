/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable, Subscription } from 'rxjs';

import {
    AppContributorDto,
    AppContributorsService,
    AppsStoreService,
    AuthService,
    AutocompleteItem,
    AutocompleteSource,
    Notification,
    NotificationService,
    TitleService,
    UserDto,
    UsersService,
    UsersProviderService
} from 'shared';

class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly usersService: UsersService,
        private readonly component: ContributorsPageComponent
    ) {
    }

    public find(query: string): Observable<AutocompleteItem[]> {
        return this.usersService.getUsers(query)
            .map(users => {
                const results: AutocompleteItem[] = [];

                for (let user of users) {
                    if (!this.component.appContributors || !this.component.appContributors.find(t => t.contributorId === user.id)) {
                        results.push(
                            new AutocompleteItem(
                                user.displayName,
                                user.email,
                                user.pictureUrl,
                                user));
                    }
                }
                return results;
            });
    }
}

@Ng2.Component({
    selector: 'sqx-contributor-page',
    styles,
    template
})
export class ContributorsPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;
    private appName: string;

    public appContributors: AppContributorDto[] = [];

    public selectedUserName: string | null = null;
    public selectedUser: UserDto | null = null;

    public currrentUserId: string;

    public usersDataSource: UsersDataSource;
    public usersPermissions = [
        'Owner',
        'Developer',
        'Editor'
    ];

    constructor(
        private readonly titles: TitleService,
        private readonly authService: AuthService,
        private readonly appsStore: AppsStoreService,
        private readonly appContributorsService: AppContributorsService,
        private readonly usersProvider: UsersProviderService,
        private readonly usersService: UsersService,
        private readonly notifications: NotificationService
    ) {
        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.currrentUserId = this.authService.user.id;

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.appName = app.name;

                    this.titles.setTitle('{appName} | Settings | Contributors', { appName: app.name });
                    this.load();
                }
            });
    }

    public load() {
        this.appContributorsService.getContributors(this.appName).retry(2)
            .subscribe(contributors => {
                this.appContributors = contributors;
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public assignContributor() {
        if (!this.selectedUser) {
            return;
        }

        const contributor = new AppContributorDto(this.selectedUser.id, 'Editor');

        this.selectedUser = null;
        this.selectedUserName = null;

        this.appContributorsService.postContributor(this.appName, contributor)
            .subscribe(() => {
                this.appContributors.push(contributor);
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appContributorsService.deleteContributor(this.appName, contributor.contributorId)
            .subscribe(() => {
                this.appContributors.splice(this.appContributors.indexOf(contributor), 1);
            }, error => {
                this.notifications.notify(Notification.error(error.displayMessage));
            });
    }

    public changePermission(contributor: AppContributorDto) {
        this.appContributorsService.postContributor(this.appName, contributor)
            .catch(error => {
                this.notifications.notify(Notification.error(error.displayMessage));

                return Observable.of(true);
            }).subscribe();
    }

    public selectUser(selection: UserDto) {
        this.selectedUser = selection;
    }

    public email(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.email);
    }

    public displayName(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.displayName);
    }

    public pictureUrl(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.pictureUrl);
    }
}
