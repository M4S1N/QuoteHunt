import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppFooterComponent } from '@shared/components/footer/footer.component';
import { AppHeaderComponent } from '@shared/components/header/header.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    AppHeaderComponent,
    AppFooterComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('QuoteHuntWebSite');
}
