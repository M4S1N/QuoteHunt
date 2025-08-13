import { Injectable, Injector } from "@angular/core";
import { BaseService } from "@core/services/base.service";
import { UrlService } from "app/config/api-url";
import { Quote } from "@shared/models/quote.model";
import { catchError, map, Observable, tap } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class QuoteService extends BaseService {

  constructor(
    injector: Injector,
    private readonly urlService: UrlService
  ) {
    super(injector);
  }

  getQuotes(params: FormData): Observable<Quote[]> {
    return this.get(this.urlService.getQuote(
      params.get("page")!.toString(),
      params.get("tag")!.toString()
    )).pipe(
      map((response: any) => response as Quote[]),
      catchError(error => {
        console.error("Error fetching quotes:", error);
        return [];
      })
    );
  }

}
