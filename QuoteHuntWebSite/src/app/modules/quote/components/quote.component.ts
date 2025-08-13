import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AsyncPipe, CommonModule } from '@angular/common';
import { QuoteCardComponent } from './quote-card/quote-card.component';
import { map, merge, shareReplay, startWith, Subject, switchMap } from 'rxjs';
import { QuoteService } from '../services/quote.service';
import { fadeIn, fadeInCard, fadeInContainer } from '@shared/animations/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-quote',
  templateUrl: './quote.component.html',
  styleUrls: ['./quote.component.scss'],
  imports: [
    CommonModule,
    AsyncPipe,
    QuoteCardComponent,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    ReactiveFormsModule
  ],
  animations: [
    fadeInContainer,
    fadeInCard,
    fadeIn
  ]
})
export class QuoteComponent {

  private readonly fb = inject(FormBuilder)
  readonly requestForm = this.fb.group({
    page: this.fb.control<string>('1'),
    tag: this.fb.control<string>(''),
  });

  private readonly quoteSearchSubject$ = new Subject<Partial<{page: string | null, tag: string | null}>>();
  readonly quotes$ = this.quoteSearchSubject$.pipe(
    startWith(this.requestForm.value),
    switchMap((params) => this.quoteService.getQuotes(params.page, params.tag)),
    shareReplay(),
  );

  readonly loading$ = merge(
    this.quoteSearchSubject$.pipe(map(_ => true)),
    this.quotes$.pipe(map(_ => false)),
  ).pipe(startWith(false));

  constructor (
    private readonly quoteService: QuoteService
  ) { }

  onTagSearch(tag?: string) {
    if (!!tag) {
      this.requestForm.setValue({page: '1', tag: tag});
    }
    this.quoteSearchSubject$.next(this.requestForm.value)
  }

  changePage(offset: number) {
    const current$ = +this.requestForm.controls.page.value! || 1;
    const next$ = Math.max(1, current$ + offset);
    this.requestForm.controls.page.setValue(next$.toString());
    this.quoteSearchSubject$.next(this.requestForm.value)
  }

}
