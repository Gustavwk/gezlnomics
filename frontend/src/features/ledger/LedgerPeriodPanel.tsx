import { FormEvent } from 'react';

type Props = {
  periodStart: string;
  periodEnd: string;
  startSaldo: string;
  periodeValg: { key: string; label: string }[];
  valgtPeriodeNoegle: string;
  setStartSaldo: (value: string) => void;
  onSaveStartsaldo: () => Promise<void>;
  onVaelgPeriode: (noegle: string) => Promise<void>;
  onOpretNaestePeriode: () => Promise<void>;
};

export function LedgerPeriodPanel({
  periodStart,
  periodEnd,
  startSaldo,
  periodeValg,
  valgtPeriodeNoegle,
  setStartSaldo,
  onSaveStartsaldo,
  onVaelgPeriode,
  onOpretNaestePeriode
}: Props) {
  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await onSaveStartsaldo();
  }

  return (
    <section className="panel">
      <h2>Lønperiode</h2>
      <p>
        {periodStart} til {periodEnd}
      </p>

      <div className="ledger-controls">
        <div className="periode-toolbar">
          <label>
            Måned/periode
            <select value={valgtPeriodeNoegle} onChange={(e) => void onVaelgPeriode(e.target.value)}>
              {periodeValg.map((p) => (
                <option key={p.key} value={p.key}>
                  {p.label}
                </option>
              ))}
            </select>
          </label>
          <button type="button" className="align-end ledger-form-btn" onClick={() => void onOpretNaestePeriode()}>
            Opret næste periode
          </button>
        </div>

        <form onSubmit={handleSubmit} className="startsaldo-form">
          <label>
            Startsaldo for denne lønperiode
            <input
              type="number"
              step="0.01"
              value={startSaldo}
              onChange={(e) => setStartSaldo(e.target.value)}
              required
            />
          </label>
          <button type="submit" className="align-end ledger-form-btn">Gem startsaldo</button>
        </form>
      </div>
    </section>
  );
}
