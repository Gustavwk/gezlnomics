import { FormEvent, useState } from 'react';
import { transaktionstyper } from '../../constants/labels';
import type { CreateTransactionPayload, Transaction } from '../../types/models';

type Props = {
  transaktioner: Transaction[];
  onCreate: (payload: CreateTransactionPayload) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
};

export function TransactionsPanel({ transaktioner, onCreate, onDelete }: Props) {
  const iDag = new Date().toISOString().slice(0, 10);
  const [txDato, setTxDato] = useState(iDag);
  const [txBelob, setTxBelob] = useState('0');
  const [txKategori, setTxKategori] = useState('Diverse');
  const [txNote, setTxNote] = useState('');
  const [txType, setTxType] = useState('0');

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onCreate({
      date: txDato,
      amount: Number(txBelob),
      category: txKategori,
      note: txNote || null,
      kind: Number(txType),
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
          Kategori
          <input value={txKategori} onChange={(e) => setTxKategori(e.target.value)} required />
        </label>
        <label>
          Type
          <select value={txType} onChange={(e) => setTxType(e.target.value)}>
            {Object.entries(transaktionstyper).map(([v, l]) => (
              <option key={v} value={v}>
                {l}
              </option>
            ))}
          </select>
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

      <ul className="liste">
        {transaktioner.map((t) => (
          <li key={t.id} className="row mellem">
            <span>
              {t.date} • {transaktionstyper[t.kind]} • {t.category} • {t.amount.toFixed(2)}
            </span>
            <button onClick={() => onDelete(t.id)}>Slet</button>
          </li>
        ))}
      </ul>
    </section>
  );
}
