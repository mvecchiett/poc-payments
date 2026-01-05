"use client";

import { useEffect, useState } from "react";
import type { PaymentIntent, PaymentIntentStatus } from "@/types/payment-intent";
import { getAllPaymentIntents } from "@/lib/payment-intents";
import { PaymentIntentRow } from "./payment-intent-row";

type FilterOption = "active" | "all" | PaymentIntentStatus;

/**
 * Componente principal que lista payment intents con filtros
 * - Fetch inicial al montar
 * - Filtros por estado (client-side)
 * - Botón de refresh
 * - Optimistic update después de acciones
 */
export function PaymentIntentList() {
  const [intents, setIntents] = useState<PaymentIntent[]>([]);
  const [filter, setFilter] = useState<FilterOption>("active");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch inicial
  useEffect(() => {
    loadIntents();
  }, []);

  async function loadIntents() {
    setLoading(true);
    setError(null);

    try {
      const data = await getAllPaymentIntents();
      setIntents(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
    } finally {
      setLoading(false);
    }
  }

  // Optimistic update cuando una acción tiene éxito
  function handleIntentUpdate(updatedIntent: PaymentIntent) {
    setIntents((prev) =>
      prev.map((intent) =>
        intent.id === updatedIntent.id ? updatedIntent : intent
      )
    );
  }

  // Filtrado client-side
  const filteredIntents = intents.filter((intent) => {
    if (filter === "all") return true;
    if (filter === "active") {
      return intent.status === "Created" || intent.status === "PendingConfirmation";
    }
    return intent.status === filter;
  });

  return (
    <div className="space-y-4">
      {/* Header con filtros y refresh */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h2 className="text-xl font-semibold text-slate-900">
          Payment Intents
        </h2>

        <button
          onClick={loadIntents}
          disabled={loading}
          className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {loading ? "Cargando..." : "↻ Refresh"}
        </button>
      </div>

      {/* Filtros */}
      <div className="flex flex-wrap gap-2">
        <FilterButton
          active={filter === "active"}
          onClick={() => setFilter("active")}
        >
          Activos
        </FilterButton>

        <FilterButton
          active={filter === "all"}
          onClick={() => setFilter("all")}
        >
          Todos
        </FilterButton>

        <div className="w-px bg-slate-200" />

        <FilterButton
          active={filter === "Created"}
          onClick={() => setFilter("Created")}
        >
          Created
        </FilterButton>

        <FilterButton
          active={filter === "PendingConfirmation"}
          onClick={() => setFilter("PendingConfirmation")}
        >
          Pending
        </FilterButton>

        <FilterButton
          active={filter === "Captured"}
          onClick={() => setFilter("Captured")}
        >
          Captured
        </FilterButton>

        <FilterButton
          active={filter === "Reversed"}
          onClick={() => setFilter("Reversed")}
        >
          Reversed
        </FilterButton>

        <FilterButton
          active={filter === "Expired"}
          onClick={() => setFilter("Expired")}
        >
          Expired
        </FilterButton>
      </div>

      {/* Estados: loading, error, empty, lista */}
      {loading && !intents.length && (
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-8 text-center text-slate-600">
          Cargando payment intents...
        </div>
      )}

      {error && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-rose-700">
          <strong>Error:</strong> {error}
        </div>
      )}

      {!loading && !error && filteredIntents.length === 0 && (
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-8 text-center text-slate-600">
          {intents.length === 0
            ? "No hay payment intents todavía. Creá uno desde Swagger o el formulario."
            : `No hay intents con filtro "${filter}".`}
        </div>
      )}

      {!loading && !error && filteredIntents.length > 0 && (
        <div className="space-y-3">
          <div className="text-sm text-slate-600">
            Mostrando {filteredIntents.length} de {intents.length} intents
          </div>

          {filteredIntents.map((intent) => (
            <PaymentIntentRow
              key={intent.id}
              intent={intent}
              onUpdate={handleIntentUpdate}
            />
          ))}
        </div>
      )}
    </div>
  );
}

// Componente auxiliar para botones de filtro
function FilterButton({
  active,
  onClick,
  children,
}: {
  active: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      onClick={onClick}
      className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
        active
          ? "bg-slate-900 text-white"
          : "bg-slate-100 text-slate-700 hover:bg-slate-200"
      }`}
    >
      {children}
    </button>
  );
}
