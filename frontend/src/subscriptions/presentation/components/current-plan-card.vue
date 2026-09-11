<script setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  plan: { type: Object, required: true },
  subscription: { type: Object, required: true },
  isProcessing: { type: Boolean, default: false },
  totalDevices: { type: Number, default: 0 },
  activeProjects: { type: Number, default: 0 }
});

defineEmits(["renew", "cancel", "view-invoices", "compare-plans"]);

const isActive = computed(() => {
  return props.subscription?.status?.toLowerCase() === "active" ||
         (typeof props.subscription?.isActive === "function" && props.subscription.isActive());
});

const isCancelled = computed(() => {
  return props.subscription?.status?.toLowerCase() === "cancelled";
});

// Calculate next renewal date (e.g. 30 days after startDate or endDate)
const renewalDateFormatted = computed(() => {
  const start = props.subscription?.startDate ? new Date(props.subscription.startDate) : new Date();
  const next = props.subscription?.endDate ? new Date(props.subscription.endDate) : new Date(start.getTime() + 30 * 24 * 60 * 60 * 1000);
  return next.toLocaleDateString("es-ES", { year: "numeric", month: "long", day: "numeric" });
});

// Quota limits based on plan name
const planLimits = computed(() => {
  const name = props.plan?.name?.toLowerCase() || "";
  if (name.includes("starter")) {
    return { maxDevices: 50, maxProjects: 5, label: "50" };
  }
  if (name.includes("pro")) {
    return { maxDevices: 200, maxProjects: 15, label: "200" };
  }
  return { maxDevices: Infinity, maxProjects: Infinity, label: "Ilimitados" };
});

const devicePercentage = computed(() => {
  if (planLimits.value.maxDevices === Infinity) return 15;
  const pct = Math.round((props.totalDevices / planLimits.value.maxDevices) * 100);
  return Math.min(pct, 100);
});

const isNearLimit = computed(() => {
  return planLimits.value.maxDevices !== Infinity && devicePercentage.value >= 80;
});
</script>

<template>
  <div class="current-plan-card bg-white rounded-2xl border border-gray-200 shadow-sm p-6 mb-8 transition-all">
    <!-- Cancelled notice banner if applicable -->
    <div
      v-if="isCancelled"
      class="bg-amber-50 border border-amber-200 text-amber-800 text-xs px-4 py-2.5 rounded-xl mb-6 flex items-center justify-between gap-3"
    >
      <div class="flex items-center gap-2">
        <i class="pi pi-exclamation-triangle text-amber-600"></i>
        <span>{{ t('subscriptions.cancelled-notice') }}</span>
      </div>
      <span class="font-semibold text-amber-900">Vigente hasta {{ renewalDateFormatted }}</span>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-center">
      <!-- 1. Plan identity & pricing (cols 4) -->
      <div class="lg:col-span-4 border-b lg:border-b-0 lg:border-r border-gray-100 pb-6 lg:pb-0 lg:pr-6">
        <div class="flex items-center gap-2.5 mb-2">
          <span class="text-xs uppercase tracking-wider font-semibold text-gray-400">
            {{ t('subscriptions.current-plan') }}
          </span>
          <span
            class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-bold"
            :class="isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-red-50 text-red-700 border border-red-200'"
          >
            <span class="w-1.5 h-1.5 rounded-full" :class="isActive ? 'bg-emerald-500 animate-pulse' : 'bg-red-500'"></span>
            {{ isActive ? t('subscriptions.status-active') : t('subscriptions.status-cancelled') }}
          </span>
        </div>

        <h2 class="text-2xl font-extrabold text-gray-900 mb-1">
          {{ plan.name }}
        </h2>
        <p class="text-xs text-gray-500 mb-4">{{ plan.description }}</p>

        <div class="flex items-baseline gap-1 mb-2">
          <span class="text-3xl font-black text-gray-900">${{ plan.price }}</span>
          <span class="text-xs text-gray-500 font-medium">/ {{ t('subscriptions.month') }}</span>
        </div>

        <div class="flex items-center gap-2 text-[11px] text-gray-400">
          <i class="pi pi-calendar text-xs"></i>
          <span>{{ t('subscriptions.next-billing') }}: <strong class="text-gray-700 font-semibold">{{ renewalDateFormatted }}</strong></span>
        </div>
      </div>

      <!-- 2. Quota & usage meters (cols 5) -->
      <div class="lg:col-span-5 border-b lg:border-b-0 lg:border-r border-gray-100 pb-6 lg:pb-0 lg:px-6">
        <h4 class="text-xs font-bold text-gray-700 uppercase tracking-wider mb-4 flex items-center gap-1.5">
          <i class="pi pi-chart-bar text-emerald-600"></i>
          {{ t('subscriptions.usage-title') }}
        </h4>

        <!-- Devices quota -->
        <div class="mb-4">
          <div class="flex justify-between items-center text-xs mb-1.5">
            <span class="font-medium text-gray-700">{{ t('subscriptions.usage-devices') }}</span>
            <span class="font-bold text-gray-900">
              {{ props.totalDevices }} / {{ planLimits.label }}
              <span v-if="planLimits.maxDevices !== Infinity" class="text-gray-400 font-normal">({{ devicePercentage }}%)</span>
            </span>
          </div>

          <div class="w-full bg-gray-100 rounded-full h-2.5 overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-500"
              :class="isNearLimit ? 'bg-amber-500' : 'bg-emerald-500'"
              :style="{ width: `${devicePercentage}%` }"
            ></div>
          </div>

          <p v-if="isNearLimit" class="text-[11px] text-amber-600 mt-1 flex items-center gap-1">
            <i class="pi pi-info-circle text-[10px]"></i>
            {{ t('subscriptions.usage-warning') }}
          </p>
        </div>

        <!-- Projects quota -->
        <div>
          <div class="flex justify-between items-center text-xs mb-1.5">
            <span class="font-medium text-gray-700">{{ t('subscriptions.usage-projects') }}</span>
            <span class="font-bold text-gray-900">
              {{ props.activeProjects }} proyectos activos
            </span>
          </div>
          <div class="flex items-center gap-2 text-[11px] text-gray-400">
            <i class="pi pi-shield-check text-emerald-600"></i>
            <span>{{ t('subscriptions.billed-via') }}</span>
          </div>
        </div>
      </div>

      <!-- 3. Actions (cols 3) -->
      <div class="lg:col-span-3 flex flex-col gap-2.5 lg:pl-6 justify-center">
        <pv-button
          :label="isCancelled ? t('subscriptions.reactivate-plan') : t('subscriptions.renew-plan')"
          icon="pi pi-sync"
          :loading="props.isProcessing"
          :disabled="props.isProcessing"
          class="bg-emerald-600 hover:bg-emerald-700 border-none text-white font-semibold py-2 px-3 text-xs rounded-xl shadow-xs justify-center"
          @click="$emit('renew')"
        />

        <pv-button
          :label="t('subscriptions.view-invoices')"
          icon="pi pi-receipt"
          severity="secondary"
          outlined
          class="border-gray-300 text-gray-700 hover:bg-gray-50 py-2 px-3 text-xs rounded-xl justify-center font-medium"
          @click="$emit('view-invoices')"
        />

        <pv-button
          v-if="isActive"
          :label="t('subscriptions.cancel-plan')"
          severity="danger"
          text
          class="text-red-500 hover:text-red-700 text-xs py-1.5 justify-center"
          :disabled="props.isProcessing"
          @click="$emit('cancel')"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.current-plan-card {
  box-shadow: 0 4px 20px -2px rgba(0, 0, 0, 0.05);
}
</style>
