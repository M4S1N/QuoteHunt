import { Routes } from "@angular/router";
import { QuoteComponent } from "./components/quote.component";

export const routes: Routes = [
    {
        path: '',
        component: QuoteComponent
    },
    {
        path: 'quote',
        component: QuoteComponent
    }
];
