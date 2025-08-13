import { Component, OnInit } from "@angular/core";
import { QuoteService } from "../services/quote.service";
import { shareReplay, Subject, switchMap, tap } from "rxjs";
import { FormBuilder, FormGroup } from "@angular/forms";
import { AsyncPipe, CommonModule } from "@angular/common";
import { FormDataService } from "@core/services/form-data.service";

@Component({
    selector: 'app-quote',
    templateUrl: './quote.component.html',
    styleUrls: ['./quote.component.scss'],
    imports: [
        CommonModule,
    ],
})
export class QuoteComponent {

    readonly requestForm: FormGroup;
    private readonly getQuotesSubject$ = new Subject<FormData>();
    readonly quotes$ = this.getQuotesSubject$.pipe(
        switchMap((formData) => this.quoteService.getQuotes(formData)),
        shareReplay()
    );

    constructor(
        private readonly fb: FormBuilder,
        private readonly quoteService: QuoteService,
        private readonly formDataService: FormDataService
    ) {
        this.requestForm = this.fb.group({
            page: this.fb.control<number>(1),
            tag: this.fb.control<string>(''),
        });
    }

    fetchQuotes() {
        if (this.requestForm.invalid) {
            console.error('Form is invalid');
            return;
        }
        this.getQuotesSubject$.next(this.formDataService.toFormData(this.requestForm));
    }
}
