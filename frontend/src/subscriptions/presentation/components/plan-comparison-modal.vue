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
    category: "Capacidad & Conectividad",
    feature: "Dispositivos IoT soportados",
    starter: "Hasta 50 dispositivos",
    pro: "Hasta 200 dispositivos",
    enterprise: "Ilimitados"
  },
  {
    category: "Capacidad & Conectividad",
    feature: "Proyectos en simultáneo",
    starter: "Hasta 5 proyectos",
    pro: "Hasta 15 proyectos",
    enterprise: "Ilimitados"
  },
  {
    category: "Usuarios & Permisos",
    feature: "Cuentas de Administrador",
    starter: "1 administrador",
    pro: "3 administradores",
    enterprise: "Ilimitados"
  },
  {
    category: "Analítica & Reportes",
    feature: "Frecuencia de telemetría y reportes",
    starter: "Reportes mensuales básicos",
    pro: "En tiempo real + analítica avanzada",
    enterprise: "Suite analítica corporativa completa"
  },
  {
    category: "Soporte & Disponibilidad",
    feature: "Nivel de Soporte Técnico",
    starter: "Email (respuesta en 48h)",
    pro: "Prioritario 24/7 (chat & tickets)",
    enterprise: "Ingeniero dedicado 24/7"
  },
  {
    category: "Soporte & Disponibilidad",
    feature: "Garantía de SLA",
    starter: "Estándar",
    pro: "99.5% uptime garantizado",
    enterprise: "99.9% uptime con SLA contractual"
  },
  {
    category: "Integraciones",
    feature: "Acceso a API de Telemetría",
    starter: "No disponible",
    pro: "API Personalizada",
    enterprise: "API Completa + Consultoría técnica"
  }
];

const handleSelect = (plan) => {
  if (plan && plan.id !== props.currentPlanId && !props.isProcessing) {
    emit("select-plan", plan);
  }
};
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :style="{ width: '900px', maxWidth: '96vw' }"
    :header="t('subscriptions.compare-modal-title')"
  >
    <div class="py-2 overflow-x-auto">
      <table class="w-full text-left border-collapse text-sm">
        <thead>
          <tr class="border-b border-gray-200">
            <th class="py-3 px-4 font-semibold text-gray-500 text-xs uppercase tracking-wider w-1/3">
              Característica
            </th>
            <th
              v-for="plan in plans"
              :key="plan.id"
              class="py-3 px-4 text-center w-1/5"
              :class="plan.name?.toLowerCase().includes('pro') ? 'bg-emerald-50/50 rounded-t-lg' : ''"
            >
              <div class="font-bold text-gray-900 text-sm">{{ plan.name }}</div>
              <div class="text-xs text-emerald-600 font-extrabold mt-0.5">
                ${{ plan.price }}/{{ t('subscriptions.month') }}
              </div>
              <div v-if="plan.id === currentPlanId" class="mt-1">
                <span class="bg-gray-100 text-gray-700 text-[10px] font-bold px-2 py-0.5 rounded-full">
                  {{ t('subscriptions.current-plan-badge') }}
                </span>
              </div>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="(row, idx) in comparisonRows"
            :key="idx"
            class="hover:bg-slate-50/60 transition-colors"
          >
            <td class="py-3 px-4 text-gray-700 font-medium text-xs">
              {{ row.feature }}
            </td>
            <td class="py-3 px-4 text-center text-xs text-gray-600">
              {{ row.starter }}
            </td>
            <td class="py-3 px-4 text-center text-xs font-semibold text-emerald-800 bg-emerald-50/30">
              {{ row.pro }}
            </td>
            <td class="py-3 px-4 text-center text-xs text-gray-800 font-medium">
              {{ row.enterprise }}
            </td>
          </tr>
        </tbody>
      </table>

      <div class="mt-6 flex justify-end">
        <pv-button
          label="Cerrar"
          severity="secondary"
          text
          @click="emit('close')"
        />
      </div>
    </div>
  </pv-dialog>
</template>
