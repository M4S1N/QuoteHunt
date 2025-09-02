import { TestBed } from "@angular/core/testing";
import { HttpTestingController, provideHttpClientTesting } from "@angular/common/http/testing";
import { MatSnackBar } from "@angular/material/snack-bar";
import { QuoteService } from "./quote.service";
import { UrlService } from "app/config/api-url";
import { environment } from "environments/environment";
import { Quote } from "@shared/models/quote.model";
import { HttpErrorResponse, provideHttpClient } from "@angular/common/http";

describe("QuoteService", () => {
  let service: QuoteService;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let urlServiceStub: Partial<UrlService>;

  beforeEach(() => {
    snackBarSpy = jasmine.createSpyObj("MatSnackBar", ["open"]);

    urlServiceStub = {
      getQuote: (page?: string | null, tag?: string | null): string => {
        let url = `/api/Quote`;
        if (!!page) {
            url += `?page=${page}`;
        }
        if (!!tag) {
            url += `&tag=${tag}`;
        }
        return url;
      }
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        QuoteService,
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: UrlService, useValue: urlServiceStub }
      ]
    });

    service = TestBed.inject(QuoteService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it("should fetch quotes successfully and show success snackbar", () => {
    const mockQuotes: Quote[] = [{ text: "Test Quote", author: "Author", tags: ["tag1", "tag2"] } as Quote];

    service.getQuotes("1", "tag1").subscribe((quotes) => {
      expect(quotes).toEqual(mockQuotes);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/Quote?page=1&tag=tag1`);
    expect(req.request.method).toBe("GET");

    req.flush(mockQuotes);

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      "Quotes fetched successfully!",
      "",
      { duration: environment.snackBarDuration }
    );
  });

  it("should handle error and show error snackbar", () => {
    service.getQuotes("1", "tag1").subscribe((quotes) => {
      expect(quotes).toEqual([]);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/Quote?page=1&tag=tag1`);
    req.flush("Error loading", { status: 500, statusText: "Server Error" });

    const actualError = snackBarSpy.open.calls.mostRecent()?.args[1];

    expect(actualError).toBeDefined();
    expect(actualError).toEqual(jasmine.any(Object));
    expect((actualError as any).status).toBe(500);
    expect((actualError as any).error).toBe("Error loading");

    expect(snackBarSpy.open).toHaveBeenCalledWith(
      "Failed to fetch quotes. Please try again.",
      jasmine.any(Object),
      { duration: environment.snackBarDuration }
    );
  });
});
