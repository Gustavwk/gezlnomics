import { FormEvent, useState } from 'react';
import { frekvenser } from '../../constants/labels';
import type { CreateRecurringRulePayload, RecurringRule } from '../../types/models';

type Props = {
  fasteUdgifter: RecurringRule[];
  onCreate: (payload: CreateRecurringRulePayload) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
};

export function FasteUdgifterPanel({ fasteUdgifter, onCreate, onDelete }: Props) {
  const fastUdgiftType = 1;
  const iDag = new Date().toISOString().slice(0, 10);
  const [fastTitel, setFastTitel] = useState('');
  const [fastBelob, setFastBelob] = useState('0');
  const [fastNote, setFastNote] = useState('');
  const [fastFrekvens, setFastFrekvens] = useState('1');
  const [fastStartDato, setFastStartDato] = useState(iDag);
  const [fastSlutDato, setFastSlutDato] = useState('');
  const [visListe, setVisListe] = useState(false);
  const antalFasteUdgifter = fasteUdgifter.length;
  const samletFastBeloeb = fasteUdgifter.reduce((sum, f) => sum + Math.abs(f.amount), 0);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onCreate({
      title: fastTitel,
      amount: Number(fastBelob),
      category: 'Faste udgifter',
      note: fastNote || null,
      ruleKind: fastUdgiftType,
      frequency: Number(fastFrekvens),
      startDate: fastStartDato,
      endDate: fastSlutDato || null,
      isActive: true
    });

    setFastTitel('');
    setFastBelob('0');
    setFastNote('');
  }

  return (
    <section className="panel recurring-panel">
      <div className="recurring-head">
        <h2>Faste udgifter</h2>
        <p className="hint">Tilbagevendende poster som husleje, internet og abonnementer.</p>
      </div>

      <form onSubmit={handleSubmit} className="recurring-form">
        <div className="recurring-grid">
          <label>
            Titel
            <input value={fastTitel} onChange={(e) => setFastTitel(e.target.value)} required />
          </label>
          <label>
            Beløb
            <input type="number" step="0.01" value={fastBelob} onChange={(e) => setFastBelob(e.target.value)} required />
          </label>
          <label>
            Frekvens
            <select value={fastFrekvens} onChange={(e) => setFastFrekvens(e.target.value)}>
              {Object.entries(frekvenser).map(([v, l]) => (
                <option key={v} value={v}>
                  {l}
                </option>
              ))}
            </select>
          </label>
          <label>
            Startdato
            <input type="date" value={fastStartDato} onChange={(e) => setFastStartDato(e.target.value)} required />
          </label>
          <label>
            Slutdato (valgfri)
            <input type="date" value={fastSlutDato} onChange={(e) => setFastSlutDato(e.target.value)} />
          </label>
        </div>

        <label className="recurring-note">
          Note (valgfri)
          <input value={fastNote} onChange={(e) => setFastNote(e.target.value)} />
        </label>

        <button type="submit" className="recurring-submit">Tilføj fast udgift</button>
      </form>

      <div className="liste-toolbar">
        <div className="liste-summary">
          <strong>{antalFasteUdgifter}</strong> poster • <strong>{samletFastBeloeb.toFixed(2)}</strong> i alt
        </div>
        <button type="button" className="btn-small" onClick={() => setVisListe((prev) => !prev)}>
          {visListe ? 'Skjul faste udgifter' : 'Vis faste udgifter'}
        </button>
      </div>

      <div className={`collapsible ${visListe ? '' : 'collapsed'}`}>
        <ul className="liste recurring-list">
          {fasteUdgifter.map((f) => (
            <li key={f.id} className="row mellem recurring-item">
              <span className="recurring-item-main">{f.title}</span>
              <span className="recurring-item-meta">{frekvenser[f.frequency]} • {f.amount.toFixed(2)}</span>
              <button onClick={() => onDelete(f.id)}>Slet</button>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
