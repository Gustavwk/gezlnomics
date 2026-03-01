import { FormEvent, useState } from 'react';
import { frekvenser, transaktionstyper } from '../../constants/labels';
import type { CreateRecurringRulePayload, RecurringRule } from '../../types/models';

type Props = {
  fasteUdgifter: RecurringRule[];
  onCreate: (payload: CreateRecurringRulePayload) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
};

export function FasteUdgifterPanel({ fasteUdgifter, onCreate, onDelete }: Props) {
  const iDag = new Date().toISOString().slice(0, 10);
  const [fastTitel, setFastTitel] = useState('');
  const [fastBelob, setFastBelob] = useState('0');
  const [fastKategori, setFastKategori] = useState('Faste udgifter');
  const [fastNote, setFastNote] = useState('');
  const [fastType, setFastType] = useState('1');
  const [fastFrekvens, setFastFrekvens] = useState('1');
  const [fastStartDato, setFastStartDato] = useState(iDag);
  const [fastSlutDato, setFastSlutDato] = useState('');

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onCreate({
      title: fastTitel,
      amount: Number(fastBelob),
      category: fastKategori,
      note: fastNote || null,
      ruleKind: Number(fastType),
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
    <section className="panel">
      <h2>Faste udgifter</h2>
      <p className="hint">Faste udgifter er tilbagevendende poster (fx husleje, internet, abonnementer).</p>

      <form onSubmit={handleSubmit} className="grid fire">
        <label>
          Titel
          <input value={fastTitel} onChange={(e) => setFastTitel(e.target.value)} required />
        </label>
        <label>
          Beløb
          <input type="number" step="0.01" value={fastBelob} onChange={(e) => setFastBelob(e.target.value)} required />
        </label>
        <label>
          Kategori
          <input value={fastKategori} onChange={(e) => setFastKategori(e.target.value)} required />
        </label>
        <label>
          Type
          <select value={fastType} onChange={(e) => setFastType(e.target.value)}>
            {Object.entries(transaktionstyper).map(([v, l]) => (
              <option key={v} value={v}>
                {l}
              </option>
            ))}
          </select>
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
        <label>
          Note
          <input value={fastNote} onChange={(e) => setFastNote(e.target.value)} />
        </label>
        <button type="submit">Tilføj fast udgift</button>
      </form>

      <ul className="liste">
        {fasteUdgifter.map((f) => (
          <li key={f.id} className="row mellem">
            <span>
              {f.title} • {transaktionstyper[f.ruleKind]} • {frekvenser[f.frequency]} • {f.amount.toFixed(2)}
            </span>
            <button onClick={() => onDelete(f.id)}>Slet</button>
          </li>
        ))}
      </ul>
    </section>
  );
}
