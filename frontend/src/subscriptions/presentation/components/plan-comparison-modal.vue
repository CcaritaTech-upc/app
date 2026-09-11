<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  visible: { type: Boolean, default: false },
  plans: { type: Array, default: () => [] },
  currentPlanId: { type: Number, default: null },
  isProcessing: { type: Boolean, default: false }
});

const emit = defineEmits(["close", "select-plan"]);

const comparisonRows = [
  {
    feature: "Dispositivos IoT soportados",
    starter: "Hasta 50 dispositivos",
    pro: "Hasta 200 dispositivos",
    enterprise: "Ilimitados"
  },
  {
    feature: "Proyectos en simultáneo",
    starter: "Hasta 5 proyectos",
    pro: "Hasta 15 proyectos",
    enterprise: "Ilimitados"
  },
  {
    feature: "Cuentas de Administrador",
    starter: "1 administrador",
    pro: "3 administradores",
    enterprise: "Ilimitados"
  },
  {
    feature: "Frecuencia de telemetría y reportes",
    starter: "Reportes mensuales básicos",
    pro: "En tiempo real + analítica avanzada",
    enterprise: "Suite corporativa completa"
  },
  {
    feature: "Nivel de Soporte Técnico",
    starter: "Email estándar",
    pro: "Prioritario 24/7 (Chat & Tickets)",
    enterprise: "Ingeniero dedicado 24/7"
  },
  {
    feature: "Garantía de SLA",
    starter: "Estándar",
    pro: "99.5% uptime garantizado",
    enterprise: "99.9% contractual"
  },
  {
    feature: "Acceso a API de Telemetría",
    starter: "No disponible",
    pro: "API Personalizada",
    enterprise: "API Completa + Consultoría"
  }
];
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :style="{ width: '850px', maxWidth: '96vw' }"
    :header="t('subscriptions.compare-modal-title')"
  >
    <div class="table-wrapper">
      <table class="comparison-table">
        <thead>
          <tr>
            <th class="th-feature">Característica</th>
            <th
              v-for="plan in plans"
              :key="plan.id"
              class="th-plan"
              :class="{ 'th-pro': plan.name?.toLowerCase().includes('pro') }"
            >
              <div class="th-plan-name">{{ plan.name }}</div>
              <div class="th-plan-price">${{ plan.price }}/{{ t('subscriptions.month') }}</div>
              <div v-if="plan.id === currentPlanId" class="th-current-badge">
                {{ t('subscriptions.current-plan-badge') }}
              </div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, idx) in comparisonRows" :key="idx" class="table-row">
            <td class="td-feature">{{ row.feature }}</td>
            <td class="td-val">{{ row.starter }}</td>
            <td class="td-val td-pro">{{ row.pro }}</td>
            <td class="td-val td-enterprise">{{ row.enterprise }}</td>
          </tr>
        </tbody>
      </table>

      <div class="modal-footer">
        <button type="button" class="btn-close" @click="emit('close')">
          Cerrar
        </button>
      </div>
    </div>
  </pv-dialog>
</template>

<style scoped>
.table-wrapper {
  overflow-x: auto;
  padding: 0.5rem 0;
  font-family: inherit;
}

.comparison-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 0.82rem;
}

.th-feature {
  padding: 1rem;
  font-size: 0.72rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  border-bottom: 2px solid #e2e8f0;
  width: 34%;
}

.th-plan {
  padding: 1rem;
  text-align: center;
  border-bottom: 2px solid #e2e8f0;
  width: 22%;
}

.th-pro {
  background: #ecfdf5;
  border-radius: 0.75rem 0.75rem 0 0;
  border-color: #a7f3d0;
}

.th-plan-name {
  font-size: 1rem;
  font-weight: 800;
  color: #0f172a;
}

.th-plan-price {
  font-size: 0.85rem;
  font-weight: 900;
  color: #059669;
  margin-top: 0.2rem;
}

.th-current-badge {
  display: inline-block;
  background: #334155;
  color: #ffffff;
  font-size: 0.65rem;
  font-weight: 700;
  padding: 0.15rem 0.55rem;
  border-radius: 9999px;
  margin-top: 0.4rem;
}

.table-row {
  transition: background-color 0.15s;
}

.table-row:hover {
  background-color: #f8fafc;
}

.td-feature {
  padding: 0.85rem 1rem;
  color: #334155;
  font-weight: 600;
  border-bottom: 1px solid #f1f5f9;
}

.td-val {
  padding: 0.85rem 1rem;
  text-align: center;
  color: #64748b;
  border-bottom: 1px solid #f1f5f9;
}

.td-pro {
  background: #f0fdf4;
  color: #065f46;
  font-weight: 700;
}

.td-enterprise {
  color: #0f172a;
  font-weight: 600;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  padding-top: 1.5rem;
}

.btn-close {
  background: #f1f5f9;
  border: 1px solid #e2e8f0;
  color: #475569;
  font-weight: 700;
  font-size: 0.85rem;
  padding: 0.6rem 1.5rem;
  border-radius: 0.65rem;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}

.btn-close:hover {
  background: #e2e8f0;
  color: #0f172a;
}
</style>
