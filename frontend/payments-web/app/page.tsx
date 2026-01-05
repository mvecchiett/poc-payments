import { PaymentIntentList } from "@/components/payment-intent-list";

/**
 * Dashboard principal - Lista de payment intents
 * Client Component delegado a PaymentIntentList
 */
export default function HomePage() {
  return (
    <main className="mx-auto max-w-6xl p-6">
      <header className="mb-6 space-y-2">
        <h1 className="text-3xl font-bold text-slate-900">
          POC Payments - Dashboard
        </h1>
        <p className="text-slate-600">
          Gestión de Payment Intents - Frontend Next.js + Backend .NET
        </p>
      </header>

      <PaymentIntentList />
    </main>
  );
}
