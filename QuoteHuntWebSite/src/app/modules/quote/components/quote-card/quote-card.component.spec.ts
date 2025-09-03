import { ComponentFixture, TestBed } from '@angular/core/testing';
import { QuoteCardComponent } from './quote-card.component';
import { By } from '@angular/platform-browser';
import { Quote } from '@shared/models/quote.model';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('QuoteCardComponent', () => {
  let component: QuoteCardComponent;
  let fixture: ComponentFixture<QuoteCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuoteCardComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(QuoteCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should render quote text, author and tags', () => {
    const mockQuote: Quote = { text: 'Test Quote', author: 'Author', tags: ['tag1', 'tag2'] };
    component.quote = mockQuote;
    component.tagSelected = 'tag1';
    fixture.detectChanges();

    const cardElement: HTMLElement = fixture.nativeElement;
    expect(cardElement.textContent).toContain(mockQuote.text);
    expect(cardElement.textContent).toContain(mockQuote.author);
    expect(cardElement.textContent).toContain('tag1');
    expect(cardElement.textContent).toContain('tag2');
  });

  it('should emit onTagClick when emit is called', () => {
		spyOn(component.onTagClick, 'emit');
		const tag = 'tag1';
		component.onTagClick.emit(tag);
		expect(component.onTagClick.emit).toHaveBeenCalledWith(tag);
	});
});
