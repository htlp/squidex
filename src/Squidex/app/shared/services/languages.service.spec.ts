/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as TypeMoq from 'typemoq';
import * as Ng2Http from '@angular/http';

import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AuthService,
    LanguageDto,
    LanguageService
} from './../';

describe('LanguageService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let languageService: LanguageService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        languageService = new LanguageService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get languages', () => {
        authService.setup(x => x.authGet('http://service/p/api/languages'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            iso2Code: 'de',
                            englishName: 'German'
                        }, {
                            iso2Code: 'en',
                            englishName: 'English'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let languages: LanguageDto[] = null;
        
        languageService.getLanguages().subscribe(result => {
            languages = result;
        }).unsubscribe();

        expect(languages).toEqual(
            [
                new LanguageDto('de', 'German'),
                new LanguageDto('en', 'English'),
            ]);

        authService.verifyAll();
    });
});