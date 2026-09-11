<script setup>
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  visible: { type: Boolean, default: false },
  invoices: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  error: { type: String, default: "" }
});

const emit = defineEmits(["close"]);

const openReceipt = (url) => {
  if (url) window.open(url, "_blank");
};
</script>

<template>
  <pv-dialog
    :visible="props.visible"
    @update:visible="emit('close')"
    :modal="true"
    :style="{ width: '740px', maxWidth: '95vw' }"
    :header="t('subscriptions.invoices-title')"
  >
    <div v-if="props.loading" class="flex flex-col justify-center items-center py-12 gap-3">
      <pv-progress-spinner style="width: 42px; height: 42px" />
      <span class="text-xs text-gray-500">Cargando recibos de Stripe...</span>
    </div>

    <div v-else class="py-1">
      <div
        v-if="props.error"
        class="bg-red-50 text-red-700 text-xs p-3 rounded-lg border border-red-200 mb-4 flex items-center gap-2"
      >
        <i class="pi pi-exclamation-circle text-red-600"></i>
        <span>{{ props.error }}</span>
      </div>

      <div
        v-if="!props.invoices.length && !props.error"
        class="flex flex-col items-center justify-center py-12 text-center"
      >
        <div class="w-16 h-16 rounded-full bg-slate-100 flex items-center justify-center mb-3">
          <i class="pi pi-receipt text-2xl text-slate-400"></i>
        </div>
        <h4 class="text-sm font-semibold text-gray-800 mb-1">
          {{ t('subscriptions.no-invoices-found') }}
        </h4>
        <p class="text-xs text-gray-500 max-w-sm">
          Tus facturas y recibos de pagos procesados aparecerán aquí automáticamente para su descarga.
        </p>
      </div>

      <div v-else-if="props.invoices.length" class="overflow-x-auto">
        <pv-data-table
          :value="props.invoices"
          class="p-datatable-sm text-xs"
          striped-rows
          responsive-layout="scroll"
        >
          <pv-column field="date" :header="t('subscriptions.invoice-date')" style="min-width: 120px">
            <template #body="{ data }">
              <span class="font-medium text-gray-700">{{ data.date }}</span>
            </template>
          </pv-column>

          <pv-column field="description" :header="t('subscriptions.invoice-description')" style="min-width: 160px">
            <template #body="{ data }">
              <span class="text-gray-900 font-semibold">{{ data.description || 'Suscripción IoBuild' }}</span>
            </template>
          </pv-column>

          <pv-column :header="t('subscriptions.invoice-amount')" style="min-width: 110px">
            <template #body="{ data }">
              <span class="font-bold text-gray-900">
                ${{ Number(data.amount).toFixed(2) }}
              </span>
              <span class="text-[10px] text-gray-400 ml-1 uppercase">{{ data.currency || 'usd' }}</span>
            </template>
          </pv-column>

          <pv-column :header="t('subscriptions.invoice-status')" style="min-width: 100px">
            <template #body="{ data }">
              <span
                v-if="data.status === 'paid'"
                class="inline-flex items-center gap-1 bg-emerald-50 text-emerald-700 text-[11px] font-semibold px-2 py-0.5 rounded-full border border-emerald-200"
              >
                <i class="pi pi-check text-[9px]"></i>
                {{ t('subscriptions.invoice-status-paid') }}
              </span>
              <span
                v-else
                class="inline-flex items-center gap-1 bg-amber-50 text-amber-700 text-[11px] font-semibold px-2 py-0.5 rounded-full border border-amber-200"
              >
                <i class="pi pi-clock text-[9px]"></i>
                {{ t('subscriptions.invoice-status-pending') }}
              </span>
            </template>
          </pv-column>

          <pv-column :header="t('subscriptions.invoice-receipt')" style="min-width: 110px" class="text-right">
            <template #body="{ data }">
              <pv-button
                v-if="data.downloadUrl"
                :label="t('subscriptions.download-receipt')"
                icon="pi pi-download"
                size="small"
                text
                class="text-emerald-600 hover:text-emerald-700 p-0 text-xs font-semibold"
                @click="openReceipt(data.downloadUrl)"
              />
              <span v-else class="text-gray-400 text-xs">—</span>
            </template>
          </pv-column>
        </pv-data-table>
      </div>

      <div class="mt-5 flex justify-end pt-3 border-t border-gray-100">
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
