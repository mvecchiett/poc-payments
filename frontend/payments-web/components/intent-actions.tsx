"use client";

import { useMemo, useState } from "react";
import { paymentsApi } from "@/lib/api";
import type { PaymentIntent } from "@/types/payment-intent";
import { StatusBadge } from "./status-badge";

export function IntentActions() {
  const [amount, setAmount] = useState<number>(1000);
  const [currency, setCurrency] = useState<string>("ARS");
  const [description, setDescription] = useState<string>("Demo intent");

  const [id, setId] = useState<string>("");
  const [intent, setIntent] = useState<PaymentIntent | null>(null);

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>("");

  const canOperate = useMemo(() => !!id.trim(), [id]);

  async function run<T>(fn: () => Promise<T>) {
    setBusy(true);
    setError("");
    try {
      return await fn();
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
      throw e;
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-xl border p-4">
        <h2 className="text-lg font-semibold">Crear Payment Intent</h2>

        <div className="mt-4 grid gap-3 sm:grid-cols-3">
          <label className="grid gap-1">
            <span className="text-sm text-slate-600">Amount</span>
            <input
              className="rounded-lg border px-3 py-2"
              type="number"
              value={amount}
              onChange={(e) => setAmount(Number(e.target.value))}
              min={1}
            />
          </label>

          <label className="grid gap-1">
            <span className="text-sm text-slate-600">Currency</span>
            <input
              className="rounded-lg border px-3 py-2"
              value={currency}
              onChange={(e) => setCurrency(e.target.value.toUpperCase())}
            />
          </label>

          <label className="grid gap-1 sm:col-span-3">
            <span className="text-sm text-slate-600">Description</span>
            <input
              className="rounded-lg border px-3 py-2"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </label>
        </div>

        <div className="mt-4 flex items-center gap-3">
          <button
            className="rounded-lg bg-black px-4 py-2 text-white disabled:opacity-50"
            disabled={busy}
            onClick={() =>
              run(async () => {
                const created = await paymentsApi.createIntent({
                  amount,
                  currency,
                  description,
                });
                setIntent(created);
                setId(created.id);
              })
            }
          >
            {busy ? "Procesando..." : "Crear"}
          </button>

          {intent?.id && (
            <span className="text-sm text-slate-600">
              id: <span className="font-mono">{intent.id}</span>
            </span>
          )}
        </div>
      </section>

      <section className="rounded-xl border p-4">
        <h2 className="text-lg font-semibold">Operar Intent</h2>

        <div className="mt-4 grid gap-3 sm:grid-cols-3">
          <label className="grid gap-1 sm:col-span-2">
            <span className="text-sm text-slate-600">Intent Id</span>
            <input
              className="rounded-lg border px-3 py-2 font-mono"
              value={id}
              onChange={(e) => setId(e.target.value)}
              placeholder="pi_..."
            />
          </label>

          <div className="flex items-end">
            <button
              className="w-full rounded-lg border px-4 py-2 disabled:opacity-50"
              disabled={!canOperate || busy}
              onClick={() =>
                run(async () => {
                  const data = await paymentsApi.getIntent(id.trim());
                  setIntent(data);
                })
              }
            >
              Refrescar
            </button>
          </div>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <button
            className="rounded-lg border px-4 py-2 disabled:opacity-50"
            disabled={!canOperate || busy}
            onClick={() =>
              run(async () => {
                const data = await paymentsApi.confirmIntent(id.trim());
                setIntent(data);
              })
            }
          >
            Confirm
          </button>

          <button
            className="rounded-lg border px-4 py-2 disabled:opacity-50"
            disabled={!canOperate || busy}
            onClick={() =>
              run(async () => {
                const data = await paymentsApi.captureIntent(id.trim());
                setIntent(data);
              })
            }
          >
            Capture
          </button>

          <button
            className="rounded-lg border px-4 py-2 disabled:opacity-50"
            disabled={!canOperate || busy}
            onClick={() =>
              run(async () => {
                const data = await paymentsApi.reverseIntent(id.trim());
                setIntent(data);
              })
            }
          >
            Reverse
          </button>
        </div>

        {intent && (
          <div className="mt-6 rounded-lg bg-slate-50 p-4">
            <div className="flex items-center justify-between gap-3">
              <div className="text-sm text-slate-600">
                Status: <StatusBadge status={intent.status} />
              </div>
              <div className="text-sm text-slate-600">
                Amount: <span className="font-semibold">{intent.amount}</span>{" "}
                {intent.currency}
              </div>
            </div>

            <pre className="mt-4 overflow-auto rounded-lg bg-white p-3 text-xs">
{JSON.stringify(intent, null, 2)}
            </pre>
          </div>
        )}

        {error && (
          <div className="mt-4 rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800">
            {error}
          </div>
        )}
      </section>
    </div>
  );
}
