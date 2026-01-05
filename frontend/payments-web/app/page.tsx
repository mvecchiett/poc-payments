import { IntentActions } from "@/components/intent-actions";

export default function HomePage() {
  return (
    <main className="mx-auto max-w-3xl p-6">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold">POC Payments</h1>
        <p className="text-slate-600">
          Frontend Next.js consumiendo API .NET (Payment Intents).
        </p>
      </header>

      <div className="mt-6">
        <IntentActions />
      </div>
    </main>
  );
}
