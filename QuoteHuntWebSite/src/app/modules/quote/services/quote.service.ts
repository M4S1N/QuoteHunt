import { Injectable } from "@angular/core";
import { BaseService } from "@core/services/base.service";
import { UrlService } from "app/config/api-url";
import { Quote } from "@shared/models/quote.model";
import { catchError, map, Observable, tap } from "rxjs";
import { MatSnackBar } from "@angular/material/snack-bar";
import { environment } from "environments/environment";
import { HttpClient } from "@angular/common/http";

@Injectable({
  providedIn: "root"
})
export class QuoteService extends BaseService {

  private readonly environment = environment

  constructor(
    http: HttpClient,
    private readonly urlService: UrlService,
    private readonly snackBar: MatSnackBar
  ) {
    super(http);
  }

  getQuotes(page: string | null | undefined, tag: string | null | undefined): Observable<Quote[]> {
    return this.get(this.urlService.getQuote(page, tag)).pipe(
      map((response: any) => {
        this.snackBar.open("Quotes fetched successfully!", "", { duration: this.environment.snackBarDuration });
        return response as Quote[]
      }),
      catchError(error => {
        this.snackBar.open("Failed to fetch quotes. Please try again.", error, { duration: this.environment.snackBarDuration });
        return [];
      })
    );
  }

}
