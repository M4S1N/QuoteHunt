import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { QuoteComponent } from './quote.component';
import { QuoteService } from '../services/quote.service';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('QuoteComponent', () => {
  let component: QuoteComponent;
  let fixture: ComponentFixture<QuoteComponent>;
  let quoteServiceSpy: jasmine.SpyObj<QuoteService>;

  beforeEach(async () => {
    quoteServiceSpy = jasmine.createSpyObj('QuoteService', ['getQuotes']);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, QuoteComponent, NoopAnimationsModule],
      providers: [
        FormBuilder,
        { provide: QuoteService, useValue: quoteServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuoteComponent);
    component = fixture.componentInstance;
  });

  it('should initialize form with default values and call getQuotes', fakeAsync(() => {
    const mockQuotes = [{ text: 'Test', author: 'Author', tags: ['tag1'] }];
    quoteServiceSpy.getQuotes.and.returnValue(of(mockQuotes));

    fixture.detectChanges();
    tick();

    expect(component.requestForm.value).toEqual({ page: '1', tag: '' });
    expect(quoteServiceSpy.getQuotes).toHaveBeenCalledWith('1', '');
    
    component.quotes$.subscribe(q => {
      expect(q).toEqual(mockQuotes);
    });
  }));
});
