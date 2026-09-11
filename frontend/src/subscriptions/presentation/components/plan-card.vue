<script setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  plan: { type: Object, required: true },
  currentPlan: { type: Object, default: null },
  isProcessing: { type: Boolean, default: false },
  isPopular: { type: Boolean, default: false }
});

const emit = defineEmits(["select"]);

const isCurrent = computed(() => {
  return props.currentPlan && Number(props.currentPlan.id) === Number(props.plan.id);
});

const isUpgrade = computed(() => {
  if (!props.currentPlan) return false;
  return Number(props.plan.price) > Number(props.currentPlan.price);
});

const planFeatures = computed(() => {
  const p = props.plan;
  if (!p) return [];
  if (Array.isArray(p.features) && p.features.length > 0) return p.features;
  if (typeof p.featuresJson === "string" && p.featuresJson.trim()) {
    try {
      const parsed = JSON.parse(p.featuresJson);
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    } catch (_) {}
  }
  if (typeof p.features === "string" && p.features.trim()) {
    try {
      const parsed = JSON.parse(p.features);
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    } catch (_) {}
  }
  return [];
});

const buttonLabel = computed(() => {
  if (isCurrent.value) return t("subscriptions.current-plan-badge");
  if (!props.currentPlan) return "Elegir " + props.plan.name;
  if (isUpgrade.value) return t("subscriptions.upgrade");
  return t("subscriptions.downgrade");
});
</script>

<template>
  <div
    class="plan-card flex flex-col justify-between p-6 bg-white rounded-2xl transition-all duration-200 relative border"
    :class="[
      isPopular
        ? 'border-2 border-emerald-500 shadow-lg shadow-emerald-500/10 ring-1 ring-emerald-500'
        : isCurrent
        ? 'border-gray-300 bg-slate-50/50 shadow-sm'
        : 'border-gray-200 hover:border-gray-300 hover:shadow-md',
      props.isProcessing ? 'opacity-60 pointer-events-none' : ''
    ]"
  >
    <!-- Badges -->
    <div v-if="isPopular" class="absolute -top-3 left-1/2 -translate-x-1/2 bg-emerald-600 text-white text-[11px] font-extrabold uppercase tracking-wider px-3 py-1 rounded-full shadow-sm flex items-center gap-1 whitespace-nowrap">
      <i class="pi pi-star-fill text-[10px]"></i>
      <span>{{ t('subscriptions.most-popular') }}</span>
    </div>

    <div v-else-if="isCurrent" class="absolute -top-3 left-1/2 -translate-x-1/2 bg-slate-700 text-white text-[11px] font-bold uppercase tracking-wider px-3 py-1 rounded-full shadow-sm whitespace-nowrap">
      {{ t('subscriptions.current-plan-badge') }}
    </div>

    <div>
      <!-- Header -->
      <div class="mb-4 text-center sm:text-left">
        <h3 class="text-xl font-bold text-gray-900 mb-1">
          {{ plan.name }}
        </h3>
        <p class="text-xs text-gray-500 min-h-[32px] line-clamp-2 leading-relaxed">
          {{ plan.description }}
        </p>
      </div>

      <!-- Price -->
      <div class="mb-6 pb-6 border-b border-gray-100 text-center sm:text-left">
        <div class="flex items-baseline justify-center sm:justify-start gap-1">
          <span class="text-4xl font-extrabold text-gray-900 tracking-tight">
            ${{ plan.price }}
          </span>
          <span class="text-sm font-medium text-gray-500">/{{ t("subscriptions.month") }}</span>
        </div>
        <span class="text-[11px] text-gray-400 font-medium">Facturación mensual</span>
      </div>

      <!-- Features -->
      <div class="mb-6">
        <span class="text-xs font-bold text-gray-700 uppercase tracking-wider block mb-3">
          Incluye:
        </span>
        <ul class="space-y-2.5">
          <li
            v-for="(feature, i) in planFeatures"
            :key="i"
            class="flex items-start text-xs text-gray-600 leading-relaxed"
          >
            <div class="w-4 h-4 rounded-full bg-emerald-100 flex items-center justify-center shrink-0 mr-2.5 mt-0.5">
              <i class="pi pi-check text-emerald-700 text-[9px] font-bold"></i>
            </div>
            <span>{{ feature }}</span>
          </li>
        </ul>
      </div>
    </div>

    <!-- Action button -->
    <div class="pt-4 border-t border-gray-50">
      <pv-button
        :label="buttonLabel"
        :disabled="isCurrent || props.isProcessing"
        :loading="props.isProcessing"
        class="w-full font-semibold py-2.5 px-4 rounded-xl text-xs transition-all flex justify-center items-center gap-2"
        :class="[
          isCurrent
            ? 'bg-gray-100 text-gray-500 border border-gray-200 cursor-not-allowed'
            : isPopular || isUpgrade
            ? 'bg-emerald-600 hover:bg-emerald-700 text-white border-none shadow-sm shadow-emerald-600/30'
            : 'bg-white hover:bg-gray-50 text-gray-700 border border-gray-300'
        ]"
        @click="!isCurrent && !props.isProcessing && $emit('select', plan)"
      />
    </div>
  </div>
</template>

<style scoped>
.plan-card {
  min-height: 480px;
}
</style>
