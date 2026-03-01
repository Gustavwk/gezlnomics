import { FormEvent, useState } from 'react';
import type { CreateTransactionPayload, Transaction } from '../../types/models';

type Props = {
  transaktioner: Transaction[];
  onCreate: (payload: CreateTransactionPayload) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
};

export function TransactionsPanel({ transaktioner, onCreate, onDelete }: Props) {
  const standardUdgiftType = 0;
  const iDag = new Date().toISOString().slice(0, 10);
  const [txDato, setTxDato] = useState(iDag);
  const [txBelob, setTxBelob] = useState('0');
  const [txNote, setTxNote] = useState('');
  const [visListe, setVisListe] = useState(true);
  const antalTransaktioner = transaktioner.length;
  const samletTransaktionsBeloeb = transaktioner.reduce((sum, t) => sum + Math.abs(t.amount), 0);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onCreate({
      date: txDato,
      amount: Number(txBelob),
      category: 'Udgift',
      note: txNote || null,
      kind: standardUdgiftType,
      status: 0
    });
    setTxNote('');
  }

  return (
    <section className="panel">
      <h2>Transaktioner</h2>

      <form onSubmit={handleSubmit} className="grid fire transaction-form">
        <label>
          Dato
          <input type="date" value={txDato} onChange={(e) => setTxDato(e.target.value)} required />
        </label>
        <label>
          Note
          <input value={txNote} onChange={(e) => setTxNote(e.target.value)} />
        </label>
        <label className="span-3">
          Beløb
          <input type="number" step="0.01" value={txBelob} onChange={(e) => setTxBelob(e.target.value)} required />
        </label>
        <button type="submit" className="align-end">Tilføj transaktion</button>
      </form>

      <div className="liste-toolbar">
        <div className="liste-summary">
          <strong>{antalTransaktioner}</strong> poster - <strong>{samletTransaktionsBeloeb.toFixed(2)}</strong> i alt
        </div>
        <button type="button" className="btn-small" onClick={() => setVisListe((prev) => !prev)}>
          {visListe ? 'Skjul udgifter' : 'Vis udgifter'}
        </button>
      </div>

      <div className={`collapsible ${visListe ? '' : 'collapsed'}`}>
        <ul className="liste">
          {transaktioner.map((t) => (
            <li key={t.id} className="row mellem">
              <span>
                {t.date} - {t.category} - {t.amount.toFixed(2)}
                {t.note ? ` - Note: ${t.note}` : ''}
              </span>
              <button onClick={() => onDelete(t.id)}>Slet</button>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
