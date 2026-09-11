<script setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  visible: { type: Boolean, default: false },
  targetPlan: { type: Object, default: null },
  currentPlan: { type: Object, default: null },
  isProcessing: { type: Boolean, default: false }
});

const emit = defineEmits(["close", "confirm"]);

const isUpgrade = computed(() => {
  if (!props.currentPlan || !props.targetPlan) return true;
  return Number(props.targetPlan.price) > Number(props.currentPlan.price);
});

const targetFeatures = computed(() => {
  if (!props.targetPlan) return [];
  const p = props.targetPlan;
  if (Array.isArray(p.features) && p.features.length > 0) return p.features;
  if (typeof p.featuresJson === "string" && p.featuresJson.trim()) {
    try {
      const parsed = JSON.parse(p.featuresJson);
      if (Array.isArray(parsed)) return parsed;
    } catch (_) {}
  }
  return [];
});
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :closable="!props.isProcessing"
    :style="{ width: '560px', maxWidth: '95vw' }"
    :header="t('subscriptions.confirm-change-title')"
  >
    <div v-if="targetPlan" class="py-2">
      <p class="text-sm text-gray-500 mb-5">
        {{ t('subscriptions.confirm-change-subtitle') }}
      </p>

      <!-- Comparison cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-3 mb-5 items-stretch">
        <div class="p-3 border border-gray-200 rounded-xl bg-gray-50 flex flex-col justify-between">
          <div>
            <span class="text-xs uppercase tracking-wider font-semibold text-gray-400">
              {{ t('subscriptions.current-tier') }}
            </span>
            <h4 class="font-bold text-gray-800 text-base mt-1">
              {{ currentPlan?.name || 'Starter' }}
            </h4>
          </div>
          <div class="mt-2 text-xl font-extrabold text-gray-600">
            ${{ currentPlan?.price || 0 }}
            <span class="text-xs font-normal text-gray-500">/{{ t('subscriptions.month') }}</span>
          </div>
        </div>

        <div class="p-3 border-2 border-emerald-500 rounded-xl bg-emerald-50/40 flex flex-col justify-between relative shadow-sm">
          <div class="absolute -top-2.5 right-3 bg-emerald-600 text-white text-[10px] font-bold px-2 py-0.5 rounded-full shadow-xs">
            {{ isUpgrade ? t('subscriptions.upgrade') : t('subscriptions.downgrade') }}
          </div>
          <div>
            <span class="text-xs uppercase tracking-wider font-semibold text-emerald-700">
              {{ t('subscriptions.new-tier') }}
            </span>
            <h4 class="font-bold text-gray-900 text-base mt-1">
              {{ targetPlan.name }}
            </h4>
          </div>
          <div class="mt-2 text-2xl font-extrabold text-emerald-600">
            ${{ targetPlan.price }}
            <span class="text-xs font-normal text-gray-500">/{{ t('subscriptions.month') }}</span>
          </div>
        </div>
      </div>

      <!-- Highlights -->
      <div class="bg-slate-50 rounded-xl p-4 mb-5 border border-slate-100">
        <h5 class="text-xs font-bold text-gray-700 uppercase tracking-wider mb-2.5 flex items-center gap-1.5">
          <i class="pi pi-sparkles text-emerald-500"></i>
          {{ t('subscriptions.unlocked-benefits') }}:
        </h5>
        <ul class="space-y-1.5 max-h-48 overflow-y-auto pr-1">
          <li
            v-for="(f, idx) in targetFeatures.slice(0, 5)"
            :key="idx"
            class="text-xs text-gray-700 flex items-center gap-2"
          >
            <i class="pi pi-check text-emerald-600 text-[11px] font-bold"></i>
            <span>{{ f }}</span>
          </li>
        </ul>
      </div>

      <!-- Stripe reassurance badge -->
      <div class="flex items-center gap-2 text-xs text-gray-500 bg-gray-50 p-2.5 rounded-lg border border-gray-100 mb-4">
        <i class="pi pi-shield text-emerald-600 text-base"></i>
        <span>{{ t('subscriptions.secure-stripe') }}</span>
      </div>

      <!-- Action buttons -->
      <div class="flex items-center justify-end gap-3 pt-2">
        <pv-button
          label="Cancelar"
          severity="secondary"
          text
          @click="emit('close')"
          :disabled="props.isProcessing"
        />
        <pv-button
          :label="props.isProcessing ? 'Conectando con Stripe...' : t('subscriptions.proceed-stripe')"
          icon="pi pi-arrow-right"
          iconPos="right"
          class="bg-emerald-600 hover:bg-emerald-700 border-none text-white font-semibold px-4 py-2 text-sm rounded-lg shadow-sm"
          :loading="props.isProcessing"
          @click="emit('confirm')"
        />
      </div>
    </div>
  </pv-dialog>
</template>
