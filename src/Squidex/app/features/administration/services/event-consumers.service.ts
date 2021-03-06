/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    ApiUrlConfig,
    Model,
    pretifyError
} from '@app/shared';

export class EventConsumerDto extends Model {
    constructor(
        public readonly name: string,
        public readonly isStopped: boolean,
        public readonly isResetting: boolean,
        public readonly error: string,
        public readonly position: string
    ) {
        super();
    }

    public with(value: Partial<EventConsumerDto>): EventConsumerDto {
        return this.clone(value);
    }
}

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getEventConsumers(): Observable<EventConsumerDto[]> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.http.get<any[]>(url).pipe(
                map(response => {
                    return response.map(item => {
                        return new EventConsumerDto(
                            item.name,
                            item.isStopped,
                            item.isResetting,
                            item.error,
                            item.position);
                    });
                }),
                pretifyError('Failed to load event consumers. Please reload.'));
    }

    public putStart(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/start`);

        return this.http.put(url, {}).pipe(
                pretifyError('Failed to start event consumer. Please reload.'));
    }

    public putStop(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/stop`);

        return this.http.put(url, {}).pipe(
                pretifyError('Failed to stop event consumer. Please reload.'));
    }

    public putReset(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/reset`);

        return this.http.put(url, {}).pipe(
                pretifyError('Failed to reset event consumer. Please reload.'));
    }
}