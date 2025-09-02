import { BaseService } from "./base.service";
import { TestBed } from "@angular/core/testing";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { HttpClient, provideHttpClient } from "@angular/common/http";

class DummyService extends BaseService {
    constructor(http: HttpClient) {
        super(http);
    }

    public testFormatQueryParams(data: any): string {
        return (this as any).formatQueryParams(data);
    }
}

describe('BaseService', () => {
    let service: DummyService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(),
                provideHttpClientTesting()
            ]
        });
        service = new DummyService(TestBed.inject(HttpClient));
    });

    it('should format simple query params', () => {
        const result = service.testFormatQueryParams({ page: 1, tag: 'life' });
        expect(result).toBe('?page=1&tag=life');
    });

    it('should ignore null or undefined values', () => {
        const result = service.testFormatQueryParams({ page: 1, tag: null, category: undefined });
        expect(result).toBe('?page=1');
    });

    it('should return empty string for empty input', () => {
        const result = service.testFormatQueryParams({});
        expect(result).toBe('');
    });
});
