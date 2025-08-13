import { Routes } from '@angular/router';
import { QuoteComponent } from './modules/quote/components/quote.component';

export const routes: Routes = [
    {
        path: '',
        loadChildren: () => import('./modules/quote/quote.routes').then(m => m.routes)
    }
];
