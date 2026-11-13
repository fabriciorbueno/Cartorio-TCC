import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { CpfCnpjPipe, TelefoneBrPipe } from './shared/pipes/documento.pipe';

describe('SAEC CartorioTcc Frontend', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('deve instanciar o componente raiz App', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('deve formatar CPF e CNPJ corretamente pelo CpfCnpjPipe', () => {
    const pipe = new CpfCnpjPipe();
    expect(pipe.transform('52998224725')).toBe('529.982.247-25');
    expect(pipe.transform('46017488000185')).toBe('46.017.488/0001-85');
  });

  it('deve formatar telefone corretamente pelo TelefoneBrPipe', () => {
    const pipe = new TelefoneBrPipe();
    expect(pipe.transform('1932314455')).toBe('(19) 3231-4455');
    expect(pipe.transform('19988112233')).toBe('(19) 98811-2233');
  });
});
