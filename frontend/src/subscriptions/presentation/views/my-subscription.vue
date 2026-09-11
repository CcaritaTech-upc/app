<script setup>
import { onMounted, ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";

import useSubscriptionStore from "../../application/subscription.store.js";
import { useAnalyticsStore } from "../../../analytics/application/analytics.store.js";
import { IamFacade } from "../../infrastructure/iam.facade.js";
import PlanCard from "../components/plan-card.vue";
import CurrentPlanCard from "../components/current-plan-card.vue";
import PreviousInvoicesModal from "../components/previous-invoices-modal.vue";
import ChangePlanModal from "../components/change-plan-modal.vue";
import PlanComparisonModal from "../components/plan-comparison-modal.vue";
import { SubscriptionApi } from "../../infrastructure/subscription-api.js";
import {
  TOAST_SUCCESS_DURATION_MS,
  TOAST_ERROR_DURATION_MS,
  TOAST_INVOICE_ERROR_DURATION_MS,
  TOAST_AUTH_ERROR_DURATION_MS
} from "../../../shared/infrastructure/constants.js";

// Stripe
import { loadStripe } from "@stripe/stripe-js";

const { t } = useI18n();
const confirm = useConfirm();
const toast = useToast();
const store = useSubscriptionStore();
const analyticsStore = useAnalyticsStore();
const subscriptionApi = new SubscriptionApi();

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY);
const isProcessing = ref(false);

// Invoices modal
const invoicesVisible = ref(false);
const invoicesLoading = ref(false);
const invoices = ref([]);
const invoicesError = ref("");

// Change plan confirmation modal
const changePlanVisible = ref(false);
const targetPlanForChange = ref(null);

// Comparison matrix modal
const comparisonVisible = ref(false);

const getBuilderId = () => {
  try {
    return IamFacade.getCurrentUserId();
  } catch (error) {
    console.error("[Subscriptions] Error getting user ID:", error);
    toast.add({
      severity: "error",
      summary: "Error de autenticación",
      detail: error.message,
      life: TOAST_AUTH_ERROR_DURATION_MS
    });
    throw error;
  }
};

const openInvoicesDialog = async () => {
  invoicesVisible.value = true;
  invoicesLoading.value = true;
  invoicesError.value = "";
  invoices.value = [];

  try {
    const builderId = getBuilderId();
    const { data } = await subscriptionApi.getInvoicesByBuilder(builderId);
    invoices.value = (data || []).map((r) => ({
      ...r,
      downloadUrl: r.receiptUrl ?? null,
      date: new Date(r.date).toLocaleDateString("es-ES", {
        year: "numeric",
        month: "short",
        day: "numeric"
      })
    }));
  } catch (error) {
    invoicesError.value = "No se pudieron cargar las facturas de Stripe.";
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "No se pudieron cargar las facturas.",
      life: TOAST_INVOICE_ERROR_DURATION_MS
    });
  } finally {
    invoicesLoading.value = false;
  }
};

onMounted(async () => {
  store.fetchAvailablePlans();
  store.fetchCurrentSubscription();

  try {
    const builderId = getBuilderId();
    if (builderId) {
      analyticsStore.fetchBuilderDashboard(builderId);
    }
  } catch (_) {}

  // Check URL parameters after returning from Stripe checkout
  const urlParams = new URLSearchParams(window.location.search);
  const sessionId = urlParams.get("session_id");
  const hasSuccess = urlParams.get("success") === "true";

  if (sessionId || hasSuccess) {
    try {
      if (sessionId) {
        const builderId = getBuilderId();
        await subscriptionApi.confirmPayment(builderId, sessionId);
      }

      toast.add({
        severity: "success",
        summary: t("subscriptions.success"),
        detail: t("subscriptions.payment-success") || "Pago procesado exitosamente.",
        life: TOAST_SUCCESS_DURATION_MS
      });

      await store.fetchCurrentSubscription();
    } catch (error) {
      console.error("Error confirming payment:", error);
      toast.add({
        severity: "warn",
        summary: t("subscriptions.warning") || "Advertencia",
        detail: "El pago fue procesado. Recarga la página si no ves tu nuevo plan actualizado.",
        life: TOAST_AUTH_ERROR_DURATION_MS
      });
    } finally {
      const cleanUrl = window.location.pathname;
      window.history.replaceState({}, document.title, cleanUrl);
    }
  }
});

const handleRenewPlan = async () => {
  if (!store.currentPlan) return;
  await handlePayPlan(store.currentPlan);
};

const handleCancelPlan = () => {
  confirm.require({
    message: t("subscriptions.confirm-cancel"),
    header: t("subscriptions.cancel-header"),
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-danger",
    accept: async () => {
      try {
        await store.cancelSubscription();
        toast.add({
          severity: "success",
          summary: t("subscriptions.success"),
          detail: t("subscriptions.cancelled-successfully"),
          life: TOAST_SUCCESS_DURATION_MS
        });
      } catch (error) {
        toast.add({
          severity: "error",
          summary: t("subscriptions.error"),
          detail: t("subscriptions.cancel-failed"),
          life: TOAST_ERROR_DURATION_MS
        });
      }
    }
  });
};

const handleSelectPlan = (plan) => {
  // If builder already has an active plan, show confirmation modal before Stripe
  if (store.currentPlan && store.currentPlan.id !== plan.id) {
    targetPlanForChange.value = plan;
    changePlanVisible.value = true;
    return;
  }
  // Otherwise direct checkout
  handlePayPlan(plan);
};

const confirmPlanChange = async () => {
  if (!targetPlanForChange.value) return;
  const plan = targetPlanForChange.value;
  changePlanVisible.value = false;
  await handlePayPlan(plan);
};

const handlePayPlan = async (plan) => {
  try {
    if (!import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY) {
      throw new Error("Falta configurar VITE_STRIPE_PUBLISHABLE_KEY en .env");
    }

    isProcessing.value = true;
    const builderId = getBuilderId();

    const { data } = await subscriptionApi.createCheckoutSession(builderId, plan.id);

    const redirectUrl = data.checkoutUrl || data.CheckoutUrl || data.url || data.Url;
    if (redirectUrl) {
      window.location.href = redirectUrl;
      return;
    }

    const sessionId = data.sessionId || data.SessionId || data.id || data.Id;
    if (sessionId) {
      const stripe = await stripePromise;
      if (!stripe) throw new Error("Stripe no se pudo inicializar");

      const { error } = await stripe.redirectToCheckout({ sessionId });
      if (error) {
        console.error("Stripe redirect error:", error);
        toast.add({
          severity: "error",
          summary: t("subscriptions.error"),
          detail: "No se pudo redirigir al pago.",
          life: TOAST_ERROR_DURATION_MS
        });
      }
      return;
    }

    throw new Error("No se recibió URL ni sessionId del servidor");
  } catch (err) {
    console.error("Payment error:", err);
    const message = err?.response?.data?.message || err?.message || "No se pudo iniciar el pago";
    toast.add({
      severity: "error",
      summary: t("subscriptions.error"),
      detail: message,
      life: TOAST_ERROR_DURATION_MS
    });
  } finally {
    isProcessing.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen bg-slate-50/60 pb-16">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-8">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <div>
          <h1 class="text-3xl font-extrabold text-gray-900 tracking-tight">
            {{ t("subscriptions.title") }}
          </h1>
          <p class="text-sm text-gray-500 mt-1">
            Administra el plan de tu empresa, supervisa cuotas de dispositivos IoT y descarga comprobantes de facturación.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <pv-button
            :label="t('subscriptions.compare-plans')"
            icon="pi pi-table"
            severity="secondary"
            outlined
            class="text-xs font-semibold py-2 px-3.5 border-gray-300 text-gray-700 bg-white hover:bg-gray-50 rounded-xl"
            @click="comparisonVisible = true"
          />

          <pv-button
            :label="t('subscriptions.view-invoices')"
            icon="pi pi-receipt"
            severity="secondary"
            outlined
            class="text-xs font-semibold py-2 px-3.5 border-gray-300 text-gray-700 bg-white hover:bg-gray-50 rounded-xl"
            @click="openInvoicesDialog"
          />
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="store.isLoading && !store.availablePlans.length" class="flex flex-col items-center justify-center py-24 gap-3">
        <pv-progress-spinner style="width: 48px; height: 48px" />
        <span class="text-sm text-gray-500">Cargando información de suscripción...</span>
      </div>

      <div v-else>
        <!-- 1. Hero Card: Current Plan & Live Quotas -->
        <CurrentPlanCard
          v-if="store.currentPlan"
          :plan="store.currentPlan"
          :subscription="store.currentSubscription"
          :totalDevices="analyticsStore.builderDashboard?.totalDevices || 0"
          :activeProjects="analyticsStore.builderDashboard?.activeProjectsCount || 0"
          :isProcessing="isProcessing"
          @renew="handleRenewPlan"
          @cancel="handleCancelPlan"
          @view-invoices="openInvoicesDialog"
          @compare-plans="comparisonVisible = true"
        />

        <!-- Banner for users without subscription -->
        <div
          v-else
          class="bg-gradient-to-r from-emerald-600 to-teal-700 text-white p-8 rounded-2xl mb-10 shadow-sm flex flex-col md:flex-row items-center justify-between gap-6"
        >
          <div>
            <div class="inline-flex items-center gap-2 bg-emerald-500/30 text-emerald-100 text-xs font-bold px-3 py-1 rounded-full mb-3">
              <i class="pi pi-sparkles"></i>
              Comienza a operar en IoBuild
            </div>
            <h2 class="text-2xl font-black mb-2">
              {{ t("subscriptions.no-subscription") }}
            </h2>
            <p class="text-emerald-100 text-sm max-w-xl leading-relaxed">
              Elige un plan de infraestructura para conectar tus dispositivos IoT, gestionar proyectos inmobiliarios y brindar acceso a los propietarios de tus unidades.
            </p>
          </div>
          <pv-button
            label="Ver Tabla Comparativa"
            icon="pi pi-arrow-right"
            iconPos="right"
            class="bg-white text-emerald-800 hover:bg-emerald-50 border-none font-bold py-3 px-5 text-sm rounded-xl shrink-0 shadow-md"
            @click="comparisonVisible = true"
          />
        </div>

        <!-- 2. Plans Grid Section -->
        <div class="mt-4">
          <div class="text-center sm:text-left mb-8">
            <h2 class="text-xl font-extrabold text-gray-900 tracking-tight">
              {{ t("subscriptions.all-plans") }}
            </h2>
            <p class="text-xs text-gray-500 mt-0.5">
              Escala tu infraestructura según la cantidad de dispositivos y unidades de tus proyectos inmobiliarios.
            </p>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 items-stretch">
            <PlanCard
              v-for="plan in store.availablePlans"
              :key="plan.id"
              :plan="plan"
              :currentPlan="store.currentPlan"
              :isPopular="plan.name?.toLowerCase().includes('pro')"
              :isProcessing="isProcessing"
              @select="handleSelectPlan"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <PreviousInvoicesModal
      :visible="invoicesVisible"
      :invoices="invoices"
      :loading="invoicesLoading"
      :error="invoicesError"
      @close="invoicesVisible = false"
    />

    <ChangePlanModal
      :visible="changePlanVisible"
      :targetPlan="targetPlanForChange"
      :currentPlan="store.currentPlan"
      :isProcessing="isProcessing"
      @close="changePlanVisible = false"
      @confirm="confirmPlanChange"
    />

    <PlanComparisonModal
      :visible="comparisonVisible"
      :plans="store.availablePlans"
      :currentPlanId="store.currentPlan?.id"
      :isProcessing="isProcessing"
      @close="comparisonVisible = false"
      @select-plan="handleSelectPlan"
    />
  </div>
</template>

<style scoped>
</style>
