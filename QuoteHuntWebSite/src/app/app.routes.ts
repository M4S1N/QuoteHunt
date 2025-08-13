import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadChildren: () => import('./modules/quote/quote.routes').then(m => m.routes)
    }
];
