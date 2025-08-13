import { Component, EventEmitter, Input, Output } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Quote } from "@shared/models/quote.model";
import { MatCardModule } from "@angular/material/card";
import { MatChipsModule } from "@angular/material/chips";

@Component({
    selector: 'app-quote-card',
    templateUrl: './quote-card.component.html',
    styleUrls: ['./quote-card.component.scss'],
    imports: [
        CommonModule,
        MatCardModule,
        MatChipsModule,
    ],
})
export class QuoteCardComponent {

    @Input() quote: Quote = { text: '', author: '', tags: [] };
    @Output() onTagClick: EventEmitter<string> = new EventEmitter<string>();
}
